using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using GorillaNetworking;
using GorillaGameModes;
using BepInEx;
using HarmonyLib;

using Utilla.Models;

namespace Utilla
{
	public class GamemodeManager : MonoBehaviour
	{
		public static GamemodeManager Instance { get; private set; }

		const string BasePrefabPath = "CustomGameManager/";

		public int PageCount => Mathf.CeilToInt(Gamemodes.Count() / 4f);

		List<Gamemode> DefaultModdedGamemodes = new List<Gamemode>()
		{
			new Gamemode("MODDED_CASUAL", "MODDED CASUAL", BaseGamemode.Casual),
			new Gamemode("MODDED_DEFAULT", "MODDED", BaseGamemode.Infection),
			new Gamemode("MODDED_HUNT", "MODDED HUNT", BaseGamemode.Hunt),
            new Gamemode("MODDED_BATTLE", "MODDED BRAWL", BaseGamemode.PaintbrawlBattle)
        };
		public List<Gamemode> Gamemodes { get; private set; } = new List<Gamemode>() { 
			new Gamemode("CASUAL", "CASUAL"),
			new Gamemode("INFECTION", "INFECTION"),
			new Gamemode("HUNT", "HUNT"),
            new Gamemode("BATTLE", "PAINTBRAWL")
        };

		List<PluginInfo> pluginInfos;

		FieldInfo fiGameModeInstance = typeof(GameMode).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
		GameMode gtGameModeInstance;

