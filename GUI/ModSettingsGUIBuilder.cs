using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace ModSettings {

	internal class ModSettingsGUIBuilder : GUIBuilder {

		internal static GameObject CreateModSettingsTab() {
			Panel_OptionsMenu panel = InterfaceManager.m_Panel_OptionsMenu;
			Transform pages = panel.transform.Find("Pages");
			GameObject tab = Object.Instantiate(panel.m_QualityTab, pages);
			tab.name = "ModSettings";

			Transform titleLabel = tab.transform.Find("TitleDisplay/Label");
			Object.Destroy(titleLabel.GetComponent<UILocalize>());
			titleLabel.GetComponent<UILabel>().text = "Mod Settings";

			GetListReflective(panel, "m_MainMenuItemTabs").Add(tab);
			GetListReflective(panel, "m_Tabs").Add(tab);

			return tab;
		}

		private static List<GameObject> GetListReflective(Panel_OptionsMenu panel, string fieldName) {
			return (List<GameObject>) AccessTools.Field(typeof(Panel_OptionsMenu), fieldName).GetValue(panel);
		}

		private readonly MenuGroup menuGroup;
		private readonly List<ModSettingsBase> tabSettings;

		internal ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI) : this(modName, settingsGUI, settingsGUI.CreateModTab(modName)) { }

		private ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI, ModTab modTab) : base(modTab.uiGrid, modTab.menuItems) {
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
