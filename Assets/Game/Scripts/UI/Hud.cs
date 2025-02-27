using System;
using Game.Scripts.Ux;
using Habby.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hud : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
	public static Hud Instance { get; private set; }

	public Slider carbonSlider;
	public Image moneyFill;
	public FeedbackPlayer moneyUpFeedback;
	public FeedbackPlayer moneyDownFeedback;
	public Image techFill;
	public FeedbackPlayer techUpFeedback;
	public FeedbackPlayer techDownFeedback;
	public Image prestigeFill;
	public FeedbackPlayer prestigeUpFeedback;
	public FeedbackPlayer prestigeDownFeedback;
	public RectTransform cardTrans;
	public RectTransform leftChoiceTrans;
	public RectTransform rightChoiceTrans;
	public LocalizedText carDesc;
	public LocalizedText choiceLeftTitle;
	public LocalizedText choiceLeftDesc;
	public LocalizedText choiceRightTitle;
	public LocalizedText choiceRightDesc;
	public float minOffset = 100;
	public float maxOffset = 500;
	public float dropOffset = 500;
	public float dropDuration = 0.2f;
	public AnimationCurve rotateZ;

	private RectTransform _trans;
	private bool _isPrepareLeft;
	private bool _isPrepareRight;
	private bool _isDragging;
	private Vector2 _startOriginPos;
	private Vector2 _startClickPoint;
	private bool _isMoving;
	private float _endDropTime;
	private float _startDropTime;
	private float _startDropPos;
	private float _targetDropPos;
	private Action _moveFinished;

	public void Awake() {
		Instance = this;
		_trans = transform as RectTransform;
		_startOriginPos = cardTrans.anchoredPosition;
	}

	private void OnEnable() {
		SetTargetPos(0f);
		UpdateCard();
	}

	private Vector2 GetEventPos(PointerEventData eventData) {
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_trans,
			eventData.position,
			eventData.pressEventCamera,
			out var pos
		);
		return pos;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (GameManager.Inst.GameOver) {
			return;
		}
		_startClickPoint = GetEventPos(eventData);
		_isDragging = true;
	}

	public void OnDrag(PointerEventData eventData) {
		if (_isDragging) {
			var offset = GetEventPos(eventData) - _startClickPoint;
			_PrepareLeft(offset.x < 0 && Mathf.Abs(offset.x) > minOffset);
			_PrepareRight(offset.x > 0 && Mathf.Abs(offset.x) > minOffset);
			SetTargetPos(_startOriginPos.x + Mathf.Clamp(offset.x, -maxOffset, maxOffset));
		}
	}

	public void OnPointerUp(PointerEventData eventData) {
		var offset = GetEventPos(eventData) - _startClickPoint;
		if (Mathf.Abs(offset.x) > minOffset) {
			if (offset.x < 0) {
				_SelectLeft();
			} else {
				_SelectRight();
			}
		} else {
			_BackToCenter();
		}

		_PrepareLeft(false);
		_PrepareRight(false);

		_isDragging = false;
	}

	private void _SelectLeft() {
		GameManager.Inst.SelectLeft();
		_StartMove(-maxOffset - dropOffset, dropDuration, NewCard);
	}

	public void _PrepareLeft(bool isPrepare) {
		if (isPrepare != _isPrepareLeft) {
			if (isPrepare) {
				PlayFeedback(GameManager.Inst.CurrentCard.leftMoney, moneyUpFeedback, moneyDownFeedback);
				PlayFeedback(GameManager.Inst.CurrentCard.leftTech, techUpFeedback, techDownFeedback);
				PlayFeedback(GameManager.Inst.CurrentCard.leftPrestige, prestigeUpFeedback, prestigeDownFeedback);
			} else {
				ResetFeedbacks();
			}
			_isPrepareLeft = isPrepare;
		}
	}

	private void _SelectRight() {
		GameManager.Inst.SelectRight();
		_StartMove(maxOffset + dropOffset, dropDuration, NewCard);
	}

	private void _PrepareRight(bool isPrepare) {
		if (isPrepare != _isPrepareRight) {
			if (isPrepare) {
				PlayFeedback(GameManager.Inst.CurrentCard.rightMoney, moneyUpFeedback, moneyDownFeedback);
				PlayFeedback(GameManager.Inst.CurrentCard.rightTech, techUpFeedback, techDownFeedback);
				PlayFeedback(GameManager.Inst.CurrentCard.rightPrestige, prestigeUpFeedback, prestigeDownFeedback);
			} else {
				ResetFeedbacks();
			}
			_isPrepareRight = isPrepare;
		}
	}

	private void PlayFeedback(float value, FeedbackPlayer upPlayer, FeedbackPlayer downPlayer) {
		if (value < 0) {
			upPlayer.Reset();
			downPlayer.Play();
		} else if (value > 0) {
			downPlayer.Reset();
			upPlayer.Play();
		} else {
			upPlayer.Reset();
			downPlayer.Reset();
		}
	}

	private void ResetFeedbacks() {
		moneyDownFeedback.Reset();
		moneyUpFeedback.Reset();
		techDownFeedback.Reset();
		techUpFeedback.Reset();
		prestigeDownFeedback.Reset();
		prestigeUpFeedback.Reset();
	}

	private void _BackToCenter() {
		ResetFeedbacks();
		_StartMove(_startOriginPos.x, dropDuration);
	}

	private void NewCard() {
		UpdateCard();
		ResetFeedbacks();
		_StartMove(_startOriginPos.x, 0);
	}

	private void UpdateCard() {
		carDesc.Key = GameManager.Inst.CurrentCard.desc;
		choiceLeftTitle.Key = GameManager.Inst.CurrentCard.choiceLeftTitle;
		choiceLeftDesc.Key = GameManager.Inst.CurrentCard.choiceLeftDesc;
		choiceRightTitle.Key = GameManager.Inst.CurrentCard.choiceRightTitle;
		choiceRightDesc.Key = GameManager.Inst.CurrentCard.choiceRightDesc;
	}

	private void SetTargetPos(float offsetX) {
		if (offsetX >= 0f) {
			var pos = leftChoiceTrans.anchoredPosition;
			pos.y = Mathf.Lerp(leftChoiceTrans.rect.height, 0f, Mathf.Abs(offsetX / minOffset));
			leftChoiceTrans.anchoredPosition = pos;
			leftChoiceTrans.rotation = Quaternion.identity;
		}
		if (offsetX <= 0f) {
			var pos = rightChoiceTrans.anchoredPosition;
			pos.y = Mathf.Lerp(rightChoiceTrans.rect.height, 0f, Mathf.Abs(offsetX / minOffset));
			rightChoiceTrans.anchoredPosition = pos;
			rightChoiceTrans.rotation = Quaternion.identity;
		}
		cardTrans.rotation = Quaternion.Euler(0, 0, -Mathf.Sign(offsetX) * rotateZ.Evaluate(Mathf.Abs(offsetX / maxOffset)));
		cardTrans.anchoredPosition = new Vector2(offsetX, _startOriginPos.y);
	}

	private void _StartMove(float posX, float time, Action onFinish = null) {
		_startDropPos = cardTrans.anchoredPosition.x;
		_targetDropPos = posX;

		_startDropTime = Time.time;
		_endDropTime = Time.time + time;

		_moveFinished = onFinish;

		_isMoving = true;
	}

	private void _Moving() {
		if (_isMoving) {
			if (Time.time < _endDropTime) {
				SetTargetPos(
					_startDropPos
					+ (_targetDropPos - _startDropPos)
					* (Time.time - _startDropTime) / (_endDropTime - _startDropTime)
				);
			} else {
				_EndMove();
			}
		}
	}

	private void _EndMove() {
		_isMoving = false;
		SetTargetPos(0f);
		_moveFinished?.Invoke();
	}

	public void Update() {
		if (_isMoving) {
			_Moving();
		}
		carbonSlider.value = Mathf.Lerp(
			carbonSlider.value, 1f - (float) DataManager.Inst.carbon / ConfigManager.Inst.maxCarbon, Time.deltaTime * 5f);
		moneyFill.fillAmount = Mathf.Lerp(
			moneyFill.fillAmount, (float) DataManager.Inst.money / ConfigManager.Inst.maxMoney, Time.deltaTime * 5f);
		techFill.fillAmount = Mathf.Lerp(
			techFill.fillAmount, (float) DataManager.Inst.tech / ConfigManager.Inst.maxTech, Time.deltaTime * 5f);
		prestigeFill.fillAmount = Mathf.Lerp(
			prestigeFill.fillAmount, (float) DataManager.Inst.prestige / ConfigManager.Inst.maxPrestige, Time.deltaTime * 5f);
	}
}