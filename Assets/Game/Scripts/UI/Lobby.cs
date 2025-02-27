using Habby.Localization;
using UnityEngine;

public class Lobby : MonoBehaviour {
	public LocalizedText unlockedEndings;

	public void Refresh() {
		unlockedEndings.SetArguments(
			DataManager.Inst.UnlockedEndings.Value.Count.ToString(),
			ConfigManager.Inst.endingConfigs.Count.ToString()
		);
	}
}