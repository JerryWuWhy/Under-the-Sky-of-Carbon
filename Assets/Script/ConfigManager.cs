using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Serializable]
    public class CardConfig
    {
        public int id;
        public int money;
        public int technology;
        public int prestige;
        public int carbon;
        
        public int nomoney;
        public int notechnology;
        public int noprestige;
        public int nocarbon;
        
        [TextArea] public string text;
        [TextArea] public string yestext;
        [TextArea] public string notext;

    }

    public List<CardConfig> cardConfigs;

    public CardConfig GetCardConfig(int id)
    {
        return cardConfigs.FirstOrDefault(h => h.id == id);
    }

    public CardConfig GetRandomCardConfig()
    {
        return cardConfigs[Random.Range(0, cardConfigs.Count)];
    }

    public List<CardConfig> GetConfigsById(int id)
    {
        return cardConfigs.Where(h => h.id == id).ToList();
    }
}