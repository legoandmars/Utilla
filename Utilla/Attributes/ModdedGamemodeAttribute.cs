using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilla.Models;

namespace Utilla
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ModdedGamemodeAttribute : Attribute
	{
		public readonly Gamemode gamemode;

		public ModdedGamemodeAttribute()
		{
			gamemode = null;
		}

		public ModdedGamemodeAttribute(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.Infection)
		{
			gamemode = new Gamemode(id, displayName, baseGamemode);
		}

		public ModdedGamemodeAttribute(string id, string displayName, Type gameManager)
		{
			gamemode = new Gamemode(id, displayName, gameManager);
		}
	}
}
