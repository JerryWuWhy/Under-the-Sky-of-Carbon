using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Resource : MonoBehaviour
{
    public TextMeshProUGUI Money;
    public TextMeshProUGUI Technology;
    public TextMeshProUGUI Prestige;
    public int money;
    public int technology;
    public int prestige;
    public int carbon;
    public Image MoneyFillIncrease;
    public Image MoneyFillDecrease;
    public Image MoneyFillBlock;
    public Image TechnologyFillIncrease;
    public Image TechnologyFillDecrease;
    public Image TechnologyFillBlock;
    public Image PrestigeFillIncrease;
    public Image PrestigeFillDecrease;
    public Image PrestigeFillBlock;


    [SerializeField] private Image MoneyFill;
    public float currentmoney = 0.1f;

    [SerializeField] private Image TechnologyFill;
    public float currenttechnology = 0.1f;

    [SerializeField] private Image PrestigeFill;
    public float currentprestige = 0.1f;

    private void UpdateMoneyhBar()
    {
        MoneyFill.fillAmount = currentmoney;
    }

    private void UpdateTechnologyBar()
    {
        TechnologyFill.fillAmount = currenttechnology;
    }

    private void UpdatePrestigeBar()
    {
        PrestigeFill.fillAmount = currentprestige;
    }

    public static Resource Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadResources();
        MoneyFillIncrease.gameObject.SetActive(false);
        MoneyFillDecrease.gameObject.SetActive(false);
        MoneyFillBlock.gameObject.SetActive(false);
        TechnologyFillIncrease.gameObject.SetActive(false);
        TechnologyFillDecrease.gameObject.SetActive(false);
        TechnologyFillBlock.gameObject.SetActive(false);
        PrestigeFillIncrease.gameObject.SetActive(false);
        PrestigeFillDecrease.gameObject.SetActive(false);
        PrestigeFillBlock.gameObject.SetActive(false);
    }

    public void LoadResources()
    {
        money = DataManager.Inst.money;
        technology = DataManager.Inst.technology;
        prestige = DataManager.Inst.prestige;
        carbon = DataManager.Inst.carbon;
    }

    private void Update()
    {
        Money.text = (money.ToString());
        Technology.text = (technology.ToString());
        Prestige.text = (prestige.ToString());
        UpdateMoneyhBar();
        currentmoney = (Resource.Instance.money) / 1000f;
        UpdateTechnologyBar();
        currenttechnology = (Resource.Instance.technology) / 1000f;
        UpdatePrestigeBar();
        currentprestige = (Resource.Instance.prestige) / 1000f;

        if (money < 0)
        {
            Carbon.Instance.Ending.SetActive(true);
            Carbon.Instance.endtext.text = ("晤，你破产了。看起来你不擅长保护你的财产。现在你和你的城市负债累累，连生计都难以维持，哪有时间再去降低碳排放。");
        }

        if (technology < 0)
        {
            Carbon.Instance.Ending.SetActive(true);
            Carbon.Instance.endtext.text = ("长期以往对科技的忽视导致了技术的倒退。现在你们不过是一群住在名为高楼大厦的洞穴里的原始人罢了。");
        }

        if (prestige < 0)
        {
            Carbon.Instance.Ending.SetActive(true);
            Carbon.Instance.endtext.text = ("你无恶不作，人民们对你的忍耐达到了极限。即使付出巨大的代价人们也依旧头也不回地逃离了这座城市。你现在是唯一一位市民了。");
        }
    }
}