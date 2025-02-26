using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Ux {
	public class RequiredAttribute : PropertyAttribute { }

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(RequiredAttribute))]
	public class RequiredDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var contentColor = GUI.contentColor;
			var backgroundColor = GUI.backgroundColor;
			if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null ||
			    property.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(property.stringValue)) {
				GUI.contentColor = GUI.backgroundColor = Color.red;
			}
			EditorGUI.PropertyField(position, property, label, true);
			GUI.contentColor = contentColor;
			GUI.backgroundColor = backgroundColor;
		}
	}
#endif
}