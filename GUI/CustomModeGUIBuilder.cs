using UnityEngine;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;


namespace ModSettings {
	internal class CustomModeGUIBuilder : GUIBuilder {

		private readonly Queue<Transform> sections = new Queue<Transform>();

		private bool afterLast;

		internal CustomModeGUIBuilder(Panel_CustomXPSetup panel) : base(CreateUIGrid(panel), panel.m_CustomXPMenuItemOrder) {
			// Remove all but the first element, which is the experience mode preset selector
			panel.m_CustomXPMenuItemOrder.RemoveRange(1, panel.m_CustomXPMenuItemOrder.Count - 1);

			Transform offset = panel.m_ScrollPanelOffsetTransform;
			int sectionCount = offset.childCount - 1; // -1 for UI Grid we just created
			for (int i = 0; i < sectionCount; ++i) {
				Transform section = offset.GetChild(i);
				sections.Enqueue(section);
			}
		}

		private static UIGrid CreateUIGrid(Panel_CustomXPSetup panel) {
			GameObject child = NGUITools.AddChild(panel.m_ScrollPanelOffsetTransform.gameObject);
			child.name = "Custom Mode Settings UIGrid";
			UIGrid uiGrid = child.AddComponent<UIGrid>();
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
			uiGrid.cellHeight = gridCellHeight;
			uiGrid.hideInactive = true;
			uiGrid.onReposition = new System.Action(() => ResizeScrollBar(panel, uiGrid));
			return uiGrid;
		}

		private static void ResizeScrollBar(Panel_CustomXPSetup panel, UIGrid uiGrid) {
			UISlider slider = panel.m_Scrollbar.GetComponentInChildren<UISlider>(true);
			float viewHeight = panel.m_ScrollPanel.height;
			float absoluteVal = slider.value * (panel.m_ScrollPanelHeight - viewHeight);
			int childCount = uiGrid.GetChildList().Count;

			float height = childCount * gridCellHeight;
			panel.m_ScrollPanelHeight = height;

			ScrollbarThumbResizer thumbResizer = slider.GetComponent<ScrollbarThumbResizer>();
			thumbResizer.SetNumSteps((int) panel.m_ScrollPanel.height, (int) height);

			slider.value = Mathf.Clamp01(absoluteVal / Mathf.Max(1, panel.m_ScrollPanelHeight - viewHeight));
			panel.OnScrollbarChange();
		}

		internal void NextSection() {
			if (sections.Count == 0) {
				if (afterLast) {
					MelonLoader.MelonLogger.Warning("Exhausted all GUI sections, skipping NextSection!");
				}
				afterLast = true;
				return;
			}

			// Re-parent GUI elements to table and update menuItemOrder
			Transform section = sections.Dequeue();
			int elements = section.childCount;

			for (int i = 0; i < elements; ++i) {
				Transform element = section.GetChild(0);
				if (element.childCount == 0) {
					// It's a header, add some padding and shift to left
					GameObject padding = NGUITools.AddChild(uiGrid.gameObject);
					GameObject parent = NGUITools.AddChild(uiGrid.gameObject);
					lastHeader = new Header(parent, padding);

					element.parent = parent.transform;
					element.localPosition = new Vector2(-70, 0);
				} else {
					// It's a setting, add to tab list
					menuItems.Add(element.gameObject);
					element.parent = uiGrid.transform;

					// Add visibility listener
					var listener = element.gameObject.AddComponent<HinterlandSettingVisibilityListener>();
					listener.Init(element.gameObject.activeSelf, lastHeader, uiGrid);
				}
			}

            UnityEngine.Object.Destroy(section.gameObject);
		}

		internal void Finish() {
			// Make sure that all sections have been added to the table
			if (sections.Count > 0) {
				MelonLoader.MelonLogger.Warning("More GUI elements in queue!");
				while (sections.Count > 0) {
					NextSection();
				}
			}
		}

		internal class HinterlandSettingVisibilityListener : MonoBehaviour {

			static HinterlandSettingVisibilityListener() {
				ClassInjector.RegisterTypeInIl2Cpp<HinterlandSettingVisibilityListener>();
			}
			public HinterlandSettingVisibilityListener(System.IntPtr ptr) : base(ptr) { }

			private bool visible;
			private Group header;
			private UIGrid uiGrid;

			[HideFromIl2Cpp]
			internal void Init(bool visible, Group header, UIGrid uiGrid) {
				this.visible = visible;
				this.header = header;
				this.uiGrid = uiGrid;

				header.NotifyChildAdded(visible);
			}

			private void OnEnable() {
				UpdateVisibility(gameObject.activeSelf);
			}

			private void OnDisable() {
				UpdateVisibility(gameObject.activeSelf);
			}

			[HideFromIl2Cpp]
			private void UpdateVisibility(bool newVisible) {
				if (visible != newVisible) {
					visible = newVisible;
					header.NotifyChildVisible(newVisible);
					uiGrid.repositionNow = true;
				}
			}
		}
	}
}
