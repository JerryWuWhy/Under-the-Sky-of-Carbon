using UnityEditor;
using UnityEngine;

namespace Habby.Localization {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LocalizedAdapter))]
	public class LocalizedAdapterEditor : Editor {
		private SerializedProperty _keyProperty;
		private SerializedProperty _argumentsProperty;
		private SerializedProperty _localizedProperty;
		private SerializedProperty _textCaseProperty;
		private SerializedProperty _removeRichTagsProperty;
		private SerializedProperty _textProperty;
		private SerializedProperty _onChangeProperty;

		private void OnEnable() {
			_keyProperty = serializedObject.FindProperty("key");
			_argumentsProperty = serializedObject.FindProperty("arguments");
			_localizedProperty = serializedObject.FindProperty("localized");
			_textProperty = serializedObject.FindProperty("text");
			_textCaseProperty = serializedObject.FindProperty("textCase");
			_removeRichTagsProperty = serializedObject.FindProperty("removeRichTags");
			_onChangeProperty = serializedObject.FindProperty("onChange");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			GUI.enabled = _localizedProperty.boolValue;
			EditorGUILayout.PropertyField(_keyProperty);
			EditorGUILayout.PropertyField(_argumentsProperty);
			GUI.enabled = true;
			GUI.enabled = !_localizedProperty.boolValue;
			EditorGUILayout.PropertyField(_textProperty);
			GUI.enabled = true;
			EditorGUILayout.PropertyField(_localizedProperty);
			EditorGUILayout.PropertyField(_textCaseProperty);
			EditorGUILayout.PropertyField(_removeRichTagsProperty);
			EditorGUILayout.PropertyField(_onChangeProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}