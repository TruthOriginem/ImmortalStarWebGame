using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

public class ChangeFontWindow : EditorWindow
{
    [MenuItem("DuanTools/换字体")]
    public static void Open()
    {
        EditorWindow.GetWindow(typeof(ChangeFontWindow));
    }

    public Font toChange;
    static Font toChangeFont;

    void OnGUI()
    {
        toChange = (Font)EditorGUILayout.ObjectField(toChange, typeof(Font), true, GUILayout.MinWidth(100f));
        toChangeFont = toChange;
        if (GUILayout.Button("变变变！"))
        {
            Change();
        }
    }

    public static void Change()
    {
        //获取所有UILabel组件
        //如果是UGUI讲UILabel换成Text就可以
        Object[] labels = Selection.GetFiltered(typeof(Text), SelectionMode.Deep);
        foreach (Object item in labels)
        {
            //如果是UGUI讲UILabel换成Text就可以
            Text label = (Text)item;
            label.font = toChangeFont;
            //label.font = toChangeFont;（UGUI）
            Debug.Log(item.name + ":" + label.text);
        }
    }
}