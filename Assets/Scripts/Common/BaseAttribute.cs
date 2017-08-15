using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// 属性字段基类，所有有属性的功能应该从这里继承。
/// </summary>
[System.Serializable]
public class BaseAttribute
{
    public float mhp;
    public float mmp;
    public float atk;
    public float def;
    public float log;
    public float lck;
    public float cri;

    /// <summary>
    /// 将所有属性乘以一个系数
    /// </summary>
    /// <param name="mult"></param>
    public void MultAllProperties(float mult)
    {
        mhp *= mult;
        mmp *= mult;
        atk *= mult;
        def *= mult;
        log *= mult;
        lck *= mult;
        cri *= mult;
    }
}
