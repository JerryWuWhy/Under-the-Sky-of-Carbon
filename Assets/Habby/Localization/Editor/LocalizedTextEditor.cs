using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TextEditor = UnityEditor.UI.TextEditor;

namespace Habby.Localization {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LocalizedText))]
	public class LocalizedTextEditor : TextEditor {
		private SerializedProperty _keyProperty;
		private SerializedProperty _argumentsProperty;
		private SerializedProperty _localizedProperty;
		private SerializedProperty _textCaseProperty;
		private SerializedProperty _textProperty;
		private SerializedProperty _fontProperty;

		protected override void OnEnable() {
			base.OnEnable();
			_keyProperty = serializedObject.FindProperty("key");
			_argumentsProperty = serializedObject.FindProperty("arguments");
			_localizedProperty = serializedObject.FindProperty("localized");
			_textCaseProperty = serializedObject.FindProperty("textCase");
			_textProperty = serializedObject.FindProperty("m_Text");
			_fontProperty = serializedObject.FindProperty("m_FontData");
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
			EditorGUILayout.PropertyField(_fontProperty);

			AppearanceControlsGUI();
			RaycastControlsGUI();
			MaskableControlsGUI();
			serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("CONTEXT/Text/Localize", true)]
		private static bool LocalizeTextValidate(MenuCommand command) {
			return command.context is not LocalizedText;
		}

		private static TextAsset GetScriptFile([CallerFilePath] string filepath = "") {
			filepath = filepath
				.Replace(Application.dataPath, "Assets")
				.Replace("Editor/LocalizedTextEditor", "LocalizedText");
			return AssetDatabase.LoadAssetAtPath<TextAsset>(filepath);
		}

		[MenuItem("CONTEXT/Text/Localize")]
		private static void LocalizeText(MenuCommand command) {
			var text = (Text) command.context;
			var go = text.gameObject;
			var so = new SerializedObject(text);
			var scriptProperty = so.FindProperty("m_Script");
			Undo.RegisterCompleteObjectUndo(go, "Localize Text");
			so.Update();
			scriptProperty.objectReferenceValue = GetScriptFile();
			so.ApplyModifiedProperties();
		}
	}
}