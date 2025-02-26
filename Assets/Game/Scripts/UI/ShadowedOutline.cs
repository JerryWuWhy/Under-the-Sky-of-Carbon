using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/ShadowedOutline")]
public class ShadowedOutline : Shadow {
	public float shadowDistance = 6f;

	private const float EffectScale = 1.5f;

	private Text _text;

	private Text Text => _text == null ? _text = GetComponent<Text>() : _text;

	public override void ModifyMesh(VertexHelper vh) {
		if (!IsActive()) {
			return;
		}

		var shadowPosList = new List<Vector2> {
			new Vector2(0, 0),
			new Vector2(0, -shadowDistance),
		};

		var verts = ListPool<UIVertex>.Get();
		vh.GetUIVertexStream(verts);

		var effectDistance = this.effectDistance;
		var neededCapacity = verts.Count * shadowPosList.Count * (1 + 4);
		if (verts.Capacity < neededCapacity)
			verts.Capacity = neededCapacity;
		var start = 0;
		var end = verts.Count;
		var downScale = Text == null ? 0.8f : Text.fontSize * 0.02f;

		foreach (var pos in shadowPosList) {
			var distance = downScale * pos;
			if (pos != Vector2.zero) {
				ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, distance.x, distance.y);
				start = end;
				end = verts.Count;
			}

			ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, distance.x + effectDistance.x * EffectScale,
				distance.y + effectDistance.y * EffectScale);
			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, distance.x + effectDistance.x * EffectScale,
				distance.y - effectDistance.y * EffectScale);
			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, distance.x - effectDistance.x * EffectScale,
				distance.y + effectDistance.y * EffectScale);
			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, distance.x - effectDistance.x * EffectScale,
				distance.y - effectDistance.y * EffectScale);
			start = end;
			end = verts.Count;
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
		ListPool<UIVertex>.Release(verts);
	}
}