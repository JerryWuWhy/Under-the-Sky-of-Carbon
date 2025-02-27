using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConfigManager))]
public class ConfigManagerEditor : Editor {
	private string _filePath;

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		if (GUILayout.Button("Import Configs")) {
			_filePath = EditorUtility.OpenFilePanel("Localization", _filePath, "csv");
			if (!string.IsNullOrEmpty(_filePath)) {
				ImportConfigs(File.ReadAllText(_filePath));
			}
		}
	}

	private void ImportConfigs(string text) {
		var csv = ParseCsv(text);
		if (csv.Count <= 1) {
			return;
		}
		var keys = csv[0];
		var type = typeof(CardConfig);
		var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
		var cardConfigs = new List<CardConfig>();
		var configManager = (ConfigManager) target;
		for (var i = 1; i < csv.Count; ++i) {
			var cardConfig = new CardConfig();
			var arr = csv[i];
			if (string.IsNullOrEmpty(arr[0])) {
				continue;
			}
			foreach (var field in fields) {
				var value = arr[keys.IndexOf(field.Name)];
				if (field.FieldType == typeof(string)) {
					field.SetValue(cardConfig, value);
				} else if (field.FieldType == typeof(int)) {
					field.SetValue(cardConfig, Convert.ToInt32(value));
				}
			}
			cardConfigs.Add(cardConfig);
		}
		configManager.cardConfigs = cardConfigs;
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
}