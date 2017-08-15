using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理Sprite的集合类，通过文件路径记录。
/// </summary>
public static class SpriteLibrary
{
    //public const Sprite LOADING = null;
    public static Dictionary<string, Sprite> pathToSprites = new Dictionary<string, Sprite>();
    /// <summary>
    /// 往指定路径里保存Sprite。会覆盖之前的Sprite。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="icon"></param>
    public static void AddSprite(string path, Sprite icon)
    {
        if (pathToSprites.ContainsKey(path))
        {
            //Resources.UnloadAsset(pathToSprites[path]);
            var oldSprite = pathToSprites[path];
            pathToSprites[path] = icon;
            if (oldSprite != null) Object.Destroy(oldSprite);
        }
        else
        {
            pathToSprites.Add(path, icon);
        }
    }
    public static void RemoveSpriteByPath(string path)
    {
        if (pathToSprites.ContainsKey(path))
        {
            var oldSprite = pathToSprites[path];
            pathToSprites.Remove(path);
            if (oldSprite != null) Object.Destroy(oldSprite);
        }
        Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// 返回该路径的Sprite
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Sprite GetSprite(string path)
    {
        if (pathToSprites.ContainsKey(path))
        {
            return pathToSprites[path];
        }
        else
        {
            return null;
        }
    }
    public static bool IsSpriteDownLoading(string path)
    {
        return pathToSprites.ContainsKey(path);
    }
    public static void SetSpriteDownLoading(string path)
    {
        RemoveSpriteByPath(path);
        pathToSprites.Add(path, null);
    }
}
