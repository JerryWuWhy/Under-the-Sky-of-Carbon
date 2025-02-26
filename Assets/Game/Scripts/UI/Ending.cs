using Habby.Localization;
using UnityEngine;

public class Ending : MonoBehaviour {
	public LocalizedText endingDesc;

	public void ShowEnding(int id) {
		endingDesc.Key = $"ENDING_{id}";
		gameObject.SetActive(true);
	}

	public void OnClick() {
		DataManager.Inst.ResetData();
		GameManager.Inst.ShowLobby();
	}
}