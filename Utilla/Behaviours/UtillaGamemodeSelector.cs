using GorillaGameModes;
using GorillaNetworking;
using GorillaTag;
using GorillaTagScripts.VirtualStumpCustomMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilla.Models;
using Utilla.Tools;

namespace Utilla.Behaviours
{
    [RequireComponent(typeof(GameModeSelectorButtonLayout)), DisallowMultipleComponent]
    internal class UtillaGamemodeSelector : MonoBehaviour
    {
        public bool Active => isActiveAndEnabled && (ZoneManagement.instance.activeZones is List<GTZone> activeZones && activeZones.Contains(Zone));

        // public List<Gamemode> BaseGameModes;
        public readonly Dictionary<bool, List<Gamemode>> SelectorGameModes = [];

        public GameModeSelectorButtonLayout Layout;
        public GTZone Zone;

        public int CurrentPage, PageCount, PageCapacity;

        private readonly List<ModeSelectButton> moddedSelectionButtons = [];
        private static GameObject fallbackTemplateButton = null;

        public async void Awake()
        {
            Layout = GetComponent<GameModeSelectorButtonLayout>();
            Zone = Layout.zone;

            while (Layout.currentButtons.Count == 0)
                await Task.Yield();

            ModifySelectionButtons();
            CreatePageButtons(Layout.currentButtons.First().gameObject);

            if (!GamemodeManager.Initialization.Task.IsCompleted)
                await GamemodeManager.Initialization.Task;

            Logging.Message($"UtillaGameModeSelector {Zone}");

            if (Active || Layout is CustomMapModeSelector)
            {
                Logging.Info("Checking game mode validity");
                CheckGameMode();
            }

            PageCount = Mathf.CeilToInt(GetSelectorGameModes().Count / (float)PageCapacity);
            ShowPage();
        }

        public void OnEnable()
        {
            NetworkSystem.Instance.OnJoinedRoomEvent += ShowPage;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += ShowPage;
        }

        public void OnDisable()
        {
            NetworkSystem.Instance.OnJoinedRoomEvent -= ShowPage;
            NetworkSystem.Instance.OnReturnedToSinglePlayer -= ShowPage;
        }

        public void OnSelectorSetup()
        {
            // if (Layout is not CustomMapModeSelector) return;

            bool sessionIsPrivate = NetworkSystem.Instance.SessionIsPrivate;
            if (SelectorGameModes.ContainsKey(sessionIsPrivate)) SelectorGameModes.Remove(sessionIsPrivate);

            ModifySelectionButtons();
            ShowPage();
        }

        public List<Gamemode> GetSelectorGameModes()
        {
            bool sessionIsPrivate = NetworkSystem.Instance.SessionIsPrivate;

            if (SelectorGameModes.TryGetValue(sessionIsPrivate, out List<Gamemode> gameModeList))
                return gameModeList;

            gameModeList = [];

            Logging.Info($"GetSelectorGameModes {Zone}");

            GameModeType[] modesForZone = Layout is CustomMapModeSelector ? [.. CustomMapModeSelector.gamemodes] : [.. GameMode.GameModeZoneMapping.GetModesForZone(Zone, NetworkSystem.Instance.SessionIsPrivate)];

            // Base gamemodes
            for (int i = 0; i < modesForZone.Length; i++)
            {
                if (GamemodeManager.Instance.DefaultGameModesPerMode.TryGetValue(modesForZone[i], out Gamemode gamemode))
                {
                    gameModeList.Add(gamemode);
                }
            }

            // Modded gamemodes
            for (int i = 0; i < modesForZone.Length; i++)
            {
                if (GamemodeManager.Instance.ModdedGamemodesPerMode.TryGetValue(modesForZone[i], out Gamemode gamemode))
                {
                    Logging.Info($"+ \"{gamemode.DisplayName}\" ({modesForZone[i].GetName()})");
                    gameModeList.Add(gamemode);
                    continue;
                }

                gameModeList.Add(null);
            }

            // Custom gamemodes
            if (GamemodeManager.Instance.CustomGameModes is List<Gamemode> customGameModes)
            {
                for (int i = 0; i < customGameModes.Count; i++)
                {
                    Gamemode gameMode = customGameModes[i];
                    Logging.Info($"+ \"{gameMode.DisplayName}\"");
                    gameModeList.Add(gameMode);
                    continue;
                }
            }

            bool superModeState = Layout.superToggleButton.isOn;
            List<Gamemode> finalGameModeList = [.. superModeState
                ? gameModeList.Where(gameMode => !gameMode.BaseGamemode.HasValue || (gameMode.BaseGamemode.Value != GameModeType.Casual && gameMode.BaseGamemode.Value != GameModeType.Infection))
                : gameModeList.Where(gameMode => !gameMode.BaseGamemode.HasValue || (gameMode.BaseGamemode.Value != GameModeType.SuperCasual && gameMode.BaseGamemode.Value != GameModeType.SuperInfect))
            ];

            if (SelectorGameModes.TryAdd(sessionIsPrivate, finalGameModeList))
            {
                Logging.Info(string.Join(", ", finalGameModeList.Select(gameMode => gameMode.DisplayName).Select(gameMode => string.Format("\"{0}\"", gameMode))));
            }

            return finalGameModeList;
        }

