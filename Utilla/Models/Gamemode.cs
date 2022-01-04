using System;

namespace Utilla.Models
{
	/// <summary>
	/// The base gamemode for a gamemode to inherit.
	/// </summary>
	/// <remarks>
	/// None should not be used from an external program.
	/// </remarks>
	public enum BaseGamemode
	{
		/// <summary>
		/// No gamemode, only used for fully custom gamemodes.
		/// </summary>
		None,
		/// <summary>
		/// The regular infection (tag) gamemode.
		/// </summary>
		Infection, 
		/// <summary>
		/// Casual gamemode, no players are infected.
		/// </summary>
		Casual,
		/// <summary>
		/// Hunt gamemode, requires at least 4 players.
		/// </summary>
		Hunt
	}

	public class Gamemode {
		public string DisplayName { get; set; }
		public string ID { get; set; }
		public BaseGamemode BaseGamemode { get; set; }
		public Type GameManager { get; set; }

		public Gamemode(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.Infection)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = baseGamemode;
		}

		public Gamemode(string id, string displayName, Type gameManager)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = BaseGamemode.None;
			this.GameManager = gameManager;
		}

		public string GamemodeString()
		{
			if (BaseGamemode == BaseGamemode.None) return ID;
			else return ID + BaseGamemode.ToString().ToUpper();
		}
	}
}
