using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PackGridPanelUI : MonoBehaviour {
    // [SerializeField]
    // private Transform[] grids;
    // [SerializeField]
    // private Transform[] eq_grids;
    [SerializeField]
    private GameObject gridPrefab;
    [SerializeField]
    private List<Transform> grids;
    [SerializeField]
    private List<Transform> eq_grids;
    [SerializeField]
    private string gridName;
    


    public Transform GetEmptyGrid()
    {
        for(int i = 0; i < grids.Count; i++)
        {
            if(grids[i].childCount == 0)
            {
                return grids[i];
            }
        }
        return CreateNewGrid();
    }
    public void ClearAllItemUI()
    {
        for (int i = 0; i < grids.Count; i++)
        {
            if (grids[i].childCount != 0)
            {
                Destroy(grids[i].GetChild(0).gameObject);
            }
        }
        
        for (int i = 0; i < eq_grids.Count; i++)
        {
            if (eq_grids[i].childCount != 0)
            {
                Destroy(eq_grids[i].GetChild(0).gameObject);
            }
        }
    }
    public void DeleteAboveRange()
    {
        int index = 25;
        
        for(int i = grids.Count - 1; i >= index; i--)
        {
            if(grids[i].childCount != 0)
            {
                index = i + 1;
                break;
            }
        }
        
        if (index == grids.Count)
        {
            return;
        }
        //Debug.Log(index);
        int count = grids.Count;
        for (int i = index; i < count; i++)
        {
            Transform grid = grids[index];
            grids.Remove(grid);
            Destroy(grid.gameObject);
        }
    }
    public Transform GetEmptyGrid(int index,bool isEquipment)
    {
        if (!isEquipment)
        {
            if (grids.Count > index && grids[index]!= null && grids[index].childCount == 0)
            {
                return grids[index];
            }
            else
            {
                if(index > grids.Count - 1)
                {
                    int count = index + 1 - grids.Count;
                    for (int i = 0;i < count;i++)
                    {
                        CreateNewGrid();
                    }
                    return grids[index];
                }
                else
                {
                    return null;
                }
            }
        }
        else
        {
            if (eq_grids[index].childCount == 0)
            {
                return eq_grids[index];
            }
            else
            {
                return null;
            }
        }
        
    }
    public Transform FindEqTypeGrid(EQ_TYPE type)
    {
        for (int i = 0; i < eq_grids.Count; i++)
        {
            BaseGridUI gridUI = eq_grids[i].GetComponent<BaseGridUI>();
            if (gridUI.GetPermittedType() == type)
            {
                return eq_grids[i];
            }
        }
        return null;
    }
    public int GetIndex(Transform grid, bool isEquipment)
    {
        if (!isEquipment)
        {
            for (int i = 0; i < grids.Count; i++)
            {
                if (grids[i] == grid)
                {
                    return i;
                }
            }
            return -1;
        }
        else
        {
            for (int i = 0; i < eq_grids.Count; i++)
            {
                if (eq_grids[i] == grid)
                {
                    return i;
                }
            }
            return -1;
        }
        
    }

    //创造新的格子
    Transform CreateNewGrid()
    {
        GameObject gridGo = GameObject.Instantiate(gridPrefab);
        Transform grid = gridGo.transform;
        grid.SetParent(grids[0].parent);
        grid.localScale = grids[0].localScale;
        grid.localPosition = grids[0].localPosition;
        grid.name = grids[0].name + " (" + grids.Count + ")";
        grids.Add(grid);
        return grid;
    }
}
