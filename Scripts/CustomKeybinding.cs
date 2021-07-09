using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ModSettings.Scripts {
	internal class CustomKeybinding : MonoBehaviour {
		static CustomKeybinding() => UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<CustomKeybinding>();
		public CustomKeybinding(System.IntPtr intPtr) : base(intPtr) { }

		internal KeyCode currentKeycodeSetting = KeyCode.None;
		internal KeyRebindingButton keyRebindingButton;
		internal System.Action OnChange;
		private bool searchingForKey = false;
		private bool ignoreNextOnClick = false;

		public void Update() {
			if (searchingForKey) MaybeUpdateKey();
			if (gameObject.activeSelf && !searchingForKey && keyRebindingButton.m_ValueLabel.text != currentKeycodeSetting.ToString()) {
				RefreshLabelValue();
			}
		}

		[HideFromIl2Cpp]
		internal void OnClick() {
			if (ignoreNextOnClick) {
				ignoreNextOnClick = false;
				return;
			}

			searchingForKey = true;
			InputManager.PushContext(this);
			ModSettingsMenu.disableMovementInput = true;
			keyRebindingButton.SetSelected(true);
			keyRebindingButton.SetValueLabel(string.Empty);
			GameAudioManager.PlayGUIButtonClick();
		}

		[HideFromIl2Cpp]
		internal void RefreshLabelValue() {
			keyRebindingButton.SetValueLabel(currentKeycodeSetting.ToString());
		}

		[HideFromIl2Cpp]
		private void MaybeUpdateKey() {
			KeyCode? pressedKey = GetPressedKeyCode();
			if (pressedKey == null) return;

			currentKeycodeSetting = pressedKey.Value;
			searchingForKey = false;
			InputManager.PopContext(this);
			ModSettingsMenu.disableMovementInput = false;
			ignoreNextOnClick = (currentKeycodeSetting >= KeyCode.Mouse0 && currentKeycodeSetting <= KeyCode.Mouse6);
			keyRebindingButton.SetSelected(false);
			keyRebindingButton.SetValueLabel(pressedKey.ToString());
			OnChange.Invoke();
		}

		[HideFromIl2Cpp]
		private KeyCode? GetPressedKeyCode() {
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Backspace)) {
				return KeyCode.None;
			}
			foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) {
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, keyCode)) {
					return keyCode;
				}
			}
			return null;
		}
	}
}
