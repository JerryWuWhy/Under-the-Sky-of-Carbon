using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Scripts.Ux.Editor {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(FeedbackPlayer), true)]
	public class FeedbackPlayerEditor : ReorderableListEditor {
		protected override string PropertyPath => "feedbacks";
		protected override List<Type> PropertyTypes => new() {
			typeof(ScaleFeedback),
			typeof(PositionFeedback),
			typeof(RotationFeedback),
			typeof(AnchoredPositionFeedback),
			typeof(TargetPositionFeedback),
			typeof(SoundFeedback),
			typeof(VibrationFeedback),
			typeof(ContinuousVibrationFeedback),
			typeof(TimeScaleFeedback),
			typeof(AnimatorFeedback),
			typeof(AnimationFeedback),
			typeof(CanvasGroupFeedback),
			typeof(ImageFeedback),
			typeof(TextFeedback),
			typeof(TextMeshFeedback),
			typeof(SpriteRendererFeedback),
			typeof(ParticleSystemFeedback),
			typeof(GameObjectFeedback),
			typeof(ShaderFloatFeedback),
			typeof(ShaderColorFeedback),
			typeof(SubPlayerFeedback),
			typeof(EventFeedback),
			typeof(ContinuousEventFeedback)
		};

		private FeedbackPlayer Player => (FeedbackPlayer) target;

		protected override void DrawListElement(Rect rect, int index, bool isActive, bool isFocused) {
			var property = List.serializedProperty.GetArrayElementAtIndex(index);
			var enabledProperty = property.FindPropertyRelative("enabled");
			var delayProperty = property.FindPropertyRelative("delay");
			var infoRect = new Rect(rect.x, rect.y,
				rect.width - EditorGUIUtility.singleLineHeight - 5f, EditorGUIUtility.singleLineHeight);
			var infoStyle = new GUIStyle {alignment = TextAnchor.MiddleRight, normal = {textColor = Color.gray}};
			EditorGUI.LabelField(infoRect, $"{delayProperty?.floatValue:F2}s", infoStyle);
			base.DrawListElement(rect, index, isActive, isFocused);
			if (property.isExpanded) {
				var buttonRect = new Rect(rect.x + rect.width - 80f,
					rect.y + rect.height - EditorGUIUtility.standardVerticalSpacing - EditorGUIUtility.singleLineHeight,
					80f, EditorGUIUtility.singleLineHeight);
				GUI.enabled = enabledProperty?.boolValue == true && IsPlayable();
				if (GUI.Button(buttonRect, "Solo")) {
					StartPreview();
					Player.Solo(index);
				}
			}
			GUI.enabled = true;
		}

		protected override float GetElementHeight(int index) {
			var property = List.serializedProperty.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(property) +
			       (property.isExpanded ? EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight : 0);
		}

		public override string GetTypeName(Type type) {
			return base.GetTypeName(type).Replace(" Feedback", "");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			GUILayout.Space(10f);

			GUI.enabled = IsPlayable();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Play")) {
				StartPreview();
				Player.Play();
			}

			if (GUILayout.Button("Pause")) {
				StartPreview();
				Player.Pause();
			}

			if (GUILayout.Button("Resume")) {
				StartPreview();
				Player.Resume();
			}

			if (GUILayout.Button("Rewind")) {
				StartPreview();
				Player.Rewind();
			}

			if (GUILayout.Button("End")) {
				StartPreview();
				Player.End();
			}

			if (GUILayout.Button("Reset")) {
				Player.Reset();
			}
			GUILayout.EndHorizontal();
			GUI.enabled = true;
		}

		private void StartPreview() {
			if (!Application.isPlaying) {
				Player.StartPreview();
			}
		}

		private void OnDisable() {
			Player.StopPreview();
		}

		private bool IsPlayable() {
			return !Application.isPlaying || Application.isPlaying
				&& PrefabStageUtility.GetPrefabStage(Player.gameObject) == null
				&& !PrefabUtility.IsPartOfPrefabAsset(Player)
				&& !PrefabUtility.IsPartOfAnyPrefab(Player);
		}
	}
}