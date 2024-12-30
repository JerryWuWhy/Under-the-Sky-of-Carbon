using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public TextMeshProUGUI Money;
    public TextMeshProUGUI Technology;
    public TextMeshProUGUI Prestige;
    public float money;
    public float technology;
    public float prestige;
    public float carbon;
    
    public static Resource Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        Money.text = (money.ToString());
        Technology.text = (technology.ToString());
        Prestige.text = (prestige.ToString());
    }
}
