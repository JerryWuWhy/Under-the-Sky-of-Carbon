using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class AnchoredPositionFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public RectTransform transform;
		public AnimationCurve positionX = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve positionY = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve anchorMinX = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve anchorMinY = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve anchorMaxX = AnimationCurve.Constant(0f, 1f, 0f);
		public AnimationCurve anchorMaxY = AnimationCurve.Constant(0f, 1f, 0f);
		public bool relative = true;

		private Vector2 _startPosition;
		private Vector2 _startAnchorMin;
		private Vector2 _startAnchorMax;

		protected override void OnStart() {
			_startPosition = transform.anchoredPosition;
			_startAnchorMin = transform.anchorMin;
			_startAnchorMax = transform.anchorMax;
		}

		protected override void OnUpdate() {
			var position = new Vector2(
				positionX.Evaluate(Progress),
				positionY.Evaluate(Progress)
			);
			var anchorMin = new Vector2(
				anchorMinX.Evaluate(Progress),
				anchorMinY.Evaluate(Progress)
			);
			var anchorMax = new Vector2(
				anchorMaxX.Evaluate(Progress),
				anchorMaxY.Evaluate(Progress)
			);
			if (relative) {
				position += _startPosition;
				anchorMin += _startAnchorMin;
				anchorMax += _startAnchorMax;
			}
			transform.anchoredPosition = position;
			transform.anchorMin = anchorMin;
			transform.anchorMax = anchorMax;
		}

		protected override void OnReset() {
			transform.anchoredPosition = _startPosition;
			transform.anchorMin = _startAnchorMin;
			transform.anchorMax = _startAnchorMax;
		}
	}
}