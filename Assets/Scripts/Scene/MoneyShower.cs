using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoneyShower : MonoBehaviour {
    public Text moneyShow;
    public Text dimenShow;
    public Text vipShow;

    public Text levelText;
    public Text ExpText;
    public Slider expSlider;
    // Update is called once per frame
    public static MoneyShower Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    public static void RefreshMoneyShower()
    {
        Instance.moneyShow.text = PlayerInfoInGame.GetMoney() + "";
        Instance.dimenShow.text = PlayerInfoInGame.GetDimenCoin() + "";
        Instance.vipShow.text = PlayerInfoInGame.VIP_Level + "";
        Instance.levelText.text = PlayerInfoInGame.Level.ToString();
        Instance.ExpText.text = PlayerInfoInGame.Exp + "/" + PlayerInfoInGame.NextExp;
        Instance.expSlider.value = (float)PlayerInfoInGame.Exp / (float)PlayerInfoInGame.NextExp;
    }
}
