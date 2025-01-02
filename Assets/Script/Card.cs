using Script;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Card : MonoBehaviour
{
    public Text cardText;
    public Text yesText;
    public Text noText;
    public static Card Instance { get; private set; }
    public ConfigManager.CardConfig CurConfig { get; private set; }

    private int _cardIndex;
    public CardMovement cardMovement;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 120;

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
        CurConfig = cardConfigs[_cardIndex];
        cardText.text = CurConfig.text;
        yesText.text = string.Format(CurConfig.yestext, CurConfig.money, CurConfig.technology,
            CurConfig.prestige, CurConfig.carbon);
        noText.text = string.Format(CurConfig.notext, CurConfig.nomoney, CurConfig.notechnology,
            CurConfig.noprestige, CurConfig.nocarbon);
        Resource.Instance.MoneyFillDecrease.gameObject.SetActive(false);
        Resource.Instance.MoneyFillIncrease.gameObject.SetActive(false);
        Resource.Instance.TechnologyFillDecrease.gameObject.SetActive(false);
        Resource.Instance.TechnologyFillIncrease.gameObject.SetActive(false);
        Resource.Instance.PrestigeFillDecrease.gameObject.SetActive(false);
        Resource.Instance.PrestigeFillIncrease.gameObject.SetActive(false);
    }

    public void OnYesClick()
    {
        Resource.Instance.money += CurConfig.nomoney;
        Resource.Instance.technology += CurConfig.notechnology;
        Resource.Instance.prestige += CurConfig.noprestige;
        Resource.Instance.carbon += CurConfig.nocarbon;

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

        DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige,
            Resource.Instance.carbon);
    }

    public void OnNoClick()
    {
        Resource.Instance.money += CurConfig.money;
        Resource.Instance.technology += CurConfig.technology;
        Resource.Instance.prestige += CurConfig.prestige;
        Resource.Instance.carbon += CurConfig.carbon;
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

        DataManager.Inst.SetData(Resource.Instance.money, Resource.Instance.technology, Resource.Instance.prestige,
            Resource.Instance.carbon);

        NextCard();
    }
}