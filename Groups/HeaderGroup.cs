using System.Collections.Generic;
using UnityEngine;

namespace ModSettings.Groups {
	internal class HeaderGroup : Group {

		private readonly List<GameObject> guiObjects;

		internal HeaderGroup(params GameObject[] guiObjects) {
			this.guiObjects = new List<GameObject>(guiObjects);
		}

		protected override void SetVisible(bool visible) {
			foreach (GameObject guiObject in guiObjects) {
				NGUITools.SetActiveSelf(guiObject, visible);
			}
		}
	}
}
