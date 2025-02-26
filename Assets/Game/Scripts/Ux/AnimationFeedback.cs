using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class AnimationFeedback : Feedback {
		public enum EndBehavior {
			Nothing = -1,
			Pause,
			End,
			Rewind
		}

		public override bool Previewable => true;

		[Required] public Animation animation;
		public string clip;
		public bool syncDuration;
		public EndBehavior endBehavior = EndBehavior.Nothing;

		private string _clip;

		protected override void OnStart() {
			_clip = string.IsNullOrEmpty(clip) ? animation.clip.name : clip;
			SetProgress(0f);
			animation.Play(_clip);
			animation[_clip].speed = Speed;
		}

		protected override void OnUpdate() {
			if (Previewing) {
				SetProgress(CurTime / animation[_clip].length);
			} else if (syncDuration) {
				duration = animation[_clip].length;
			}
		}

		protected override void OnPause() {
			animation.Stop(_clip);
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
			animation.Play(_clip);
			animation[_clip].normalizedTime = progress;
			animation.Sample();
			animation.Stop(_clip);
		}
	}
}