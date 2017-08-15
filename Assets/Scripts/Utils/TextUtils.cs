using UnityEngine;
using System.Collections;
using System.Text;

public static class TextUtils
{
    /// <summary>
    /// 将一个文字染成指定颜色，注意，rgba范围为0~255
    /// </summary>
    /// <param name="text"></param>
    /// <param name="r">0~255</param>
    /// <param name="g">0~255</param>
    /// <param name="b">0~255</param>
    /// <param name="a">0~255</param>
    /// <returns></returns>
    public static string GetColoredText(string text, float r, float g, float b, float a)
    {
        StringBuilder sb = new StringBuilder();
        Color color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        sb.Append("<color=#");
        sb.Append(ColorUtility.ToHtmlStringRGBA(color));
        sb.Append(">");
        sb.Append(text);
        sb.Append("</color>");
        return sb.ToString();
    }
    public static string GetColoredText(string text, Color color)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<color=#");
        sb.Append(ColorUtility.ToHtmlStringRGBA(color));
        sb.Append(">");
        sb.Append(text);
        sb.Append("</color>");
        return sb.ToString();
    }
    public static string GetYellowText(string text)
    {
        StringBuilder sb = new StringBuilder();
        Color color = Color.yellow;
        sb.Append("<color=#");
        sb.Append(ColorUtility.ToHtmlStringRGBA(color));
        sb.Append(">");
        sb.Append(text);
        sb.Append("</color>");
        return sb.ToString();
    }

    public static string GetGreenText(string text)
    {
        return "<color=#008000ff>" + text + "</color>";
    }
    public static string GetMpText(string text)
    {
        return "<color=#00D4FFFF>" + text + "</color>";
    }
    public static string GetMoneyText(object text)
    {
        return "<color=#00B5FFFF>" + text + "</color>";
    }
    public static string GetExpText(object text)
    {
        return "<color=#977DFFFF>" + text + "</color>";
    }
    public static string GetSpbText(string text)
    {
        return "<b><color=#67FFFFFF>" + text + "</color></b>";
    }
    /// <summary>
    /// 获得指定整数的罗马数字
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static string GetLoumaNumber(int n)
    {
        int[] arabic = new int[13];
        string[] roman = new string[13];
        int i = 0;
        string o = "";

        arabic[0] = 1000;
        arabic[1] = 900;
        arabic[2] = 500;
        arabic[3] = 400;
        arabic[4] = 100;
        arabic[5] = 90;
        arabic[6] = 50;
        arabic[7] = 40;
        arabic[8] = 10;
        arabic[9] = 9;
        arabic[10] = 5;
        arabic[11] = 4;
        arabic[12] = 1;

        roman[0] = "M";
        roman[1] = "CM";
        roman[2] = "D";
        roman[3] = "CD";
        roman[4] = "C";
        roman[5] = "XC";
        roman[6] = "L";
        roman[7] = "XL";
        roman[8] = "X";
        roman[9] = "IX";
        roman[10] = "V";
        roman[11] = "IV";
        roman[12] = "I";

        while (n > 0)
        {
            while (n >= arabic[i])
            {
                n = n - arabic[i];
                o = o + roman[i];
            }
            i++;
        }
        return o;
    }
    /// <summary>
    /// 用于富文本
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetSizedString(string text, int size)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<size=");
        sb.Append(size);
        sb.Append(">");
        sb.Append(text);
        sb.Append("</size>");
        return sb.ToString();
    }
    /// <summary>
    /// 10000 = 10k,10000000 = 10m,100000000 = 10kw
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string GetOmitNumberString(long number)
    {
        if (number >= 100000000)
        {
            return (number / 10000000).ToString() + "kw";
        }
        else if (number >= 10000000)
        {
            return (number / 1000000).ToString() + "m";
        }
        else if (number >= 10000)
        {
            return (number / 1000).ToString() + "k";
        }
        else
        {
            return number.ToString();
        }

    }
    public static string GetRarityColorCode(int rarity = 1)
    {
        switch (rarity)
        {
            case 1:
                return "grey";
            case 2:
                return "white";
            case 3:
                return "#008000ff";//绿
            case 4:
                return "blue";//蓝色
            case 5:
                return "#9D2FFFFF";//紫色
            case 6:
                return "#d9d919FF";//金色
            default:
                return "";
        }
    }
}
