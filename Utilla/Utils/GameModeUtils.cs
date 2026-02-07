using GorillaGameModes;
using System;
using System.Globalization;
using System.Linq;
using Utilla.Behaviours;
using Utilla.Models;

namespace Utilla.Utils
{
    public static class GameModeUtils
    {
        public static Gamemode CurrentGamemode { get; internal set; }

        public static Gamemode FindGamemodeInString(string gmString)
        {
            if (gmString.Contains('|'))
            {
                string[] split = gmString.Split('|');
                bool useSeperator = split.Length > 2;
                return useSeperator ? GetGamemode(gamemode => split[^1] == gamemode.ID) : null;
            }

            return GetGamemode(gamemode => gmString.EndsWith(gamemode.ID));
        }

        public static Gamemode GetGamemodeFromId(string id) => GetGamemode(gamemode => gamemode.ID == id);

        public static Gamemode GetGamemode(Func<Gamemode, bool> predicate)
        {
            if (GamemodeManager.HasInstance && GamemodeManager.Instance.Gamemodes.LastOrDefault(predicate) is Gamemode gameMode)
                return gameMode;
            return null;
        }

        public static string GetGameModeName(GameModeType gameModeType)
        {
            string modeName = (GetGameModeInstance(gameModeType) is GorillaGameManager gameManager) ? gameManager.GameModeName() : GameMode.GameModeZoneMapping.GetModeName(gameModeType);
            return (modeName.ToLower() == gameModeType.GetName().ToLower()) ? gameModeType.GetName() : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(modeName.ToLower());
        }

        public static GorillaGameManager GetGameModeInstance(GameModeType gameModeType)
        {
            if (GameMode.GetGameModeInstance(gameModeType) is GorillaGameManager gameManager && gameManager)
                return gameManager;
            return null;
        }

        public static bool IsSuperGameMode(this GameModeType gameMode) => gameMode == GameModeType.SuperInfect || gameMode == GameModeType.SuperCasual;
    }
}
