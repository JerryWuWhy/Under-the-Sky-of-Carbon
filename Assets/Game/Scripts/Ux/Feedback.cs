using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Scripts.Ux {
	[Serializable]
	public class Feedback {
		public bool enabled = true;
		public string label;
		public float delay;
		public float duration = 1f;

		public virtual bool Previewable => false;
		public bool Previewing { get; private set; }
		public bool Started { get; private set; }
		public bool Ended { get; private set; }
		public float CurTime { get; private set; }
		public float Progress { get; private set; }
		public float Speed { get; private set; }

		internal void Start(float speed) {
			if (enabled) {
				CurTime = 0f;
				Started = true;
				Speed = speed;
				Invoke(OnStart);
			}
		}

		protected virtual void OnStart() { }

		internal void End() {
			if (enabled) {
				Ended = true;
				Invoke(OnEnd);
			}
		}

		protected virtual void OnEnd() { }

		internal void Update(float time) {
			if (enabled) {
				CurTime = time;
				Progress = duration > 0f ? Mathf.Clamp01(CurTime / duration) : 1f;
				Invoke(OnUpdate);
			}
		}

		protected virtual void OnUpdate() { }

		internal void Pause() {
			if (enabled) {
				Invoke(OnPause);
			}
		}

		protected virtual void OnPause() { }

		internal void Resume() {
			if (enabled) {
				Invoke(OnResume);
			}
		}

		protected virtual void OnResume() { }

		internal void Reset() {
			if (Started) {
				Invoke(OnReset);
			}
			Started = false;
			Ended = false;
		}

		protected virtual void OnReset() { }

		internal void ResetState() {
			Started = false;
			Ended = false;
		}

		private void Invoke(Action action) {
			try {
				if (!Previewing || Previewing && Previewable) {
					action();
				}
			} catch (Exception e) {
				Debug.LogError(e);
			}
		}

#if UNITY_EDITOR
		internal void StartPreview() {
			Previewing = true;
			OnStartPreview();
		}

		protected virtual void OnStartPreview() { }

		internal void StopPreview() {
			Previewing = false;
			OnStopPreview();
		}

		protected virtual void OnStopPreview() { }

		internal void Validate() {
			label = label.Trim();
			OnValidate();
		}

		protected virtual void OnValidate() { }
#endif
	}
}