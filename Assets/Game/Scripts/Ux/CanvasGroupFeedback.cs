using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class CanvasGroupFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public CanvasGroup canvasGroup;
		public AnimationCurve alpha = AnimationCurve.Constant(0f, 1f, 1f);
		public bool relative = true;

		private float _startAlpha;

		protected override void OnStart() {
			_startAlpha = canvasGroup.alpha;
		}

		protected override void OnUpdate() {
			var alpha = this.alpha.Evaluate(Progress);
			if (relative) {
				alpha *= _startAlpha;
			}
			canvasGroup.alpha = Mathf.Clamp01(alpha);
		}

		protected override void OnReset() {
			canvasGroup.alpha = _startAlpha;
		}
	}
}