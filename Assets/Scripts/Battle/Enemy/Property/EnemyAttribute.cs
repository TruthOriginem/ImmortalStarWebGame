using UnityEngine;
using System.Collections;
using SerializedClassForJson;
/// <summary>
/// 怪物的基本属性
/// </summary>
public class EnemyAttribute{
    public int idkey;
    public string name;
    public string id;
    public EnemyProperty growth;//怪物成长,包含经验
    public EnemyProperty baseP;//怪物基础属性，包含经验
    public TempItemDrops[] dropItems;//怪物掉落道具，有等级限制
    private Texture2D iconTexture;

    /// <summary>
    /// 图片的文件夹路径
    /// </summary>
    private static string IMAGE_FOLDER_PATH = "icons/enemies/";
    /// <summary>
    /// 图片的统一后缀名
    /// </summary>
    private static string IMAGE_FILE_SUFFIX = ".png";

    public EnemyAttribute(int idkey,string id,string name,EnemyProperty baseP,EnemyProperty growP,Texture2D iconTexture, TempItemDrops[] dropItems)
    {
        this.idkey = idkey;
        this.id = id;
        this.name = name;
        this.growth = growP;
        this.baseP = baseP;
        this.iconTexture = iconTexture;
        this.dropItems = dropItems;
        SpriteLibrary.AddSprite(GetCompletedFilePathById(id), Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f)));
    }


    /// <summary>
    /// 得到每升一级，属性得到的加成的值
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetGrowthValue(PROPERTY_TYPE type)
    {
        for (int i = 0; i < growth.properties.Count; i++)
        {
            if (growth.properties[i].type == type)
            {
                return growth.properties[i].value;
            }
        }
        return 0;
    }
    
    /// <summary>
    /// 有后缀名。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetIconNameById(string id)
    {
        return id + IMAGE_FILE_SUFFIX;
    }

    /// <summary>
    /// 直接获得指定id的文件路径。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetCompletedFilePathById(string id)
    {
        return IMAGE_FOLDER_PATH + GetIconNameById(id);
    }
    public Sprite GetIconSprite()
    {
        return SpriteLibrary.GetSprite(GetCompletedFilePathById(id));
    }
}
