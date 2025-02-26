// using UnityEngine;
// using UnityEngine.UI;
//
// public class Carbon : MonoBehaviour {
// 	[SerializeField] private Image CarbonFill;
// 	public float currentHealth = 1f;
// 	public GameObject Ending;
// 	public Text endtext;
//
// 	public static Carbon Instance { get; private set; }
//
// 	private void Awake() {
// 		Instance = this;
// 	}
//
// 	void Update() {
// 		UpdateHealthBar();
// 		currentHealth = (GameManager.Instance.carbon) / 100f;
// 		if (currentHealth <= 0) {
// 			Ending.SetActive(true);
// 			if (GameManager.Instance.money >= GameManager.Instance.prestige &&
// 			    GameManager.Instance.money >= GameManager.Instance.technology) {
// 				endtext.text =
// 					("你成功了。市民们在你的带领下实现了完美的无碳生活。再也不会有一丝一毫的二氧化碳及其他温室气体被肆意排放到空中了。人们总是打量着自己的富足生活感叹：难以想象不久之前我们还要通过扩建污染性强的制造业工厂去获取微博的薪水。哈哈，现在，我们从别的国家买就好了。");
// 			}
// 			if (GameManager.Instance.technology >= GameManager.Instance.money &&
// 			    GameManager.Instance.technology >= GameManager.Instance.prestige) {
// 				endtext.text =
// 					("你成功了。虽然温室气体仍旧在被源源不断地生产，但是对于拥有者远超邻国科技数个年代的你来说这不成问题。什么温室气体，二氧化碳，不过本质上还是一些特定排列的分子罢了。科技的大手会重组它们，无害化它们，为本市的碳中和计划再添一笔。");
// 			}
// 			if (GameManager.Instance.prestige >= GameManager.Instance.money &&
// 			    GameManager.Instance.prestige >= GameManager.Instance.technology) {
// 				endtext.text =
// 					("你成功了。靠着树立的可靠，为人民服务的形象，你成功抑制了人们肆意排放温室气体的冲动。在你的强力手腕下，本市的所有车辆均已被电动化，工厂均在处理后不产生任何有害气体。同时，靠着极其广泛的人脉，你聘请来了众多贝诺尔得主致力于从物理层面消灭排放。但你们还有的是时间。");
// 			}
// 		}
// 		if (currentHealth >= 1) {
// 			Ending.SetActive(true);
// 			if (GameManager.Instance.money >= GameManager.Instance.prestige &&
// 			    GameManager.Instance.money >= GameManager.Instance.technology) {
// 				endtext.text =
// 					("你一败涂地。在花费了大量精力，科技，和权威尝试阻止事态崩溃后，天上的星星，月亮，和太阳还是都消失了。城市被笼罩上了一层永远驱逐不走的雾霾屏障，同时也熄灭了名为希望的火苗。但没关系——你有钱啊！权贵们已经从中赚取了数不尽的财富，跑到随便一个北欧国家，太平洋小岛去享受人生也是一种乐趣啊。");
// 			}
// 			if (GameManager.Instance.technology >= GameManager.Instance.money &&
// 			    GameManager.Instance.technology >= GameManager.Instance.prestige) {
// 				endtext.text =
// 					("你一败涂地。曾经你以为科技可以从根本上解决难题，但是人们好像更倾向于加强开采机的效率，加大工厂的生产效率，而非去研发什么虚无缥缈的可回收能源解决方案。毕竟谁会和利益过不去啊！只有傻子才会取研发还无油水可榨的新能源技术。看着面前壮观的排排黑烟以及直插云霄的巨型提炼机，你无奈但欣慰地笑了。");
// 			}
// 			if (GameManager.Instance.prestige >= GameManager.Instance.money &&
// 			    GameManager.Instance.prestige >= GameManager.Instance.technology) {
// 				endtext.text = ("你一败涂地。人民拥护你，但是你却利用自己的权利成为了一个暴君。人民顺从你，但是你却把他们当成了奴隶。现在，你坐在被黑烟笼罩的权利王座上，迷茫地眺望着那不存在的未来。");
// 			}
// 		}
// 	}
//
// 	private void UpdateHealthBar() {
// 		CarbonFill.fillAmount = currentHealth;
// 	}
// }