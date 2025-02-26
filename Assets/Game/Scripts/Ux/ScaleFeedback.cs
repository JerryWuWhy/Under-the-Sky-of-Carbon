using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ScaleFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Transform transform;
		public AnimationCurve scaleX = AnimationCurve.Constant(0f, 1f, 1f);
		public AnimationCurve scaleY = AnimationCurve.Constant(0f, 1f, 1f);
		public AnimationCurve scaleZ = AnimationCurve.Constant(0f, 1f, 1f);
		public bool relative = true;

		private Vector3 _startScale;

		protected override void OnStart() {
			_startScale = transform.localScale;
		}

		protected override void OnUpdate() {
			var scale = new Vector3(
				scaleX.Evaluate(Progress),
				scaleY.Evaluate(Progress),
				scaleZ.Evaluate(Progress)
			);
			if (relative) {
				scale.x *= _startScale.x;
				scale.y *= _startScale.y;
				scale.z *= _startScale.z;
			}
			transform.localScale = scale;
		}

		protected override void OnReset() {
			transform.localScale = _startScale;
		}
	}
}