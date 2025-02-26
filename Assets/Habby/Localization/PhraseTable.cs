using System;
using System.Collections.Generic;
using UnityEngine;

namespace Habby.Localization {
	public class PhraseTable : ScriptableObject {
		[Serializable]
		public class Phrase {
			public string key;
			public string translation;
		}

		public List<Phrase> phrases;

		public Dictionary<string, string> ToDictionary() {
			var dict = new Dictionary<string, string>();
			foreach (var phrase in phrases) {
				var key = phrase.key;
				var translation = phrase.translation;
				if (dict.ContainsKey(key)) {
					Debug.LogError(new ArgumentException($"An item with the same key already exists. Key: {key}"));
				} else if (!string.IsNullOrEmpty(key)) {
					dict.Add(key, translation);
				}
			}
			return dict;
		}
	}
}