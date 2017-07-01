using System;
using System.Text;

public static class GlobalSettings
{
    public static bool SHOW_HEAL_DIALOG = true;//恢复生命时是否提示
    public static bool SHOW_ENHANCE_DIALOG = true;//强化时是否提示
    public static float ENEMY_ENHANCE_FACTOR = 1f;//怪物加强参数

    public static int ENCRYPT_PERSET = 1;
    public static int ENCRYPT_NOW = 0;

    #region 网络传输加密
    public static int SetEncryptPerset(string key)
    {
        //Debug.Log("origin:" + key);
        //Debug.Log("now:" + EncryptMd5(key));
        ENCRYPT_PERSET = GetEncryptPerset(key);
        //Debug.Log(ENCRYPT_PERSET);
        return ENCRYPT_PERSET;
        //4ad9b0374bebaa8fb4060e491501bc17 原OnlineKey
        //4ad9b0374bebaa8fb4060e491501bc17
        //4bf2f5942kommn2ur1850z625068df18 加密OnlineKey
        //1*******....第一个数字代表ENCRYPT_NOW的位数n,第2~1+n位的数字代表传输的原OK的Post序号
        //******...代表了根据序号再加密的Key,Key的加密方式是原OK的每位+Post序号+加密OnlineKey的Post序号,
    }
    public static int GetEncryptPerset(string key)
    {
        int num = 0;
        for (int i = 0; i < key.Length; i++)
        {
            num += Convert.ToInt32(key[i]);
        }
        num = (num / 3) % 10 + 1;
        return num;
    }
    public static string GetEncryptKey()
    {
        ENCRYPT_NOW += ENCRYPT_PERSET;
        //Debug.Log("现在的EN:" + ENCRYPT_NOW);
        StringBuilder sb = new StringBuilder();
        int n = 0;
        int temp = ENCRYPT_NOW;
        while (temp > 0)
        {
            temp /= 10;
            n++;
        }
        sb.Append(n);
        sb.Append(ENCRYPT_NOW);
        string s = EncryptMd5(PlayerInfoInGame.OnlineKey, ENCRYPT_NOW);
        //Debug.Log("现在的s:" + s);
        string aok = EncryptMd5(PlayerInfoInGame.OnlineKey);
        //Debug.Log("现在的aok:" + aok);
        s = EncryptMd5(s, GetEncryptPerset(aok));
        sb.Append(s);

        //Debug.Log("现在的S:" + sb.ToString());

        return sb.ToString();
    }

    public static string EncryptMd5(string key)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < key.Length; i++)
        {
            var mchar = key[i];
            if (mchar >= 48 && mchar <= 57)
            {
                int num = mchar - 48;
                num += i;
                num %= 10;
                if (num < 0)
                {
                    num += 10;
                }
                sb.Append(num);
            }
            else
            if (mchar >= 65 && mchar <= 90)
            {
                int num = mchar - 65;
                num += i;
                num %= 26;
                if (num < 0)
                {
                    num += 26;
                }
                //8+60 = 68 -> 16  16 - 60 -44 -18
                sb.Append((char)(num + 65));
            }
            else
            if (mchar >= 97 && mchar <= 122)
            {
                int num = mchar - 97;
                num += i;
                num %= 26;
                if (num < 0)
                {
                    num += 26;
                }
                sb.Append((char)(num + 97));
            }
        }
        return sb.ToString();
    }
    public static string EncryptMd5(string key, int add)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < key.Length; i++)
        {
            var mchar = key[i];
            if (mchar >= 48 && mchar <= 57)
            {
                int num = mchar - 48;
                num += add;
                num %= 10;
                if (num < 0)
                {
                    num += 10;
                }
                sb.Append(num);
            }
            else
            if (mchar >= 65 && mchar <= 90)
            {
                int num = mchar - 65;
                num += add;
                num %= 26;
                if (num < 0)
                {
                    num += 26;
                }
                sb.Append((char)(num + 65));
            }
            else
            if (mchar >= 97 && mchar <= 122)
            {
                int num = mchar - 97;
                num += add;
                num %= 26;
                if (num < 0)
                {
                    num += 26;
                }
                sb.Append((char)(num + 97));
            }
        }
        return sb.ToString();
    }
    #endregion
}
