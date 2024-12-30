using  System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public TextMeshProUGUI Money;
    public TextMeshProUGUI Technology;
    public TextMeshProUGUI Prestige;
    public int money;
    public int technology;
    public int prestige;
    public int carbon;
    
    public static Resource Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
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
    }
}
