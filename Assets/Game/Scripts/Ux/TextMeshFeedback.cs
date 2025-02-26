using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class TextMeshFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public TextMesh text;
		public Gradient colorOverTime = new();
		public bool multiply = true;

		private Color _startColor;

		protected override void OnStart() {
			_startColor = text.color;
		}

		protected override void OnUpdate() {
			var color = colorOverTime.Evaluate(Progress);
			if (multiply) {
				color *= _startColor;
			}
			text.color = color;
		}

		protected override void OnReset() {
			text.color = _startColor;
		}
	}
}