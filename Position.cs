namespace ModSettings {
	public class Position {

		public static readonly Position AboveAll = new Position(0);
		public static readonly Position AboveGameStart = new Position(0);
		public static readonly Position BelowGameStart = new Position(1);
		public static readonly Position AboveEnvironment = new Position(1);
		public static readonly Position BelowEnvironment = new Position(2);
		public static readonly Position AboveHealth = new Position(2);
		public static readonly Position BelowHealth = new Position(3);
		public static readonly Position AboveGear = new Position(3);
		public static readonly Position BelowGear = new Position(4);
		public static readonly Position AboveWildlifeSpawns = new Position(4);
		public static readonly Position BelowWildlifeSpawns = new Position(5);
		public static readonly Position AboveWildlifeBehaviour = new Position(5);
		public static readonly Position BelowWildlifeBehaviour = new Position(6);
		public static readonly Position AboveWildlifeStruggle = new Position(6);
		public static readonly Position BelowWildlifeStruggle = new Position(7);
		public static readonly Position BelowAll = new Position(7);

		private Position(int index) {
			this.Index = index;
		}

		internal int Index { get; }
	}
}
