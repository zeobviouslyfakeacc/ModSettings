using System.Reflection;
using UnityEngine;
using Il2Cpp;
using MelonLoader;

namespace ModSettings {

	internal class ModSettingsGUIBuilder : GUIBuilder {

		internal static GameObject CreateModSettingsTab(Panel_OptionsMenu panel)
        {
			Transform pages = panel.transform.Find("Pages");
			GameObject tab = UnityEngine.Object.Instantiate(panel.m_QualityTab, pages);
			tab.name = "ModSettings";

			Transform titleLabel = tab.transform.Find("TitleDisplay/Label");
            UnityEngine.Object.Destroy(titleLabel.GetComponent<UILocalize>());
			titleLabel.GetComponent<UILabel>().text = "Mod Settings";

			panel.m_MainMenuItemTabs.Add(tab);
			panel.m_Tabs.Add(tab);

			return tab;
		}

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

		protected override void SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue) {
			base.SetSettingsField(modSettings, field, newValue);
			settingsGUI.NotifySettingsNeedConfirmation();
		}

		private class MenuGroup : Group {

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
}
