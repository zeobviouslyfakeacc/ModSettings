using System;

namespace ModSettings {
	public abstract class Setting {

		internal bool IsBuilt { get; private set; } = false;

		public string NameText { get; set; }
		public bool NameLocalize { get; set; }
		public string DescriptionText { get; set; }
		public bool DescriptionLocalize { get; set; }

		public Action OnChange { get; set; }

		internal void Build(GUIBuilder guiBuilder) {
			DoBuild(guiBuilder);
			IsBuilt = true;
		}

		protected abstract void DoBuild(GUIBuilder guiBuilder);
	}
}
