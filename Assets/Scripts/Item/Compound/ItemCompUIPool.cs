using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCompUIPool : MonoBehaviour
{
    public GameObject itemCompUIPrefab;
    public Transform itemCompUIParent;
    public List<ItemToCompoundUI> itemCompUIs;
    public static ItemCompUIPool Instance { get; set; }
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DisableAllObjects();
    }
    public void UpdatePoolByList(List<ItemCompoundData> datas)
    {
        DisableAllObjects();
        if (datas.Count > itemCompUIs.Count)
        {
            int more = datas.Count - itemCompUIs.Count;
            for (int i = 0; i < more; i++)
            {
                ItemToCompoundUI ui = Instantiate(itemCompUIPrefab, itemCompUIParent, false).GetComponent<ItemToCompoundUI>();
                itemCompUIs.Add(ui);
            }
        }
        for (int i = 0; i < datas.Count; i++)
        {
            ActivateItem(datas[i], i);
        }
        if (itemCompUIs[0].gameObject.activeInHierarchy)
        {
            itemCompUIs[0].SelectThis();
        }
    }

    public void ActivateItem(ItemCompoundData data, int index)
    {
        var item = itemCompUIs[index];
        item.gameObject.SetActive(true);
        item.UpdateUI(data);
    }
    void DisableAllObjects()
    {
        foreach (var item in itemCompUIs)
        {
            item.gameObject.SetActive(false);
        }
    }
}
