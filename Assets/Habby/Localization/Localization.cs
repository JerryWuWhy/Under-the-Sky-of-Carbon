using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Habby.Localization {
	public class Localization : ScriptableObject {
		public List<SystemLanguage> languages;
		public SystemLanguage editorLanguage = (SystemLanguage) (-1);
		public SystemLanguage playerLanguage = (SystemLanguage) (-1);

		public static event Action OnLanguageChange;

		public static SystemLanguage Language { get; private set; }

		private static bool _initialized;
		private static Localization _instance;
		private static Dictionary<string, string> _translations;

		private static void Init() {
			if (_initialized) {
				return;
			}
			if (_instance == null) {
				_instance = Resources.Load<Localization>(nameof(Localization));
			}
			var languages = _instance.languages;
			var language = Application.isPlaying ? _instance.playerLanguage : _instance.editorLanguage;
			if (language < 0) {
				if (languages.Contains(Application.systemLanguage)) {
					language = Application.systemLanguage;
				} else if (languages.Contains(SystemLanguage.English)) {
					language = SystemLanguage.English;
				} else if (languages.Count > 0) {
					language = languages[0];
				}
			}
			_initialized = true;
			SetLanguage(language);
		}

		public static void SetLanguage(SystemLanguage language) {
			var phraseTable = Resources.Load<PhraseTable>($"Translations/{language}");
			if (phraseTable == null) {
				throw new ArgumentException($"Language {language} does not exists");
			}
			Language = language;
			_translations = phraseTable.ToDictionary();
			Resources.UnloadAsset(phraseTable);
			OnLanguageChange?.Invoke();
		}

		public static string GetString(string key) {
			TryGetString(key, out var translation);
			return translation;
		}

		public static string GetStringFormat(string key, params object[] args) {
			TryGetString(key, out var translation);
			return string.Format(translation ?? string.Empty, args);
		}

		public static bool TryGetString(string key, out string translation) {
			Init();
			if (_translations == null || string.IsNullOrEmpty(key)) {
				translation = key;
				return false;
			}
			if (_translations.TryGetValue(key, out translation)) {
				return true;
			} else {
				translation = key;
				return false;
			}
		}

		public static bool HasKey(string key) {
			Init();
			return _translations.ContainsKey(key);
		}

		public static string[] GetAllKeys() {
			Init();
			return _translations.Keys.ToArray();
		}

		public static void Reload() {
			_initialized = false;
			Init();
		}
	}
}