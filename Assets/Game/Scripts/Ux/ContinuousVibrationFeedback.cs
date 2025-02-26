using System;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ContinuousVibrationFeedback : Feedback {
		public float amplitude;
		public float frequency;

		protected override void OnStart() {
			// VibrationMgr.Inst.Vibrate(amplitude, frequency, duration / Speed);
		}
	}
}