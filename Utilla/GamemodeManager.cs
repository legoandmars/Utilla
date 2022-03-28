using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GorillaNetworking;
using BepInEx;
using System.Reflection;
using System.Linq.Expressions;
using Photon.Pun;
using Utilla.Models;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Utilla
{
	public partial class GamemodeManager : MonoBehaviour
	{
		public static GamemodeManager Instance { get; private set; }

		const string ModdedDefault = "MODDED_DEFAULT";
		const string ModdedCasual = "MODDED_CASUAL";
		const string ModdedHunt = "MODDED_HUNT";
		const int PageSize = 3;

		const string UIRootPath = "Level/forest/lower level/UI";
		private const string AnchorPath = "Selector Buttons/anchor";
		private const string GamemodesListPath = "Tree Room Texts/Game Mode List Text";

		const string BasePrefabPath = "CustomGameManager/";

		int page;
		int PageCount => Mathf.CeilToInt(gamemodes.Count() / 3f);

		Text gamemodesText;
		List<Gamemode> DefaultModdedGamemodes = new List<Gamemode>()
		{
			new Gamemode(ModdedCasual, "MODDED CASUAL", BaseGamemode.Casual),
			new Gamemode(ModdedDefault, "MODDED", BaseGamemode.Infection),
			new Gamemode(ModdedHunt, "MODDED HUNT", BaseGamemode.Hunt)
		};
		List<Gamemode> gamemodes = new List<Gamemode>() { 
			new Gamemode("CASUAL", "CASUAL"),
			new Gamemode("INFECTION", "INFECTION"),
			new Gamemode("HUNT", "HUNT")
		};
		ModeSelectButton[] modeSelectButtons = Array.Empty<ModeSelectButton>();

		Material buttonMaterial = Resources.Load<Material>("objects/treeroom/materials/plastic");

		public class PluginInfo
		{
			public BaseUnityPlugin Plugin { get; set; }
			public Gamemode[] Gamemodes { get; set; }
			public Action<string> OnGamemodeJoin { get; set; }
			public Action<string> OnGamemodeLeave { get; set; }

			public override string ToString()
			{
				return $"{Plugin.Info.Metadata.Name} [{string.Join(", ", Gamemodes.Select(x => x.DisplayName))}]";
			}
		}

		List<PluginInfo> pluginInfos = new List<PluginInfo>();

		void Awake()
		{
			Instance = this;
			Events.RoomJoined += OnRoomJoin;
			Events.RoomLeft += OnRoomLeft;

			transform.parent = GameObject.Find(UIRootPath).transform;

            GorillaComputer.instance.currentGameMode = PlayerPrefs.GetString("currentGameMode", "INFECTION");

			CreatePluginInfos();

			Transform anchor = transform.parent.Find(AnchorPath);
			Transform[] buttons = Enumerable.Range(0, PageSize).Select(x => anchor.GetChild(x)).ToArray();
			modeSelectButtons = buttons.Select(x => x.GetComponent<ModeSelectButton>()).ToArray();

			CreateGamemodes();
			CreatePrefabPool();

			CreateGamemodeText();

			CreatePageButtons(buttons[0].gameObject);

			ShowPage(0);
		}

		void CreateGamemodes()
		{
			gamemodes.AddRange(DefaultModdedGamemodes);

			HashSet<Gamemode> additonalGamemodes = new HashSet<Gamemode>();
			foreach (var info in pluginInfos)
			{
				additonalGamemodes.UnionWith(info.Gamemodes);
			}

			foreach (var gamemode in DefaultModdedGamemodes)
			{
				additonalGamemodes.Remove(gamemode);
			}

			gamemodes.AddRange(additonalGamemodes);
		}

		void CreateGamemodeText()
		{
			gamemodesText = transform.parent.Find(GamemodesListPath).gameObject.GetComponent<Text>();
			gamemodesText.lineSpacing = 2.2f;
			gamemodesText.transform.localScale *= 0.75f;
			gamemodesText.transform.position += gamemodesText.transform.right * 0.1f;
			gamemodesText.horizontalOverflow = HorizontalWrapMode.Overflow;
		}

		void CreatePageButtons(GameObject templateButton)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.SetActive(false);
			MeshFilter meshFilter = cube.GetComponent<MeshFilter>();

			GameObject CreatePageButton(string text, Action onPressed)
			{
				GameObject button = GameObject.Instantiate(templateButton);
				button.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
				button.GetComponent<Renderer>().material = buttonMaterial;
				button.transform.parent = templateButton.transform.parent;
				button.transform.localRotation = templateButton.transform.localRotation;
				button.transform.localScale = templateButton.transform.localScale;

				button.transform.GetChild(0).gameObject.SetActive(true);
				Text buttonText = button.GetComponentInChildren<Text>();
				if (buttonText != null)
				{
					buttonText.text = text;
					buttonText.transform.localScale = Vector3.Scale(buttonText.transform.localScale, new Vector3(2, 2, 1));
				}

				GameObject.Destroy(button.GetComponent<ModeSelectButton>());
				button.AddComponent<PageButton>().onPressed += onPressed;

				return button;
			}

			GameObject nextPageButton = CreatePageButton("-->", NextPage);
			nextPageButton.transform.localPosition = new Vector3(-0.575f, nextPageButton.transform.position.y, nextPageButton.transform.position.z);

			GameObject previousPageButton = CreatePageButton("<--", PreviousPage);
			previousPageButton.transform.localPosition = new Vector3(-0.575f, -0.318f, previousPageButton.transform.position.z);

			Destroy(cube);
		}

		void CreatePluginInfos()
		{
			foreach (var info in BepInEx.Bootstrap.Chainloader.PluginInfos)
			{
				if (info.Value == null) continue;
				BaseUnityPlugin plugin = info.Value.Instance;
				if (plugin == null) continue;
				Type type = plugin.GetType();

				IEnumerable<Gamemode> gamemodes = GetGamemodes(type);

				if (gamemodes.Count() > 0)
				{
					pluginInfos.Add(new PluginInfo
					{
						Plugin = plugin,
						Gamemodes = gamemodes.ToArray(),
						OnGamemodeJoin = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeJoinAttribute)),
						OnGamemodeLeave = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeLeaveAttribute))
					});
				}
			}
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

		void CreatePrefabPool()
		{
			List<GameObject> prefabs = new List<GameObject>();
			foreach (Gamemode gamemode in gamemodes)
			{
				if (gamemode.GameManager is null) continue;

				GameObject prefab = new GameObject(gamemode.ID);
				prefab.SetActive(false);
				prefab.AddComponent(gamemode.GameManager);
				prefab.AddComponent<PhotonView>();
				prefabs.Add(prefab);
			}

			DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
			if (pool != null)
			{
				prefabs.ForEach(prefab => pool.ResourceCache.Add(BasePrefabPath + prefab.name, prefab));
			}
		}

		internal void OnRoomJoin(object sender, Events.RoomJoinedArgs args)
		{
			string gamemode = args.Gamemode;

			if (PhotonNetwork.IsMasterClient)
			{
				foreach(Gamemode g in gamemodes.Where(x => x.GameManager != null))
				{
					if (gamemode.Contains(g.ID))
					{
						GameObject go = PhotonNetwork.InstantiateRoomObject(BasePrefabPath + g.ID, Vector3.zero, Quaternion.identity);
						go.SetActive(true);
						break;
					}
				}
			}

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

		public void NextPage()
		{
			if (page < PageCount - 1)
			{
				ShowPage(page + 1);
			}
		}

		public void PreviousPage()
		{
			if (page > 0)
			{
				ShowPage(page - 1);
			}
		}

		void ShowPage(int page)
		{
			this.page = page;
			List<Gamemode> currentGamemodes = gamemodes.Skip(page * PageSize).Take(PageSize).ToList();

			int counter = 0;
			for (int i = 0; i < modeSelectButtons.Length; i++)
			{
				if (i < currentGamemodes.Count)
				{
					modeSelectButtons[i].enabled = true;
					modeSelectButtons[i].gameMode = currentGamemodes[i].GamemodeString;
				}
				else
				{
					modeSelectButtons[i].enabled = false;
					modeSelectButtons[i].gameMode = "";
					counter++;
				}
			}

			string displayText = string.Join("\n", currentGamemodes.Select(x => x.DisplayName));
			for (int i = 0; i < counter; i++)
			{
				displayText += '\n';
			}
			gamemodesText.text = displayText;

			GorillaComputer.instance.OnModeSelectButtonPress(GorillaComputer.instance.currentGameMode);
		}
	}
}
