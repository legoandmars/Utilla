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
		const string GamemodePrefix = "MODDED_";

		public string DisplayName { get; }
		public string ID { get; }
		public string GamemodeString { get; }
		public BaseGamemode BaseGamemode { get; }
		public Type GameManager { get; }

		public Gamemode(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.Infection)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = baseGamemode;

			GamemodeString = GamemodePrefix + ID + (BaseGamemode == BaseGamemode.None ? "" : BaseGamemode.ToString().ToUpper());
		}

		public Gamemode(string id, string displayName, Type gameManager)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = BaseGamemode.None;
			this.GameManager = gameManager;

			GamemodeString = GamemodePrefix + ID;
		}

		/// <remarks>This should only be used interally to create base game gamemodes</remarks>
		internal Gamemode(string id, string displayName)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = BaseGamemode.None;

			GamemodeString = ID;
		}
	}
}
