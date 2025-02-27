using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager Inst { get; private set; }
	public Lobby lobby;
	public Hud hud;
	public Ending ending;

	public CardConfig CurrentCard { get; private set; }
	public CardConfig NextCard { get; private set; }
	public float CarbonPercent => 1f - (float) DataManager.Inst.Carbon / ConfigManager.Inst.maxCarbon;
	public float MoneyPercent => (float) DataManager.Inst.Money / ConfigManager.Inst.maxMoney;
	public float TechPercent => (float) DataManager.Inst.Tech / ConfigManager.Inst.maxTech;
	public float PrestigePercent => (float) DataManager.Inst.Prestige / ConfigManager.Inst.maxPrestige;
	public bool GameOver { get; private set; }

	private void Awake() {
		Inst = this;
		Application.targetFrameRate = 60;
	}

	private void Start() {
		ShowLobby();
	}

	public void StartGame() {
		if (DataManager.Inst.CurrentCardId > 0 && DataManager.Inst.NextCardId > 0) {
			CurrentCard = ConfigManager.Inst.GetCardConfig(DataManager.Inst.CurrentCardId);
			NextCard = ConfigManager.Inst.GetCardConfig(DataManager.Inst.NextCardId);
		} else {
			SwitchCard();
		}
		GameOver = false;
		lobby.gameObject.SetActive(false);
		hud.gameObject.SetActive(true);
		hud.StartGame();
	}

	public void ShowLobby() {
		lobby.Refresh();
		lobby.gameObject.SetActive(true);
		hud.gameObject.SetActive(false);
		ending.gameObject.SetActive(false);
	}

	public void SelectLeft() {
		DataManager.Inst.Money.Value += CurrentCard.leftMoney;
		DataManager.Inst.Tech.Value += CurrentCard.leftTech;
		DataManager.Inst.Prestige.Value += CurrentCard.leftPrestige;
		DataManager.Inst.Carbon.Value += CurrentCard.leftCarbon;
		SwitchCard();
		CheckEnding();
	}

	public void SelectRight() {
		DataManager.Inst.Money.Value += CurrentCard.rightMoney;
		DataManager.Inst.Tech.Value += CurrentCard.rightTech;
		DataManager.Inst.Prestige.Value += CurrentCard.rightPrestige;
		DataManager.Inst.Carbon.Value += CurrentCard.rightCarbon;
		SwitchCard();
		CheckEnding();
	}

	private void CheckEnding() {
		if (DataManager.Inst.Carbon <= 0 || DataManager.Inst.Carbon >= ConfigManager.Inst.maxCarbon) {
			GameOver = true;
			CancelInvoke(nameof(End));
			Invoke(nameof(End), 0.8f);
		}
	}

	private void End() {
		var endingId = 0;
		hud.gameObject.SetActive(false);
		ending.gameObject.SetActive(true);
		if (DataManager.Inst.Carbon <= 0) {
			if (DataManager.Inst.Money >= DataManager.Inst.Prestige &&
			    DataManager.Inst.Money >= DataManager.Inst.Tech) {
				endingId = 1;
			} else if (DataManager.Inst.Tech >= DataManager.Inst.Money &&
			           DataManager.Inst.Tech >= DataManager.Inst.Prestige) {
				endingId = 2;
			} else if (DataManager.Inst.Prestige >= DataManager.Inst.Money &&
			           DataManager.Inst.Prestige >= DataManager.Inst.Tech) {
				endingId = 3;
			}
		} else if (DataManager.Inst.Carbon >= ConfigManager.Inst.maxCarbon) {
			if (DataManager.Inst.Money >= DataManager.Inst.Prestige &&
			    DataManager.Inst.Money >= DataManager.Inst.Tech) {
				endingId = 4;
			} else if (DataManager.Inst.Tech >= DataManager.Inst.Money &&
			           DataManager.Inst.Tech >= DataManager.Inst.Prestige) {
				endingId = 5;
			} else if (DataManager.Inst.Prestige >= DataManager.Inst.Money &&
			           DataManager.Inst.Prestige >= DataManager.Inst.Tech) {
				endingId = 6;
			}
		}
		DataManager.Inst.ResetGameData();
		DataManager.Inst.UnlockEnding(endingId);
		ending.ShowEnding(endingId);
	}

	private void SwitchCard() {
		CurrentCard = NextCard ?? ConfigManager.Inst.GetRandomCardConfig();
		NextCard = ConfigManager.Inst.GetRandomCardConfig();
		DataManager.Inst.CurrentCardId.Value = CurrentCard.id;
		DataManager.Inst.NextCardId.Value = NextCard.id;
		DataManager.Inst.SaveData();
	}
}