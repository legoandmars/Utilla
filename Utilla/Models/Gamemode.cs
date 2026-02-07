using GorillaGameModes;
using System;
using Utilla.Tools;
using Utilla.Utils;

namespace Utilla.Models
{
    /// <summary>
    /// The base gamemode for a gamemode to inherit.
    /// </summary>
    /// <remarks>
    /// None should not be used from an external program.
    /// </remarks>
    [Obsolete]
    public enum BaseGamemode
    {
        /// <summary>
        /// There is no gamemode manager to rely on, this should only be used by the Utilla mod when preparing modded gamemodes or gamemodes using a unique gamemode manager.
        /// </summary>
        None,
        /// <summary>
        /// Infection gamemode, requires at least four participating players for infection and under for tag.
        /// </summary>
        Infection,
        /// <summary>
        /// Casual gamemode, no players are affected by the gamemode, such as tagging or infecting.
        /// </summary>
        Casual,
        /// <summary>
        /// Hunt gamemode, requires at least four participating players.
        /// </summary>
        Hunt,
        /// <summary>
        /// Paintbrawl gamemode, requires at least two participating players.
        /// </summary>
        Paintbrawl
    }

    public class Gamemode
    {
        /// <summary>
        /// The title of the Gamemode visible through the gamemode selector
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The internal ID of the Gamemode
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// An optional reference of a game mode to inherit
        /// </summary>
        public GameModeType? BaseGamemode { get; }

        /// <summary>
        /// An optional reference of a game mode manager to create
        /// </summary>
        public Type GameManager { get; }

        internal Gamemode(GameModeType gameModeType)
        {
            BaseGamemode = gameModeType;

            ID = gameModeType.ToString();
            DisplayName = GameModeUtils.GetGameModeName(gameModeType);

            Logging.Message($"Replicated base gamemode: based on {gameModeType} type");
        }

        public Gamemode(string id, string displayName, GameModeType? game_mode_type = null)
        {
            BaseGamemode = game_mode_type;

            ID = game_mode_type.HasValue && !id.Contains(game_mode_type.Value.ToString()) ? string.Concat(id, game_mode_type) : id;
            DisplayName = displayName;

            Logging.Message($"Constructed custom gamemode: {id} based on {(game_mode_type.HasValue ? game_mode_type.Value : "no")} type");
        }

        public Gamemode(string id, string displayName, Type gameManager)
        {
            ID = id;
            DisplayName = displayName;
            GameManager = gameManager;

            Logging.Message($"Constructed custom gamemode: {id} with {gameManager.GetType()} manager");
        }
    }
}
