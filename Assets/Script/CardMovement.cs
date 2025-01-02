using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Script
{
    public class CardMovement : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static CardMovement Instance { get; private set; }
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
            Instance = this;
            _startOriginPos = targetTrans.anchoredPosition;
            _endDropTime = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out _startClickPoint
            );
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

            _PrepareLeft(false);
            _PrepareRight(false);

            _isDragging = false;
        }

        private void _SelectLeft()
        {
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
                if (isPrepare)
                {
                    Resource.Instance.MoneyFillDecrease.fillAmount = 0f;
                    Resource.Instance.MoneyFillIncrease.fillAmount = 0f;
                    Resource.Instance.MoneyFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.money >= 0)
                    {
                        Resource.Instance.MoneyFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.money + Resource.Instance.money) / 1000F;
                        Resource.Instance.MoneyFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.money < 0)
                    {
                        Resource.Instance.MoneyFillDecrease.fillAmount = Resource.Instance.money / 1000F;
                        Resource.Instance.MoneyFillBlock.fillAmount =
                            (Resource.Instance.money + Card.Instance.CurConfig.money) / 1000F;
                        Resource.Instance.MoneyFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.MoneyFillBlock.gameObject.SetActive(true);
                    }


                    Resource.Instance.TechnologyFillDecrease.fillAmount = 0f;
                    Resource.Instance.TechnologyFillIncrease.fillAmount = 0f;
                    Resource.Instance.TechnologyFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.technology >= 0)
                    {
                        Resource.Instance.TechnologyFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.technology + Resource.Instance.technology) / 1000F;
                        Resource.Instance.TechnologyFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.technology < 0)
                    {
                        Resource.Instance.TechnologyFillDecrease.fillAmount = Resource.Instance.technology / 1000F;
                        Resource.Instance.TechnologyFillBlock.fillAmount =
                            (Resource.Instance.technology + Card.Instance.CurConfig.technology) / 1000F;
                        Resource.Instance.TechnologyFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.TechnologyFillBlock.gameObject.SetActive(true);
                    }
                    
                    
                    Resource.Instance.PrestigeFillDecrease.fillAmount = 0f;
                    Resource.Instance.PrestigeFillIncrease.fillAmount = 0f;
                    Resource.Instance.PrestigeFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.prestige >= 0)
                    {
                        Resource.Instance.PrestigeFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.prestige + Resource.Instance.prestige) / 1000F;
                        Resource.Instance.PrestigeFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.prestige < 0)
                    {
                        Resource.Instance.PrestigeFillDecrease.fillAmount = Resource.Instance.prestige / 1000F;
                        Resource.Instance.PrestigeFillBlock.fillAmount =
                            (Resource.Instance.prestige + Card.Instance.CurConfig.prestige) / 1000F;
                        Resource.Instance.PrestigeFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.PrestigeFillBlock.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void _SelectRight()
        {
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
                if (isPrepare)
                {
                    Resource.Instance.MoneyFillDecrease.fillAmount = 0f;
                    Resource.Instance.MoneyFillIncrease.fillAmount = 0f;
                    Resource.Instance.MoneyFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.nomoney >= 0)
                    {
                        Resource.Instance.MoneyFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.nomoney + Resource.Instance.money) / 1000F;
                        Resource.Instance.MoneyFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.nomoney < 0)
                    {
                        Resource.Instance.MoneyFillDecrease.fillAmount = Resource.Instance.money / 1000F;
                        Resource.Instance.MoneyFillBlock.fillAmount =
                            (Resource.Instance.money + Card.Instance.CurConfig.nomoney) / 1000F;
                        Resource.Instance.MoneyFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.MoneyFillBlock.gameObject.SetActive(true);
                    }


                    Resource.Instance.TechnologyFillDecrease.fillAmount = 0f;
                    Resource.Instance.TechnologyFillIncrease.fillAmount = 0f;
                    Resource.Instance.TechnologyFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.notechnology >= 0)
                    {
                        Resource.Instance.TechnologyFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.notechnology + Resource.Instance.technology) / 1000F;
                        Resource.Instance.TechnologyFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.notechnology < 0)
                    {
                        Resource.Instance.TechnologyFillDecrease.fillAmount = Resource.Instance.technology / 1000F;
                        Resource.Instance.TechnologyFillBlock.fillAmount =
                            (Resource.Instance.technology + Card.Instance.CurConfig.notechnology) / 1000F;
                        Resource.Instance.TechnologyFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.TechnologyFillBlock.gameObject.SetActive(true);
                    }
                    
                    
                    Resource.Instance.PrestigeFillDecrease.fillAmount = 0f;
                    Resource.Instance.PrestigeFillIncrease.fillAmount = 0f;
                    Resource.Instance.PrestigeFillBlock.fillAmount = 0f;
                    if (Card.Instance.CurConfig.noprestige >= 0)
                    {
                        Resource.Instance.PrestigeFillIncrease.fillAmount =
                            (Card.Instance.CurConfig.noprestige + Resource.Instance.prestige) / 1000F;
                        Resource.Instance.PrestigeFillIncrease.gameObject.SetActive(true);
                    }

                    if (Card.Instance.CurConfig.noprestige < 0)
                    {
                        Resource.Instance.PrestigeFillDecrease.fillAmount = Resource.Instance.prestige / 1000F;
                        Resource.Instance.PrestigeFillBlock.fillAmount =
                            (Resource.Instance.prestige + Card.Instance.CurConfig.noprestige) / 1000F;
                        Resource.Instance.PrestigeFillDecrease.gameObject.SetActive(true);
                        Resource.Instance.PrestigeFillBlock.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void _BackToCenter()
        {
            _StartMove(_startOriginPos.x, moveTime);

            Resource.Instance.MoneyFillIncrease.gameObject.SetActive(false);
            Resource.Instance.MoneyFillDecrease.gameObject.SetActive(false);
            Resource.Instance.MoneyFillBlock.gameObject.SetActive(false);
            Resource.Instance.MoneyFillIncrease.fillAmount = 0f;
            Resource.Instance.MoneyFillDecrease.fillAmount = 0f;
            Resource.Instance.MoneyFillBlock.fillAmount = 0f;

            Resource.Instance.TechnologyFillIncrease.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillDecrease.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillBlock.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillIncrease.fillAmount = 0f;
            Resource.Instance.TechnologyFillDecrease.fillAmount = 0f;
            Resource.Instance.TechnologyFillBlock.fillAmount = 0f;
            
            Resource.Instance.PrestigeFillIncrease.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillDecrease.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillBlock.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillIncrease.fillAmount = 0f;
            Resource.Instance.PrestigeFillDecrease.fillAmount = 0f;
            Resource.Instance.PrestigeFillBlock.fillAmount = 0f;
        }

        private void NewCard()
        {
            onNewCard?.Invoke();
            _StartMove(_startOriginPos.x, 0);

            Resource.Instance.MoneyFillIncrease.gameObject.SetActive(false);
            Resource.Instance.MoneyFillDecrease.gameObject.SetActive(false);
            Resource.Instance.MoneyFillBlock.gameObject.SetActive(false);
            Resource.Instance.MoneyFillIncrease.fillAmount = 0f;
            Resource.Instance.MoneyFillDecrease.fillAmount = 0f;
            Resource.Instance.MoneyFillBlock.fillAmount = 0f;

            Resource.Instance.TechnologyFillIncrease.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillDecrease.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillBlock.gameObject.SetActive(false);
            Resource.Instance.TechnologyFillIncrease.fillAmount = 0f;
            Resource.Instance.TechnologyFillDecrease.fillAmount = 0f;
            Resource.Instance.TechnologyFillBlock.fillAmount = 0f;
            
            Resource.Instance.PrestigeFillIncrease.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillDecrease.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillBlock.gameObject.SetActive(false);
            Resource.Instance.PrestigeFillIncrease.fillAmount = 0f;
            Resource.Instance.PrestigeFillDecrease.fillAmount = 0f;
            Resource.Instance.PrestigeFillBlock.fillAmount = 0f;
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