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
	[Serializable]
	public class SoundConfig {
		[HideInInspector] public string name;
		public string filename;
		public bool loop;
		public bool singleton;
		public float minInterval = 0.05f;
		public bool gameplaySound;
		public float volume = 1f;
		public float maxDistance = 100f;
	}

	[Serializable]
	public class MusicConfig {
		public string name;
		public string filename;
		public bool loop;
		public float volume = 1f;
	}

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

	public SoundConfig[] sounds;
	public MusicConfig[] musics;

	public int sfxBufferCount = 10;
	public float musicFadingDuration = 1f;

	private Dictionary<Sound, string> _sfxClipNames;
	private Dictionary<string, SoundConfig> _sfxConfigs;
	private Dictionary<string, AudioClip> _sfxClips;
	private Dictionary<string, float> _sfxTimes;
	private AudioSource[] _sfxSources;
	private AudioSource _musicSource;
	private List<AudioSource> _sceneAudioSources;
	private Transform[] _sfxTransforms;
	private Transform _trans;

	private int _musicClipIndex;
	private float _musicVolume = 1f;
	private bool _gameplaySoundMuted;
	private float _gameplayVolumeScale = 1f;

	public enum Music { }

	public enum Sound { }

	private void Awake() {
		Inst = this;
		_trans = transform;
		_sfxSources = new AudioSource[sfxBufferCount];
		_sfxTransforms = new Transform[sfxBufferCount];
		_sfxClipNames = new Dictionary<Sound, string>();
		_sfxConfigs = new Dictionary<string, SoundConfig>(sounds.Length);
		_sfxClips = new Dictionary<string, AudioClip>(sounds.Length);
		_sfxTimes = new Dictionary<string, float>(sounds.Length);
		_sceneAudioSources = new List<AudioSource>();

		for (var i = 0; i < sfxBufferCount; i++) {
			var holder = CreateGameObject("SFX_" + i);
			_sfxSources[i] = holder.AddComponent<AudioSource>();
			_sfxSources[i].playOnAwake = false;
			_sfxSources[i].minDistance = 1f;
			_sfxSources[i].maxDistance = 100f;
			_sfxTransforms[i] = holder.transform;
		}

		_musicSource = gameObject.AddComponent<AudioSource>();

		foreach (var name in Enum.GetNames(typeof(Sound))) {
			_sfxClipNames.Add((Sound) _sfxClipNames.Count, name);
		}

		foreach (var soundConfig in sounds) {
			var soundClip = Resources.Load<AudioClip>(soundPath + soundConfig.filename);
			if (soundClip == null) {
				Debug.LogError($"Clip {soundConfig.name}({soundConfig.filename}) not exists");
			}
			_sfxConfigs.Add(soundConfig.name, soundConfig);
			_sfxClips.Add(soundConfig.name, soundClip);
			_sfxTimes.Add(soundConfig.name, 0f);
		}
	}

	private GameObject CreateGameObject(string objName) {
		var result = new GameObject(objName) {
			transform = {
				parent = transform,
				localPosition = Vector3.zero,
				localRotation = Quaternion.identity,
				localScale = Vector3.one
			}
		};
		return result;
	}

	public void RegisterSceneAudioSource(AudioSource aso) {
		if (_sceneAudioSources.Contains(aso)) return;
		_sceneAudioSources.Add(aso);
		if (!SfxOn) {
			aso.Stop();
		}
	}

	public void UnRegisterSceneAudioSource(AudioSource aso) {
		if (_sceneAudioSources.Contains(aso)) {
			_sceneAudioSources.Remove(aso);
		}
	}

	public void ResetParents() {
		foreach (var sfxTrans in _sfxTransforms) {
			sfxTrans.parent = _trans;
		}
	}

	public bool IsPlaying(string sound) {
		var source = GetSfxSource(sound);
		return source != null && source.isPlaying;
	}

	public bool IsPlaying(Sound sound) {
		return IsPlaying(_sfxClipNames[sound]);
	}

	public void Play2D(string sound, float volume = 1f, float pitch = 1f, float delay = 0f) {
		PlaySfx(sound, volume, pitch, delay, _trans, Vector3.zero, true);
	}

	public void Play2D(Sound sound, float volume = 1f, float pitch = 1f, float delay = 0f) {
		PlaySfx(_sfxClipNames[sound], volume, pitch, delay, _trans, Vector3.zero, true);
	}

	public void Play(string sound, float volume = 1f, float pitch = 1f, float delay = 0f,
		Transform parent = null, Vector3 position = default) {
		PlaySfx(sound, volume, pitch, delay, parent, position, false);
	}

	public void Play(Sound sound, float volume = 1f, float pitch = 1f, float delay = 0f,
		Transform parent = null, Vector3 position = default) {
		Play(_sfxClipNames[sound], volume, pitch, delay, parent, position);
	}

	public void Play(Music music, float volume = 1f) {
		PlayMusic((int) music, volume);
	}

	private AudioSource GetSfxSource(string sound) {
		var clip = _sfxClips[sound];
		foreach (var sfxSource in _sfxSources) {
			if (sfxSource.clip == clip) {
				return sfxSource;
			}
		}
		return null;
	}

	private AudioSource GetSfxSource(Sound sound) {
		return GetSfxSource(_sfxClipNames[sound]);
	}

	public void Stop(string sound) {
		GetSfxSource(sound)?.Stop();
	}

	public void Stop(Sound sound) {
		Stop(_sfxClipNames[sound]);
	}

	public void Stop(Sound sound, int sourceIndex) {
		if (sourceIndex < 0 || sourceIndex >= _sfxSources.Length) {
			return;
		}
		var sfxSource = _sfxSources[sourceIndex];
		if (sfxSource.clip == _sfxClips[_sfxClipNames[sound]]) {
			sfxSource.Stop();
		}
	}

	public void StopGameplaySound() {
		foreach (var sfxSource in _sfxSources) {
			foreach (var (name, clip) in _sfxClips) {
				if (sfxSource.clip == clip) {
					var soundConfig = _sfxConfigs[name];
					if (soundConfig.gameplaySound) {
						sfxSource.Stop();
						break;
					}
				}
			}
		}
	}

	public void ScaleGameplayVolume(float scale) {
		if (_gameplaySoundMuted) return;
		_gameplayVolumeScale = scale;
		foreach (var sfxSource in _sfxSources) {
			foreach (var (name, clip) in _sfxClips) {
				if (sfxSource.clip == clip) {
					var soundConfig = _sfxConfigs[name];
					if (soundConfig.gameplaySound) {
						sfxSource.volume = soundConfig.volume * scale;
						break;
					}
				}
			}
		}
	}

	public void MuteGameplaySound() {
		if (!Inst) return;
		_gameplaySoundMuted = true;
		foreach (var sfxSource in _sfxSources) {
			foreach (var (name, clip) in _sfxClips) {
				if (sfxSource.clip == clip) {
					var soundConfig = _sfxConfigs[name];
					if (soundConfig.gameplaySound) {
						sfxSource.mute = true;
						break;
					}
				}
			}
		}
	}

	public void RestoreGameplaySound() {
		if (!Inst) return;
		_gameplaySoundMuted = false;
		foreach (var sfxSource in _sfxSources) {
			foreach (var (name, clip) in _sfxClips) {
				if (sfxSource.clip == clip) {
					var soundConfig = _sfxConfigs[name];
					if (soundConfig.gameplaySound) {
						sfxSource.mute = false;
						break;
					}
				}
			}
		}
	}

	public void SetSoundVolume(string sound, float volume) {
		var sfxSource = GetSfxSource(sound);
		if (sfxSource == null) return;
		var config = _sfxConfigs[sound];
		sfxSource.volume = volume * config.volume * (config.gameplaySound ? _gameplayVolumeScale : 1f);
	}

	public void SetSoundVolume(Sound sound, float volume) {
		SetSoundVolume(_sfxClipNames[sound], volume);
	}

	public void SetSoundPitch(string sound, float pitch) {
		var sfxSource = GetSfxSource(sound);
		if (sfxSource == null) return;
		sfxSource.pitch = pitch;
	}

	public void SetSoundPitch(Sound sound, float pitch) {
		SetSoundPitch(_sfxClipNames[sound], pitch);
	}

	private void PlaySfx(string sound, float volume, float pitch, float delay,
		Transform parent, Vector3 position, bool is2D) {
		if (!SfxOn) return;
		if (string.IsNullOrEmpty(sound)) return;
		if (!_sfxClips.ContainsKey(sound)) {
			Debug.LogError("Sound not exists: " + sound);
			return;
		}

		var soundConfig = _sfxConfigs[sound];
		if (soundConfig.gameplaySound && _gameplaySoundMuted) {
			return;
		}

		if (delay > 0f) {
			StartCoroutine(PlaySfxDelayed(sound, volume, pitch, delay, parent, position, is2D));
			return;
		}

		if (Time.realtimeSinceStartup > _sfxTimes[sound] + soundConfig.minInterval) {
			_sfxTimes[sound] = Time.realtimeSinceStartup;
		} else {
			return;
		}

		var minLength = float.MaxValue;
		var minLengthIndex = 0;
		for (int i = 0, j = 0; i < sfxBufferCount; i++) {
			var sfxSource = _sfxSources[i];
			if (!sfxSource.isPlaying) {
				PlaySfx(sound, i, volume, pitch, parent, position, is2D);
				return;
			}

			var clipLength = sfxSource.clip.length;
			if (minLength > clipLength) {
				minLength = clipLength;
				minLengthIndex = j;
			}

			j++;
		}

		PlaySfx(sound, minLengthIndex, volume, pitch, parent, position, is2D);
	}

	private void PlaySfx(string sound, int sourceIndex, float volume, float pitch,
		Transform parent, Vector3 position, bool is2D) {
		var soundConfig = _sfxConfigs[sound];
		var sfxSource = _sfxSources[sourceIndex];
		var sfxTrans = _sfxTransforms[sourceIndex];

		if (soundConfig.singleton) {
			Stop(sound);
		}

		if (soundConfig.gameplaySound) {
			volume *= _gameplayVolumeScale;
		}

		sfxSource.Stop();
		sfxTrans.parent = parent == null ? _trans : parent;
		sfxTrans.localPosition = position;
		sfxTrans.localRotation = Quaternion.identity;
		sfxSource.pitch = pitch;
		sfxSource.clip = _sfxClips[sound];
		sfxSource.volume = soundConfig.volume * volume * (soundConfig.gameplaySound ? _gameplayVolumeScale : 1f);
		sfxSource.mute = soundConfig.gameplaySound && _gameplaySoundMuted;
		sfxSource.loop = soundConfig.loop;
		sfxSource.spatialBlend = is2D ? 0f : 1f;
		sfxSource.rolloffMode = AudioRolloffMode.Custom;
		sfxSource.maxDistance = soundConfig.maxDistance;
		sfxSource.Play();
	}

	private IEnumerator PlaySfxDelayed(string sound, float volume, float pitch, float delay,
		Transform parent, Vector3 position, bool is2D) {
		yield return new WaitForSeconds(delay);
		PlaySfx(sound, volume, pitch, 0f, parent, position, is2D);
	}

	private void PlayMusic(int clipIndex, float volume = 0f) {
		_musicClipIndex = clipIndex;
		_musicVolume = volume > 0f ? volume : _musicVolume;
		if (!MusicOn) {
			return;
		}
		if (_musicSource.isPlaying) {
			_musicSource.Stop();
		}
		var music = musics[clipIndex];
		_musicSource.clip = Resources.Load<AudioClip>(musicPath + music.filename);
		_musicSource.loop = music.loop;
		_musicSource.volume = music.volume * _musicVolume;
		_musicSource.timeSamples = 0;
		_musicSource.Play();
		StopCoroutine(nameof(PlayMusicCoroutine));
		StopCoroutine(nameof(CrossFadeCoroutine));
		StopCoroutine(nameof(FadeInCoroutine));
		StopCoroutine(nameof(FadeOutCoroutine));
		// StartCoroutine(nameof(PlayMusicCoroutine));
	}

	public void PlayMusic(float volume = 0f) {
		PlayMusic(_musicClipIndex, volume);
	}

	private IEnumerator PlayMusicCoroutine() {
		var music = musics[_musicClipIndex];
		var request = Resources.LoadAsync<AudioClip>(musicPath + music.filename);
		yield return request;
		_musicSource.clip = request.asset as AudioClip;
		_musicSource.loop = music.loop;
		_musicSource.volume = music.volume * _musicVolume;
		_musicSource.timeSamples = 0;
		_musicSource.Play();
	}

	public void StopMusic() {
		_musicSource.Stop();
		_musicSource.clip = null;
		StopCoroutine(nameof(PlayMusicCoroutine));
		StopCoroutine(nameof(CrossFadeCoroutine));
		StopCoroutine(nameof(FadeInCoroutine));
		StopCoroutine(nameof(FadeOutCoroutine));
	}

	public void FadeInMusic() {
		if (!MusicOn) {
			return;
		}
		if (_musicSource.clip) {
			var music = musics[_musicClipIndex];
			_musicSource.UnPause();
			_musicSource.volume = music.volume * _musicVolume;
		} else {
			PlayMusic();
		}
		StopCoroutine(nameof(PlayMusicCoroutine));
		StopCoroutine(nameof(CrossFadeCoroutine));
		StopCoroutine(nameof(FadeInCoroutine));
		StopCoroutine(nameof(FadeOutCoroutine));
		StartCoroutine(nameof(FadeInCoroutine));
	}

	public void FadeOutMusic() {
		if (_musicSource.clip == null) {
			return;
		}
		StopCoroutine(nameof(PlayMusicCoroutine));
		StopCoroutine(nameof(CrossFadeCoroutine));
		StopCoroutine(nameof(FadeInCoroutine));
		StopCoroutine(nameof(FadeOutCoroutine));
		StartCoroutine(nameof(FadeOutCoroutine));
	}

	private IEnumerator FadeInCoroutine() {
		var time = 0f;
		var volume = _musicSource.volume;
		while (time < musicFadingDuration) {
			time += Time.unscaledDeltaTime;
			_musicSource.volume = time / musicFadingDuration * volume;
			yield return null;
		}
	}

	private IEnumerator FadeOutCoroutine() {
		var time = 0f;
		var volume = _musicSource.volume;
		while (time < musicFadingDuration) {
			time += Time.unscaledDeltaTime;
			_musicSource.volume = (1f - time / musicFadingDuration) * volume;
			yield return null;
		}
		_musicSource.Pause();
	}

	public void CrossFadeMusic(Music music) {
		_musicClipIndex = (int) music;
		if (!MusicOn) {
			return;
		}
		if (_musicSource.clip == null) {
			Play(music);
			return;
		}
		StopCoroutine(nameof(PlayMusicCoroutine));
		StopCoroutine(nameof(CrossFadeCoroutine));
		StopCoroutine(nameof(FadeInCoroutine));
		StopCoroutine(nameof(FadeOutCoroutine));
		StartCoroutine(nameof(CrossFadeCoroutine));
	}

	private IEnumerator CrossFadeCoroutine() {
		var volume = _musicSource.volume;
		var factor = 1f;
		while (factor > 0f) {
			factor -= musicFadingDuration * Time.unscaledDeltaTime;
			_musicSource.volume = factor * volume;
			yield return null;
		}
		PlayMusic(_musicClipIndex);
	}

