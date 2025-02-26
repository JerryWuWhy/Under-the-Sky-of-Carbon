using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Scripts.Ux {
	[Serializable]
	public class ContinuousEventFeedback : Feedback {
		public override bool Previewable => true;

		public AnimationCurve valueCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		public UnityEvent startEvent;
		public UnityEvent<float> updateEvent;
		public UnityEvent endEvent;
		public UnityEvent resetEvent;

		protected override void OnStart() {
			startEvent.Invoke();
		}

		protected override void OnUpdate() {
			updateEvent.Invoke(valueCurve.Evaluate(Progress));
		}

		protected override void OnEnd() {
			endEvent.Invoke();
		}

		protected override void OnReset() {
			resetEvent.Invoke();
		}
	}
}