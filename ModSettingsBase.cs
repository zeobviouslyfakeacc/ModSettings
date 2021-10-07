using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModSettings {
	public abstract class ModSettingsBase {

		internal readonly FieldInfo[] fields;
		internal readonly Dictionary<FieldInfo, object> confirmedValues;
		private readonly List<Action> refreshActions;

		private readonly Visibility menuVisibility;
		private readonly Visibility visibility;
		private readonly Dictionary<FieldInfo, Visibility> fieldVisibilities;

		protected ModSettingsBase() {
			fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			confirmedValues = new Dictionary<FieldInfo, object>(fields.Length);
			refreshActions = new List<Action>();

			menuVisibility = new Visibility();
			menuVisibility.SetVisible(false);
			menuVisibility.AddVisibilityListener((visible) => {
				// Reset public fields to last confirmed values when leaving settings menu
				if (visible) {
					foreach (FieldInfo field in fields) {
						confirmedValues[field] = field.GetValue(this);
					}
					RefreshGUI();
				} else {
					foreach (FieldInfo field in fields) {
						field.SetValue(this, confirmedValues[field]);
					}
				}
			});

			visibility = new Visibility(menuVisibility);
			fieldVisibilities = new Dictionary<FieldInfo, Visibility>(fields.Length);
			foreach (FieldInfo field in fields) {
				fieldVisibilities.Add(field, new Visibility(visibility));
			}

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

		public void RefreshGUI() {
			if (!menuVisibility.IsVisible())
				return;

			foreach (Action refreshAction in refreshActions) {
				refreshAction();
			}
		}

		internal void SetMenuVisible(bool visible) {
			menuVisibility.SetVisible(visible);
		}

		public bool IsVisible() {
			return visibility.IsVisible();
		}

		internal bool IsUserVisible() {
			return visibility.IsSelfVisible();
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

		internal void SetFieldValue(FieldInfo field, object newValue) {
			object oldValue = field.GetValue(this);
			if (oldValue != newValue) {
				field.SetValue(this, newValue);
				CallOnChange(field, oldValue, newValue);
			}
		}

		protected virtual void OnConfirm() { }

		protected virtual void OnChange(FieldInfo field, object oldValue, object newValue) { }

		internal void CallOnConfirm() {
			try {
				OnConfirm();
			} catch (Exception e) {
				MelonLoader.MelonLogger.Error("Exception in OnConfirm handler\n" + e.ToString());
			}

			foreach (FieldInfo field in fields) {
				confirmedValues[field] = field.GetValue(this);
			}
		}

		internal void CallOnChange(FieldInfo field, object oldValue, object newValue) {
			try {
				OnChange(field, oldValue, newValue);
			} catch (Exception e) {
				MelonLoader.MelonLogger.Error("Exception in OnChange handler\n" + e.ToString());
			}
		}

		internal delegate void OnVisibilityChange(bool visible);

		internal void AddRefreshAction(Action onRefresh) {
			refreshActions.Add(onRefresh);
		}

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
				throw new ArgumentException("Field name must be a non-empty string", "fieldName");
			}

			FieldInfo field = GetType().GetField(fieldName);
			if (field == null) {
				throw new ArgumentException("Could not find field with name " + fieldName, "fieldName");
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
			private readonly List<Visibility> children = new List<Visibility>();
			private bool parentVisible = true;
			private bool visible = true;

			internal Visibility() { }

			internal Visibility(Visibility parent) {
				parent.children.Add(this);
				SetParentVisible(parent.IsVisible());
			}

			internal void AddChild(Visibility child) {
				children.Add(child);
				child.SetParentVisible(IsVisible());
			}

			internal bool IsSelfVisible() {
				return visible;
			}

			internal bool IsVisible() {
				return parentVisible && visible;
			}

			private void SetParentVisible(bool parentVisible) {
				if (this.parentVisible == parentVisible) {
					return;
				}

				this.parentVisible = parentVisible;
				if (visible) {
					ChangeVisibility(parentVisible);
				}
			}

			internal void SetVisible(bool visible) {
				if (this.visible == visible) {
					return;
				}

				this.visible = visible;
				if (parentVisible) {
					ChangeVisibility(visible);
				}
			}

			private void ChangeVisibility(bool visible) {
				foreach (Visibility child in children) {
					child.SetParentVisible(visible);
				}

				foreach (OnVisibilityChange listener in visibilityListeners) {
					listener.Invoke(visible);
				}
			}

			internal void AddVisibilityListener(OnVisibilityChange listener) {
				visibilityListeners.Add(listener);
			}
		}
	}
}
