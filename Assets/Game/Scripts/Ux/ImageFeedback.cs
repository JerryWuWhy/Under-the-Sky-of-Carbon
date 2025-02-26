using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ImageFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Image image;
		public Gradient colorOverTime = new();
		public bool multiply = true;
		public bool changeSprite;
		public Sprite sprite;

		private Color _startColor;
		private Sprite _startSprite;

		protected override void OnStart() {
			_startColor = image.color;
			_startSprite = image.sprite;

			if (changeSprite) {
				image.sprite = sprite;
			}
		}

		protected override void OnUpdate() {
			var color = colorOverTime.Evaluate(Progress);
			if (multiply) {
				color *= _startColor;
			}
			image.color = color;
		}

		protected override void OnReset() {
			image.color = _startColor;
			image.sprite = _startSprite;
		}
	}
}