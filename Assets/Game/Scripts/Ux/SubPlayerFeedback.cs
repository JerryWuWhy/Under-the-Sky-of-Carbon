using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Ux {
	[Serializable]
	public class SubPlayerFeedback : Feedback {
		public enum StartBehavior {
			Nothing = -1,
			Rewind,
			Reset
		}

		public enum EndBehavior {
			Nothing = -1,
			Pause,
			End,
			Rewind,
			Reset
		}

		public override bool Previewable => true;

		[Required] public List<Component> players;
		public bool includeChildren;
		public float interval;
		public bool syncDuration;
		public StartBehavior startBehavior = StartBehavior.Nothing;
		public EndBehavior endBehavior = EndBehavior.Nothing;

		private int _nextPlayIndex;
		private float _nextPlayTime;
		private List<FeedbackPlayer> _players;

		protected override void OnStart() {
			_nextPlayIndex = 0;
			_nextPlayTime = 0f;
			UpdatePlayers();
			if (syncDuration) {
				duration = Mathf.Max(interval * (_players.Count - 1), 0f);
			}
			switch (startBehavior) {
				case StartBehavior.Rewind:
					foreach (var player in _players) {
						player.Rewind();
					}
					break;
				case StartBehavior.Reset:
					foreach (var player in _players) {
						player.Reset();
					}
					break;
			}
		}

		protected override void OnUpdate() {
			while (CurTime >= _nextPlayTime && _nextPlayIndex < _players.Count) {
				var player = _players[_nextPlayIndex];
				player.Play();
				if (syncDuration) {
					duration = Mathf.Max(duration, _nextPlayIndex * interval + player.ActualDuration);
				}
				_nextPlayIndex += 1;
				_nextPlayTime += interval;
			}
		}

		protected override void OnPause() {
			for (var i = 0; i < _nextPlayIndex; ++i) {
				var player = _players[i];
				player.Pause();
			}
		}

		protected override void OnResume() {
			for (var i = 0; i < _nextPlayIndex; ++i) {
				var player = _players[i];
				player.Resume();
			}
		}

		protected override void OnEnd() {
			switch (endBehavior) {
				case EndBehavior.Pause:
					foreach (var player in _players) {
						player.Pause();
					}
					break;
				case EndBehavior.End:
					foreach (var player in _players) {
						player.End();
					}
					break;
				case EndBehavior.Rewind:
					foreach (var player in _players) {
						player.Rewind();
					}
					break;
				case EndBehavior.Reset:
					foreach (var player in _players) {
						player.Reset();
					}
					break;
			}
		}

		protected override void OnReset() {
			foreach (var player in _players) {
				player.Reset();
			}
		}

		private void UpdatePlayers() {
			_players ??= new List<FeedbackPlayer>();
			_players.Clear();
			foreach (var item in players) {
				if (item == null) {
					continue;
				}
				if (item is FeedbackPlayer player) {
					_players.Add(player);
				} else if (includeChildren) {
					_players.AddRange(item.GetComponentsInChildren<FeedbackPlayer>());
				}
			}
		}

#if UNITY_EDITOR
		protected override void OnStartPreview() {
			UpdatePlayers();
			foreach (var player in _players) {
				player.StartPreview();
			}
		}

		protected override void OnStopPreview() {
			UpdatePlayers();
			foreach (var player in _players) {
				player.StopPreview();
			}
		}

		protected override void OnValidate() {
			for (var i = 0; i < players.Count; ++i) {
				var item = players[i];
				if (item != null && item is not FeedbackPlayer && item.TryGetComponent(typeof(FeedbackPlayer), out var player)) {
					players[i] = player;
				}
			}
		}
#endif
	}
}