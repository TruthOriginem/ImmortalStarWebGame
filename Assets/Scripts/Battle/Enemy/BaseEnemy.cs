using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 用于记录要出现在战斗中的敌方单位
/// </summary>
[System.Serializable]
public class BaseEnemy{
    public string id;//怪物属性的id
    public int Level;
    public bool showIcon = false;
}
