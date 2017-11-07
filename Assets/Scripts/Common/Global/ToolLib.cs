using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// 对数组的额外处理方法。
/// </summary>
public static class EArray
{
    /// <summary>
    /// 检查该数组是否为空指针且长度为0。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(object[] array)
    {
        return !(array != null && array.Length > 0);
    }
}
public static class EMath
{
    /// <summary>
    /// 四舍五入至指定位数。
    /// </summary>
    /// <param name="number"></param>
    /// <param name="digit"></param>
    /// <returns></returns>
    public static float Round(float number, int digit)
    {
        int mult = 1;
        while (digit > 0)
        {
            mult *= 10;
            digit--;
        }
        int tmp = Mathf.RoundToInt(number * mult);
        number = tmp / (float)mult;
        return number;
    }
}
public static class EDictionary
{
    /// <summary>
    /// 将一个字典对象序列化为简单的Json字符串。
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dicts"></param>
    /// <returns></returns>
    public static string SerializeToJson<K,V>(Dictionary<K, V> dicts)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        string[] pairs = new string[dicts.Count];
        int index = 0;
        foreach (var kv in dicts)
        {
            pairs[index++] = string.Format("\"{0}\":{1}", kv.Key, kv.Value);
        }
        sb.Append(string.Join(",", pairs));
        sb.Append("}");
        return sb.ToString();
    }
}