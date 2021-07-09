using HarmonyLib;
using ModSettings.Scripts;

namespace ModSettings.Patches {
	internal static class TextBoxPatches {
		//Gets called twice when selecting
		[HarmonyPatch(typeof(UIInput), "OnSelectEvent")]
		internal static class UIInput_OnSelectEvent {
			private static void Postfix(UIInput __instance) {
				if (__instance.GetComponent<TextBoxHandler>() != null) {
					ModSettingsMenu.disableMovementInput = true;
					InputManager.PushContext(__instance); //It doesn't hurt anything for this to be called twice
				}
			}
		}

		//Gets called once when deselecting
		[HarmonyPatch(typeof(UIInput), "OnDeselectEvent")]
		internal static class UIInput_OnDeselectEvent {
			private static void Postfix(UIInput __instance) {
				TextBoxHandler customInput = __instance.GetComponent<TextBoxHandler>();
				if (customInput != null) {
					ModSettingsMenu.disableMovementInput = false;
					InputManager.PopContext(__instance);
					customInput.onDeselect?.Invoke();
				}
			}
		}
	}
}
