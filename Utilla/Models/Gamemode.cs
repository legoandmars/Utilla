using System;

namespace Utilla.Models
{
	public enum BaseGamemode
	{
		None,
		Infection, 
		Casual
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
