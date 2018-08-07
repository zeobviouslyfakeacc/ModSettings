using System.Collections.Generic;
using UnityEngine;

namespace ModSettings {
	internal class ModTab {

		internal readonly UIGrid uiGrid;
		internal readonly List<GameObject> menuItems;
		internal readonly List<ModSettingsBase> modSettings;

		internal int selectedIndex;
		internal float scrollBarHeight;
		internal bool requiresConfirmation;

		internal ModTab(UIGrid uiGrid, List<GameObject> menuItems) {
			this.uiGrid = uiGrid;
			this.menuItems = menuItems;
			modSettings = new List<ModSettingsBase>();
		}
	}
}
