using ModSettings.Groups;
using ModSettings.Scripts;
using System.Reflection;
using UnityEngine;
using Il2Cpp = Il2CppSystem.Collections.Generic;

namespace ModSettings {
	public abstract class GUIBuilder {

		internal const int gridCellHeight = 33;

		internal readonly UIGrid uiGrid;
		internal readonly Il2Cpp.List<GameObject> menuItems;

		internal HeaderGroup lastHeader;

		protected GUIBuilder(UIGrid uiGrid, Il2Cpp.List<GameObject> menuItems) {
			this.uiGrid = uiGrid;
			this.menuItems = menuItems;
		}

		internal virtual void AddSettings(ModSettingsBase modSettings) {
			modSettings.MakeGUIContents(this);
		}

		/// <returns>True if changed. False otherwise</returns>
		internal virtual bool SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue) {
			return modSettings.SetFieldValue(field, newValue);
		}

		internal virtual bool SetSettingsField<T>(ModSettingsBase modSettings, FieldInfo field, T newValue) {
			return modSettings.SetFieldValue(field, newValue);
		}


		internal GameObject CreateSetting(NameAttribute name, DescriptionAttribute description, GameObject prefab, string labelName) {
			return CreateSetting(name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false, prefab, labelName);
		}

		internal GameObject CreateSetting(string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize, GameObject prefab, string labelName) {
			GameObject setting = NGUITools.AddChild(uiGrid.gameObject, prefab);
			setting.name = "Custom Setting (" + nameText + ")";

			Transform labelTransform = setting.transform.Find(labelName);
			SetLabelText(labelTransform, nameText, nameLocalize);

			DescriptionHolder descriptionHolder = setting.AddComponent<DescriptionHolder>();
			descriptionHolder.SetDescription(descriptionText ?? string.Empty, descriptionLocalize);

			menuItems.Add(setting);
			return setting;
		}

		internal static void SetLabelText(Transform transform, string text, bool localize) {
			if (localize) {
				UILocalize uiLocalize = transform.GetComponent<UILocalize>();
				uiLocalize.key = text;
			} else {
				UnityEngine.Object.Destroy(transform.GetComponent<UILocalize>());
				UILabel uiLabel = transform.GetComponent<UILabel>();
				uiLabel.text = text;
			}
		}

		internal void SetVisibilityListener(ModSettingsBase modSettings, FieldInfo field, GameObject guiObject, HeaderGroup header) {
			bool startVisible = modSettings.IsFieldVisible(field);
			if (guiObject.activeSelf != startVisible) {
				guiObject.SetActive(startVisible);
			}
			header?.NotifyChildAdded(startVisible);

			modSettings.AddVisibilityListener(field, (visible) => {
				guiObject.SetActive(visible);
				header?.NotifyChildVisible(visible);
				uiGrid.repositionNow = true;
			});
		}
	}
}
