using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Scripts.Ux {
	[Serializable]
	public class SoundFeedback : Feedback {
		public override bool Previewable => true;

		public string sound;
		public float volumeMin = 1f;
		public float volumeMax = 1f;
		public float pitchMin = 1f;
		public float pitchMax = 1f;

		protected override void OnStart() {
			PlaySound(sound, Random.Range(volumeMin, volumeMax), Random.Range(pitchMin, pitchMax));
		}

		protected override void OnPause() {
			if (!Previewing) {
				SoundManager.Inst.StopSound(sound);
			}
		}

		private void PlaySound(string sound, float volume, float pitch) {
			if (!Previewing) {
				SoundManager.Inst.PlaySound(sound, volume, pitch);
				return;
			}
#if UNITY_EDITOR
			var soundMgr = Object.FindObjectOfType<SoundManager>() ?? Resources.FindObjectsOfTypeAll<SoundManager>()[0];
			OnStopPreview();
			var audioGo = new GameObject {hideFlags = HideFlags.HideAndDontSave};
			_previewSource = audioGo.AddComponent<AudioSource>();
			_previewSource.clip = Resources.Load<AudioClip>(soundMgr.soundPath + sound);
			_previewSource.pitch = pitch;
			_previewSource.volume = volume;
			_previewSource.spatialBlend = 0f;
			_previewSource.rolloffMode = AudioRolloffMode.Custom;
			_previewSource.Play();
#endif
		}

#if UNITY_EDITOR
		private AudioSource _previewSource;

		protected override void OnStopPreview() {
			if (_previewSource) {
				Object.DestroyImmediate(_previewSource.gameObject);
				_previewSource = null;
			}
		}
#endif
	}
}