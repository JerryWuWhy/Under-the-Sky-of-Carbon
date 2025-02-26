using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Habby.Localization {
	public enum TextCase {
		Raw,
		UpperCase,
		LowerCase,
		TitleCase
	}

	[AddComponentMenu("UI/Text - Localized", 100)]
	public class LocalizedText : Text {
		public string Key {
			get => key;
			set => SetText(value);
		}

		public List<string> Arguments {
			get => arguments;
			set => SetArguments(value);
		}

		public bool localized = true;
		public TextCase textCase;
		public bool removeRichTags;

		[SerializeField] [LocalizationKey] private string key;
		[SerializeField] private List<string> arguments;

		private readonly UIVertex[] _tmpVerts = new UIVertex[4];

		protected override void Awake() {
			base.Awake();
			Localization.OnLanguageChange += OnLanguageChange;
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			Localization.OnLanguageChange -= OnLanguageChange;
		}

		private void UseFitSettings() {
			var settings = GetGenerationSettings(rectTransform.rect.size);
			settings.resizeTextForBestFit = false;

			if (!resizeTextForBestFit) {
				cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
				return;
			}

			var minSize = resizeTextMinSize;
			var txtLen = text.Length;
			for (var i = resizeTextMaxSize; i >= minSize; --i) {
				settings.fontSize = i;
				cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
				if (cachedTextGenerator.characterCountVisible == txtLen) break;
			}
		}

		protected override void OnPopulateMesh(VertexHelper toFill) {
			if (null == font) return;

			m_DisableFontTextureRebuiltCallback = true;
			UseFitSettings();

			// Apply the offset to the vertices
			var verts = cachedTextGenerator.verts;
			var unitsPerPixel = 1 / pixelsPerUnit;
			var vertCount = verts.Count;

			// We have no verts to process just return (case 1037923)
			if (vertCount <= 0) {
				toFill.Clear();
				return;
			}

			var roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
			toFill.Clear();
			if (roundingOffset != Vector2.zero) {
				for (var i = 0; i < vertCount; ++i) {
					var tempVertsIndex = i & 3;
					_tmpVerts[tempVertsIndex] = verts[i];
					_tmpVerts[tempVertsIndex].position *= unitsPerPixel;
					_tmpVerts[tempVertsIndex].position.x += roundingOffset.x;
					_tmpVerts[tempVertsIndex].position.y += roundingOffset.y;
					if (tempVertsIndex == 3)
						toFill.AddUIVertexQuad(_tmpVerts);
				}
			} else {
				for (var i = 0; i < vertCount; ++i) {
					var tempVertsIndex = i & 3;
					_tmpVerts[tempVertsIndex] = verts[i];
					_tmpVerts[tempVertsIndex].position *= unitsPerPixel;
					if (tempVertsIndex == 3)
						toFill.AddUIVertexQuad(_tmpVerts);
				}
			}

			m_DisableFontTextureRebuiltCallback = false;
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
			base.text = text;
			localized = false;
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

		protected override void OnEnable() {
			base.OnEnable();
			UpdateText();
		}

		private void OnLanguageChange() {
			UpdateText();
		}

#if UNITY_EDITOR
		protected override void Reset() {
			base.Reset();
			raycastTarget = false;
			supportRichText = false;
		}

		protected override void OnValidate() {
			base.OnValidate();
			UpdateText();
		}
#endif
	}
}