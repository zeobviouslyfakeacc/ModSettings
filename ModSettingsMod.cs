using MelonLoader;

namespace ModSettings {
	internal class ModSettingsMod : MelonMod {

		public override void OnApplicationStart() {
#if DEBUG
			ModSettingsExample.BasicExample.OnLoad();
			ModSettingsExample.OnChangeExample.OnLoad();
			ModSettingsExample.VisibilityExample.OnLoad();
#endif
		}
	}
}
