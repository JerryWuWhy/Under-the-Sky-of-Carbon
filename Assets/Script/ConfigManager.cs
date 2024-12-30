using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public String text;
    }
    
    public List<CardConfig> cardconfig;
    public CardConfig GetHouseConfig(int id)
    {
        return cardconfig.FirstOrDefault(h => h.id == id);
    }
    public List<CardConfig> GetConfigsById(int id)
    {
        return cardconfig.Where(h => h.id == id).ToList();
    }
    
}
