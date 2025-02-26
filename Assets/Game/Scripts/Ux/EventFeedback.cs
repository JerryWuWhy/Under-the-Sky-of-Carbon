using System;
using UnityEngine.Events;

namespace Game.Scripts.Ux {
	[Serializable]
	public class EventFeedback : Feedback {
		public override bool Previewable => true;

		public UnityEvent startEvent;
		public UnityEvent resetEvent;

		protected override void OnStart() {
			startEvent.Invoke();
		}

		protected override void OnReset() {
			resetEvent.Invoke();
		}
	}
}