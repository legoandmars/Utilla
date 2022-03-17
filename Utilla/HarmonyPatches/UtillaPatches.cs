using HarmonyLib;
using System.Reflection;

namespace Utilla.HarmonyPatches
{
    /// <summary>
    /// Apply and remove all of our Harmony patches through this class
    /// </summary>
    public class UtillaPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public const string InstanceId = "com.legoandmars.gorillatag.utilla";

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(InstanceId);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}