using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GorillaNetworking;
using Utilla.Models;
using GorillaExtensions;
using HarmonyLib;
using System.Globalization;
using TMPro;
using UnityEngine.Events;

namespace Utilla
{
	public class GamemodeSelector : MonoBehaviour
	{
		public const int PageSize = 4;

		GameModeSelectorButtonLayout layout;

        ModeSelectButton[] modeSelectButtons = Array.Empty<ModeSelectButton>();

        ModeSelectButtonInfoData[] baseSelectionInfo;

        static int _globalSelectionPage;

		public void Initialize(Transform parent, Transform anchor, Transform buttonLayout)
		{
			if (!parent || !anchor || !buttonLayout)
			{
				UtillaLogging.Error("GamemodeSelector could not be initialized, all Transform parameters are required to ensure a smooth transition for custom gamemode usage.");
				throw new NullReferenceException();
			}

			transform.parent = parent;

			var selectorTitle = anchor.Find("GameModes Title Text");

			if (selectorTitle)
			{
                selectorTitle.transform.localPosition += Vector3.right * -0.04f;
			}

			var buttons = Enumerable.Range(0, PageSize).Select(x => buttonLayout.GetChild(x));

			modeSelectButtons = buttons.Select(x => x.GetComponent<ModeSelectButton>()).ToArray();

            foreach (var mb in modeSelectButtons)
			{
				mb.transform.localPosition += Vector3.right * -0.055f;

				TMP_Text gamemodeTitle = (TMP_Text)AccessTools.Field(mb.GetType(), "gameModeTitle").GetValue(mb);

                gamemodeTitle.fontSize = 64f;
                gamemodeTitle.transform.localPosition = new Vector3(gamemodeTitle.transform.localPosition.x, 0f, gamemodeTitle.transform.localPosition.z + 0.08f);
			}

			layout = buttonLayout.GetComponent<GameModeSelectorButtonLayout>();

			baseSelectionInfo = ((GameModeSelectorButtonLayoutData)AccessTools.Field(layout.GetType(), "data").GetValue(layout)).Info;

            CreatePageButtons(buttons.First().gameObject);

			if (GamemodeManager.Instance.Gamemodes != null)
			{
				UtillaLogging.Log($"Current page of the GamemodeSelector is set to {_globalSelectionPage}.");
                ShowPage(_globalSelectionPage);
            }
		}

		public void GetSelectorGamemodes(out List<Gamemode> baseGamemodes, out List<Gamemode> moddedGamemodes)
		{
            baseGamemodes = new List<Gamemode>();
            moddedGamemodes = new List<Gamemode>();

            if (modeSelectButtons == null || modeSelectButtons.Length == 0 || !layout) return;

            for (int i = 0; i < modeSelectButtons.Length; i++)
			{
                ModeSelectButtonInfoData info = baseSelectionInfo[i];
                baseGamemodes.Add(info);
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

			foreach(Gamemode bm in baseGamemodes)
			{
				bool isInfection = bm.DisplayName.ToUpper() == "INFECTION";
				bool isBattle = bm.DisplayName.ToLower() == "paintbrawl";

                BaseGamemode baseMode = isBattle ? BaseGamemode.Battle : isInfection ? BaseGamemode.Infection : Enum.Parse<BaseGamemode>(textInfo.ToTitleCase(bm.ID.ToLower())); // i.e (referencing the titlecase), INFECTION = Infection, PAINTBRAWL (displayname) = Paintbrawl
                string moddedTitle = isBattle ? "MODDED BATTLE" :  isInfection ? "MODDED" : $"MODDED {bm.ID.ToUpper()}"; // i.e, INFECTION = MODDED, PAINTBRAWL (displayname) = MODDED BATTLE (id)

                moddedGamemodes.Add(new Gamemode($"MODDED_{bm.ID.ToUpper()}", moddedTitle, baseMode));
            }
        }

        static GameObject fallbackTemplateButton = null;
		void CreatePageButtons(GameObject templateButton)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.SetActive(false);
			MeshFilter meshFilter = cube.GetComponent<MeshFilter>();

			GameObject CreatePageButton(string text, Action onPressed)
			{
				GameObject button = GameObject.Instantiate(templateButton.transform.childCount == 0 ? fallbackTemplateButton : templateButton);
				button.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
				button.GetComponent<Renderer>().material = templateButton.GetComponent<GorillaPressableButton>().unpressedMaterial;
				button.transform.parent = templateButton.transform.parent;
				button.transform.localRotation = templateButton.transform.localRotation;
				button.transform.localScale = Vector3.one * 0.1427168f; // shouldn't hurt anyone for now 

				button.transform.GetChild(0).gameObject.SetActive(true);
				Text buttonText = button.GetComponentInChildren<Text>();
				if (buttonText != null)
				{
					buttonText.text = text;
					buttonText.transform.localScale = Vector3.Scale(buttonText.transform.localScale, new Vector3(2, 2, 1));
				}

				TMP_Text tmpText = button.transform.Find("Title")?.GetComponent<TMP_Text>() ?? button.GetComponentInChildren<TMP_Text>();
				if (tmpText)
				{
					tmpText.gameObject.SetActive(false);
                }

				var unityEvent = new UnityEvent();
				unityEvent.AddListener(new UnityAction(onPressed));

				GameObject.Destroy(button.GetComponent<ModeSelectButton>());
				button.AddComponent<GorillaPressableButton>().onPressButton = unityEvent;

				if (!button.GetComponentInParent<Canvas>())
				{
					Canvas canvas = button.transform.parent.gameObject.GetOrAddComponent<Canvas>();
					canvas.renderMode = RenderMode.WorldSpace;
				}

				return button;
			}

			GameObject nextPageButton = CreatePageButton("-->", NextPage);
			nextPageButton.transform.localPosition = new Vector3(-0.775f, nextPageButton.transform.position.y + 0.005f, nextPageButton.transform.position.z);

			GameObject previousPageButton = CreatePageButton("<--", PreviousPage);
			previousPageButton.transform.localPosition = new Vector3(-0.775f, -0.633f, previousPageButton.transform.position.z);

			Destroy(cube);

			if (templateButton.transform.childCount != 0)
			{
				fallbackTemplateButton = templateButton;
			}
		}

		public void NextPage()
		{
			if (_globalSelectionPage < GamemodeManager.Instance.PageCount - 1)
			{
				ShowPage(_globalSelectionPage + 1);
			}
		}

		public void PreviousPage()
		{
			if (_globalSelectionPage > 0)
			{
				ShowPage(_globalSelectionPage - 1);
			}
		}

		public void ShowPage(int page)
		{
            _globalSelectionPage = page;

			List<Gamemode> currentGamemodes = GamemodeManager.Instance.Gamemodes.Skip(page * PageSize).Take(PageSize).ToList();

			Gamemode nullGamemode = new Gamemode("", "");

			for (int i = 0; i < modeSelectButtons.Length; i++)
			{
				if (i < currentGamemodes.Count)
				{
					Gamemode customMode = currentGamemodes[i];
		
                    modeSelectButtons[i].enabled = true;

					modeSelectButtons[i].SetInfo(baseSelectionInfo.FirstOrDefault(i => i.Mode == customMode.ID) ?? customMode);
				}
				else
				{
					modeSelectButtons[i].enabled = false;

					modeSelectButtons[i].SetInfo(nullGamemode);
				}
			}

            GorillaComputer.instance.OnModeSelectButtonPress(GorillaComputer.instance.currentGameMode.Value, GorillaComputer.instance.leftHanded);
        }
	}
}
