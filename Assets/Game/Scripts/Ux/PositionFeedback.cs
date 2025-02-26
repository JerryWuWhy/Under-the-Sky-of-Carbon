using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class PositionFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Transform transform;
		public AnimationCurve positionX = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve positionY = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve positionZ = AnimationCurve.Constant(0f, 1f, 0f);
		public bool relative = true;
		public bool local = true;

		private Vector3 _startPosition;

		protected override void OnStart() {
			_startPosition = local ? transform.localPosition : transform.position;
		}

		protected override void OnUpdate() {
			var position = new Vector3(
				positionX.Evaluate(Progress),
				positionY.Evaluate(Progress),
				positionZ.Evaluate(Progress)
			);
			if (relative) {
				position.x += _startPosition.x;
				position.y += _startPosition.y;
				position.z += _startPosition.z;
			}
			if (local) {
				transform.localPosition = position;
			} else {
				transform.position = position;
			}
		}

		protected override void OnReset() {
			if (local) {
				transform.localPosition = _startPosition;
			} else {
				transform.position = _startPosition;
			}
		}
	}
}