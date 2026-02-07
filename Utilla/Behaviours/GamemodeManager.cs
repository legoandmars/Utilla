using BepInEx;
using BepInEx.Bootstrap;
using GorillaGameModes;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Utilla.Attributes;
using Utilla.Models;
using Utilla.Patches;
using Utilla.Tools;
using Utilla.Utils;

namespace Utilla.Behaviours
{
    internal class GamemodeManager : MonoBehaviour
    {
        public static GamemodeManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public static TaskCompletionSource<GamemodeManager> Initialization { get; private set; } = new();

        public List<Gamemode> Gamemodes { get; private set; }
        public List<Gamemode> ModdedGamemodes { get; private set; }

        public readonly Dictionary<GameModeType, Gamemode> DefaultGameModesPerMode = [];
        public readonly Dictionary<GameModeType, Gamemode> ModdedGamemodesPerMode = [];

        // Custom game modes
        public List<Gamemode> CustomGameModes;
        private GameObject customGameModeContainer;
        private List<PluginInfo> pluginInfos;

        public void Awake()
        {
            if (Initialization.Task.IsCompleted) return;

            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            Events.RoomJoined += OnRoomJoin;
            Events.RoomLeft += OnRoomLeft;

            customGameModeContainer = new GameObject("Utilla Custom Game Modes");
            customGameModeContainer.transform.SetParent(GameMode.instance.gameObject.transform);

            string currentGameMode = PlayerPrefs.GetString(GorillaComputerPatches.ModePreferenceKey, GameModeType.Infection.ToString());
            GorillaComputer.instance.currentGameMode.Value = currentGameMode;

            GameModeType[] gameModeTypes = [.. Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>()];
            for (int i = 0; i < gameModeTypes.Length; i++)
            {
                if (i == (int)GameModeType.Count) break;

                GameModeType modeType = gameModeTypes[i];
                if (!DefaultGameModesPerMode.TryAdd(modeType, new Gamemode(modeType))) continue;
                ModdedGamemodesPerMode.Add(modeType, new Gamemode(Constants.ModdedPrefix, $"Modded {GameModeUtils.GetGameModeName(modeType)}", modeType));
            }

            Logging.Info($"Modded Game Modes: {string.Join(", ", ModdedGamemodesPerMode.Select(item => item.Value).Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");
            ModdedGamemodes = [.. ModdedGamemodesPerMode.Values];

            Gamemodes = [.. DefaultGameModesPerMode.Values];

            pluginInfos = GetPluginInfos();
            CustomGameModes = GetGamemodes(pluginInfos);
            Logging.Info($"Custom Game Modes: {string.Join(", ", CustomGameModes.Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");
            Gamemodes.AddRange(ModdedGamemodes.Concat(CustomGameModes));
            Gamemodes.ForEach(AddGamemodeToPrefabPool);
            Logging.Info($"Game Modes: {string.Join(", ", Gamemodes.Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");

            Initialization.SetResult(this);
        }

        public List<Gamemode> GetGamemodes(List<PluginInfo> infos)
        {
            List<Gamemode> gamemodes = [];

            HashSet<Gamemode> additonalGamemodes = [];
            foreach (var info in infos)
            {
                additonalGamemodes.UnionWith(info.Gamemodes);
            }

            foreach (var gamemode in ModdedGamemodes)
            {
                additonalGamemodes.Remove(gamemode);
            }

            gamemodes.AddRange(additonalGamemodes);

            return gamemodes;
        }

