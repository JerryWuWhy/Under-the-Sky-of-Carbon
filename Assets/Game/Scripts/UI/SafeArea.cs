using UnityEngine;
using UnityEngine.UI;

public class SafeArea : MonoBehaviour {
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
		var offsetMin = safeArea.position * factor;
		var offsetMax = (safeArea.max - new Vector2(Screen.width, Screen.height)) * factor;
		_trans.offsetMin = offsetMin;
		_trans.offsetMax = offsetMax;
		_trans.hasChanged = false;
	}
}