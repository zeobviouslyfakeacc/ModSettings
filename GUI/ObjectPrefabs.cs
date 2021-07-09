using ModSettings.Extentions;
using UnityEngine;

namespace ModSettings {
	internal static class ObjectPrefabs {
		internal static readonly GameObject comboBoxPrefab;
		internal static readonly GameObject customComboBoxPrefab;
		internal static readonly GameObject displayPrefab;
		internal static readonly GameObject emptyPrefab;
		internal static readonly GameObject headerLabelPrefab;
		internal static readonly GameObject keyEntryPrefab;
		internal static readonly GameObject sliderPrefab;
		internal static readonly GameObject textEntryPrefab;

		static ObjectPrefabs() {
			Transform firstSection = InterfaceManager.m_Panel_CustomXPSetup.m_ScrollPanelOffsetTransform.GetChild(0);

			headerLabelPrefab = UnityEngine.Object.Instantiate(firstSection.Find("Header").gameObject);
			headerLabelPrefab.SetActive(false);

			comboBoxPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_CustomXPSetup.m_AllowInteriorSpawnPopupList.gameObject);
			comboBoxPrefab.SetActive(false);

			customComboBoxPrefab = MakeCustomComboBoxPrefab();
			customComboBoxPrefab.SetActive(false);

			displayPrefab = MakeDisplayPrefab();
			displayPrefab.SetActive(false);

			emptyPrefab = MakeEmptyPrefab();
			emptyPrefab.SetActive(false);

			keyEntryPrefab = MakeKeyEntryPrefab();
			keyEntryPrefab.SetActive(false);

			textEntryPrefab = MakeTextEntryPrefab();
			textEntryPrefab.SetActive(false);

			UnityEngine.Object.DestroyImmediate(InterfaceManager.m_Panel_OptionsMenu.m_FieldOfViewSlider.m_SliderObject.GetComponent<GenericSliderSpawner>());
			sliderPrefab = UnityEngine.Object.Instantiate(InterfaceManager.m_Panel_OptionsMenu.m_FieldOfViewSlider.gameObject);
			sliderPrefab.SetActive(false);
			sliderPrefab.transform.Find("Label_FOV").localPosition = new Vector3(-10, 0, -1);

			// Fix slider hitbox
			BoxCollider collider = sliderPrefab.GetComponentInChildren<BoxCollider>();
			collider.center = new Vector3(150, 0);
			collider.size = new Vector3(900, 30);
		}

		private static GameObject MakeCustomComboBoxPrefab() {
			GameObject result = GameObject.Instantiate(comboBoxPrefab);
			GameObject.DestroyImmediate(result.GetComponent<ConsoleComboBox>());
			return result;
		}

		private static GameObject MakeDisplayPrefab() {
			GameObject result = GameObject.Instantiate(comboBoxPrefab);

			GameObject.DestroyImmediate(result.GetComponent<ConsoleComboBox>());
			result.DestroyChild("Button_Decrease");
			result.DestroyChild("Button_Increase");

			return result;
		}

		private static GameObject MakeEmptyPrefab() {
			GameObject result = GameObject.Instantiate(comboBoxPrefab);

			GameObject.DestroyImmediate(result.GetComponent<ConsoleComboBox>());
			result.DestroyChild("Button_Decrease");
			result.DestroyChild("Button_Increase");
			result.DestroyChild("Label_Value");

			return result;
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
			result.DestroyChild("Button_Decrease");
			result.DestroyChild("Button_Increase");
			result.DestroyChild("Label_Value");

			keybindingButton.DestroyChild("Label_Name");

			return result;
		}

		private static GameObject MakeTextEntryPrefab() {
			GameObject result = GameObject.Instantiate(comboBoxPrefab);

			GameObject originalTextBox = InterfaceManager.m_Panel_Confirmation?.m_GenericMessageGroup?.m_InputField?.gameObject;
			GameObject newTextBox = GameObject.Instantiate(originalTextBox);

			newTextBox.transform.position = result.transform.FindChild("Label_Value").position;
			newTextBox.transform.parent = result.transform;
			newTextBox.name = "Text_Box";

			TextInputField textInputField = newTextBox.GetComponent<TextInputField>();
			textInputField.m_MaxLength = 25;

			GameObject.DestroyImmediate(result.GetComponent<ConsoleComboBox>());
			result.DestroyChild("Button_Decrease");
			result.DestroyChild("Button_Increase");
			result.DestroyChild("Label_Value");

			newTextBox.DestroyChild("bg");
			newTextBox.DestroyChild("glow");

			result.AddComponent<UIButton>();

			return result;
		}
	}
}
