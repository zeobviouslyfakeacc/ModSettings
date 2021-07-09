using UnityEngine;

namespace ModSettings.Extentions {
	internal static class GameObjectExtensions {
		public static GameObject GetChild(this GameObject parent, string childName) {
			return parent?.transform?.FindChild(childName)?.gameObject;
		}

		public static void DestroyChild(this GameObject parent, string childName) {
			GameObject child = parent?.transform?.FindChild(childName)?.gameObject;
			if (child) GameObject.DestroyImmediate(child);
		}
	}
}