        public void CheckGameMode()
        {
            if (!Active)
            {
                ShowPage();
                return;
            }

            string currentMode = GorillaComputer.instance.currentGameMode.Value;
            if (GamemodeManager.Instance.CustomGameModes.Exists(mode => mode.ID == currentMode)) return;

            if (Layout is CustomMapModeSelector)
            {
                SetDefaultMode(CustomMapModeSelector.defaultGamemodeForLoadedMap.GetName());
                return;
            }

            var modeNames = GetSelectorGameModes().Where(mode => mode != null).Select(mode => mode.ID).ToArray();
            int index = Array.IndexOf(modeNames, currentMode);

            if (index == -1)
            {
                SetDefaultMode(modeNames.First());
                return;
            }

            CurrentPage = Mathf.FloorToInt(index / (float)PageCapacity);
            ShowPage();
        }

        public void SetDefaultMode(string defaultMode)
        {
            Logging.Message($"SetDefaultMode : {defaultMode}");

            string currentMode = GorillaComputer.instance.currentGameMode.Value;
            bool isModded = currentMode.Contains(Constants.ModdedPrefix);

            if (isModded)
            {
                bool isBaseMode = Enum.TryParse(defaultMode, out GameModeType modeType);
                if (isBaseMode && GamemodeManager.Instance.ModdedGamemodesPerMode.TryGetValue(modeType, out Gamemode moddedGamemode)) defaultMode = moddedGamemode.ID;
            }

            GorillaComputer.instance.SetGameModeWithoutButton(defaultMode);

            var modeNames = GetSelectorGameModes().Select(game_mode => game_mode != null ? game_mode.ID : string.Empty).ToArray();
            int index = Array.IndexOf(modeNames, defaultMode);
            CurrentPage = Mathf.Max(Mathf.FloorToInt(index / (float)PageCapacity), 0);
            Logging.Info($"Set to {defaultMode} on page {CurrentPage}");

            ShowPage();
        }

        public void NextPage()
        {
            CurrentPage = (CurrentPage + 1) % PageCount;
            ShowPage();
        }

        public void PreviousPage()
        {
            CurrentPage = (CurrentPage <= 0) ? PageCount - 1 : CurrentPage - 1;
            ShowPage();
        }

        public void ShowPage()
        {
            List<Gamemode> allGamemodes = GetSelectorGameModes();
            List<Gamemode> shownGamemodes = [.. allGamemodes.Skip(CurrentPage * PageCapacity).Take(PageCapacity)];

            for (int i = 0; i < Layout.currentButtons.Count; i++)
            {
                Gamemode gamemode = shownGamemodes.ElementAtOrDefault(i);
                bool hasMode = gamemode != null && gamemode.ID != null && gamemode.ID.Length != 0;

                ModeSelectButton button = Layout.currentButtons[i];
                if (button.gameObject.activeSelf != hasMode) button.gameObject.SetActive(hasMode);

                if (!button.gameObject.activeSelf)
                {
                    button.enabled = false;
                    button.SetInfo(string.Empty, string.Empty, false, null);
                    continue;
                }

                button.enabled = true;
                button.SetInfo(gamemode.ID, gamemode.DisplayName.ToUpper(), false, null);
                button.OnGameModeChanged(GorillaComputer.instance.currentGameMode.Value);
            }
        }

