using System;

namespace Utilla.Attributes
{
    /// <summary>
    /// This attribute marks a method to be called when a modded lobby is joined.
    /// </summary>
    /// <remarks>
    /// The method must either take no arguments, or a string for the gamemode.
    /// Use <c>String.Contains</c> to test if a lobby is of a specific gamemode.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class ModdedGamemodeJoinAttribute : Attribute
    {
    }
}
