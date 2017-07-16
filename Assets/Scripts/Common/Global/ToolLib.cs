
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
    public static bool IsNullOrEmpty<T>(T[] array)
    {
        return !(array != null && array.Length > 0);
    }
}
