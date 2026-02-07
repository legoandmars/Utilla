using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Utilla.Behaviours;

namespace Utilla
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;

        public Plugin()
        {
            Logger = base.Logger;

            DontDestroyOnLoad(this);

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
            Events.GameInitialized += OnGameInitialized;
        }

        public void OnGameInitialized(object sender, EventArgs args)
        {
            DontDestroyOnLoad(new GameObject($"{Constants.Name} {Constants.Version}", typeof(UtillaNetworkController), typeof(GamemodeManager), typeof(ConductBoardManager)));
        }
    }
}
