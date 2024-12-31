using System;
using Script;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Card : MonoBehaviour
{
    public Button yes;
    public Button no;
    public Text cardText;
    public Text yesText;
    public Text noText;
    public static Card Instance { get; private set; }
    private int _cardIndex;
    private ConfigManager.CardConfig _cardConfig;
    public CardMovement cardMovement;

    private void Awake()
    {
        Instance = this;
        
        cardMovement.onSelectRight = OnYesClick;
        cardMovement.onSelectLeft = OnNoClick;
        cardMovement.onNewCard = NextCard;
    }

    private void Start()
    {
        NextCard();
    }

    private void NextCard()
    {
        var cardConfigs = ConfigManager.Instance.cardConfigs;
        _cardIndex = (_cardIndex + Random.Range(1, cardConfigs.Count)) % cardConfigs.Count;
        _cardConfig = cardConfigs[_cardIndex];
        cardText.text = _cardConfig.text;
        yesText.text = string.Format(_cardConfig.yestext, _cardConfig.money, _cardConfig.technology, _cardConfig.prestige, _cardConfig.carbon);
        noText.text = string.Format(_cardConfig.notext, _cardConfig.nomoney, _cardConfig.notechnology, _cardConfig.noprestige, _cardConfig.nocarbon);
    }

    public void OnYesClick()
    {
        
            Resource.Instance.money += _cardConfig.money;
            Resource.Instance.technology += _cardConfig.technology;
            Resource.Instance.prestige += _cardConfig.prestige;
            Resource.Instance.carbon += _cardConfig.carbon;
            if (Resource.Instance.money >= 1000)
            {
                Resource.Instance.money = 1000;
            }
            if (Resource.Instance.technology >= 1000)
            {
                Resource.Instance.technology = 1000;
            }
            if (Resource.Instance.prestige >= 1000)
            {
                Resource.Instance.prestige = 1000;
            }
            DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige, Resource.Instance.carbon);
        
    }

    public void OnNoClick()
    {
        
            Resource.Instance.money += _cardConfig.nomoney;
            Resource.Instance.technology += _cardConfig.notechnology;
            Resource.Instance.prestige += _cardConfig.noprestige;
            Resource.Instance.carbon += _cardConfig.nocarbon;
            NextCard();
            if (Resource.Instance.money >= 1000)
            {
                Resource.Instance.money = 1000;
            }
            if (Resource.Instance.technology >= 1000)
            {
                Resource.Instance.technology = 1000;
            }
            if (Resource.Instance.prestige >= 1000)
            {
                Resource.Instance.prestige = 1000;
            }
            DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige, Resource.Instance.carbon);
        
        NextCard();
    }
}