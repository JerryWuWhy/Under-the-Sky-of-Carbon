using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class AnimatorFeedback : Feedback {
		public enum EndBehavior {
			Nothing = -1,
			Pause,
			End,
			Rewind
		}

		public override bool Previewable => true;

		[Required] public Animator animator;
		[Required] public string state;
		public int layer = -1;
		public bool syncDuration;
		public EndBehavior endBehavior = EndBehavior.Nothing;

		private float _lastTime;

		protected override void OnStart() {
			SetProgress(0f);
			animator.speed = Speed;
			_lastTime = 0f;
		}

		protected override void OnUpdate() {
			if (Previewing) {
				var deltaTime = CurTime - _lastTime;
				animator.Update(animator.updateMode == AnimatorUpdateMode.Normal ? deltaTime * Time.timeScale : deltaTime);
				_lastTime = CurTime;
			} else if (syncDuration) {
				duration = 0f;
				for (var i = 0; i < animator.layerCount; ++i) {
					var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
					if (stateInfo.IsName(state)) {
						duration = Mathf.Max(duration, stateInfo.length);
					}
				}
			}
		}

		protected override void OnPause() {
			animator.speed = 0f;
		}

		protected override void OnResume() {
			animator.speed = Speed;
		}

		protected override void OnEnd() {
			switch (endBehavior) {
				case EndBehavior.Pause:
					Pause();
					break;
				case EndBehavior.End:
					SetProgress(1f);
					break;
				case EndBehavior.Rewind:
					SetProgress(0f);
					break;
			}
		}

		protected override void OnReset() {
			SetProgress(0f);
		}

		private void SetProgress(float progress) {
			animator.Play(state, layer, progress);
			animator.Update(0f);
			animator.speed = 0f;
		}
	}
}