using System;
using Harmony;
using UnityEngine;

namespace ModSettings {
	internal static class CustomModePatches {

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "Start", new Type[0])]
		private static class BuildCustomModeGUIPatch {
			private static void Postfix() {
				DateTime tStart = DateTime.UtcNow;

				try {
					Debug.Log("[ModSettings] Building Mod Settings GUI");
					ModSettingsMenu.BuildGUI();
					Debug.Log("[ModSettings] Building Custom Mode GUI");
					CustomModeMenu.BuildGUI();
				} catch (Exception e) {
					Debug.LogError("[ModSettings] Exception while building GUI");
					Debug.LogException(e);
				}

				long timeMillis = (long) (DateTime.UtcNow - tStart).TotalMilliseconds;
				Debug.Log("[ModSettings] Done! Took " + timeMillis + " ms. Have a nice day!");
			}
		}

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
			private static void Postfix() {
				CustomModeMenu.CallOnConfirm();
			}
		}
	}
}
