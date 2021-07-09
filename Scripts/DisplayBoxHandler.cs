using System;
using System.Reflection;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ModSettings.Scripts {
	internal class DisplayBoxHandler : MonoBehaviour {
		static DisplayBoxHandler() => UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<DisplayBoxHandler>();

		private UILabel uiLabel;
		private FieldInfo fieldInfo;
		private ModSettingsBase modSettings;

		public DisplayBoxHandler(IntPtr intPtr) : base(intPtr) { }

		[HideFromIl2Cpp]
		internal void UpdateLabel() {
			uiLabel.text = Convert.ToString(fieldInfo.GetValue(modSettings)) ?? "";
		}

		[HideFromIl2Cpp]
		internal void Initialize(ModSettingsBase modSettings, FieldInfo fieldInfo, UILabel uiLabel) {
			this.modSettings = modSettings;
			this.fieldInfo = fieldInfo;
			this.uiLabel = uiLabel;
		}
	}
}
