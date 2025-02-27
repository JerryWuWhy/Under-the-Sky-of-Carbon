using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SoundManager : MonoBehaviour {
	public static event Action OnMusicOnChange;
	public static event Action OnSfxOnChange;
	public static SoundManager Inst { get; private set; }

	public static bool SfxOn {
		get => DataManager.Inst.SfxOn.Value;
		set {
			DataManager.Inst.SfxOn.UpdateAndSave(value);
			OnSfxOnChange?.Invoke();
		}
	}

	public static bool MusicOn {
		get => DataManager.Inst.MusicOn.Value;
		set {
			DataManager.Inst.MusicOn.UpdateAndSave(value);
			OnMusicOnChange?.Invoke();
		}
	}

	public string soundPath;
	public string musicPath;
	public int sfxBufferCount;
	public string defaultMusic;

	private readonly List<AudioSource> _sfxSources = new();
	private Dictionary<string, AudioClip> _sfxClips = new();
	private AudioSource _musicSource;

	private void Awake() {
		Inst = this;
		for (var i = 0; i < sfxBufferCount; i++) {
			_sfxSources.Add(CreateAudioSource("SFX_" + i));
		}
		_musicSource = CreateAudioSource("Music");
		if (!string.IsNullOrEmpty(defaultMusic)) {
			PlayMusic(defaultMusic);
		}
	}

	private AudioSource CreateAudioSource(string name) {
		var holder = new GameObject(name) {
			transform = {
				parent = transform,
				localPosition = Vector3.zero,
				localRotation = Quaternion.identity,
				localScale = Vector3.one
			}
		};
		var audioSource = holder.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.minDistance = 1f;
		audioSource.maxDistance = 100f;
		return audioSource;
	}

	public void PlaySound(string name, float volume = 1f, float pitch = 1f) {
		var minLength = float.MaxValue;
		var minLengthIndex = 0;
		for (int i = 0, j = 0; i < sfxBufferCount; i++) {
			var item = _sfxSources[i];
			if (!item.isPlaying) {
				minLengthIndex = i;
				break;
			}

			var clipLength = item.clip.length;
			if (minLength > clipLength) {
				minLength = clipLength;
				minLengthIndex = j;
			}

			j++;
		}

		if (!_sfxClips.TryGetValue(name, out var clip)) {
			clip = Resources.Load<AudioClip>(soundPath + name);
			_sfxClips.Add(name, clip);
		}

		var sfxSource = _sfxSources[minLengthIndex];
		sfxSource.Stop();
		sfxSource.pitch = pitch;
		sfxSource.clip = clip;
		sfxSource.volume = volume;
		sfxSource.spatialBlend = 0f;
		sfxSource.rolloffMode = AudioRolloffMode.Custom;
		sfxSource.Play();
	}

	public void StopSound(string name) {
		foreach (var sfxSource in _sfxSources) {
			if (sfxSource && sfxSource.clip.name == name) {
				sfxSource.Stop();
			}
		}
	}

	public void PlayMusic(string name, float volume = 1f) {
		if (_musicSource.isPlaying) {
			_musicSource.Stop();
		}
		_musicSource.clip = Resources.Load<AudioClip>(musicPath + name);
		_musicSource.loop = true;
		_musicSource.volume = volume;
		_musicSource.timeSamples = 0;
		_musicSource.Play();
	}

	public void StopMusic() {
		_musicSource.Stop();
	}
}