#if UNITY_EDITOR
	private void OnValidate() {
		foreach (var sound in sounds) {
			if (!string.IsNullOrEmpty(sound.filename)) {
				sound.name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
						Path.GetFileName(sound.filename).Replace("_", " "))
					.Replace(" ", "");
			}
		}
	}

	[ContextMenu("Rearrange Sounds")]
	private void RearrangeSounds() {
		sounds = sounds.ToList()
			.Where(sound => !string.IsNullOrEmpty(sound.filename))
			.OrderBy(sound => sound.filename)
			.ToArray();
		EditorUtility.SetDirty(this);
	}
#endif
}


public class SoundNameAttribute : PropertyAttribute { }
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SoundNameAttribute))]
public class SoundNameDrawer : PropertyDrawer {
	private static SoundManager _soundManager;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
		var contentColor = GUI.contentColor;
		var backgroundColor = GUI.backgroundColor;
		_soundManager = _soundManager == null ? Resources.FindObjectsOfTypeAll<SoundManager>().ElementAtOrDefault(0) : _soundManager;
		if (!string.IsNullOrWhiteSpace(property.stringValue) &&
		    _soundManager != null && _soundManager.sounds.All(sound => sound.name != property.stringValue)) {
			GUI.contentColor = GUI.backgroundColor = Color.red;
		}
		var textRect = new Rect(rect.x, rect.y, rect.width - 25f, rect.height);
		EditorGUI.PropertyField(textRect, property, label, true);
		GUI.contentColor = contentColor;
		GUI.backgroundColor = backgroundColor;
		var dropdownRect = new Rect(rect.xMax - 20f, rect.y, 20f, rect.height);
		if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(), FocusType.Passive) && _soundManager) {
			var menu = new GenericMenu();
			foreach (var sound in _soundManager.sounds) {
				menu.AddItem(new GUIContent(sound.filename), property.stringValue == sound.name, () => {
					property.serializedObject.Update();
					property.stringValue = sound.name;
					property.serializedObject.ApplyModifiedProperties();
				});
			}
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Open Sound Manager"), false, () => {
				AssetDatabase.OpenAsset(_soundManager);
			});
			menu.AddItem(new GUIContent("Open Sound Folder"), false, () => {
				var path = Path.GetDirectoryName("Assets/Game/Resources/" + _soundManager.soundPath);
				var soundFolder = AssetDatabase.LoadAssetAtPath<Object>(path);
				AssetDatabase.OpenAsset(soundFolder);
			});
			menu.AddItem(new GUIContent("Open Music Folder"), false, () => {
				var path = Path.GetDirectoryName("Assets/Game/Resources/" + _soundManager.musicPath);
				var soundFolder = AssetDatabase.LoadAssetAtPath<Object>(path);
				AssetDatabase.OpenAsset(soundFolder);
			});
			menu.ShowAsContext();
		}
	}
}

[CustomPropertyDrawer(typeof(SoundManager.SoundConfig))]
public class SoundConfigPropertyDrawer : PropertyDrawer {
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
	}

	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
		var filename = property.FindPropertyRelative("filename").stringValue;
		var infoRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
		var infoStyle = new GUIStyle {alignment = TextAnchor.MiddleRight, normal = {textColor = Color.gray}};
		EditorGUI.PropertyField(rect, property, label, property.isExpanded);
		EditorGUI.LabelField(infoRect, Path.GetDirectoryName(filename), infoStyle);
	}
}

[CustomEditor(typeof(SoundManager))]
public class DefaultGameConfigurationEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
	}
}
#endif