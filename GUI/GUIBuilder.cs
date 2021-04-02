using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static ModSettings.AttributeFieldTypes;
using Il2Cpp = Il2CppSystem.Collections.Generic;

namespace ModSettings {
	internal abstract class GUIBuilder {

		internal const int gridCellHeight = 33;

		private static readonly GameObject headerLabelPrefab;
		private static readonly GameObject sliderPrefab;
		private static readonly GameObject comboBoxPrefab;
		private static readonly GameObject keyEntryPrefab;

		static GUIBuilder() {
			Transform firstSection = InterfaceManager.m_Panel_CustomXPSetup.m_ScrollPanelOffsetTransform.GetChild(0);

			headerLabelPrefab = UnityEngine.Object.Instantiate(firstSection.Find("Header").gameObject);
			headerLabelPrefab.SetActive(false);

			comboBoxPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_CustomXPSetup.m_AllowInteriorSpawnPopupList.gameObject);
			comboBoxPrefab.SetActive(false);

			keyEntryPrefab = MakeKeyEntryPrefab();
			keyEntryPrefab.SetActive(false);

			UnityEngine.Object.DestroyImmediate(InterfaceManager.m_Panel_OptionsMenu.m_FieldOfViewSlider.m_SliderObject.GetComponent<GenericSliderSpawner>());
			sliderPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_OptionsMenu.m_FieldOfViewSlider.gameObject);
			sliderPrefab.SetActive(false);
			sliderPrefab.transform.Find("Label_FOV").localPosition = new Vector3(-10, 0, -1);

