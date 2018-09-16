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

			private static readonly MethodInfo MOVEMENT_METHOD = AccessTools.Method(typeof(Panel_OptionsMenu), "GetGenericSliderMovementHorizontal");

			private static void Postfix(Panel_OptionsMenu __instance, ref int index, List<GameObject> menuItems) {
				ConsoleSlider slider = menuItems[index].GetComponentInChildren<ConsoleSlider>();
				if (!slider || !slider.m_Slider || slider.m_Slider.numberOfSteps > 1)
					return; // Not a stepless slider

				float oldMovementWithTimer = (float) MOVEMENT_METHOD.Invoke(__instance, new object[0]);
				if (oldMovementWithTimer != 0f)
					return; // Already called OnIncrease or OnDecrease, we don't need to do that again

				float newMovement = GetMenuInputHorizontal();
				if (newMovement < 0f) {
					slider.OnDecrease();
				} else if (newMovement > 0f) {
					slider.OnIncrease();
				}
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

		private static void UpdateSliderMoveAmount() {
			sliderMoveAmount = MOVEMENT_SPEED * Mathf.Abs(GetMenuInputHorizontal());
		}

		private static float GetMenuInputHorizontal() {
			MENU_DEADZONE_FIELD.SetValue(null, MENU_DEADZONE);
			float result = InputManager.GetMenuNavigationPrimary().x + InputManager.GetMenuNavigationSecondary().x;
			MENU_DEADZONE_FIELD.SetValue(null, OLD_MENU_DEADZONE);
			return result;
		}
	}
}
