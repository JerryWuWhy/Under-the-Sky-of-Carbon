using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class SpriteRendererFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public SpriteRenderer spriteRenderer;
		public Gradient colorOverTime = new();
		public bool multiply = true;
		public bool changeSprite;
		public Sprite sprite;
		public bool flipX;
		public bool flipY;

		private Color _startColor;
		private Sprite _startSprite;
		private bool _startFlipX;
		private bool _startFlipY;

		protected override void OnStart() {
			_startColor = spriteRenderer.color;
			_startSprite = spriteRenderer.sprite;
			_startFlipX = spriteRenderer.flipX;
			_startFlipY = spriteRenderer.flipY;

			if (changeSprite) {
				spriteRenderer.sprite = sprite;
			}
			spriteRenderer.flipX = flipX;
			spriteRenderer.flipY = flipY;
		}

		protected override void OnUpdate() {
			var color = colorOverTime.Evaluate(Progress);
			if (multiply) {
				color *= _startColor;
			}
			spriteRenderer.color = color;
		}

		protected override void OnReset() {
			spriteRenderer.color = _startColor;
			spriteRenderer.flipX = _startFlipX;
			spriteRenderer.flipY = _startFlipY;
			if (changeSprite) {
				spriteRenderer.sprite = _startSprite;
			}
		}
	}
}