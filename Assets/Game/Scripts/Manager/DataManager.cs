using System.Collections.Generic;
using Habby.Storage;
using UnityEngine;

public class DataManager : MonoBehaviour {
	public static DataManager Inst { get; private set; }

	public int money;
	public int tech;
	public int prestige;
	public int carbon;
	private StorageContainer _dataContainer;
	private StorageContainer _settingsContainer;

	public StorageProperty<List<int>> UnlockedEndings { get; private set; }
	public StorageProperty<bool> MusicOn { get; private set; }
	public StorageProperty<bool> SfxOn { get; private set; }

	private void Awake() {
		Inst = this;
		_dataContainer = Storage.GetContainer("Data");
		_settingsContainer = Storage.GetContainer("Settings");
		UnlockedEndings = _dataContainer.Get("UnlockedEndings", new List<int>());
		MusicOn = _settingsContainer.Get("MusicOn", true);
		SfxOn = _settingsContainer.Get("SfxOn", true);
	}

	public void LoadData() {
		money = _dataContainer.Get("money", ConfigManager.Inst.defMoney);
		tech = _dataContainer.Get("tech", ConfigManager.Inst.defTech);
		prestige = _dataContainer.Get("prestige", ConfigManager.Inst.defPrestige);
		carbon = _dataContainer.Get("carbon", ConfigManager.Inst.defCarbon);
	}

	public void SetData(int money, int tech, int prestige, int carbon) {
		this.money = money;
		this.tech = tech;
		this.prestige = prestige;
		this.carbon = carbon;
		_dataContainer.Set("money", money);
		_dataContainer.Set("tech", tech);
		_dataContainer.Set("prestige", prestige);
		_dataContainer.Set("carbon", carbon);
		_dataContainer.Save();
	}

	public void UnlockEnding(int endingId) {
		if (UnlockedEndings.Value.Contains(endingId)) {
			return;
		}
		UnlockedEndings.Value.Add(endingId);
		UnlockedEndings.Save();
	}

	public void ResetData() {
		SetData(
			ConfigManager.Inst.defMoney,
			ConfigManager.Inst.defTech,
			ConfigManager.Inst.defPrestige,
			ConfigManager.Inst.defCarbon
		);
	}
}