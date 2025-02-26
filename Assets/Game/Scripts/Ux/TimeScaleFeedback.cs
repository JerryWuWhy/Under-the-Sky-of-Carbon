using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class TimeScaleFeedback : Feedback {
		public float timeScale = 0.2f;

		private float _startTimeScale;

		protected override void OnStart() {
			_startTimeScale = Time.timeScale;
			Time.timeScale = timeScale;
		}

		protected override void OnEnd() {
			Time.timeScale = _startTimeScale;
		}
	}
}