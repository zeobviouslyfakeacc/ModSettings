using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModSettings {
	public abstract class ModSettingsBase {

		private readonly FieldInfo[] fields;
		private readonly Visibility visibility;
		private readonly Dictionary<FieldInfo, Visibility> fieldVisibilities;

		protected ModSettingsBase() {
			fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

			fieldVisibilities = new Dictionary<FieldInfo, Visibility>(fields.Length);
			foreach (FieldInfo field in fields) {
				fieldVisibilities.Add(field, new Visibility());
			}

			visibility = new Visibility();
			visibility.AddVisibilityListener((visible) => {
				foreach (Visibility visibility in fieldVisibilities.Values) {
					visibility.SetParentVisible(visible);
				}
			});
			visibility.SetParentVisible(false);

			Attributes.ValidateFields(this);
		}

		public void AddToCustomModeMenu(Position position) {
			CustomModeMenu.RegisterSettings(this, position);
		}

		public void AddToModSettings(string modName) {
			AddToModSettings(modName, MenuType.Both);
		}

		public void AddToModSettings(string modName, MenuType menuType) {
			ModSettingsMenu.RegisterSettings(this, modName, menuType);
		}

		public bool IsVisible() {
			return visibility.IsVisible();
		}

		internal void OverrideVisible(bool visible) {
			visibility.SetParentVisible(visible);
		}

		public void SetVisible(bool visible) {
			visibility.SetVisible(visible);
		}

		public bool IsFieldVisible(string fieldName) {
			FieldInfo field = GetFieldForName(fieldName);
			return IsFieldVisible(field);
		}

		public bool IsFieldVisible(FieldInfo field) {
			return GetFieldVisibility(field).IsVisible();
		}

		public void SetFieldVisible(string fieldName, bool visible) {
			FieldInfo field = GetFieldForName(fieldName);
			SetFieldVisible(field, visible);
		}

		public void SetFieldVisible(FieldInfo field, bool visible) {
			GetFieldVisibility(field).SetVisible(visible);
		}

		protected void RequiresConfirmation() {
			InterfaceManager.m_Panel_OptionsMenu.SettingsNeedConfirmation();
		}

		protected virtual void OnConfirm() { }

		protected virtual void OnChange(FieldInfo field, object oldValue, object newValue) { }

		internal void CallOnChange(FieldInfo field, object oldValue, object newValue) {
			try {
				OnChange(field, oldValue, newValue);
			} catch (Exception e) {
				UnityEngine.Debug.LogError("[ModSettings] Exception in OnChange handler");
				UnityEngine.Debug.LogException(e);
			}
		}

		internal void CallOnConfirm() {
			try {
				OnConfirm();
			} catch (Exception e) {
				UnityEngine.Debug.LogError("[ModSettings] Exception in OnConfirm handler");
				UnityEngine.Debug.LogException(e);
			}
		}

		internal delegate void OnVisibilityChange(bool visible);

		internal void AddVisibilityListener(OnVisibilityChange listener) {
			visibility.AddVisibilityListener(listener);
		}

		internal void AddVisibilityListener(FieldInfo field, OnVisibilityChange listener) {
			GetFieldVisibility(field).AddVisibilityListener(listener);
		}

		internal FieldInfo[] GetFields() {
			return fields;
		}

		private FieldInfo GetFieldForName(string fieldName) {
			if (string.IsNullOrEmpty(fieldName)) {
				throw new ArgumentException("[ModSettings] Field name must be a non-empty string", "fieldName");
			}

			FieldInfo field = GetType().GetField(fieldName);
			if (field == null) {
				throw new ArgumentException("[ModSettings] Could not find field with name " + fieldName, "fieldName");
			}
			return field;
		}

		private Visibility GetFieldVisibility(FieldInfo field) {
			if (field == null) {
				throw new ArgumentNullException("field");
			}
			if (fieldVisibilities.TryGetValue(field, out Visibility visibility)) {
				return visibility;
			} else {
				throw new ArgumentException("Field " + field.Name + " not part of class " + GetType().Name, "field");
			}
		}

		private class Visibility {

			private readonly List<OnVisibilityChange> visibilityListeners = new List<OnVisibilityChange>();
			private bool parentVisible = true;
			private bool visible = true;

			internal bool IsVisible() {
				return parentVisible && visible;
			}

			internal void SetParentVisible(bool parentVisible) {
				if (this.parentVisible == parentVisible) {
					return;
				}

				this.parentVisible = parentVisible;
				if (visible) {
					foreach (OnVisibilityChange listener in visibilityListeners) {
						listener.Invoke(parentVisible);
					}
				}
			}

			internal void SetVisible(bool visible) {
				if (this.visible == visible) {
					return;
				}

				this.visible = visible;
				if (parentVisible) {
					foreach (OnVisibilityChange listener in visibilityListeners) {
						listener.Invoke(visible);
					}
				}
			}

			internal void AddVisibilityListener(OnVisibilityChange listener) {
				visibilityListeners.Add(listener);
			}
		}
	}
}
