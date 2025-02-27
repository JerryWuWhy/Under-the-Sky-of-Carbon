using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager Inst { get; private set; }
	public Lobby lobby;
	public Hud hud;
	public Ending ending;

	public CardConfig CurrentCard { get; private set; }
	public CardConfig NextCard { get; private set; }
	public float CarbonPercent => 1f - (float) DataManager.Inst.carbon / ConfigManager.Inst.maxCarbon;
	public float MoneyPercent => (float) DataManager.Inst.money / ConfigManager.Inst.maxMoney;
	public float TechPercent => (float) DataManager.Inst.tech / ConfigManager.Inst.maxTech;
	public float PrestigePercent => (float) DataManager.Inst.prestige / ConfigManager.Inst.maxPrestige;
	public bool GameOver { get; private set; }

	private void Awake() {
		Inst = this;
		Application.targetFrameRate = 60;
	}

	private void Start() {
		lobby.Refresh();
	}

	public void StartGame() {
		GameOver = false;
		DataManager.Inst.LoadData();
		SwitchCard();
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
		DataManager.Inst.SetData(
			DataManager.Inst.money + CurrentCard.leftMoney,
			DataManager.Inst.tech + CurrentCard.leftTech,
			DataManager.Inst.prestige + CurrentCard.leftPrestige,
			DataManager.Inst.carbon + CurrentCard.leftCarbon
		);
		SwitchCard();
		CheckEnding();
	}

	public void SelectRight() {
		DataManager.Inst.SetData(
			DataManager.Inst.money + CurrentCard.rightMoney,
			DataManager.Inst.tech + CurrentCard.rightTech,
			DataManager.Inst.prestige + CurrentCard.rightPrestige,
			DataManager.Inst.carbon + CurrentCard.rightCarbon
		);
		SwitchCard();
		CheckEnding();
	}

	private void CheckEnding() {
		if (DataManager.Inst.carbon <= 0 || DataManager.Inst.carbon >= ConfigManager.Inst.maxCarbon) {
			GameOver = true;
			CancelInvoke(nameof(End));
			Invoke(nameof(End), 0.5f);
		}
	}

	private void End() {
		hud.gameObject.SetActive(false);
		ending.gameObject.SetActive(true);
		if (DataManager.Inst.carbon <= 0) {
			if (DataManager.Inst.money >= DataManager.Inst.prestige &&
			    DataManager.Inst.money >= DataManager.Inst.tech) {
				DataManager.Inst.UnlockEnding(1);
				ending.ShowEnding(1);
			} else if (DataManager.Inst.tech >= DataManager.Inst.money &&
			           DataManager.Inst.tech >= DataManager.Inst.prestige) {
				DataManager.Inst.UnlockEnding(2);
				ending.ShowEnding(2);
			} else if (DataManager.Inst.prestige >= DataManager.Inst.money &&
			           DataManager.Inst.prestige >= DataManager.Inst.tech) {
				DataManager.Inst.UnlockEnding(3);
				ending.ShowEnding(3);
			}
		} else if (DataManager.Inst.carbon >= ConfigManager.Inst.maxCarbon) {
			if (DataManager.Inst.money >= DataManager.Inst.prestige &&
			    DataManager.Inst.money >= DataManager.Inst.tech) {
				DataManager.Inst.UnlockEnding(4);
				ending.ShowEnding(4);
			} else if (DataManager.Inst.tech >= DataManager.Inst.money &&
			           DataManager.Inst.tech >= DataManager.Inst.prestige) {
				DataManager.Inst.UnlockEnding(5);
				ending.ShowEnding(5);
			} else if (DataManager.Inst.prestige >= DataManager.Inst.money &&
			           DataManager.Inst.prestige >= DataManager.Inst.tech) {
				DataManager.Inst.UnlockEnding(6);
				ending.ShowEnding(6);
			}
		}
	}

	private void SwitchCard() {
		CurrentCard = NextCard ?? ConfigManager.Inst.GetRandomCardConfig();
		NextCard = ConfigManager.Inst.GetRandomCardConfig();
	}
}