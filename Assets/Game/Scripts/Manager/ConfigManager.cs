using System;
using System.Collections.Generic;
using System.Linq;
using Habby.Localization;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class CardConfig {
	public int id;
	public int leftMoney;
	public int leftTech;
	public int leftPrestige;
	public int leftCarbon;
	public int rightMoney;
	public int rightTech;
	public int rightPrestige;
	public int rightCarbon;

	[LocalizationKey] public string desc;
	[LocalizationKey] public string choiceLeftTitle;
	[LocalizationKey] public string choiceLeftDesc;
	[LocalizationKey] public string choiceRightTitle;
	[LocalizationKey] public string choiceRightDesc;
}

public class ConfigManager : MonoBehaviour {
	public static ConfigManager Inst { get; private set; }

	private void Awake() {
		Inst = this;
	}

	public int defMoney = 333;
	public int defTech = 333;
	public int defPrestige = 333;
	public int defCarbon = 50;

	public int maxMoney = 666;
	public int maxTech = 666;
	public int maxPrestige = 666;
	public int maxCarbon = 100;

	public List<CardConfig> cardConfigs;

	public CardConfig GetCardConfig(int id) {
		return cardConfigs.FirstOrDefault(h => h.id == id);
	}

	public CardConfig GetRandomCardConfig() {
		return cardConfigs[Random.Range(0, cardConfigs.Count)];
	}
}