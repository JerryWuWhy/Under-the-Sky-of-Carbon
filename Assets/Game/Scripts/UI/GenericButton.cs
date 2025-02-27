using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class GenericButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
	public RectTransform wrapper;
	public float scale = 0.8f;
	public bool interactable = true;
	public UnityEvent onClick;
	[SoundName] public string tapSound = "Tap";

	private int _tweenId;

	private void Awake() {
		if (!GetComponent<Graphic>()) {
			gameObject.AddComponent<RaycastBlocker>();
		}
	}

	public void OnPointerDown(PointerEventData e) {
		if (interactable && wrapper) {
			LeanTween.cancel(_tweenId);
			_tweenId = LeanTween
				.scale(wrapper, Vector3.one * scale, 0.1f)
				.setIgnoreTimeScale(true)
				.uniqueId;
		}
		if (interactable) {
			// VibrationMgr.Inst.Vibrate(VibrationType.Tick);
		} else {
			e.eligibleForClick = false;
		}
	}

	public void OnPointerUp(PointerEventData e) {
		if (wrapper && e.eligibleForClick) {
			LeanTween.cancel(_tweenId);
			if (interactable) {
				wrapper.localScale = Vector3.one * scale;
				_tweenId = LeanTween
					.scale(wrapper, Vector3.one, 0.3f)
					.setIgnoreTimeScale(true)
					.setEaseOutElastic()
					.uniqueId;
			} else {
				wrapper.localScale = Vector3.one;
			}
		}
	}

	public void OnPointerClick(PointerEventData e) {
		if (interactable && e.eligibleForClick) {
			onClick.Invoke();
			SoundManager.Inst.Play2D(tapSound);
		}
	}
}