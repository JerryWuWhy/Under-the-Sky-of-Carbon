using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class TargetPositionFeedback : Feedback {
		public override bool Previewable => transform != null && target != null;

		[Required] public Transform transform;
		[Required] public Transform target;
		public Vector3 offset;
		public AnimationCurve curveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		public AnimationCurve curveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		public AnimationCurve curveZ = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		private Vector3 _startPosition;
		private Vector3 _startLocalPosition;

		protected override void OnStart() {
			_startPosition = transform.position;
			_startLocalPosition = transform.localPosition;
		}

		protected override void OnUpdate() {
			var pos = target.position + offset;
			pos.x = Mathf.LerpUnclamped(_startPosition.x, pos.x, curveX.Evaluate(Progress));
			pos.y = Mathf.LerpUnclamped(_startPosition.y, pos.y, curveY.Evaluate(Progress));
			pos.z = Mathf.LerpUnclamped(_startPosition.z, pos.z, curveZ.Evaluate(Progress));
			transform.position = Vector3.Lerp(_startPosition, pos, Progress);
		}

		protected override void OnReset() {
			transform.localPosition = _startLocalPosition;
		}
	}
}