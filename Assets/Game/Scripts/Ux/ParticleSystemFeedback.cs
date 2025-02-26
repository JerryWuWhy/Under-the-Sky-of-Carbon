using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ParticleSystemFeedback : Feedback {
		public enum EndBehavior {
			Nothing = -1,
			StopEmittingAndClear = ParticleSystemStopBehavior.StopEmittingAndClear,
			StopEmitting = ParticleSystemStopBehavior.StopEmitting
		}

		public override bool Previewable => true;

		[Required] public ParticleSystem particleSystem;
		public EndBehavior endBehavior = EndBehavior.Nothing;

		private float _lastTime;

		protected override void OnStart() {
			particleSystem.Play();
			_lastTime = 0f;
		}

#if UNITY_EDITOR
		protected override void OnUpdate() {
			if (Previewing) {
				particleSystem.Simulate(CurTime - _lastTime, true, false, true);
				_lastTime = CurTime;
			}
		}
#endif

		protected override void OnEnd() {
			if (endBehavior != EndBehavior.Nothing) {
				particleSystem.Stop(true, (ParticleSystemStopBehavior) endBehavior);
			}
		}

		protected override void OnPause() {
			particleSystem.Pause();
		}

		protected override void OnReset() {
			particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
}