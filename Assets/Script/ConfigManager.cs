using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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
        [TextArea] public string text;
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