        private void CreatePageButtons(GameObject templateButton)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.SetActive(false);
            MeshFilter meshFilter = cube.GetComponent<MeshFilter>();

            GameObject CreatePageButton(string text, Action onPressed)
            {
                // button creation
                GameObject button = Instantiate(templateButton.transform.childCount == 0 ? fallbackTemplateButton : templateButton);

                // button appearence
                button.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                button.GetComponent<Renderer>().material = templateButton.GetComponent<GorillaPressableButton>().unpressedMaterial;

                // button location
                button.transform.parent = templateButton.transform.parent;
                button.transform.localRotation = templateButton.transform.localRotation;
                button.transform.localScale = Vector3.one * 0.1427168f; // shouldn't hurt anyone for now 

                TMP_Text tmpText = button.transform.Find("Title")?.GetComponent<TMP_Text>() ?? button.GetComponentInChildren<TMP_Text>(true);
                if (tmpText)
                {
                    tmpText.gameObject.SetActive(true);
                    tmpText.enabled = true;
                    tmpText.transform.localPosition = Vector3.forward * 0.525f;
                    tmpText.transform.localEulerAngles = Vector3.up * 180f;
                    tmpText.transform.localScale = Vector3.Scale(tmpText.transform.localScale, new Vector3(0.5f, 0.5f, 1));
                    tmpText.text = text;
                    tmpText.color = Color.black;
                    tmpText.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    if (tmpText.TryGetComponent(out StaticLodGroup group)) Destroy(group);
                }
                else if (button.GetComponentInChildren<Text>() is Text buttonText)
                {
                    buttonText.text = text;
                    buttonText.transform.localScale = Vector3.Scale(buttonText.transform.localScale, new Vector3(2, 2, 1));
                }

                // button behaviour
                Destroy(button.GetComponent<ModeSelectButton>());
                var unityEvent = new UnityEvent();
                unityEvent.AddListener(new UnityAction(onPressed));
                var pressable_button = button.AddComponent<GorillaPressableButton>();
                pressable_button.onPressButton = unityEvent;

                if (button.transform.Find("NewSplash") is Transform splash && splash && splash.gameObject.activeSelf)
                    splash.gameObject.SetActive(false);

                return button;
            }

            bool isCustomSelector = Layout is CustomMapModeSelector;

            GameObject nextPageButton = CreatePageButton("-->", NextPage);
            nextPageButton.transform.localPosition = new Vector3(isCustomSelector ? -0.78f : -0.745f, isCustomSelector ? 0.1f : -0.095f, -0.03f);

            GameObject previousPageButton = CreatePageButton("<--", PreviousPage);
            previousPageButton.transform.localPosition = new Vector3(isCustomSelector ? -0.78f : -0.745f, -0.75f, -0.03f);

            Destroy(cube);

            if (templateButton.transform.childCount != 0)
            {
                fallbackTemplateButton = templateButton;
            }
        }

        private void ModifySelectionButtons()
        {
            PageCapacity = Layout.currentButtons.Count(button => button.gameObject.activeSelf);

            foreach (var mb in Layout.currentButtons)
            {
                if (moddedSelectionButtons.Contains(mb)) continue;
                moddedSelectionButtons.Add(mb);

                TMP_Text gamemodeTitle = mb.gameModeTitle;
                gamemodeTitle.fontSizeMax = gamemodeTitle.fontSize;
                gamemodeTitle.fontSizeMin = 0f;
                gamemodeTitle.enableAutoSizing = true;
                gamemodeTitle.transform.localPosition = new Vector3(gamemodeTitle.transform.localPosition.x, 0f, gamemodeTitle.transform.localPosition.z + 0.08f);
            }
        }
    }
}
