using System;

namespace ModSettings {
	/// <summary>
	/// Adds a padding header if the title is null or whitespace.<br/>
	/// Adds a title header otherwise.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class SectionAttribute : Attribute {
		private string title;
		private bool localize = false;

		public SectionAttribute() { }

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
