using UnityEngine;

namespace ModSettings {
	public static class Empty {
		public static void AddEmptySetting(this GUIBuilder guiBuilder, string nameText) => AddEmptySetting(guiBuilder, nameText, false, null, false);
		public static void AddEmptySetting(this GUIBuilder guiBuilder, string nameText, string descriptionText) => AddEmptySetting(guiBuilder, nameText, false, descriptionText, false);
		public static void AddEmptySetting(this GUIBuilder guiBuilder, string nameText, bool nameLocalize) => AddEmptySetting(guiBuilder, nameText, nameLocalize, null, false);
		public static void AddEmptySetting(this GUIBuilder guiBuilder, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize) {
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.emptyPrefab, "Label");

			//Replacement for the visibility listener
			bool startVisible = true;
			if (setting.activeSelf != startVisible) {
				setting.SetActive(startVisible);
			}
			guiBuilder.lastHeader?.NotifyChildAdded(startVisible);
		}
	}
}
