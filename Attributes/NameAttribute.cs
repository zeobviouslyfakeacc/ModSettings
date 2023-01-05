namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class NameAttribute : Attribute {
		private string name;
		private bool localize = false;

		public NameAttribute(string name) {
			this.name = name;
		}

		public string Name {
			get => name;
		}

		public bool Localize {
			get => localize;
			set => localize = value;
		}
	}
}
