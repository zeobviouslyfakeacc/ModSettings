using System;
using HarmonyLib;
using UnityEngine;
using Il2Cpp;
using Il2CppInterop.Runtime;

namespace ModSettings {
	internal static class CustomModePatches {

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "UpdateMenuNavigation")]
		private static class UpdateCustomModeDescriptionPatch {
			private static void Postfix(Panel_CustomXPSetup __instance, ref int index) {
				GameObject setting = __instance.m_CustomXPMenuItemOrder[index];
				if (setting == null)
					return;

				DescriptionHolder description = setting.GetComponent<DescriptionHolder>();
				if (description != null)
					__instance.m_TooltipLabel.text = description.Text;
			}
		}

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "Enable", new Type[] { typeof(bool) })]
		private static class SetSettingsEnabled {
			private static void Prefix(bool enable) {
				CustomModeMenu.SetSettingsVisible(enable);
			}
		}

		[HarmonyPatch(typeof(InterfaceManager), "TryDestroyPanel_Internal", new Type[] { typeof(Il2CppSystem.Type) })]
		private static class PreventCustomModePanelDestruction {
			private static bool Prefix(Il2CppSystem.Type panelType, ref bool __result) {
				if (panelType == Il2CppType.Of<Panel_CustomXPSetup>()) {
					__result = false;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "DoScroll")]
		private static class CustomModeAbsoluteValueScrollPatch {
			private static void Prefix(Panel_CustomXPSetup __instance, ref float scrollAmount) {
				if (Mathf.Abs(scrollAmount) < 1f) {
					float height = __instance.m_ScrollPanel.height;
					float scrollHeight = Math.Max(height, __instance.m_ScrollPanelHeight - height);
					scrollAmount *= 1923f / scrollHeight;
				}
			}
		}

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "OnContinue")]
		private static class CustomGameStartedPatch {
			private static void Prefix() {
				CustomModeMenu.CallOnConfirm();
			}
		}
	}
}
