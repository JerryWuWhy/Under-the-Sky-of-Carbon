using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Habby.Localization {
	public class LocalizedAdapter : MonoBehaviour {
		public string Key {
			get => key;
			set => SetText(value);
		}

		public List<string> Arguments {
			get => arguments;
			set => SetArguments(value);
		}

		[TextArea(3, 10)] public string text;
		public bool localized = true;
		public TextCase textCase;
		public bool removeRichTags;
		[Space] public UnityEvent<string> onChange;

		[SerializeField] [LocalizationKey] private string key;
		[SerializeField] private List<string> arguments;

		private void Awake() {
			Localization.OnLanguageChange += OnLanguageChange;
		}

		private void OnDestroy() {
			Localization.OnLanguageChange -= OnLanguageChange;
		}

		public void SetText(string key, params string[] arguments) {
			this.key = key;
			this.arguments = arguments.ToList();
			localized = true;
			UpdateText();
		}

		public void SetArgument(int index, string value) {
			for (var i = arguments.Count - 1; i < index; ++i) {
				arguments.Add(string.Empty);
			}
			arguments[index] = value;
			UpdateText();
		}

		public void SetArguments(List<string> values) {
			arguments.Clear();
			arguments.AddRange(values);
			UpdateText();
		}

		public void SetArguments(params string[] values) {
			arguments.Clear();
			arguments.AddRange(values);
			UpdateText();
		}

		public void SetTextDirectly(string text) {
			this.text = text;
			localized = false;
			onChange?.Invoke(text);
		}

		private void UpdateText() {
			if (localized) {
				if (arguments == null || arguments.Count == 0) {
					text = GetFinalText(Localization.GetString(key));
				} else {
					var args = new object[arguments.Count];
					for (var i = 0; i < arguments.Count; ++i) {
						args[i] = arguments[i];
					}
					text = GetFinalText(Localization.GetStringFormat(key, args));
				}
			}
			onChange?.Invoke(text);
		}

		private string GetFinalText(string text) {
			var finalText = text;
			switch (textCase) {
				case TextCase.UpperCase:
					finalText = text.ToUpper();
					break;
				case TextCase.LowerCase:
					finalText = text.ToLower();
					break;
				case TextCase.TitleCase:
					finalText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
					break;
				case TextCase.Raw:
				default:
					break;
			}
			if (removeRichTags) {
				finalText = new Regex("</?[^>]+>").Replace(finalText, "");
			}
			return finalText;
		}

		private void OnEnable() {
			UpdateText();
		}

		private void OnLanguageChange() {
			UpdateText();
		}

#if UNITY_EDITOR
		private void OnValidate() {
			UpdateText();
		}
#endif
	}
}