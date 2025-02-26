using System;
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaStretcher : MonoBehaviour {
	[Flags]
	public enum Direction {
		Top = 1 << 0,
		Left = 1 << 1,
		Right = 1 << 2,
		Bottom = 1 << 3,
		Everything = Top | Left | Right | Bottom
	}

	public Direction direction = Direction.Everything;

	private RectTransform _trans;
	private CanvasScaler _canvasScaler;

	private void Awake() {
		_trans = (RectTransform) transform;
		_canvasScaler = GetComponentInParent<CanvasScaler>();
	}

	private void Update() {
		if (!_trans.hasChanged) return;
		var safeArea = Screen.safeArea;
		var factor = _canvasScaler.referenceResolution.x * (1f - _canvasScaler.matchWidthOrHeight) / Screen.width +
		             _canvasScaler.referenceResolution.y * _canvasScaler.matchWidthOrHeight / Screen.height;
		var offsetMin = -safeArea.position * factor;
		var offsetMax = -(safeArea.max - new Vector2(Screen.width, Screen.height)) * factor;
		var actualMin = _trans.offsetMin;
		var actualMax = _trans.offsetMax;
		if (direction.HasFlag(Direction.Top)) {
			actualMax.y = offsetMax.y;
		}
		if (direction.HasFlag(Direction.Left)) {
			actualMin.x = offsetMin.x;
		}
		if (direction.HasFlag(Direction.Right)) {
			actualMax.x = offsetMax.x;
		}
		if (direction.HasFlag(Direction.Bottom)) {
			actualMin.y = offsetMin.y;
		}
		_trans.offsetMin = actualMin;
		_trans.offsetMax = actualMax;
		_trans.hasChanged = false;
	}
}