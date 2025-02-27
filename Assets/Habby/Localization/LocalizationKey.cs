using UnityEngine;
using UnityEditor;

namespace Habby.Localization {
	public class LocalizationKeyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
	public class LocalizationKeyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
			var contentColor = GUI.contentColor;
			var backgroundColor = GUI.backgroundColor;
			var propertyRect = new Rect(rect.x, rect.y, rect.width - 20f, rect.height);
			if (property.propertyType == SerializedPropertyType.String &&
			    !string.IsNullOrEmpty(property.stringValue) &&
			    !Localization.HasKey(property.stringValue)) {
				GUI.contentColor = GUI.backgroundColor = Color.red;
			}
			label.tooltip = Localization.GetString(property.stringValue);
			EditorGUI.PropertyField(propertyRect, property, label, true);
			GUI.contentColor = contentColor;
			GUI.backgroundColor = backgroundColor;
			var dropdownRect = new Rect(rect.xMax - 20f, rect.y, 20f, rect.height);
			if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(), FocusType.Passive)) {
				var menu = new GenericMenu();
				var keys = Localization.GetAllKeys();
				foreach (var key in keys) {
					menu.AddItem(new GUIContent(key), property.stringValue == key, () => {
						property.serializedObject.Update();
						property.stringValue = key;
						property.serializedObject.ApplyModifiedProperties();
					});
				}
				menu.ShowAsContext();
			}
		}
	}
#endif
}