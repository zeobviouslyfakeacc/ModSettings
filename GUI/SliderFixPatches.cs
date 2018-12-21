using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;

namespace ModSettings {
	internal static class SliderFixPatches {

		private const float MENU_DEADZONE = 0.05f;
		private const float MOVEMENT_SPEED = 0.01f;

		private static readonly MethodInfo MENU_MOVEMENT_METHOD = AccessTools.Method(typeof(Panel_OptionsMenu), "GetGenericSliderMovementHorizontal");
		private static readonly FieldInfo MENU_DEADZONE_FIELD = AccessTools.Field(typeof(InputSystemRewired), "m_MenuNavigationDeadzone");
		private static readonly float OLD_MENU_DEADZONE = (float) MENU_DEADZONE_FIELD.GetValue(null);

		// Public to allow access from UISlider#OnKey(KeyCode)
		public static float sliderMoveAmount;

		[HarmonyPatch(typeof(UISlider), "OnKey", new Type[] { typeof(KeyCode) })]
		private static class MakeSteplessSlidersMoveSmoothly {
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				foreach (CodeInstruction instruction in instructions) {
					// Find the constant load of 0.125f (= wrong, old move amount for stepless sliders) ...
					if (instruction.opcode == OpCodes.Ldc_R4 && (float) instruction.operand == 0.125f) {
						// ... and replace it with a load of the static field SteplessSliderFixPatches.sliderMoveAmount
						instruction.opcode = OpCodes.Ldsfld;
						instruction.operand = AccessTools.Field(typeof(SliderFixPatches), "sliderMoveAmount");
					}

					yield return instruction;
				}
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "UpdateMenuNavigationGeneric")]
		private static class DisableTimerForSteplessSliderMove {

			private static void Postfix(ref int index, List<GameObject> menuItems) {
				ConsoleSlider slider = menuItems[index].GetComponentInChildren<ConsoleSlider>();
				if (!slider || !slider.m_Slider || slider.m_Slider.numberOfSteps > 1)
					return; // Not a stepless slider

				float oldMovementWithTimer = GetTimeredMenuInputHorizontal();
				if (oldMovementWithTimer != 0f)
					return; // Already called OnIncrease or OnDecrease, we don't need to do that again

				MoveSlider(slider, GetRawMenuInputHorizontal());
			}
		}

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "DoMainScreenControls")]
		private static class MakeControllersMoveSlidersInCustomSettingsPanel {

			private static readonly FieldInfo SELECTED_INDEX_FIELD = AccessTools.Field(typeof(Panel_CustomXPSetup), "m_CustomXPSelectedButtonIndex");

			private static void Postfix(Panel_CustomXPSetup __instance) {
				int selectedIndex = (int) SELECTED_INDEX_FIELD.GetValue(__instance);
				List<GameObject> menuItems = __instance.m_CustomXPMenuItemOrder;

				ConsoleSlider slider = menuItems[selectedIndex].GetComponentInChildren<ConsoleSlider>();
				if (!slider || !slider.m_Slider)
					return; // Not a slider

				bool isStepless = (slider.m_Slider.numberOfSteps <= 1);
				float movement = isStepless ? GetRawMenuInputHorizontal() : GetTimeredMenuInputHorizontal();

				MoveSlider(slider, movement);
			}
		}

		[HarmonyPatch(typeof(ConsoleSlider), "OnIncrease", new Type[0])]
		private static class UpdateOnIncrease {
			private static void Prefix() {
				UpdateSliderMoveAmount();
			}
		}

		[HarmonyPatch(typeof(ConsoleSlider), "OnDecrease", new Type[0])]
		private static class UpdateOnDecrease {
			private static void Prefix() {
				UpdateSliderMoveAmount();
			}
		}

		[HarmonyPatch(typeof(UISlider), "OnStart", new Type[0])]
		private static class FixSliderForegroundBarColor {
			private static void Postfix(UISlider __instance) {
				if (__instance is UIScrollBar || __instance.onDragFinished == null)
					return;

				UIWidget foreground = (UIWidget) AccessTools.Field(typeof(UIProgressBar), "mFG").GetValue(__instance);
				foreground.gameObject.AddMissingComponent<SliderBarDepthFixer>();
			}
		}

		private static void MoveSlider(ConsoleSlider slider, float movement) {
			if (movement < 0f) {
				slider.OnDecrease();
			} else if (movement > 0f) {
				slider.OnIncrease();
			}
		}

		private static void UpdateSliderMoveAmount() {
			sliderMoveAmount = MOVEMENT_SPEED * Mathf.Abs(GetRawMenuInputHorizontal());
		}

		private static float GetTimeredMenuInputHorizontal() {
			return (float) MENU_MOVEMENT_METHOD.Invoke(InterfaceManager.m_Panel_OptionsMenu, new object[0]);
		}

		private static float GetRawMenuInputHorizontal() {
			Panel_OptionsMenu options = InterfaceManager.m_Panel_OptionsMenu;
			MENU_DEADZONE_FIELD.SetValue(null, MENU_DEADZONE);
			float result = InputManager.GetMenuNavigationPrimary(options).x + InputManager.GetMenuNavigationSecondary(options).x;
			MENU_DEADZONE_FIELD.SetValue(null, OLD_MENU_DEADZONE);
			return result;
		}

		internal class SliderBarDepthFixer : MonoBehaviour {

			private const int TARGET_DEPTH = 25;

			private void OnEnable() {
				gameObject.GetComponentInChildren<UIWidget>().depth = TARGET_DEPTH;
			}
		}
	}
}
