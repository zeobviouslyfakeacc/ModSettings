using UnityEngine;
using UnhollowerBaseLib.Attributes;

namespace ModSettings {
	internal class CustomKeybinding : MonoBehaviour {
		static CustomKeybinding() => UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<CustomKeybinding>();
		public CustomKeybinding(System.IntPtr intPtr) : base(intPtr) { }

		internal KeyCode currentKeycodeSetting = KeyCode.None;
		internal KeyRebindingButton keyRebindingButton;
		private bool searchingForKey = false;
		internal System.Action OnChange;

		public void Update() {
			if (searchingForKey) MaybeUpdateKey();
			if (this.gameObject.activeSelf && !searchingForKey && keyRebindingButton.m_ValueLabel.text != currentKeycodeSetting.ToString()) {
				RefreshLabelValue();
			}
		}

		[HideFromIl2Cpp]
		internal void OnClick() {
			searchingForKey = true;
			keyRebindingButton.SetSelected(true);
			keyRebindingButton.SetValueLabel(string.Empty);
		}

		[HideFromIl2Cpp]
		internal void RefreshLabelValue() {
			keyRebindingButton.SetValueLabel(currentKeycodeSetting.ToString());
		}

		[HideFromIl2Cpp]
		private void MaybeUpdateKey() {
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Backspace))
			{
				currentKeycodeSetting = KeyCode.None;
				searchingForKey = false;
				keyRebindingButton.SetSelected(false);
				keyRebindingButton.SetValueLabel(KeyCode.None.ToString());
				OnChange.Invoke();
				return;
			}
			foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, kcode)) {
					currentKeycodeSetting = kcode;
					searchingForKey = false;
					keyRebindingButton.SetSelected(false);
					keyRebindingButton.SetValueLabel(kcode.ToString());
					OnChange.Invoke();
					return;
				}
			}
		}
	}
}
