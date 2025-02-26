using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ShaderFloatFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public Renderer renderer;
		public string property;
		public AnimationCurve curve;
		public bool includeChildren;
		public bool usePropertyBlock = true;

		private Renderer[] _renderers;
		private MaterialPropertyBlock _mpb;
		private float _startValue;

		private MaterialPropertyBlock GetPropertyBlock() {
			_mpb ??= new MaterialPropertyBlock();
			return _mpb;
		}

		private float GetValue() {
			if (usePropertyBlock) {
				var mpb = GetPropertyBlock();
				renderer.GetPropertyBlock(_mpb);
				if (mpb.HasFloat(property)) {
					return mpb.GetFloat(property);
				}
				return renderer.sharedMaterial.GetFloat(property);
			}
			return renderer.material.GetFloat(property);
		}

		private void SetValue(float value) {
			if (_renderers == null) {
				SetValue(renderer, value);
			} else {
				foreach (var renderer in _renderers) {
					SetValue(renderer, value);
				}
			}
		}

		private void SetValue(Renderer renderer, float value) {
			if (usePropertyBlock) {
				var mpb = GetPropertyBlock();
				renderer.GetPropertyBlock(_mpb);
				mpb.SetFloat(property, value);
				renderer.SetPropertyBlock(mpb);
			} else {
				renderer.material.SetFloat(property, value);
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
			SetValue(curve.Evaluate(Progress));
		}

		protected override void OnReset() {
			SetValue(_startValue);
		}
	}
}