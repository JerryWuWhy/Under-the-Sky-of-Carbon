using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Ux.Editor {
	public abstract class ReorderableListEditor : UnityEditor.Editor {
		protected virtual string PropertyPath => throw new NotImplementedException();
		protected virtual List<Type> PropertyTypes => throw new NotImplementedException();

		protected ReorderableList List { get; private set; }
		protected SerializedProperty ListProperty { get; private set; }

		protected virtual void OnEnable() {
			ListProperty = serializedObject.FindProperty(PropertyPath);
			List = new ReorderableList(serializedObject, ListProperty, true, true, true, true) {
				drawHeaderCallback = DrawListHeader,
				drawElementCallback = DrawListElement,
				elementHeightCallback = GetElementHeight,
				onAddDropdownCallback = AddDropdown,
			};
		}

		protected virtual void DrawListHeader(Rect rect) {
			EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(ListProperty.name));
		}

		protected virtual void DrawListElement(Rect rect, int index, bool isActive, bool isFocused) {
			var property = ListProperty.GetArrayElementAtIndex(index);
			var enabledProperty = property.FindPropertyRelative("enabled");
			var labelProperty = property.FindPropertyRelative("label");
			var titleRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			var toggleRect = new Rect(rect.x + rect.width - EditorGUIUtility.singleLineHeight, rect.y,
				EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
			if (Event.current.type == EventType.ContextClick && titleRect.Contains(Event.current.mousePosition)) {
				ShowContextMenu(index);
				Event.current.Use();
			}
			if (enabledProperty != null) {
				EditorGUI.PropertyField(toggleRect, enabledProperty, GUIContent.none);
			}
			rect.xMin = EditorGUIUtility.fieldWidth;
			GUI.enabled = enabledProperty == null || enabledProperty.boolValue;
			EditorGUI.PropertyField(rect, property,
				new GUIContent(property.hasMultipleDifferentValues
					? "{Mixed Values}"
					: labelProperty?.stringValue ?? $"Element {index}"),
				property.isExpanded);
			GUI.enabled = true;
		}

		protected virtual float GetElementHeight(int index) {
			var property = ListProperty.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(property);
		}

		protected virtual void ShowContextMenu(int index) {
			var menu = new GenericMenu();
			var property = ListProperty.GetArrayElementAtIndex(index);
			if (property.prefabOverride) {
				var method = typeof(PrefabUtility).GetMethod("HandleApplyRevertMenuItems",
					BindingFlags.NonPublic | BindingFlags.Static);
				var delegateType = Type.GetType("UnityEditor.AddApplyMenuItemDelegate, UnityEditor");
				var applyAction = (Action<GUIContent, Object, Object>) ((menuItemContent, sourceObject, _) => {
					menu.AddItem(menuItemContent, false, () => {
						PrefabUtility.ApplyPropertyOverride(
							ListProperty.GetArrayElementAtIndex(index),
							AssetDatabase.GetAssetPath(sourceObject),
							InteractionMode.UserAction);
					});
				});
				var applyDelegate = Delegate.CreateDelegate(delegateType!, applyAction.Target, applyAction.Method);
				var revertAction = (Action<GUIContent>) (menuItemContent => {
					menu.AddItem(menuItemContent, false, () => {
						PrefabUtility.RevertPropertyOverride(
							ListProperty.GetArrayElementAtIndex(index),
							InteractionMode.UserAction);
					});
				});
				method?.Invoke(null, new object[] {
					null, target, applyDelegate, revertAction, false, serializedObject.targetObjects.Length
				});
			}
			if (menu.GetItemCount() > 0) {
				menu.AddSeparator("");
			}
			menu.AddItem(new GUIContent("Copy"), false, () => {
				serializedObject.Update();
				var json = JsonUtility.ToJson(property.managedReferenceValue, true);
				EditorGUIUtility.systemCopyBuffer = json;
				serializedObject.ApplyModifiedProperties();
			});
			menu.AddItem(new GUIContent("Paste"), false, () => {
				serializedObject.Update();
				JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, property.managedReferenceValue);
				serializedObject.ApplyModifiedProperties();
			});
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Duplicate"), false, () => {
				serializedObject.Update();
				var obj = property.managedReferenceValue;
				var json = JsonUtility.ToJson(property.managedReferenceValue, true);
				ListProperty.InsertArrayElementAtIndex(index + 1);
				ListProperty.GetArrayElementAtIndex(index + 1).managedReferenceValue =
					JsonUtility.FromJson(json, obj.GetType());
				serializedObject.ApplyModifiedProperties();
			});
			menu.AddItem(new GUIContent("Delete"), false, () => {
				serializedObject.Update();
				ListProperty.DeleteArrayElementAtIndex(index);
				serializedObject.ApplyModifiedProperties();
			});
			menu.ShowAsContext();
		}

		private void AddDropdown(Rect buttonRect, ReorderableList list) {
			var menu = new GenericMenu();
			foreach (var type in PropertyTypes) {
				var name = GetTypeName(type);
				menu.AddItem(new GUIContent(name), false, () => {
					serializedObject.Update();
					var obj = Activator.CreateInstance(type);
					var index = ListProperty.arraySize;
					ListProperty.InsertArrayElementAtIndex(index);
					var newItem = ListProperty.GetArrayElementAtIndex(index);
					newItem.managedReferenceValue = obj;
					var labelProperty = newItem.FindPropertyRelative("label");
					if (labelProperty != null && labelProperty.propertyType == SerializedPropertyType.String) {
						labelProperty.stringValue = name;
					}
					serializedObject.ApplyModifiedProperties();
				});
			}
			menu.ShowAsContext();
		}

		public virtual string GetTypeName(Type type) {
			return ObjectNames.NicifyVariableName(type.Name);
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			var expanded = true;
			var property = serializedObject.GetIterator();
			while (property.NextVisible(expanded)) {
				expanded = false;
				if (property.name == "m_Script") {
					GUI.enabled = false;
					EditorGUILayout.PropertyField(property, true);
					GUI.enabled = true;
				} else if (property.propertyPath == ListProperty.propertyPath) {
					EditorGUILayout.Space();
					List.DoLayoutList();
				} else {
					EditorGUILayout.PropertyField(property, true);
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}