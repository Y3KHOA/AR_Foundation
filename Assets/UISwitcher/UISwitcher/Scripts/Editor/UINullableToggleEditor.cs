using UnityEditor;
using UnityEngine;

namespace UISwitcher {
	[CustomEditor(typeof(UINullableToggle))]
	public class UINullableToggleEditor : UnityEditor.UI.SelectableEditor {
		private SerializedProperty _isOnNullable, _nullableValueEnabled, _onValueChangedEvent, _onNullableValueChangedEvent;
		private UINullableToggle _nullableToggle;

		private const string MIXED_FIELD_NAME = "isOn";
		private const string NULLABLE_VALUES_ENABLED_FIELD_NAME = "NullableValueEnabled";
		protected override void OnEnable() {
			base.OnEnable();
			_isOnNullable = serializedObject.FindProperty("m_isOnNullable");
			_nullableValueEnabled = serializedObject.FindProperty("m_nullValueEnabled");
			_nullableToggle = serializedObject.targetObject as UINullableToggle;
			_onValueChangedEvent = serializedObject.FindProperty("onValueChanged");
			_onNullableValueChangedEvent = serializedObject.FindProperty("onValueChangedNullable");
		}

		public override void OnInspectorGUI() {
			EditorGUI.BeginChangeCheck();
			_nullableValueEnabled.boolValue = EditorGUILayout.Toggle(NULLABLE_VALUES_ENABLED_FIELD_NAME, _nullableValueEnabled.boolValue);

			if (_nullableValueEnabled.boolValue)
				MoveBetweenTrueFalseNullable();
			else
				MoveBetweenTrueFalse();

			EditorGUILayout.PropertyField(_onValueChangedEvent, true);
			EditorGUILayout.PropertyField(_onNullableValueChangedEvent, true);
			serializedObject.ApplyModifiedProperties();

			_nullableToggle.OnChanged( /*_isOnNullable.intValue*/);

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(_nullableToggle);

			DrawUILine(Color.black);
			base.OnInspectorGUI();
		}

		private void MoveBetweenTrueFalse() {
			if (_isOnNullable.intValue > 0) {
				var value = EditorGUILayout.Toggle(MIXED_FIELD_NAME, true);
				if (!value)
					_isOnNullable.intValue = 0;

			}
			else {
				var value = EditorGUILayout.Toggle(MIXED_FIELD_NAME, false);
				if (value)
					_isOnNullable.intValue = 1;
				else
					_isOnNullable.intValue = 0;
			}

		}
		private void MoveBetweenTrueFalseNullable() {
			switch (_isOnNullable.intValue) {
				case < 0: {
					EditorGUI.showMixedValue = true;
					var value = EditorGUILayout.Toggle(MIXED_FIELD_NAME, false);
					if (value)
						_isOnNullable.intValue = 1;

					EditorGUI.showMixedValue = false;
					break;
				}
				case > 0: {
					var value = EditorGUILayout.Toggle(MIXED_FIELD_NAME, true);
					if (!value)
						_isOnNullable.intValue = 0;
					break;
				}
				case 0: {
					var value = EditorGUILayout.Toggle(MIXED_FIELD_NAME, false);
					if (value)
						_isOnNullable.intValue = -1;

					break;
				}
			}
		}

		private static void DrawUILine(Color color, int thickness = 1, int padding = 10) {
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			rect.height = thickness;
			rect.y += padding / 2;
			rect.x -= 2;
			rect.width += 6;
			EditorGUI.DrawRect(rect, color);
		}
	}
}

/*if (!value.HasValue) {
	EditorGUI.showMixedValue = true;

	val = EditorGUILayout.Toggle(MIXED_FIELD_NAME, false);
	if (val)
		value = false;

	EditorGUI.showMixedValue = false;
}
else {
	if (value.Value) {
		val = EditorGUILayout.Toggle(MIXED_FIELD_NAME, true);
		value = !val ? null : val;
	}
	else {
		val = EditorGUILayout.Toggle(MIXED_FIELD_NAME, false);
		value = val;
	}
}*/
//_isOnNullable.intValue = UINullableToggleUtils.NullableBoolToInt(value);