        List<PluginInfo> GetPluginInfos()
        {
            List<PluginInfo> infos = [];

            foreach (var info in Chainloader.PluginInfos)
            {
                if (info.Value is null) continue;
                BaseUnityPlugin plugin = info.Value.Instance;
                if (plugin is null) continue;
                Type type = plugin.GetType();

                IEnumerable<Gamemode> gamemodes = GetGamemodes(type);

                if (gamemodes.Any())
                {
                    infos.Add(new PluginInfo
                    {
                        Plugin = plugin,
                        Gamemodes = [.. gamemodes],
                        OnGamemodeJoin = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeJoinAttribute)),
                        OnGamemodeLeave = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeLeaveAttribute))
                    });
                }
            }

            return infos;
        }

        Action<string> CreateJoinLeaveAction(BaseUnityPlugin plugin, Type baseType, Type attribute)
        {
            ParameterExpression param = Expression.Parameter(typeof(string));
            ParameterExpression[] paramExpression = [param];
            ConstantExpression instance = Expression.Constant(plugin);
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Action<string> action = null;
            foreach (var method in baseType.GetMethods(bindingFlags).Where(m => m.GetCustomAttribute(attribute) != null))
            {
                var parameters = method.GetParameters();
                MethodCallExpression methodCall;
                if (parameters.Length == 0)
                {
                    methodCall = Expression.Call(instance, method);
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    methodCall = Expression.Call(instance, method, param);
                }
                else
                {
                    continue;
                }

                action += Expression.Lambda<Action<string>>(methodCall, paramExpression).Compile();
            }

            return action;
        }

        HashSet<Gamemode> GetGamemodes(Type type)
        {
            IEnumerable<ModdedGamemodeAttribute> attributes = type.GetCustomAttributes<ModdedGamemodeAttribute>();

            HashSet<Gamemode> gamemodes = [];
            if (attributes is not null)
            {
                foreach (ModdedGamemodeAttribute attribute in attributes)
                {
                    if (attribute.gamemode is not null)
                    {
                        gamemodes.Add(attribute.gamemode);
                        continue;
                    }
                    gamemodes.UnionWith(ModdedGamemodes);
                }
            }

            return gamemodes;
        }

        void AddGamemodeToPrefabPool(Gamemode gamemode)
        {
            if (GameMode.gameModeKeyByName.ContainsKey(gamemode.ID))
            {
                Logging.Warning($"Game Mode already exists: has ID {gamemode.ID}");
                return;
            }

            if (gamemode.BaseGamemode.HasValue && gamemode.ID != gamemode.BaseGamemode.Value.GetName())
            {
                GameMode.gameModeKeyByName.Add(gamemode.ID, GameMode.gameModeKeyByName[gamemode.BaseGamemode.Value.GetName()]);
                return;
            }

            if (gamemode.GameManager is null) return;

            Type gmType = gamemode.GameManager;

            if (gmType is null || !gmType.IsSubclassOf(typeof(GorillaGameManager)))
            {
                GameModeType? gmKey = gamemode.BaseGamemode;

                if (gmKey == null)
                {
                    Logging.Warning($"Game Mode not made cuz lack of info: has ID {gamemode.ID}");
                    return;
                }

                GameMode.gameModeKeyByName[gamemode.ID] = (int)gmKey;
                //GameMode.gameModeKeyByName[gamemode.DisplayName] = (int)gmKey;
                GameMode.gameModeNames.Add(gamemode.ID);
                return;
            }

            GameObject prefab = new($"{gamemode.ID}: {gmType.Name}");
            prefab.SetActive(false);

            GorillaGameManager gameMode = prefab.AddComponent(gmType) as GorillaGameManager;
            int gameModeKey = (int)gameMode.GameType();

            if (GameMode.gameModeTable.ContainsKey(gameModeKey))
            {
                Logging.Error($"Game Mode with name '{GameMode.gameModeTable[gameModeKey].GameModeName()}' is already using GameType '{gameModeKey}'.");
                Destroy(prefab);
                return;
            }

            GameMode.gameModeTable[gameModeKey] = gameMode;
            GameMode.gameModeKeyByName[gamemode.ID] = gameModeKey;
            //GameMode.gameModeKeyByName[gamemode.DisplayName] = gameModeKey;
            GameMode.gameModeNames.Add(gamemode.ID);
            GameMode.gameModes.Add(gameMode);

            prefab.transform.SetParent(customGameModeContainer.transform);
            prefab.SetActive(true);

            if (gameMode.fastJumpLimit == 0 || gameMode.fastJumpMultiplier == 0)
            {
                Logging.Warning($"FAST JUMP SPEED AREN'T ASSIGNED FOR {gmType.Name}!!! ASSIGN THESE ASAP");

                float[] speed = gameMode.LocalPlayerSpeed();
                gameMode.fastJumpLimit = speed[0];
                gameMode.fastJumpMultiplier = speed[1];
            }
        }

        internal void OnRoomJoin(object sender, Events.RoomJoinedArgs args)
        {
            string gamemode = args.Gamemode;

            Logging.Info($"Joined room: with game mode {gamemode}");

            foreach (var pluginInfo in pluginInfos)
            {
                Logging.Info($"Plugin {pluginInfo.Plugin.Info.Metadata.Name}: {string.Join(", ", pluginInfo.Gamemodes.Select(gm => gm.ID))}");

                if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.ID)))
                {
                    try
                    {
                        pluginInfo.OnGamemodeJoin?.Invoke(gamemode);//
                        Logging.Message("Plugin is suitable for game mode");
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Join action could not be called");
                        Logging.Error(ex);
                    }
                    continue;
                }

                Logging.Message("Plugin is unsupported for game mode");
            }
        }

        internal void OnRoomLeft(object sender, Events.RoomJoinedArgs args)
        {
            string gamemode = args.Gamemode;

            Logging.Info($"Left room: with game mode {gamemode}");

            foreach (var pluginInfo in pluginInfos)
            {
                if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.ID)))
                {
                    try
                    {
                        pluginInfo.OnGamemodeLeave?.Invoke(gamemode);
                        //Logging.Info($"Plugin {pluginInfo.Plugin.Info.Metadata.Name} is suitable for game mode");
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Leave action could not be called");
                        Logging.Error(ex);
                    }
                }
            }
        }
    }
}
