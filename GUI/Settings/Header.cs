using ModSettings.Groups;
using UnityEngine;

namespace ModSettings {
	public static class Header {
		public static void AddHeader(this GUIBuilder guiBuilder, SectionAttribute section) => AddHeader(guiBuilder, section?.Title, section?.Localize ?? false);
		public static void AddHeader(this GUIBuilder guiBuilder, string title) => AddHeader(guiBuilder, title, false);
		public static void AddHeader(this GUIBuilder guiBuilder, string title, bool localize) {
			GameObject padding = NGUITools.AddChild(guiBuilder.uiGrid.gameObject);
			GameObject header = NGUITools.AddChild(guiBuilder.uiGrid.gameObject);
			GameObject label = NGUITools.AddChild(header, ObjectPrefabs.headerLabelPrefab);

			label.SetActive(true);
			label.transform.localPosition = new Vector2(-70, 0);
			label.name = "Custom Header (" + title ?? "" + ")";
			GUIBuilder.SetLabelText(label.transform, title ?? "", localize);

			guiBuilder.lastHeader = new HeaderGroup(header, padding);
		}

		public static void AddPaddingHeader(this GUIBuilder guiBuilder) {
			GameObject padding = NGUITools.AddChild(guiBuilder.uiGrid.gameObject);
			guiBuilder.lastHeader = new HeaderGroup(padding);
		}
	}
}
