using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Ux {
	public class FeedbackPlayer : MonoBehaviour {
		public bool playOnActive;
		public bool resetOnInactive;
		public bool loop;
		public bool ignoreTimeScale;
		public float initialDelay;
		public float loopDelay;
		public float speed = 1f;
		[SerializeReference] public List<Feedback> feedbacks = new();

		private float _time;
		private int _soloIndex;
		private Action _callback;

		public bool Playing { get; private set; }
		public float ActualDuration { get; private set; }
		public float TotalTime { get; private set; }
		public float CurrentTime => Mathf.Clamp(_time, 0f, TotalTime);

		public void OnEnable() {
			if (playOnActive) {
				Play();
			}
		}

		public void OnDisable() {
			if (resetOnInactive) {
				Reset();
			}
		}

		private void Update() {
			Update(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
		}

		private void Update(float deltaTime) {
			if (!Playing) {
				return;
			}
			var ended = true;
			ActualDuration = 0f;
			TotalTime = 0f;
			for (var i = 0; i < feedbacks.Count; ++i) {
				var feedback = feedbacks[i];
				if (!feedback.enabled) {
					continue;
				}
				if (_soloIndex >= 0 && _soloIndex != i) {
					continue;
				}
				var feedbackTime = _soloIndex < 0 ? _time - feedback.delay : _time;
				if (feedbackTime >= 0f && !feedback.Started) {
					feedback.Start(speed);
				}
				if (feedback.Started && !feedback.Ended) {
					feedback.Update(feedbackTime);
				}
				if (!feedback.Ended && feedbackTime >= feedback.duration) {
					feedback.End();
				}
				ended = ended && feedback.Ended;
				ActualDuration = Mathf.Max(ActualDuration, (feedback.delay + feedback.duration) / speed);
				TotalTime = Mathf.Max(TotalTime, feedback.delay + feedback.duration);
			}
			_time += deltaTime * speed;
			if (ended) {
				if (loop && _soloIndex < 0 && ActualDuration > 0f) {
					Play(-loopDelay);
				} else {
					Playing = false;
					_callback?.Invoke();
				}
			}
		}

		public void Solo(int index) {
			Reset();
			Playing = true;
			_soloIndex = index;
			Update(0f);
		}

		public void Play(Action callback = null) {
			Play(-initialDelay, callback);
		}

		public void Play(float time, Action callback = null) {
			Reset();
			Playing = true;
			_time = time;
			_soloIndex = -1;
			_callback = callback;
			Update(0f);
		}

		public void Pause() {
			if (!Playing) {
				return;
			}
			Playing = false;
			foreach (var feedback in feedbacks) {
				if (feedback.Started) {
					feedback.Pause();
				}
			}
		}

		public void Resume() {
			if (Playing) {
				return;
			}
			Playing = true;
			foreach (var feedback in feedbacks) {
				if (feedback.Started) {
					feedback.Resume();
				}
			}
		}

		public void Rewind() {
			Play(0f);
			Pause();
		}

		public void End() {
			Pause();
			foreach (var feedback in feedbacks) {
				if (!feedback.Started) {
					feedback.Start(speed);
				}
				if (feedback.Started && !feedback.Ended) {
					feedback.Update(1f);
				}
				if (!feedback.Ended) {
					feedback.End();
				}
			}
		}

		public void Reset() {
			_time = 0f;
			Playing = false;
			for (var i = feedbacks.Count - 1; i >= 0; --i) {
				var feedback = feedbacks[i];
				feedback.Reset();
			}
		}

		public void ResetState() {
			Pause();
			_time = 0f;
			for (var i = feedbacks.Count - 1; i >= 0; --i) {
				var feedback = feedbacks[i];
				feedback.ResetState();
			}
		}

		public T GetFeedback<T>() where T : Feedback {
			foreach (var feedback in feedbacks) {
				if (feedback is T t) {
					return t;
				}
			}
			return null;
		}

		public T GetFeedback<T>(string label) where T : Feedback {
			foreach (var feedback in feedbacks) {
				if (feedback is T t && t.label == label) {
					return t;
				}
			}
			return null;
		}

		public T[] GetFeedbacks<T>() where T : Feedback {
			return feedbacks.Where(feedback => feedback is T).Cast<T>().ToArray();
		}

#if UNITY_EDITOR
		private float _editorTime;

		public void StartPreview() {
			foreach (var feedback in feedbacks) {
				feedback?.StartPreview();
			}
			if (!IsActionAttached(OnEditorUpdate)) {
				EditorApplication.update += OnEditorUpdate;
			}
			_editorTime = 0f;
		}

		public void StopPreview() {
			if (IsActionAttached(OnEditorUpdate)) {
				EditorApplication.update -= OnEditorUpdate;
				if (this != null) {
					Reset();
				}
			}
			foreach (var feedback in feedbacks) {
				feedback?.StopPreview();
			}
		}

		private void OnEditorUpdate() {
			var time = (float) EditorApplication.timeSinceStartup;
			var deltaTime = _editorTime > 0f ? time - _editorTime : 0f;
			_editorTime = time;
			Update(ignoreTimeScale ? deltaTime : deltaTime * Time.timeScale);
			SceneView.RepaintAll();
		}

		private bool IsActionAttached(Action action) {
			var invocations = EditorApplication.update.GetInvocationList();
			foreach (var del in invocations) {
				if (del.Method == action.Method && del.Target == action.Target) {
					return true;
				}
			}
			return false;
		}

		private void OnValidate() {
			foreach (var feedback in feedbacks) {
				feedback?.Validate();
			}
			StopPreview();
		}
#endif
	}
}