using HarmonyLib;
using UnityEngine;
using Il2CppCollections = Il2CppSystem.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;

namespace ModSettings {
	internal static class SliderFixPatches {

		private const float MENU_DEADZONE = 0.05f;
		private const float MOVEMENT_SPEED = 0.01f;

		[HarmonyPatch(typeof(Panel_OptionsMenu), "UpdateMenuNavigationGeneric")]
		private static class DisableTimerForSteplessSliderMove {

			private static void Postfix(ref int index, Il2CppCollections.List<GameObject> menuItems) {
				ConsoleSlider slider = menuItems[index]?.GetComponentInChildren<ConsoleSlider>();
				if (slider == null || slider.m_Slider == null || slider.m_Slider.numberOfSteps > 1)
					return; // Not a stepless slider

				float oldMovementWithTimer = GetTimeredMenuInputHorizontal();
				if (oldMovementWithTimer != 0f)
					return; // Already called OnIncrease or OnDecrease, we don't need to do that again

				MoveSlider(slider, GetRawMenuInputHorizontal());
			}
		}

		[HarmonyPatch(typeof(Panel_CustomXPSetup), "DoMainScreenControls")]
		private static class MakeControllersMoveSlidersInCustomSettingsPanel {

			private static void Postfix(Panel_CustomXPSetup __instance) {
				int selectedIndex = __instance.m_CustomXPSelectedButtonIndex;
				Il2CppCollections.List<GameObject> menuItems = __instance.m_CustomXPMenuItemOrder;

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
			private static bool Prefix(ConsoleSlider __instance) {
				return PatchSteplessMovement(__instance, 1f);
			}
		}

		[HarmonyPatch(typeof(ConsoleSlider), "OnDecrease", new Type[0])]
		private static class UpdateOnDecrease {
			private static bool Prefix(ConsoleSlider __instance) {
				return PatchSteplessMovement(__instance, -1f);
			}
		}

		[HarmonyPatch(typeof(UISlider), "OnStart", new Type[0])]
		private static class FixSliderForegroundBarColor {
			private static void Postfix(UISlider __instance) {
				if (__instance.TryCast<UIScrollBar>() || __instance.onDragFinished == null)
					return;

				GameObject foreground = __instance.mFG.gameObject;
				if (!foreground.GetComponent<SliderBarDepthFixer>()) {
					foreground.AddComponent<SliderBarDepthFixer>();
				}
			}
		}

		private static bool PatchSteplessMovement(ConsoleSlider consoleSlider, float direction) {
			UISlider slider = consoleSlider.m_Slider;
			if (!slider.enabled || slider.numberOfSteps >= 2)
				return true; // Run original

			float sliderMoveAmount = direction * MOVEMENT_SPEED * Mathf.Abs(GetRawMenuInputHorizontal());
			slider.value = slider.mValue + sliderMoveAmount;
			if (EventDelegate.IsValid(consoleSlider.onChange)) {
				EventDelegate.Execute(consoleSlider.onChange);
			}
			return false; // Don't run original
		}

		private static void MoveSlider(ConsoleSlider slider, float movement) {
			if (movement < 0f) {
				slider.OnDecrease();
			} else if (movement > 0f) {
				slider.OnIncrease();
			}
		}

		private static float GetTimeredMenuInputHorizontal() {
			return InterfaceManager.GetPanel<Panel_OptionsMenu>().GetGenericSliderMovementHorizontal();
		}

		private static float GetRawMenuInputHorizontal() {
			Panel_OptionsMenu options = InterfaceManager.GetPanel<Panel_OptionsMenu>();
			float origDeadzone = InputSystemRewired.m_MenuNavigationDeadzone;
			InputSystemRewired.m_MenuNavigationDeadzone = MENU_DEADZONE;
			float result = InputManager.GetMenuNavigationPrimary(options).x + InputManager.GetMenuNavigationSecondary(options).x;
			InputSystemRewired.m_MenuNavigationDeadzone = origDeadzone;
			return result;
		}

		internal class SliderBarDepthFixer : MonoBehaviour {

			static SliderBarDepthFixer() {
				ClassInjector.RegisterTypeInIl2Cpp<SliderBarDepthFixer>();
			}
			public SliderBarDepthFixer(IntPtr ptr) : base(ptr) { }

			private const int TARGET_DEPTH = 25;

			private void OnEnable() {
				gameObject.GetComponentInChildren<UIWidget>().depth = TARGET_DEPTH;
			}
		}
	}
}
