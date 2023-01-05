using Il2CppCollections = Il2CppSystem.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace ModSettings {
	internal class ModTab {

		internal readonly UIGrid uiGrid;
		internal readonly Il2CppCollections.List<GameObject> menuItems;
		internal readonly List<ModSettingsBase> modSettings;

		internal float scrollBarHeight;
		internal bool requiresConfirmation;

		internal ModTab(UIGrid uiGrid, Il2CppCollections.List<GameObject> menuItems) {
			this.uiGrid = uiGrid;
			this.menuItems = menuItems;
			modSettings = new List<ModSettingsBase>();
		}
	}
}
