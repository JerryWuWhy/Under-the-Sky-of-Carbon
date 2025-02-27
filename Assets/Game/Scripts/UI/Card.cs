using UnityEngine;

public class Card : MonoBehaviour {
	public RectTransform root;
	public GameObject cardGo;
	public float cardScale;

	public void Setup(CardConfig cardConfig) {
		if (cardGo) {
			Destroy(cardGo);
		}
		var org = Resources.Load<GameObject>(cardConfig.filepath);
		cardGo = Instantiate(org, root);
		cardGo.transform.localScale = new Vector3(cardScale, cardScale, cardScale);
	}
}