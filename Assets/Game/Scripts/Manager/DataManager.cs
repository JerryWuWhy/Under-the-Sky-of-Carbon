using System.Collections.Generic;
using Habby.Storage;
using UnityEngine;

public class DataManager : MonoBehaviour {
	public static DataManager Inst { get; private set; }

	public StorageProperty<int> Money { get; private set; }
	public StorageProperty<int> Tech { get; private set; }
	public StorageProperty<int> Prestige { get; private set; }
	public StorageProperty<int> Carbon { get; private set; }
	public StorageProperty<int> CurrentCardId { get; private set; }
	public StorageProperty<int> NextCardId { get; private set; }
	public StorageProperty<List<int>> UnlockedEndings { get; private set; }
	public StorageProperty<bool> MusicOn { get; private set; }
	public StorageProperty<bool> SfxOn { get; private set; }

	private StorageContainer _dataContainer;
	private StorageContainer _settingsContainer;

	private void Awake() {
		Inst = this;
		_dataContainer = Storage.GetContainer("Data");
		_settingsContainer = Storage.GetContainer("Settings");
		Money = _dataContainer.Get("money", ConfigManager.Inst.defMoney);
		Tech = _dataContainer.Get("tech", ConfigManager.Inst.defTech);
		Prestige = _dataContainer.Get("prestige", ConfigManager.Inst.defPrestige);
		Carbon = _dataContainer.Get("carbon", ConfigManager.Inst.defCarbon);
		CurrentCardId = _dataContainer.Get("CurrentCardId", 0);
		NextCardId = _dataContainer.Get("NextCardId", 0);
		UnlockedEndings = _dataContainer.Get("UnlockedEndings", new List<int>());
		MusicOn = _settingsContainer.Get("MusicOn", true);
		SfxOn = _settingsContainer.Get("SfxOn", true);
	}

	public void UnlockEnding(int endingId) {
		if (UnlockedEndings.Value.Contains(endingId)) {
			return;
		}
		UnlockedEndings.Value.Add(endingId);
		UnlockedEndings.Save();
	}

	public void ResetGameData() {
		Money.Value = ConfigManager.Inst.defMoney;
		Tech.Value = ConfigManager.Inst.defTech;
		Prestige.Value = ConfigManager.Inst.defPrestige;
		Carbon.Value = ConfigManager.Inst.defCarbon;
		CurrentCardId.Value = 0;
		NextCardId.Value = 0;
		SaveData();
	}

	public void SaveData() {
		_dataContainer.Save();
	}
}