		FieldInfo fiGameModeTable = typeof(GameMode).GetField("gameModeTable", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<int, GorillaGameManager> gtGameModeTable;

		FieldInfo fiGameModeKeyByName = typeof(GameMode).GetField("gameModeKeyByName", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<string, int> gtGameModeKeyByName;

		FieldInfo fiGameModes = typeof(GameMode).GetField("gameModes", BindingFlags.Static | BindingFlags.NonPublic);
		List<GorillaGameManager> gtGameModes;

		List<string> gtGameModeNames;

		GameObject moddedGameModesObject;

		struct GameModeSelectorPath
		{
			public string name;
			public string buttonPath;
			public string gamemodesPath;
			public string transformToFind;
		}

		Dictionary<string, GameModeSelectorPath> gameModeButtonsDict = new Dictionary<string, GameModeSelectorPath>() {
			{
				"GorillaTag",
				new GameModeSelectorPath() {
					name = "TreehouseSelector",
					buttonPath = "anchor",
					gamemodesPath = "anchor",
					transformToFind = "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/ModeSelector_Group/Selector Buttons"
                }
			},
			{
				"Beach",
				new GameModeSelectorPath() {
					name = "BeachSelector",
					buttonPath = "modeselectbox (3)/anchor",
					gamemodesPath = "UI FOR BEACH COMPUTER",
					transformToFind = "Beach/BeachComputer"
				}
			},
			{   "Mountain",
				new GameModeSelectorPath() {
					name = "MountainSelector",
					buttonPath = "Geometry/goodigloo/modeselectbox (1)/anchor",
					gamemodesPath = "UI/Text",
					transformToFind = "Mountain"
				}
			},
			{   "Skyjungle",
				new GameModeSelectorPath() {
					name = "SkySelector",
					buttonPath = "anchor",
					gamemodesPath = "ModeSelectorText",
					transformToFind = "skyjungle/UI/-- Clouds ModeSelectBox UI --"
				}
			},
			{
				"Rotating",
				new GameModeSelectorPath() {
					name = "RotatingSelector",
					buttonPath = "anchor",
					gamemodesPath = "ModeSelectorText",
					transformToFind = "RotatingMap/SwampLevel/UI (1)/-- Rotating ModeSelectBox UI --"
                }
			}
		};

		void Start()
		{
			Instance = this;
			Events.RoomJoined += OnRoomJoin;
			Events.RoomLeft += OnRoomLeft;

			gtGameModeInstance = fiGameModeInstance.GetValue(null) as GameMode;
			gtGameModeTable = fiGameModeTable.GetValue(null) as Dictionary<int, GorillaGameManager>;
			gtGameModeKeyByName = fiGameModeKeyByName.GetValue(null) as Dictionary<string, int>;
			gtGameModes = fiGameModes.GetValue(null) as List<GorillaGameManager>;
			gtGameModeNames = GameMode.gameModeNames;

			moddedGameModesObject = new GameObject("Modded Game Modes");
			moddedGameModesObject.transform.SetParent(gtGameModeInstance.gameObject.transform);
			// transform.parent = GameObject.Find(UIRootPath).transform;

            GorillaComputer.instance.currentGameMode.Value = PlayerPrefs.GetString("currentGameMode", "INFECTION");

			pluginInfos = GetPluginInfos();

			Gamemodes.AddRange(GetGamemodes(pluginInfos));
			Gamemodes.ForEach(gamemode => AddGamemodeToPrefabPool(gamemode));

			InitializeSelector(gameModeButtonsDict["GorillaTag"]);

			SceneManager.sceneLoaded += OnSceneChange;
		}

		void OnSceneChange(Scene scene, LoadSceneMode loadMode)
		{
			if (gameModeButtonsDict.TryGetValue(scene.name, out var buttonData))
			{
				InitializeSelector(buttonData);
			}
		}

		// void InitializeSelector(string name, Transform parent, string buttonPath, string gamemodesPath)
		void InitializeSelector(GameModeSelectorPath gmPathData)
		{
			try
			{
				Transform parent = GameObject.Find(gmPathData.transformToFind)?.transform;
				var selector = new GameObject(gmPathData.name).AddComponent<GamemodeSelector>();

				// child objects might be removed when gamemodes is released, keeping default behaviour for now
				var ButtonParent = parent.Find(gmPathData.buttonPath);
				foreach(Transform child in ButtonParent) {
					if (child.gameObject.name.StartsWith("ENABLE FOR BETA"))
					{
						ButtonParent = child;
						break;
					}
				}

				// gameobject name for the text object changed but might change back after gamemodes is released
				var GamemodesList = parent.Find(gmPathData.gamemodesPath);
				foreach (Transform child in GamemodesList) {
					if (child.gameObject.name.StartsWith("Game Mode List Text ENABLE FOR BETA"))
					{
						GamemodesList = child;
						break;
					}
				}

				selector.Initialize(parent, ButtonParent, GamemodesList);
			}
			catch (Exception e)
			{
				Debug.LogError($"Utilla: Failed to initialize {name}: {e}");
			}

		}

		List<Gamemode> GetGamemodes(List<PluginInfo> infos)
		{
			List<Gamemode> gamemodes = new List<Gamemode>();
			gamemodes.AddRange(DefaultModdedGamemodes);

			HashSet<Gamemode> additonalGamemodes = new HashSet<Gamemode>();
			foreach (var info in infos)
			{
				additonalGamemodes.UnionWith(info.Gamemodes);
			}

			foreach (var gamemode in DefaultModdedGamemodes)
			{
				additonalGamemodes.Remove(gamemode);
			}

			gamemodes.AddRange(additonalGamemodes);

			return gamemodes;
		}

		List<PluginInfo> GetPluginInfos()
		{
			List<PluginInfo> infos = new List<PluginInfo>();
			foreach (var info in BepInEx.Bootstrap.Chainloader.PluginInfos)
			{
				if (info.Value == null) continue;
				BaseUnityPlugin plugin = info.Value.Instance;
				if (plugin == null) continue;
				Type type = plugin.GetType();

				IEnumerable<Gamemode> gamemodes = GetGamemodes(type);

				if (gamemodes.Count() > 0)
				{
					infos.Add(new PluginInfo
					{
						Plugin = plugin,
						Gamemodes = gamemodes.ToArray(),
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
			ParameterExpression[] paramExpression = new ParameterExpression[] { param };
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

			HashSet<Gamemode> gamemodes = new HashSet<Gamemode>();
			if (attributes != null)
			{
				foreach (ModdedGamemodeAttribute attribute in attributes)
				{
					if (attribute.gamemode != null)
					{
						gamemodes.Add(attribute.gamemode);
					}
					else
					{
						gamemodes.UnionWith(DefaultModdedGamemodes);
					}
				}
			}

			return gamemodes;
		}

		void AddGamemodeToPrefabPool(Gamemode gamemode)
		{
			if (gamemode.GameManager is null) return;
            if (gtGameModeKeyByName.ContainsKey(gamemode.GamemodeString) || gtGameModeKeyByName.ContainsKey(gamemode.DisplayName))
            {
                Debug.LogError($"game with name \"{gamemode.GamemodeString}\" or \"{gamemode.DisplayName}\" already exists");
                return;
            }

			Type gmType = gamemode.GameManager;
			if (gmType == null || !gmType.IsSubclassOf(typeof(GorillaGameManager)))
			{
				GameModeType? gmKey = gamemode.BaseGamemode switch
				{
					BaseGamemode.Casual => GameModeType.Casual,
					BaseGamemode.Infection => GameModeType.Infection,
					BaseGamemode.Hunt => GameModeType.Hunt,
					BaseGamemode.PaintbrawlBattle => GameModeType.Battle,
					_ => null
				};

				if (gmKey == null)
				{
					return;
				}

				gtGameModeKeyByName[gamemode.GamemodeString] = (int)gmKey;
				gtGameModeKeyByName[gamemode.DisplayName] = (int)gmKey;
				gtGameModeNames.Add(gamemode.DisplayName);
				return;
			}
			
			GameObject prefab = new GameObject(gamemode.ID);
			prefab.SetActive(false);
			var gameMode = prefab.AddComponent(gamemode.GameManager) as GorillaGameManager;
			int gameModeKey = (int)gameMode.GameType();		

			if (gtGameModeTable.ContainsKey(gameModeKey))
			{
				Debug.LogError($"GameMode {gtGameModeTable[gameModeKey].GameModeName()} is already using GameType {gameModeKey}");
				GameObject.Destroy(prefab);
				return;
			}

			gtGameModeTable[gameModeKey] = gameMode;
			gtGameModeKeyByName[gamemode.GamemodeString] = gameModeKey;
			gtGameModeKeyByName[gamemode.DisplayName] = gameModeKey;
			gtGameModeNames.Add(gamemode.DisplayName);
			gtGameModes.Add(gameMode);

			prefab.transform.SetParent(moddedGameModesObject.transform);
			prefab.SetActive(true);

        }

		internal void OnRoomJoin(object sender, Events.RoomJoinedArgs args)
		{
			string gamemode = args.Gamemode;

			foreach (var pluginInfo in pluginInfos)
			{
				if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.GamemodeString)))
				{
					try
					{
						pluginInfo.OnGamemodeJoin?.Invoke(gamemode);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
		}

		internal void OnRoomLeft(object sender, Events.RoomJoinedArgs args)
		{
			string gamemode = args.Gamemode;

			foreach (var pluginInfo in pluginInfos)
			{
				if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.GamemodeString)))
				{
					try
					{
						pluginInfo.OnGamemodeLeave?.Invoke(gamemode);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
		}
	}
}
