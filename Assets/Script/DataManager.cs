using Habby.Storage;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public int money;
    public int technology;
    public int prestige;
    public int carbon;
    private StorageContainer _dataContainer;
    public static DataManager Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
        _dataContainer = Storage.GetContainer("Data");
        money = _dataContainer.Get("money", 333);
        technology = _dataContainer.Get("technology", 333);
        prestige = _dataContainer.Get("prestige", 333);
        carbon = _dataContainer.Get("carbon", 50);
        _dataContainer.Save();
    }

    public void SetData(int money, int technology, int prestige, int carbon)
    {
        _dataContainer.Set("money", money);
        _dataContainer.Set("technology", technology);
        _dataContainer.Set("prestige", prestige);
        _dataContainer.Set("carbon", carbon);
        _dataContainer.Save();
    }

    public void Restart()
    {
        SetData(333, 333, 333, 50);
        Resource.Instance.LoadResources();
        Carbon.Instance.Ending.SetActive(false);
    }
}