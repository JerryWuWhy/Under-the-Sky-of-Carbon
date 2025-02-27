using Habby.Localization;
using UnityEngine;

public class Ending : MonoBehaviour {
	public LocalizedText endingDesc;

	public void ShowEnding(int id) {
		var endingConfig = ConfigManager.Inst.GetEndingConfig(id);
		endingDesc.Key = endingConfig.desc;
		gameObject.SetActive(true);
	}

	public void OnClick() {
		GameManager.Inst.ShowLobby();
	}
}