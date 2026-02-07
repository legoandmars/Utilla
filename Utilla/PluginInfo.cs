using BepInEx;
using System;
using System.Linq;
using Utilla.Models;

namespace Utilla
{
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
}
