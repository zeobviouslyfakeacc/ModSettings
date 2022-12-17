namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DescriptionAttribute : Attribute {
		private string description;
		private bool localize = false;

		public DescriptionAttribute(string description) {
			this.description = description;
		}

		public string Description {
			get => description;
		}

		public bool Localize {
			get => localize;
			set => localize = value;
		}
	}
}
