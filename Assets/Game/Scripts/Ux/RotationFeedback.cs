using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class RotationFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Transform transform;
		public AnimationCurve angleX = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve angleY = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve angleZ = AnimationCurve.Constant(0f, 1f, 0f);
		public bool relative = true;
		public bool local = true;

		private Quaternion _startRotation;

		protected override void OnStart() {
			_startRotation = local ? transform.localRotation : transform.rotation;
		}

		protected override void OnUpdate() {
			var rotation = Quaternion.Euler(new Vector3(
				angleX.Evaluate(Progress),
				angleY.Evaluate(Progress),
				angleZ.Evaluate(Progress)
			));
			if (relative) {
				rotation *= _startRotation;
			}
			if (local) {
				transform.localRotation = rotation;
			} else {
				transform.rotation = rotation;
			}
		}

		protected override void OnReset() {
			if (local) {
				transform.localRotation = _startRotation;
			} else {
				transform.rotation = _startRotation;
			}
		}
	}
}