			// Fix slider hitbox
			BoxCollider collider = sliderPrefab.GetComponentInChildren<BoxCollider>();
			collider.center = new Vector3(150, 0);
			collider.size = new Vector3(900, 30);
		}

		protected readonly UIGrid uiGrid;
		protected readonly Il2Cpp.List<GameObject> menuItems;

		protected Header lastHeader;

		protected GUIBuilder(UIGrid uiGrid, Il2Cpp.List<GameObject> menuItems) {
			this.uiGrid = uiGrid;
			this.menuItems = menuItems;
		}

		private static GameObject MakeKeyEntryPrefab() {
			GameObject result = GameObject.Instantiate(comboBoxPrefab);

			Transform rebindingTab = InterfaceManager.m_Panel_OptionsMenu.m_RebindingTab.transform;
			GameObject originalButton = rebindingTab?.FindChild("GameObject")?.FindChild("LeftSide")?.FindChild("Button_Rebinding")?.gameObject;
			GameObject keybindingButton = GameObject.Instantiate(originalButton);

			keybindingButton.transform.position = result.transform.FindChild("Label_Value").position;
			keybindingButton.transform.parent = result.transform;
			keybindingButton.name = "Keybinding_Button";

			GameObject.DestroyImmediate(result.GetComponent<ConsoleComboBox>());
			DestroyChild(result, "Button_Decrease");
			DestroyChild(result, "Button_Increase");
			DestroyChild(result, "Label_Value");

			DestroyChild(keybindingButton, "Label_Name");

			return result;
		}

		private static void DestroyChild(GameObject parent, string childName) {
			GameObject child = parent?.transform?.FindChild(childName)?.gameObject;
			if(child) GameObject.DestroyImmediate(child);
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

				if (slider != null) {
					AddSliderSetting(modSettings, field, name, description, slider);
				} else if (choice != null) {
					AddChoiceSetting(modSettings, field, name, description, choice);
				} else {
					// No Slider or Choice annotation, determine GUI object from field type
					Type fieldType = field.FieldType;

					if (fieldType == typeof(UnityEngine.KeyCode)) {
						AddKeySetting(modSettings, field, name, description);
					} else if (fieldType.IsEnum) {
						AddChoiceSetting(modSettings, field, name, description, ChoiceAttribute.ForEnumType(fieldType));
					} else if (fieldType == typeof(bool)) {
						AddChoiceSetting(modSettings, field, name, description, ChoiceAttribute.YesNoAttribute);
					} else if (IsFloatType(fieldType)) {
						AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultFloatRange);
					} else if (IsIntegerType(fieldType)) {
						AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultIntRange);
					} else {
						throw new ArgumentException("Unsupported field type: " + fieldType.Name);
					}
				}
			}
		}

		protected virtual void SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue) {
			modSettings.SetFieldValue(field, newValue);
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

		private void AddKeySetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description) {
			// Create menu item
			GameObject setting = CreateSetting(name, description, keyEntryPrefab, "Label");
			GameObject keyButtonObject = setting.transform.FindChild("Keybinding_Button").gameObject;

			CustomKeybinding customKeybinding = setting.AddComponent<CustomKeybinding>();
			customKeybinding.keyRebindingButton = keyButtonObject.GetComponent<KeyRebindingButton>();
			customKeybinding.currentKeycodeSetting = (KeyCode)field.GetValue(modSettings);
			customKeybinding.RefreshLabelValue();

			UIButton uiButton = keyButtonObject.GetComponent<UIButton>();
			EventDelegate.Set(uiButton.onClick, new Action(customKeybinding.OnClick));
			customKeybinding.OnChange = new Action(() => UpdateKeyValue(modSettings, field, customKeybinding));
			modSettings.AddRefreshAction(() => UpdateKeyChoice(modSettings, field, customKeybinding));

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private void UpdateKeyValue(ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding) {
			SetSettingsField(modSettings, field, customKeybinding.currentKeycodeSetting);
		}

		private void UpdateKeyChoice(ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding) {
			KeyCode keyCode = (KeyCode)field.GetValue(modSettings);
			customKeybinding.currentKeycodeSetting = keyCode;
			customKeybinding.keyRebindingButton.SetValueLabel(keyCode.ToString());
		}

		private void AddChoiceSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, ChoiceAttribute choice) {
			// Create menu item
			GameObject setting = CreateSetting(name, description, comboBoxPrefab, "Label");
			ConsoleComboBox comboBox = setting.GetComponent<ConsoleComboBox>();

			// Add selectable values
			comboBox.items.Clear();
			foreach (string choiceName in choice.Names) {
				comboBox.items.Add(choiceName);
			}
			comboBox.m_Localize = choice.Localize;

			// Add listener and set default value
			EventDelegate.Set(comboBox.onChange, new Action(() => UpdateChoiceValue(modSettings, field, comboBox.GetCurrentIndex())));
			modSettings.AddRefreshAction(() => UpdateChoiceComboBox(modSettings, field, comboBox));

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private void UpdateChoiceValue(ModSettingsBase modSettings, FieldInfo field, int selectedIndex) {
			Type fieldType = field.FieldType;
			if (fieldType.IsEnum) {
				fieldType = Enum.GetUnderlyingType(fieldType);
			}

			SetSettingsField(modSettings, field, Convert.ChangeType(selectedIndex, fieldType, null));
		}

		private static void UpdateChoiceComboBox(ModSettingsBase modSettings, FieldInfo field, ConsoleComboBox comboBox) {
			int value = Convert.ToInt32(field.GetValue(modSettings));
			comboBox.value = comboBox.items[value];
		}

		private void AddSliderSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, SliderAttribute range) {
			// Create menu
			GameObject setting = CreateSetting(name, description, sliderPrefab, "Label_FOV");
			ConsoleSlider slider = setting.GetComponent<ConsoleSlider>();
			UILabel uiLabel = slider.m_SliderObject.GetComponentInChildren<UILabel>();
			UISlider uiSlider = slider.m_SliderObject.GetComponentInChildren<UISlider>();

			// Sanitize user values, especially if the field type is int
			bool isFloat = IsFloatType(field.FieldType);
			float from = isFloat ? range.From : Mathf.Round(range.From);
			float to = isFloat ? range.To : Mathf.Round(range.To);
			int numberOfSteps = range.NumberOfSteps;
			if (numberOfSteps < 0) {
				numberOfSteps = isFloat ? 1 : Mathf.RoundToInt(Mathf.Abs(from - to)) + 1;
			}
			string numberFormat = range.NumberFormat;
			if (string.IsNullOrEmpty(numberFormat)) {
				numberFormat = isFloat ? SliderAttribute.DefaultFloatFormat : SliderAttribute.DefaultIntFormat;
			}

			// Add listeners to update setting value
			EventDelegate.Callback callback = new Action(() => UpdateSliderValue(modSettings, field, uiSlider, uiLabel, from, to, numberFormat));
			EventDelegate.Set(slider.onChange, callback);
			EventDelegate.Set(uiSlider.onChange, callback);
			modSettings.AddRefreshAction(() => UpdateSlider(modSettings, field, uiSlider, uiLabel, from, to, numberFormat));

			// Set default value and number of steps
			float defaultValue = Convert.ToSingle(field.GetValue(modSettings));
			uiSlider.value = (defaultValue - from) / (to - from);
			uiSlider.numberOfSteps = numberOfSteps;
			UpdateSliderLabel(field, uiLabel, defaultValue, numberFormat);

			// Control visibility
			SetVisibilityListener(modSettings, field, setting, lastHeader);
		}

		private void UpdateSliderValue(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat) {
			float sliderValue = from + slider.value * (to - from);
			if (SliderMatchesField(modSettings, field, sliderValue)) return;
			if (IsIntegerType(field.FieldType)) {
				sliderValue = Mathf.Round(sliderValue);
			}

			UpdateSliderLabel(field, label, sliderValue, numberFormat);
			SetSettingsField(modSettings, field, Convert.ChangeType(sliderValue, field.FieldType, null));

			if (modSettings.IsVisible() && slider.numberOfSteps > 1) {
				GameAudioManager.PlayGUISlider();
			}
		}

		private static void UpdateSlider(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat) {
			float sliderValue = from + slider.value * (to - from);
			if (SliderMatchesField(modSettings, field, sliderValue)) return;

			float value = Convert.ToSingle(field.GetValue(modSettings));
			slider.value = (value - from) / (to - from);
			UpdateSliderLabel(field, label, value, numberFormat);
		}

		private static bool SliderMatchesField(ModSettingsBase modSettings, FieldInfo field, float sliderValue) {
			if (IsFloatType(field.FieldType)) {
				float oldValue = Convert.ToSingle(field.GetValue(modSettings));
				return sliderValue == oldValue;
			} else {
				long oldValue = Convert.ToInt64(field.GetValue(modSettings));
				long longValue = (long) Mathf.Round(sliderValue);
				return oldValue == longValue;
			}
		}

		private static void UpdateSliderLabel(FieldInfo field, UILabel label, float value, string numberFormat) {
			if (IsFloatType(field.FieldType)) {
				label.text = string.Format(numberFormat, value);
			} else {
				long longValue = (long) Mathf.Round(value);
				label.text = string.Format(numberFormat, longValue);
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
