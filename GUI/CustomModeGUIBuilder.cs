using System.Collections.Generic;
using UnityEngine;

namespace ModSettings {
	internal class CustomModeGUIBuilder : GUIBuilder {

		private readonly Queue<Transform> sections = new Queue<Transform>();

		private bool afterLast;

		internal CustomModeGUIBuilder(Panel_CustomXPSetup panel) : base(CreateUIGrid(panel), panel.m_CustomXPMenuItemOrder) {
			panel.m_CustomXPMenuItemOrder.Clear();

			Transform offset = panel.m_ScrollPanelOffsetTransform;
			int sectionCount = offset.childCount - 1; // -1 for UI Grid we just created
			for (int i = 0; i < sectionCount; ++i) {
				Transform section = offset.GetChild(i);
				sections.Enqueue(section);
			}
		}

		private static UIGrid CreateUIGrid(Panel_CustomXPSetup panel) {
			UIGrid uiGrid = NGUITools.AddChild<UIGrid>(panel.m_ScrollPanelOffsetTransform.gameObject);
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
			uiGrid.cellHeight = gridCellHeight;
			uiGrid.hideInactive = true;
			uiGrid.onReposition = () => ResizeScrollBar(panel, uiGrid);
			return uiGrid;
		}

		private static void ResizeScrollBar(Panel_CustomXPSetup panel, UIGrid uiGrid) {
			UISlider slider = panel.m_Scrollbar.GetComponentInChildren<UISlider>(true);
			float viewHeight = panel.m_ScrollPanel.GetViewSize().y;
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
					Debug.LogWarning("[ModSettings] Exhausted all GUI sections, skipping NextSection!");
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
				}
			}

			Object.Destroy(section.gameObject);
		}

		internal void Finish() {
			// Make sure that all sections have been added to the table
			if (sections.Count > 0) {
				Debug.LogWarning("[ModSettings] More GUI elements in queue!");
				while (sections.Count > 0) {
					NextSection();
				}
			}
		}
	}
}
