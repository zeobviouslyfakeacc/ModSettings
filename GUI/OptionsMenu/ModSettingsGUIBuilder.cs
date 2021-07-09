using ModSettings.Groups;
using System.Collections.Generic;
using System.Reflection;

namespace ModSettings {

	internal class ModSettingsGUIBuilder : GUIBuilder {
		private readonly ModSettingsGUI settingsGUI;
		private readonly MenuGroup menuGroup;
		private readonly List<ModSettingsBase> tabSettings;

		internal ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI) : this(modName, settingsGUI, settingsGUI.CreateModTab(modName)) { }

		private ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI, ModTab modTab) : base(modTab.uiGrid, modTab.menuItems) {
			this.settingsGUI = settingsGUI;
			menuGroup = new MenuGroup(modName, settingsGUI);
			tabSettings = modTab.modSettings;
		}

		internal override void AddSettings(ModSettingsBase modSettings) {
			base.AddSettings(modSettings);
			tabSettings.Add(modSettings);

			menuGroup.NotifyChildAdded(modSettings.IsVisible());
			modSettings.AddVisibilityListener((visible) => {
				menuGroup.NotifyChildVisible(visible);
			});
		}

		internal override bool SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue) {
			bool result = base.SetSettingsField(modSettings, field, newValue);
			if (result) settingsGUI.NotifySettingsNeedConfirmation();
			return result;
		}

		internal override bool SetSettingsField<T>(ModSettingsBase modSettings, FieldInfo field, T newValue) {
			bool result = base.SetSettingsField<T>(modSettings, field, newValue);
			if (result) settingsGUI.NotifySettingsNeedConfirmation();
			return result;
		}
	}
}
