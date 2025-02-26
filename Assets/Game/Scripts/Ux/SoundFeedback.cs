using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Scripts.Ux {
	[Serializable]
	public class SoundFeedback : Feedback {
		public override bool Previewable => true;

		[SoundName] public string sound;
		public float volumeMin = 1f;
		public float volumeMax = 1f;
		public float pitchMin = 1f;
		public float pitchMax = 1f;

		protected override void OnStart() {
			PlaySound(sound, Random.Range(volumeMin, volumeMax), Random.Range(pitchMin, pitchMax));
		}

		protected override void OnPause() {
			if (!Previewing) {
				SoundManager.Inst.Stop(sound);
			}
		}

		private void PlaySound(string sound, float volume, float pitch) {
			if (!Previewing) {
				SoundManager.Inst.Play2D(sound, volume, pitch);
				return;
			}
#if UNITY_EDITOR
			var soundMgr = Resources.FindObjectsOfTypeAll<SoundManager>()[0];
			OnStopPreview();
			foreach (var soundConfig in soundMgr.sounds) {
				if (soundConfig.name == sound) {
					var audioGo = new GameObject {hideFlags = HideFlags.HideAndDontSave};
					_previewSource = audioGo.AddComponent<AudioSource>();
					_previewSource.clip = Resources.Load<AudioClip>(soundMgr.soundPath + soundConfig.filename);
					_previewSource.pitch = pitch;
					_previewSource.volume = soundConfig.volume * volume;
					_previewSource.loop = soundConfig.loop;
					_previewSource.spatialBlend = 0f;
					_previewSource.rolloffMode = AudioRolloffMode.Custom;
					_previewSource.maxDistance = soundConfig.maxDistance;
					_previewSource.Play();
					break;
				}
			}
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