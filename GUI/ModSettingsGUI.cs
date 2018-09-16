using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	internal class ModSettingsGUI : MonoBehaviour {

		private const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		private readonly Dictionary<string, ModTab> modTabs = new Dictionary<string, ModTab>();
		private ModTab currentTab = null;
		private int selectedIndex = 0;

		private ConsoleComboBox modSelector;
		private UIPanel scrollPanel;
		private Transform scrollPanelOffset;
		private GameObject scrollBar;
		private UISlider scrollBarSlider;

		internal ModSettingsGUI() {
			Transform contentArea = transform.Find("GameObject");

			DestroyOldSettings(contentArea);
			modSelector = CreateModSelector(contentArea);
			scrollPanel = CreateScrollPanel(contentArea);
			scrollPanelOffset = CreateOffsetTransform(scrollPanel);
			scrollBar = CreateScrollBar(scrollPanel);
			scrollBarSlider = scrollBar.GetComponentInChildren<UISlider>(true);
		}

		private static void DestroyOldSettings(Transform contentArea) {
			int settingsCount = contentArea.childCount;
			for (int i = settingsCount - 1; i >= 2; --i) {
				Transform setting = contentArea.GetChild(i);
				setting.parent = null;
				Destroy(setting.gameObject);
			}
		}

		private ConsoleComboBox CreateModSelector(Transform contentArea) {
			ConsoleComboBox modSelector = contentArea.Find("Quality").GetComponent<ConsoleComboBox>();

			// Make the combobox a lot larger
			int xOffset = 200;
			Vector3 offset = new Vector3(xOffset, 0);

			Transform transform = modSelector.transform;
			transform.localPosition -= offset;
			transform.Find("Button_Increase").localPosition += offset;
			transform.Find("Label_Value").localPosition += offset / 2f;
			Transform background = transform.Find("Console_Background");
			background.localPosition += offset / 2f;
			background.GetComponent<UISprite>().width += xOffset;

			EventDelegate.Set(modSelector.onChange, () => SelectMod(modSelector.value));
			return modSelector;
		}

		private static UIPanel CreateScrollPanel(Transform contentArea) {
			UIPanel panel = NGUITools.AddChild<UIPanel>(contentArea.gameObject);
			panel.gameObject.name = "ScrollPanel";
			panel.baseClipRegion = new Vector4(0, 0, 2000, 520);
			panel.clipOffset = new Vector2(500, -260);
			panel.clipping = UIDrawCall.Clipping.SoftClip;
			panel.depth = 100;
			return panel;
		}

		private static Transform CreateOffsetTransform(UIPanel scrollPanel) {
			GameObject offset = NGUITools.AddChild(scrollPanel.gameObject);
			offset.name = "Offset";
			return offset.transform;
		}

		private GameObject CreateScrollBar(UIPanel scrollPanel) {
			// Terrible code in this method. I've given up trying to fix their scroll bar layout
			Panel_CustomXPSetup xpModePanel = InterfaceManager.m_Panel_CustomXPSetup;
			GameObject scrollbarParent = xpModePanel.m_Scrollbar;
			GameObject scrollbarPrefab = scrollbarParent.transform.GetChild(0).gameObject;
			GameObject scrollbar = NGUITools.AddChild(gameObject, scrollbarPrefab);
			scrollbar.name = "Scrollbar";
			scrollbar.transform.localPosition = new Vector2(415, -40);

			int height = (int) scrollPanel.height;
			UISlider slider = scrollbar.GetComponentInChildren<UISlider>(true);
			slider.backgroundWidget.GetComponent<UISprite>().height = height;
			slider.foregroundWidget.GetComponent<UISprite>().height = height;
			scrollbar.transform.Find("glow").GetComponent<UISprite>().height = height + 44;

			EventDelegate.Set(slider.onChange, () => OnScroll(slider, true));

			return scrollbar;
		}

		private void Update() {
			if (currentTab == null)
				return;

			UpdateMenuNavigationGeneric();
			UpdateDescriptionLabel();

			if (currentTab.scrollBarHeight > 0) {
				float scroll = Input.GetAxis("Mouse ScrollWheel");
				float scrollAmount = 60f / currentTab.scrollBarHeight;
				if (scroll < 0) {
					scrollBarSlider.value += scrollAmount;
				} else if (scroll > 0) {
					scrollBarSlider.value -= scrollAmount;
				}
			}
		}

		private void OnScroll(UISlider slider, bool playSound) {
			scrollPanelOffset.localPosition = new Vector2(0, slider.value * (currentTab?.scrollBarHeight ?? 0));
			if (playSound) {
				GameAudioManager.PlayGUIScroll();
			}
		}

		private void SelectMod(string modName) {
			if (currentTab != null) {
				currentTab.uiGrid.gameObject.SetActive(false);
			}

			selectedIndex = 0;

			if (modTabs.TryGetValue(modName, out currentTab)) {
				currentTab = modTabs[modName];
				currentTab.uiGrid.gameObject.SetActive(true);

				SetConfirmButtonVisible(currentTab.requiresConfirmation);

				ResizeScrollBar(currentTab);
				EnsureSelectedSettingVisible();
			}
		}

		private void UpdateMenuNavigationGeneric() {
			MethodInfo updateMethod = typeof(Panel_OptionsMenu).GetMethod("UpdateMenuNavigationGeneric", bindingFlags);
			object[] args = { selectedIndex, currentTab.menuItems };
			updateMethod.Invoke(InterfaceManager.m_Panel_OptionsMenu, args);

			selectedIndex = (int) args[0];
			EnsureSelectedSettingVisible();
		}

		private void UpdateDescriptionLabel() {
			GameObject setting = currentTab.menuItems[selectedIndex];
			DescriptionHolder description = setting.GetComponent<DescriptionHolder>();

			if (description == null)
				return;

			UILabel descriptionLabel = InterfaceManager.m_Panel_OptionsMenu.m_OptionDescriptionLabel;
			descriptionLabel.text = description.Text;
			descriptionLabel.transform.parent = setting.transform;
			descriptionLabel.transform.localPosition = new Vector3(655, 0);
			descriptionLabel.gameObject.SetActive(true);
		}

		private void EnsureSelectedSettingVisible() {
			if (Utils.GetMenuMovementVertical(true, false) == 0f)
				return;

			if (selectedIndex == 0) {
				scrollBarSlider.value = 0;
				return;
			}

			GameObject setting = currentTab.menuItems[selectedIndex];
			float settingY = -setting.transform.localPosition.y;
			float scrollPanelTop = scrollPanelOffset.localPosition.y + GUIBuilder.gridCellHeight;
			float scrollPanelBottom = scrollPanelOffset.localPosition.y + scrollPanel.height - GUIBuilder.gridCellHeight;

			if (settingY < scrollPanelTop) {
				scrollBarSlider.value += (settingY - scrollPanelTop) / currentTab.scrollBarHeight;
				GameAudioManager.PlayGUIScroll();
			} else if (settingY > scrollPanelBottom) {
				scrollBarSlider.value += (settingY - scrollPanelBottom) / currentTab.scrollBarHeight;
				GameAudioManager.PlayGUIScroll();
			}
		}

		internal void Enable(Panel_OptionsMenu parentMenu) {
			bool inMainMenu = InterfaceManager.IsMainMenuActive();
			MenuType menuType = inMainMenu ? MenuType.MainMenuOnly : MenuType.InGameOnly;
			ModSettingsMenu.SetSettingsVisible(menuType);

			GameAudioManager.PlayGUIButtonClick();
			MethodInfo method = typeof(Panel_OptionsMenu).GetMethod("SetTabActive", bindingFlags);
			method.Invoke(parentMenu, new object[] { gameObject });
		}

		private void OnDisable() {
			ModSettingsMenu.SetAllSettingsInvisible();

			foreach (ModTab tab in modTabs.Values) {
				tab.requiresConfirmation = false;
			}
		}

		internal void SetSettingsNeedConfirmation(bool value) {
			currentTab.requiresConfirmation = value;
			SetConfirmButtonVisible(value);
		}

		private void SetConfirmButtonVisible(bool value) {
			FieldInfo needConfirmation = typeof(Panel_OptionsMenu).GetField("m_SettingsNeedConfirmation", BindingFlags.NonPublic | BindingFlags.Instance);
			needConfirmation.SetValue(InterfaceManager.m_Panel_OptionsMenu, value);
		}

		internal void CallOnConfirm() {
			if (currentTab == null)
				return;

			foreach (ModSettingsBase modSettings in currentTab.modSettings) {
				modSettings.CallOnConfirm();
			}
		}

		internal ModTab CreateModTab(string modName) {
			UIGrid grid = CreateUIGrid(modName);
			List<GameObject> menuItems = new List<GameObject>() { modSelector.gameObject };
			ModTab modTab = new ModTab(grid, menuItems);

			grid.onReposition = () => ResizeScrollBar(modTab);

			modTabs.Add(modName, modTab);
			AddModSelector(modName);

			return modTab;
		}

		internal void AddModSelector(string modName) {
			bool firstMod = modSelector.items.Count == 0;
			modSelector.items.Add(modName);

			if (firstMod) {
				modSelector.value = modName;
				SelectMod(modName);
			}
		}

		internal void RemoveModSelector(string modName) {
			modSelector.items.Remove(modName);
		}

		private UIGrid CreateUIGrid(string modName) {
			UIGrid uiGrid = NGUITools.AddChild<UIGrid>(scrollPanelOffset.gameObject);
			uiGrid.gameObject.name = "Mod settings grid (" + modName + ")";
			uiGrid.gameObject.SetActive(false);
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
			uiGrid.cellHeight = GUIBuilder.gridCellHeight;
			uiGrid.hideInactive = true;
			return uiGrid;
		}

		private void ResizeScrollBar(ModTab modTab) {
			int childCount = modTab.uiGrid.GetChildList().Count;

			if (modTab == currentTab) {
				float absoluteVal = scrollBarSlider.value * modTab.scrollBarHeight;

				float height = childCount * GUIBuilder.gridCellHeight;
				modTab.scrollBarHeight = height - scrollPanel.height;

				ScrollbarThumbResizer thumbResizer = scrollBarSlider.GetComponent<ScrollbarThumbResizer>();
				thumbResizer.SetNumSteps((int) scrollPanel.height, (int) height);

				scrollBarSlider.value = Mathf.Clamp01(absoluteVal / Mathf.Max(1, modTab.scrollBarHeight));
				OnScroll(scrollBarSlider, false);
			} else {
				modTab.scrollBarHeight = childCount * GUIBuilder.gridCellHeight - scrollPanel.height;
			}

			scrollBar.SetActive(currentTab.scrollBarHeight > 0);
		}
	}
}
