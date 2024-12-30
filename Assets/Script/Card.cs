using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Card : MonoBehaviour
{
    public Button Yes;
    public Button No;
    public TextMeshProUGUI cardtext;
    public int cardid;

    public static Card Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OnYesClick()
    {
        cardid = Random.Range(1, 6);
        ConfigManager.CardConfig config = ConfigManager.Instance.GetHouseConfig(cardid);
        cardtext.text = (config.text);
        if (config != null)
        {
            Resource.Instance.money += config.money;
            Resource.Instance.technology += config.technology;
            Resource.Instance.prestige += config.prestige;
            Resource.Instance.carbon += config.carbon;
        }
    }

    public void OnNoClick()
    {
        cardid = Random.Range(1, 6);
        ConfigManager.CardConfig config = ConfigManager.Instance.GetHouseConfig(cardid);
        cardtext.text = (config.text);
    }
}

