using System;

namespace Game.Scripts.Ux {
	[Serializable]
	public class VibrationFeedback : Feedback {
		// public VibrationType type;

		protected override void OnStart() {
			// VibrationMgr.Inst.Vibrate(type);
		}
	}
}