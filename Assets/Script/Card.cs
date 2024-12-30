using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Button yes;
    public Button no;
    public Text cardText;
    public static Card Instance { get; private set; }
    private int _cardIndex;
    private ConfigManager.CardConfig _cardConfig;

    private void Awake()
    {
        Instance = this;
        NextCard();
    }

    private void NextCard()
    {
        var cardConfigs = ConfigManager.Instance.cardConfigs;
        _cardIndex = (_cardIndex + Random.Range(1, cardConfigs.Count)) % cardConfigs.Count;
        _cardConfig = cardConfigs[_cardIndex];
        cardText.text = string.Format(_cardConfig.text, _cardConfig.money, _cardConfig.technology, _cardConfig.prestige, _cardConfig.carbon);
    }

    public void OnYesClick()
    {
        if (Resource.Instance.money >= -(_cardConfig.money) && Resource.Instance.technology >= -(_cardConfig.technology) && Resource.Instance.prestige >= -(_cardConfig.prestige))
        {
            Resource.Instance.money += _cardConfig.money;
            Resource.Instance.technology += _cardConfig.technology;
            Resource.Instance.prestige += _cardConfig.prestige;
            Resource.Instance.carbon += _cardConfig.carbon;
            NextCard();
            DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige, Resource.Instance.carbon);
        }
    }

    public void OnNoClick()
    {
        NextCard();
    }
}