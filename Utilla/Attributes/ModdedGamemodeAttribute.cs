using GorillaGameModes;
using System;
using Utilla.Models;

namespace Utilla.Attributes
{
    /// <summary>
    /// Tells Utilla that a plugin wants to use attributes for room join callbacks.
    /// </summary>
    /// <remarks>
    /// This attribute does nothing by itself.
    /// This attribute must be applied to the class marked with [BepInPlugin].
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ModdedGamemodeAttribute : Attribute
    {
        public readonly Gamemode gamemode;

        public ModdedGamemodeAttribute()
        {
            gamemode = null;
        }

        public ModdedGamemodeAttribute(string id, string displayName, GameModeType gameModeType = GameModeType.Infection)
        {
            gamemode = new Gamemode(id, displayName, gameModeType);
        }

        [Obsolete]
        public ModdedGamemodeAttribute(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.Infection)
        {
            GameModeType? gameModeType = (baseGamemode != BaseGamemode.None && Enum.TryParse(baseGamemode.ToString(), out GameModeType result)) ? result : null;
            gamemode = new Gamemode(id, displayName, gameModeType);
        }

        public ModdedGamemodeAttribute(string id, string displayName, Type gameManager)
        {
            gamemode = new Gamemode(id, displayName, gameManager);
        }
    }
}
