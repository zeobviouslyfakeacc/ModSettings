using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	internal abstract class GUIBuilder {

		internal const int gridCellHeight = 33;

		private static readonly GameObject headerLabelPrefab;
		private static readonly GameObject sliderPrefab;
		private static readonly GameObject comboBoxPrefab;

		static GUIBuilder() {
			Transform firstSection = InterfaceManager.m_Panel_CustomXPSetup.m_ScrollPanelOffsetTransform.GetChild(0);

			headerLabelPrefab = UnityEngine.Object.Instantiate(firstSection.Find("Header").gameObject);
			headerLabelPrefab.SetActive(false);

			comboBoxPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_CustomXPSetup.m_AllowInteriorSpawnPopupList.gameObject);
			comboBoxPrefab.SetActive(false);

			sliderPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_OptionsMenu.m_FieldOfViewSlider.gameObject);
			UnityEngine.Object.DestroyImmediate(sliderPrefab.GetComponent<ConsoleSlider>().m_SliderObject.GetComponent<GenericSliderSpawner>());
			sliderPrefab.SetActive(false);

			// Fix slider hitbox
			BoxCollider collider = sliderPrefab.GetComponentInChildren<BoxCollider>();
			collider.center = new Vector3(150, 0);
			collider.size = new Vector3(900, 30);
		}

		protected readonly UIGrid uiGrid;
		protected readonly List<GameObject> menuItems;

		protected Header lastHeader;

		protected GUIBuilder(UIGrid uiGrid, List<GameObject> menuItems) {
			this.uiGrid = uiGrid;
			this.menuItems = menuItems;
		}

		internal virtual void AddSettings(ModSettingsBase modSettings) {
			foreach (FieldInfo field in modSettings.GetFields()) {
				Attributes.GetAttributes(field, out SectionAttribute section, out NameAttribute name,
						out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice);

				if (section != null) {
					AddHeader(section);
				} else if (lastHeader == null) {
					AddPaddingHeader();
				}

				Type fieldType = field.FieldType;
				if (fieldType == typeof(bool)) {
					AddYesNoSetting(modSettings, field, name, description);
				} else if (fieldType == typeof(float)) {
					if (slider != null) {
						AddSliderSetting(modSettings, field, name, description, slider);
					} else {
						AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultFloatRange);
					}
				} else if (fieldType == typeof(int)) {
					if (choice != null) {
						AddChoiceSetting(modSettings, field, name, description, choice);
					} else if (slider != null) {
						AddSliderSetting(modSettings, field, name, description, slider);
					} else {
						AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultIntRange);
					}
				} else if (fieldType.IsEnum) {
					ChoiceAttribute choiceAttr = choice ?? ChoiceAttribute.ForEnumType(fieldType);
					AddChoiceSetting(modSettings, field, name, description, choiceAttr);
				}
			}
		}

		private void AddHeader(SectionAttribute section) {
			GameObject padding = NGUITools.AddChild(uiGrid.gameObject);
			GameObject header = NGUITools.AddChild(uiGrid.gameObject);
			GameObject label = NGUITools.AddChild(header, headerLabelPrefab);

			label.SetActive(true);
			label.transform.localPosition = new Vector2(-70, 0);
			label.name = "Custom Header (" + section.Title + ")";
			SetLabelText(label.transform, section.Title, section.Localize);

			lastHeader = new Header(header, padding);
		}

		private void AddPaddingHeader() {
			GameObject padding = NGUITools.AddChild(uiGrid.gameObject);
			lastHeader = new Header(padding);
		}

		private void AddYesNoSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description) {
			// Create menu item
			GameObject setting = CreateSetting(name, description, comboBoxPrefab, "Label");
			ConsoleComboBox comboBox = setting.GetComponent<ConsoleComboBox>();

			// Add listener and set default value
			EventDelegate.Set(comboBox.onChange, () => UpdateBooleanValue(modSettings, field, comboBox));
			bool defaultValue = (bool) field.GetValue(modSettings);
			comboBox.value = comboBox.items[defaultValue ? 1 : 0];

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private static void UpdateBooleanValue(ModSettingsBase modSettings, FieldInfo field, ConsoleComboBox comboBox) {
			bool oldValue = (bool) field.GetValue(modSettings);
			bool newValue = (comboBox.GetCurrentIndex() == 1);
			if (oldValue != newValue) {
				field.SetValue(modSettings, newValue);
				modSettings.CallOnChange(field, oldValue, newValue);
			}
		}

		private void AddChoiceSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, ChoiceAttribute choice) {
			// Create menu item
			GameObject setting = CreateSetting(name, description, comboBoxPrefab, "Label");
			ConsoleComboBox comboBox = setting.GetComponent<ConsoleComboBox>();

			// Add selectable values
			comboBox.items.Clear();
			comboBox.items.AddRange(choice.Names);
			comboBox.m_Localize = choice.Localize;

			// Add listener and set default value
			EventDelegate.Set(comboBox.onChange, () => UpdateChoiceValue(modSettings, field, comboBox));
			int defaultValue = (int) field.GetValue(modSettings);
			comboBox.value = comboBox.items[defaultValue];

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private static void UpdateChoiceValue(ModSettingsBase modSettings, FieldInfo field, ConsoleComboBox comboBox) {
			int oldValue = (int) field.GetValue(modSettings);
			int newValue = comboBox.GetCurrentIndex();
			if (oldValue != newValue) {
				field.SetValue(modSettings, newValue);
				modSettings.CallOnChange(field, oldValue, newValue);
			}
		}

		private void AddSliderSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, SliderAttribute range) {
			// Create menu
			GameObject setting = CreateSetting(name, description, sliderPrefab, "Label_FOV");
			ConsoleSlider slider = setting.GetComponent<ConsoleSlider>();
			UILabel uiLabel = slider.m_SliderObject.GetComponentInChildren<UILabel>();
			UISlider uiSlider = slider.m_SliderObject.GetComponentInChildren<UISlider>();

			// Sanitize user values, especially if the field type is int
			float from, to;
			int numberOfSteps;
			string numberFormat;
			if (field.FieldType == typeof(float)) {
				from = range.From;
				to = range.To;
				numberOfSteps = (range.NumberOfSteps < 0) ? 1 : range.NumberOfSteps;
				numberFormat = range.NumberFormat ?? SliderAttribute.DefaultFloatFormat;
			} else {
				from = Mathf.Round(range.From);
				to = Mathf.Round(range.To);
				numberOfSteps = (range.NumberOfSteps < 0) ? Mathf.RoundToInt(Mathf.Abs(from - to)) + 1 : range.NumberOfSteps;
				numberFormat = range.NumberFormat ?? SliderAttribute.DefaultIntFormat;
			}

			// Add listeners to update setting value
			EventDelegate.Callback callback = () => UpdateSliderValue(modSettings, field, uiSlider, uiLabel, from, to, numberFormat);
			EventDelegate.Set(slider.onChange, callback);
			EventDelegate.Set(uiSlider.onChange, callback);

			// Set default value and number of steps
			float defaultValue = Convert.ToSingle(field.GetValue(modSettings));
			uiSlider.value = (defaultValue - from) / (to - from);
			uiSlider.numberOfSteps = numberOfSteps;

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private static void UpdateSliderValue(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat) {
			float sliderValue = from + slider.value * (to - from);

			if (field.FieldType == typeof(float)) {
				float oldValue = (float) field.GetValue(modSettings);

				label.text = string.Format(numberFormat, sliderValue);
				if (oldValue != sliderValue) {
					GameAudioManager.PlayGUISlider();
					field.SetValue(modSettings, sliderValue);
					modSettings.CallOnChange(field, oldValue, sliderValue);
				}
			} else {
				int intValue = Mathf.RoundToInt(sliderValue);
				int oldValue = (int) field.GetValue(modSettings);

				label.text = string.Format(numberFormat, intValue);
				if (oldValue != intValue) {
					GameAudioManager.PlayGUISlider();
					field.SetValue(modSettings, intValue);
					modSettings.CallOnChange(field, oldValue, intValue);
				}
			}
		}

		private GameObject CreateSetting(NameAttribute name, DescriptionAttribute description, GameObject prefab, string labelName) {
			GameObject setting = NGUITools.AddChild(uiGrid.gameObject, prefab);
			setting.name = "Custom Setting (" + name.Name + ")";

			Transform labelTransform = setting.transform.Find(labelName);
			SetLabelText(labelTransform, name.Name, name.Localize);

			DescriptionHolder descriptionHolder = setting.AddComponent<DescriptionHolder>();
			descriptionHolder.SetDescription(description?.Description ?? string.Empty, description?.Localize ?? false);

			menuItems.Add(setting);
			return setting;
		}

		private static void SetLabelText(Transform transform, string text, bool localize) {
			if (localize) {
				UILocalize uiLocalize = transform.GetComponent<UILocalize>();
				uiLocalize.key = text;
			} else {
				UnityEngine.Object.Destroy(transform.GetComponent<UILocalize>());
				UILabel uiLabel = transform.GetComponent<UILabel>();
				uiLabel.text = text;
			}
		}

		private void SetVisibilityListener(ModSettingsBase modSettings, FieldInfo field, GameObject guiObject, Header header) {
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

		protected class Header : Group {

			private readonly List<GameObject> guiObjects;

			internal Header(params GameObject[] guiObjects) {
				this.guiObjects = new List<GameObject>(guiObjects);
			}

			protected override void SetVisible(bool visible) {
				foreach (GameObject guiObject in guiObjects) {
					NGUITools.SetActiveSelf(guiObject, visible);
				}
			}
		}
	}
}
