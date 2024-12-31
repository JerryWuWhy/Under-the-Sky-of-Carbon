using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Script
{
    public class CardMovement : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform rectTransform;
        public RectTransform targetTrans;
        public float minOffset = 50;
        public float maxOffset = 60;
        public float edgeMoveOffset = 300;
        public float moveTime = 0.5f;
        public AnimationCurve rotateZ;

        public Animation animPrepareLeft;
        private const string LeftPrepareFadeIn = "LeftPrepareFadeIn";
        private const string LeftPrepareFadeOut = "LeftPrepareFadeOut";
        public Animation animPrepareRight;
        private const string RightPrepareFadeIn = "RightPrepareFadeIn";
        private const string RightPrepareFadeOut = "RightPrepareFadeOut";

        private bool _isPrepareLeft = false;
        private bool _isPrepareRight = false;

        public Action onSelectLeft;
        public Action onSelectRight;
        public Action onNewCard;

        private bool _isDragging = false;
        private Vector2 _startOriginPos = new Vector2(0, 60);
        private Vector2 _startClickPoint;

        private bool _isMoving;
        private float _endDropTime = 0;
        private float _startDropTime = 0;
        private float _startDropPos = 0;
        private float _targetDropPos = 0;

        public void Awake()
        {
            _startOriginPos = targetTrans.anchoredPosition;
            _endDropTime = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 将屏幕坐标转换为 RectTransform 的局部坐标
            // Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out _startClickPoint
            );
            // _startClickPoint = rectTransform.anchoredPosition - localPoint;
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var tmpClickPoint
                );
                Vector2 offset = tmpClickPoint - _startClickPoint;

                _PrepareLeft(offset.x < 0 && Mathf.Abs(offset.x) > minOffset);
                _PrepareRight(offset.x > 0 && Mathf.Abs(offset.x) > minOffset);

                SetTargetPos(_startOriginPos.x + Mathf.Clamp(offset.x, -maxOffset, maxOffset));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var tmpClickPoint
            );
            Vector2 offset = tmpClickPoint - _startClickPoint;
            if (Mathf.Abs(offset.x) > minOffset)
            {
                if (offset.x < 0)
                {
                    _SelectLeft();
                }
                else
                {
                    _SelectRight();
                }
            }
            else
            {
                _BackToCenter();
            }


            _isDragging = false;
        }

        private void _SelectLeft()
        {
            // Debug.Log("Left");

            _StartMove(-maxOffset - edgeMoveOffset, moveTime, NewCard);
            onSelectLeft?.Invoke();
        }

        public void _PrepareLeft(bool isPrepare)
        {
            if (isPrepare != _isPrepareLeft)
            {
                animPrepareRight.Stop();
                animPrepareRight.Rewind();
                animPrepareRight.Play(isPrepare ? RightPrepareFadeIn : RightPrepareFadeOut);
                _isPrepareLeft = isPrepare;
            }
        }

        private void _SelectRight()
        {
            // Debug.Log("Right");

            onSelectRight?.Invoke();
            _StartMove(maxOffset + edgeMoveOffset, moveTime, NewCard);
        }

        private void _PrepareRight(bool isPrepare)
        {
            if (isPrepare != _isPrepareRight)
            {
                animPrepareLeft.Stop();
                animPrepareLeft.Rewind();
                animPrepareLeft.Play(isPrepare ? LeftPrepareFadeIn : LeftPrepareFadeOut);
                _isPrepareRight = isPrepare;
            }
        }

        private void _BackToCenter()
        {
            // Debug.Log("Center");

            _StartMove(_startOriginPos.x, moveTime);
        }

        private void NewCard()
        {
            // Debug.Log("New Card");

            onNewCard?.Invoke();
            _StartMove(_startOriginPos.x, 0);
        }

        private void SetTargetPos(float offsetX)
        {
            targetTrans.rotation =
                Quaternion.Euler(0, 0, (offsetX > 0 ? 1 : -1) * rotateZ.Evaluate(Mathf.Abs(offsetX / maxOffset)));
            targetTrans.anchoredPosition = new Vector2(offsetX, _startOriginPos.y);
        }

        private Action _moveFinished = null;

        private void _StartMove(float posX, float time, Action onFinish = null)
        {
            _startDropPos = targetTrans.anchoredPosition.x;
            _targetDropPos = posX;

            _startDropTime = Time.time;
            _endDropTime = Time.time + time;

            _moveFinished = onFinish;

            _isMoving = true;
        }

        private void _Moving()
        {
            if (_isMoving)
            {
                if (Time.time < _endDropTime)
                {
                    SetTargetPos(
                        _startDropPos
                        + (_targetDropPos - _startDropPos)
                        * (Time.time - _startDropTime) / (_endDropTime - _startDropTime)
                    );
                }
                else
                {
                    _EndMove();
                }
            }
        }

        private void _EndMove()
        {
            _isMoving = false;
            SetTargetPos(_targetDropPos);
            _moveFinished?.Invoke();
        }

        public void Update()
        {
            if (_isMoving) _Moving();
        }
    }
}