using System;
using Script;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Card : MonoBehaviour
{
    public Button yes;
    public Button no;
    public Text cardText;
    public Text leftText;
    public Text rightText;
    public static Card Instance { get; private set; }
    private int _cardIndex;
    private ConfigManager.CardConfig _cardConfig;

    public CardMovement cardMovement;

    private void Awake()
    {
        Instance = this;
        
        cardMovement.onSelectLeft = OnNoClick;
        cardMovement.onSelectRight = OnYesClick;

        cardMovement.onNewCard = NextCard;
    }

    private void Start()
    {
        NextCard();
    }

    private void NextCard()
    {
        // Debug.Log("NextCard");
        var cardConfigs = ConfigManager.Instance.cardConfigs;
        _cardIndex = (_cardIndex + Random.Range(1, cardConfigs.Count)) % cardConfigs.Count;
        _cardConfig = cardConfigs[_cardIndex];
        cardText.text = string.Format(_cardConfig.text, _cardConfig.money, _cardConfig.technology, _cardConfig.prestige,
            _cardConfig.carbon);
    }

    public void OnYesClick()
    {
        if (Resource.Instance.money >= -(_cardConfig.money) &&
            Resource.Instance.technology >= -(_cardConfig.technology) &&
            Resource.Instance.prestige >= -(_cardConfig.prestige))
        {
            Resource.Instance.money += _cardConfig.money;
            Resource.Instance.technology += _cardConfig.technology;
            Resource.Instance.prestige += _cardConfig.prestige;
            Resource.Instance.carbon += _cardConfig.carbon;
            DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige,
                Resource.Instance.carbon);
        }
    }

    public void OnNoClick()
    {
        // NextCard();
    }
}