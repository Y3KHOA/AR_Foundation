using UnityEditor;
using UnityEngine;

namespace UISwitcher {

	[CustomEditor(typeof(UISwitcher))]
	public class UISwitcherEditor : UINullableToggleEditor {
		private SerializedProperty _tipRect;
		private SerializedProperty _onColor;
		private SerializedProperty _offColor;
		private SerializedProperty _nullColor;
		private SerializedProperty _backgroundGraphic;
		private SerializedProperty _onValueChangedEvent;

		protected override void OnEnable() {
			base.OnEnable();
			_tipRect = serializedObject.FindProperty("tipRect");
			_onColor = serializedObject.FindProperty("onColor");
			_offColor = serializedObject.FindProperty("offColor");
			_nullColor = serializedObject.FindProperty("nullColor");
			_backgroundGraphic = serializedObject.FindProperty("backgroundGraphic");

		}

		public override void OnInspectorGUI() {
			EditorGUILayout.PropertyField(_tipRect);
			EditorGUILayout.PropertyField(_backgroundGraphic);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_onColor);
			EditorGUILayout.PropertyField(_offColor);
			EditorGUILayout.PropertyField(_nullColor);
			//	EditorGUILayout.PropertyField(_onValueChangedEvent, true);
			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();

			DrawUILine(Color.black);
		}

		private static void DrawUILine(Color color, int thickness = 1, int padding = 10) {
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}
	}
}