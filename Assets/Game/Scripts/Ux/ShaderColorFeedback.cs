using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ShaderColorFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Renderer renderer;
		public string property;
		public Gradient colorOverTime;
		public bool includeChildren;
		public bool usePropertyBlock = true;

		private Renderer[] _renderers;
		private MaterialPropertyBlock _mpb;
		private Color _startValue;

		private MaterialPropertyBlock GetPropertyBlock() {
			_mpb ??= new MaterialPropertyBlock();
			return _mpb;
		}

		private Color GetValue() {
			if (usePropertyBlock) {
				var mpb = GetPropertyBlock();
				renderer.GetPropertyBlock(_mpb);
				if (mpb.HasColor(property)) {
					return mpb.GetColor(property);
				}
				return renderer.sharedMaterial.GetColor(property);
			}
			return renderer.material.GetColor(property);
		}

		private void SetValue(Color value) {
			if (_renderers == null) {
				SetValue(renderer, value);
			} else {
				foreach (var renderer in _renderers) {
					SetValue(renderer, value);
				}
			}
		}

		private void SetValue(Renderer renderer, Color value) {
			if (usePropertyBlock) {
				var mpb = GetPropertyBlock();
				renderer.GetPropertyBlock(_mpb);
				mpb.SetColor(property, value);
				renderer.SetPropertyBlock(mpb);
			} else {
				renderer.material.SetColor(property, value);
			}
		}

		protected override void OnStart() {
			_startValue = GetValue();
			if (includeChildren) {
				_renderers ??= renderer.GetComponentsInChildren<Renderer>();
			} else {
				_renderers = null;
			}
		}

		protected override void OnUpdate() {
			SetValue(colorOverTime.Evaluate(Progress));
		}

		protected override void OnReset() {
			SetValue(_startValue);
		}
	}
}