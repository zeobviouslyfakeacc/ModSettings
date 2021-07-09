namespace ModSettings.Groups {
	internal class MenuGroup : Group {

		private readonly string modName;
		private readonly ModSettingsGUI modSettings;

		internal MenuGroup(string modName, ModSettingsGUI modSettings) {
			this.modName = modName;
			this.modSettings = modSettings;
		}

		protected override void SetVisible(bool visible) {
			if (visible) {
				modSettings.AddModSelector(modName);
			} else {
				modSettings.RemoveModSelector(modName);
			}
		}
	}
}
