
namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class SectionAttribute : Attribute {
		private string title;
		private bool localize = false;

		public SectionAttribute(string title) {
			this.title = title;
		}

		public string Title {
			get => title;
		}

		public bool Localize {
			get => localize;
			set => localize = value;
		}
	}
}
