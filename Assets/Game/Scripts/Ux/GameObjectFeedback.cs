using System;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class GameObjectFeedback : Feedback {
		public override bool Previewable => true;

		[Required] public GameObject gameObject;
		public bool active;

		private bool _startActive;

		protected override void OnStart() {
			_startActive = gameObject.activeSelf;
			gameObject.SetActive(active);
		}

		protected override void OnReset() {
			gameObject.SetActive(_startActive);
		}
	}
}