using System;
using BepInEx;
using Utilla.HarmonyPatches;
using System.Reflection;
using Utilla.Utils;

namespace Utilla
{

    [BepInPlugin("org.legoandmars.gorillatag.utilla", "Utilla", "1.1.0")]
    public class Utilla : BaseUnityPlugin
    {
        void Awake()
        {
            RoomUtils.RoomCode = RoomUtils.RandomString(6); // Generate a random room code in case we need it
            UtillaPatches.ApplyHarmonyPatches();
        }
    }
}
