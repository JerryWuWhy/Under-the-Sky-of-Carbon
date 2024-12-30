using UnityEngine;
using UnityEngine.UI;

public class Carbon : MonoBehaviour
{
    [SerializeField] private Image CarbonFill;
    public float currentHealth = 1f;

    void Update()
    {
        UpdateHealthBar();
        currentHealth = (Resource.Instance.carbon) / 100;
    }
    
    private void UpdateHealthBar()
    {
        if (CarbonFill != null)
        {
            CarbonFill.fillAmount = currentHealth;
        }
    }
}
