using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Habby.Localization {
	[CustomEditor(typeof(Localization))]
	public class LocalizationEditor : Editor {
		private Localization Target => (Localization) target;
		private string TranslationsDir => $"{Path.GetDirectoryName(AssetDatabase.GetAssetPath(Target))}/Translations";

		private string _filePath;
		private PhraseTable[] _phraseTables;
		private List<SystemLanguage> _languages;
		private string[] _langNames;
		private SerializedProperty _languagesProperty;
		private SerializedProperty _editorLangProperty;
		private SerializedProperty _playerLangProperty;

		private void OnEnable() {
			_languagesProperty = serializedObject.FindProperty("languages");
			_editorLangProperty = serializedObject.FindProperty("editorLanguage");
			_playerLangProperty = serializedObject.FindProperty("playerLanguage");
		}

		public override void OnInspectorGUI() {
			LoadLanguages();
			serializedObject.Update();

			var editorLangIndex = _languages.IndexOf(Target.editorLanguage);
			editorLangIndex = EditorGUILayout.Popup("Editor Language", editorLangIndex, _langNames);
			if (editorLangIndex >= 0 && editorLangIndex < _languages.Count) {
				_editorLangProperty.intValue = (int) _languages[editorLangIndex];
			}

			var playerLangIndex = _languages.IndexOf(Target.playerLanguage);
			playerLangIndex = EditorGUILayout.Popup("Player Language", playerLangIndex, _langNames);
			if (playerLangIndex >= 0 && playerLangIndex < _languages.Count) {
				_playerLangProperty.intValue = (int) _languages[playerLangIndex];
			}

			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button("Save & Reload")) {
				SaveAndReload();
			}

			if (GUILayout.Button("Import Translations")) {
				_filePath = EditorUtility.OpenFilePanel("Localization", _filePath, "csv");
				if (!string.IsNullOrEmpty(_filePath)) {
					ImportTranslations(File.ReadAllText(_filePath));
				}
			}
		}

		private void LoadLanguages() {
			_phraseTables = Resources.LoadAll<PhraseTable>("Translations");
			_languages = new List<SystemLanguage> {(SystemLanguage) (-1)};
			foreach (var phraseTable in _phraseTables) {
				if (Enum.TryParse<SystemLanguage>(phraseTable.name, out var language)) {
					_languages.Add(language);
				}
			}
			_langNames = _languages.Select(lang =>
				lang < 0 ? "System Language" : ObjectNames.NicifyVariableName(lang.ToString())).ToArray();
		}

		private void SaveAndReload() {
			EditorUtility.SetDirty(Target);
			AssetDatabase.SaveAssets();
			Localization.Reload();
		}

		private void ImportTranslations(string text) {
			var lines = text
				.Replace("\r\n", "\n")
				.Replace("\n\r", "\n")
				.Replace("\r", "\n")
				.Split('\n');

			if (lines.Length <= 1) return;
			var csv = ParseCsv(text);
			var langStrings = csv[0].Skip(1);
			var languages = new List<SystemLanguage>();
			foreach (var langString in langStrings) {
				if (Enum.TryParse<SystemLanguage>(langString, out var lang)) {
					languages.Add(lang);
				}
			}
			var phraseTables = new Dictionary<SystemLanguage, PhraseTable>();
			if (!AssetDatabase.IsValidFolder(TranslationsDir)) {
				Directory.CreateDirectory(TranslationsDir);
				AssetDatabase.Refresh();
			}
			foreach (var language in languages) {
				var path = GetPhrasePath(language);
				var phraseTable = AssetDatabase.LoadAssetAtPath<PhraseTable>(path);
				if (phraseTable == null) {
					phraseTable = CreateInstance<PhraseTable>();
					AssetDatabase.CreateAsset(phraseTable, path);
				}
				phraseTable.phrases = new List<PhraseTable.Phrase>();
				phraseTables.Add(language, phraseTable);
				EditorUtility.SetDirty(phraseTable);
			}
			foreach (var row in csv.Skip(1)) {
				var key = row[0];
				if (string.IsNullOrWhiteSpace(key)) {
					continue;
				}
				for (var i = 1; i < row.Count && i < languages.Count + 1; ++i) {
					var lang = languages[i - 1];
					var translation = row[i];
					var phraseTable = phraseTables[lang];
					phraseTable.phrases.Add(new PhraseTable.Phrase {key = key, translation = translation});
				}
			}
			_languagesProperty.arraySize = languages.Count;
			for (var i = 0; i < languages.Count; ++i) {
				_languagesProperty.GetArrayElementAtIndex(i).intValue = (int) languages[i];
			}
			SaveAndReload();
		}

		private static List<List<string>> ParseCsv(string csv) {
			var parsedCsv = new List<List<string>>();
			var row = new List<string>();
			var field = "";
			var inQuotedField = false;
			if (!csv.EndsWith("\n")) {
				csv += "\n";
			}
			for (var i = 0; i < csv.Length; i++) {
				var current = csv[i];
				var next = i == csv.Length - 1 ? ' ' : csv[i + 1];

				if (current != '"' && current != ',' && current != '\r' && current != '\n' || current != '"' && inQuotedField) {
					field += current;
				} else if (current == ' ' || current == '\t') {
					// Ignore whitespaces
				} else if (current == '"') {
					if (inQuotedField && next == '"') {
						i++;
						field += current;
					} else if (inQuotedField) {
						row.Add(field);
						if (next == ',') {
							i++;
						}
						field = string.Empty;
						inQuotedField = false;
					} else {
						inQuotedField = true;
					}
				} else if (current == ',') {
					row.Add(field);
					field = string.Empty;
				} else if (current == '\n') {
					if (!string.IsNullOrEmpty(field)) {
						row.Add(field);
					}
					parsedCsv.Add(new List<string>(row));
					field = string.Empty;
					row.Clear();
				}
			}

			return parsedCsv;
		}

		private string GetPhrasePath(SystemLanguage language) {
			return $"{TranslationsDir}/{language}.asset";
		}

		[MenuItem("Habby/Localization/Config")]
		private static void ShowConfig() {
			var config = Resources.Load<Localization>(nameof(Localization));
			if (config == null) {
				config = CreateInstance<Localization>();
				AssetDatabase.CreateAsset(config, "Assets/Habby/Localization/Resources/Localization.asset");
			}
			Selection.activeObject = config;
		}
	}
}