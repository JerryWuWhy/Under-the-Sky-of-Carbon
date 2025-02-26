using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ux {
	[Serializable]
	public class TextFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Text text;
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