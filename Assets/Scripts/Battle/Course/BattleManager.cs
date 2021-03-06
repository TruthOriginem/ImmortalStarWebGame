﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using GameId;
using SerializedClassForJson;

public class BattleManager : MonoBehaviour
{
    public enum BATTLE_TYPE
    {
        NORMAL,//普通剧本的战斗
        EXPEDITION,//远征
        MACHINEMATCH,
        DEEPMEMORY
    }
    public CanvasGroup battleCanvas;
    /// <summary>
    /// 敌人dropdown
    /// </summary>
    public Dropdown enemyList;

    public Scrollbar courseScrollBar;
    public RectTransform courseRect;

    public Slider enemyHp;
    public Slider enemyMp;

    public Slider playerHp;
    public Slider playerMp;

    public Text enemyName;
    public Text enemyProperty;

    public Text playerName;
    public Text playerProperty;

    public RectTransform courseContent;
    public Text resultText;
    public Text countDownText;

    public Toggle autoBattleToggle;


    //private bool versusPlayer = false;
    /// <summary>
    /// 现在的战斗类型
    /// </summary>
    private BATTLE_TYPE nowBattleType = BATTLE_TYPE.NORMAL;
    private BatInsGridData linkedInstanceGridData;

    private BattleResult battleResult;
    private bool isInBattle = false;

    /// <summary>
    /// 现在list里选中的敌人
    /// </summary>
    private BattleUnit nowEnemy;
    private BattleUnit nowPlayer;

    private List<BattleUnit> deadShow = new List<BattleUnit>();

    public static BattleManager Instance { get; set; }
    private Battle battle;

    public bool ShouldHide { get; set; }
    /// <summary>
    /// 一键结束战斗！
    /// </summary>
    private bool fastBattle = false;

    /// <summary>
    /// 是否胜利
    /// </summary>
    private bool win = false;
    /// <summary>
    /// 结算的字符串。
    /// </summary>
    public static string ResultString;

    private bool isTextBoxDirty = false;

    public Button backButton;
    public Button reAttackButton;


    void Awake()
    {
        Instance = this;
        ShouldHide = false;
    }

    void Update()
    {
        #region 血条和能量条
        if (isInBattle)
        {
            if (nowEnemy != null)
            {
                float hpValue = nowEnemy.tempRecord.hp / nowEnemy.tempRecord.GetValue(Attrs.MHP);
                float mpValue = nowEnemy.tempRecord.mp / nowEnemy.tempRecord.GetValue(Attrs.MMP);
                if (float.IsNaN(mpValue))
                {
                    mpValue = 0f;
                }

                enemyHp.value = Mathf.Lerp(enemyHp.value, hpValue, Time.deltaTime * 10f);
                enemyMp.value = Mathf.Lerp(enemyMp.value, mpValue, Time.deltaTime * 10f);
            }
            if (nowPlayer != null)
            {
                float hpValue = nowPlayer.tempRecord.hp / nowPlayer.tempRecord.GetValue(Attrs.MHP);
                float mpValue = nowPlayer.tempRecord.mp / nowPlayer.tempRecord.GetValue(Attrs.MMP);
                playerHp.value = Mathf.Lerp(playerHp.value, hpValue, Time.deltaTime * 10f);
                playerMp.value = Mathf.Lerp(playerMp.value, mpValue, Time.deltaTime * 10f);
            }
        }
        #endregion
        if (isTextBoxDirty)
        {
            #region 死亡时换文字
            if (battle.courses.Count >= 1)
            {
                if (nowEnemy.tempRecord.hp <= 0f)
                {
                    enemyList.captionText.text = "<color=red>" + nowEnemy.name + "</color>";
                    enemyName.text = "<color=red>" + nowEnemy.name + "(已死亡)</color>";
                }
                else
                {
                    enemyName.text = nowEnemy.name;
                }

                for (int i = 0; i < battle.enemyUnits.Count; i++)
                {
                    if (battle.enemyUnits[i].tempRecord.hp <= 0f && !deadShow.Contains(battle.enemyUnits[i]))
                    {
                        enemyList.options[i].text = "<color=red>" + enemyList.options[i].text + "</color>";
                        deadShow.Add(battle.enemyUnits[i]);
                    }
                }

                if (nowPlayer.tempRecord.hp <= 0f)
                {
                    playerName.text = "<color=red>" + PlayerInfoInGame.NickName + "(Lv." + nowPlayer.level + ")(已死亡)</color>";
                }
                else
                {
                    playerName.text = PlayerInfoInGame.NickName + "(Lv." + nowPlayer.level + ")";

                }
            }
            #endregion
            playerProperty.text = nowPlayer.GetPropertyDescription();
            enemyProperty.text = nowEnemy.GetPropertyDescription();
            //courseContent.anchoredPosition = new Vector2(0f, 0f);
            var height = courseContent.rect.height;
            courseContent.anchoredPosition = new Vector2(0f, height);
            //courseScrollBar.value = 0f;
            //courseRect.;
            isTextBoxDirty = false;
        }
        if (ShouldHide == true)
        {
            SetBattleCanvasVisible(false);
            ShouldHide = false;
        }
    }
    /// <summary>
    /// 战斗开始，直接得出战斗结果，然后播放,注意，这个并不会显示Battle的场景，需要其他脚本调用
    /// </summary>
    /// <param name="instance"></param>
    public Coroutine InitBattle(BatInsGridData instance)
    {
        linkedInstanceGridData = instance;
        nowBattleType = BATTLE_TYPE.NORMAL;
        autoBattleToggle.gameObject.SetActive(true);
        battleResult = new BattleResult();

        return StartCoroutine(InitBattleCor(instance.enemys.GetActualData(instance.sId)));
    }
    /// <summary>
    /// 远征的战斗。
    /// </summary>
    /// <param name="enemySpawnData"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Coroutine InitBattle(ExpeditionBattleInfo info)
    {
        nowBattleType = BATTLE_TYPE.EXPEDITION;
        autoBattleToggle.gameObject.SetActive(false);
        battleResult = new BattleResult();
        battleResult.SetLinkedInfo(info);
        return StartCoroutine(InitBattleCor(info.enemySpawnData));
    }
    public Coroutine InitBattle(MachineMatchManager manager)
    {
        nowBattleType = BATTLE_TYPE.MACHINEMATCH;
        autoBattleToggle.gameObject.SetActive(false);
        battleResult = new BattleResult();
        battleResult.SetLinkedInfo(manager);
        return StartCoroutine(InitBattleCor(manager));
    }
    public Coroutine InitBattle(DeepMemoryManager manager)
    {
        nowBattleType = BATTLE_TYPE.DEEPMEMORY;
        autoBattleToggle.gameObject.SetActive(false);
        battleResult = new BattleResult();
        battleResult.SetLinkedInfo(manager);
        return StartCoroutine(InitBattleCor(manager));
    }
    /// <summary>
    /// 再次战斗
    /// </summary>
    public void ReBattle()
    {
        InitBattle(linkedInstanceGridData);
    }

    IEnumerator InitBattleCor(object enemyData)
    {
        //Debug.Log(JsonUtility.ToJson(enemySpawnData));
        //battle.courses.Clear();
        battle = null;
        ResultString = TextUtils.GetSizedString("结算:\n", 16);
        if (nowBattleType == BATTLE_TYPE.NORMAL && linkedInstanceGridData != null)
        {
            int powderAmount;
            if (linkedInstanceGridData.CanUseResetPowder(out powderAmount))
            {
                SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.BOSS_RESET_POWDER, -powderAmount).ToJson());
                ResultString += "本次挑战您使用了" + powderAmount + "个Boss重置粉末，并且您还剩下" + (ItemDataManager.GetItemAmount(Items.BOSS_RESET_POWDER) - powderAmount) + "个。\n";
            }
        }
        resultText.text = ResultString;
        fastBattle = false;
        resultText.transform.localPosition = new Vector3(0f, 0f, 0f);
        courseContent.localPosition = new Vector3(0f, 0f, 0f);
        ClearEnemyList();
        //更新玩家属性
        yield return RequestBundle.RequestSyncUpdate();
        //初始化战斗
        battle = new Battle(enemyData);
        //下拉菜单加入敌人选项
        AddEnemyList();

        //设置当前玩家、当前敌人，界面调整
        nowPlayer = battle.playerUnits[0];
        nowEnemy = battle.enemyUnits[0];
        float hpValue = nowEnemy.tempRecord.hp / nowEnemy.tempRecord.GetValue(Attrs.MHP);
        float mpValue = nowEnemy.tempRecord.mp / nowEnemy.tempRecord.GetValue(Attrs.MMP);
        if (float.IsNaN(mpValue))
        {
            mpValue = 0f;
        }
        enemyHp.value = hpValue;
        enemyMp.value = mpValue;
        hpValue = nowPlayer.tempRecord.hp / nowPlayer.tempRecord.GetValue(Attrs.MHP);
        mpValue = nowPlayer.tempRecord.mp / nowPlayer.tempRecord.GetValue(Attrs.MMP);
        playerHp.value = hpValue;
        playerMp.value = mpValue;

        MakeTextBoxDirty();
        //生成回合播放回合
        InitCourses();

    }

    /// <summary>
    /// 生成过程，并生成结算
    /// </summary>
    void InitCourses()
    {
        ClearCourses();
        BattleCourse start_course = new BattleCourse(battle, BattleCourse.TAG.START, battle.courses.Count);
        start_course.GenerateCourse();
        battle.courses.Add(start_course);
        bool shouldEnd = false;
        float lostTime = 0;
        do
        {
            BattleCourse course = new BattleCourse(battle, BattleCourse.TAG.NORMAL, battle.courses.Count);
            shouldEnd = course.GenerateCourse();
            battle.courses.Add(course);
            lostTime += course.timeToWait;
        } while (!shouldEnd && battle.courses.Count < 100);

        BattleCourse end_course = new BattleCourse(battle, BattleCourse.TAG.END, battle.courses.Count);
        end_course.GenerateCourse();
        battle.courses.Add(end_course);
        //判断是否胜利
        win = battle.IsWin();

        //设置战斗结束后的结算
        switch (nowBattleType)
        {
            case BATTLE_TYPE.NORMAL:
                battleResult.GainResult(win ? linkedInstanceGridData : null, Mathf.RoundToInt(lostTime));
                break;
            default:
                battleResult.GainResultByLinkedInfo(win);
                break;
        }
        battleResult = null;
        PlayerCourses();
    }

    /// <summary>
    /// 正式开始播放动画
    /// </summary>
    public void PlayerCourses()
    {
        StartCoroutine(CourtineForCourses());
    }

    /// <summary>
    /// 播放回合动画的协程,并处理结束后的事
    /// </summary>
    /// <returns></returns>
    IEnumerator CourtineForCourses()
    {
        isInBattle = true;
        backButton.gameObject.SetActive(false);
        reAttackButton.gameObject.SetActive(false);
        //回合处理
        foreach (BattleCourse course in battle.courses)
        {
            //courseText.text += course.PlayCourse();
            AddCourse(course.PlayCourse());
            MakeTextBoxDirty();
            yield return 1;
            MakeTextBoxDirty();
            if (!fastBattle)
            {
                yield return new WaitForSecondsRealtime(course.timeToWait);
            }
        }
        //结束
        HandleBattleEndByType(nowBattleType);
        //处理结算
        resultText.text += ResultString;
        //nowEnemy = null;
        //nowPlayer = null;
    }
    /// <summary>
    /// 为战斗结束的UI进行收尾。
    /// </summary>
    /// <param name="type"></param>
    void HandleBattleEndByType(BATTLE_TYPE type)
    {
        switch (nowBattleType)
        {
            case BATTLE_TYPE.NORMAL:
                HandleNormalBattleEnd();
                break;
            default:
                HandleNoRepeatBattleEnd();
                break;
        }
    }
    #region UI收尾
    /// <summary>
    /// 一般的剧情战斗结束后进行的改动。
    /// </summary>
    void HandleNormalBattleEnd()
    {
        SetRebattleButton(true);
        if (!IsAutoBattleToggled())
        {
            backButton.gameObject.SetActive(true);
            isInBattle = false;
            if (!linkedInstanceGridData.Interactive)
            {
                SetRebattleButton(false);
            }
        }
        else
        {
            SetRebattleButton(false);
            if (linkedInstanceGridData.Interactive)
            {
                backButton.gameObject.SetActive(false);
                StartCoroutine(WaitAndRebattle());
            }
            else
            {
                backButton.gameObject.SetActive(true);
                isInBattle = false;
            }
        }
    }
    /// <summary>
    /// 不能重复击败的改动
    /// </summary>
    void HandleNoRepeatBattleEnd()
    {
        SetRebattleButton(false);
        backButton.gameObject.SetActive(true);
        isInBattle = false;
    }
    #endregion

    /// <summary>
    /// 等待三秒后再次战斗，可以被终止
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitAndRebattle()
    {
        float waitTime = 3f;
        //可被中断
        while (waitTime > 0f)
        {
            waitTime -= Time.deltaTime;
            countDownText.text = "倒计时" + waitTime.ToString("0.0") + "秒，可取消上面的旋钮以取消该次战斗。";
            if (!IsAutoBattleToggled())
            {
                countDownText.text = "";
                HandleBattleEndByType(nowBattleType);
                yield break;
            }
            yield return 0;
        }
        countDownText.text = "";
        ReBattle();
    }
    /// <summary>
    /// 战斗过程中，
    /// </summary>
    /// <returns></returns>
    public bool IsInBattle()
    {
        return isInBattle;
    }
    /// <summary>
    /// 返回自动战斗按钮是否被勾选
    /// </summary>
    /// <returns></returns>
    bool IsAutoBattleToggled()
    {
        return autoBattleToggle.isOn;
    }

    /// <summary>
    /// 一键结束战斗
    /// </summary>
    public void MakeFastBattle()
    {
        fastBattle = true;
    }
    /// <summary>
    /// 设置重新战斗的按钮是否可见
    /// <param name="show"></param>
    /// </summary>
    void SetRebattleButton(bool show)
    {
        reAttackButton.gameObject.SetActive(show);
    }
    /// <summary>
    /// 设置按钮调用的方法
    /// </summary>
    public void SetBackInvoke(UnityAction action)
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(action);
    }

    /// <summary>
    /// 令下一帧更新textbox（玩家和怪物）
    /// </summary>
    public void MakeTextBoxDirty()
    {
        isTextBoxDirty = true;
    }

    #region 敌人列表相关
    /// <summary>
    /// 敌人列表更改时调用
    /// </summary>
    /// <param name="index"></param>
    public void ChangeEnemyIndex(int index)
    {
        if (battle != null)
        {
            nowEnemy = battle.enemyUnits[index];
            MakeTextBoxDirty();
        }
    }

    /// <summary>
    /// 敌人列表加入所有敌人单位.
    /// </summary>
    void AddEnemyList()
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        foreach (BattleUnit unit in battle.enemyUnits)
        {
            var enemy = EnemyDataManager.AskForEnemyAttribute(unit.id);
            Sprite sprite = null;
            if (enemy != null)
            {
                sprite = enemy.GetIconSprite();
            }
            string name = unit.name;
            Dropdown.OptionData optionData = new Dropdown.OptionData(name, sprite);
            optionDatas.Add(optionData);
        }
        enemyList.AddOptions(optionDatas);
        enemyList.value = 0;
    }
    void ClearEnemyList()
    {
        enemyList.ClearOptions();
    }
    #endregion

    /// <summary>
    /// 设置战斗画布是否显示
    /// </summary>
    /// <param name="visible"></param>
    public void SetBattleCanvasVisible(bool visible)
    {
        battleCanvas.alpha = visible ? 1f : 0f;
        battleCanvas.interactable = visible;
        battleCanvas.blocksRaycasts = visible;
    }
    #region 战斗流程内文字预制体
    /// <summary>
    /// 清空回合一些实例。(战斗过程那些字)
    /// </summary>
    void ClearCourses()
    {
        for (int i = 0; i < courseContent.childCount; i++)
        {
            Destroy(courseContent.GetChild(i).gameObject);
        }
        nowCourse = RECORD_MAXCOURSE + 1;
    }
    private Text tempText;
    private static int RECORD_MAXCOURSE = 1;
    private int nowCourse;
    void AddCourse(string text)
    {
        if (nowCourse <= RECORD_MAXCOURSE)
        {
            tempText.text += string.Format("{0}{1}", text, (nowCourse != RECORD_MAXCOURSE ? "\n" : ""));
            nowCourse++;
        }
        else
        {
            tempText = Instantiate(Resources.Load("Prefab/Battle/Course") as GameObject, courseContent, false).GetComponent<Text>();
            tempText.text = text + "\n";
            nowCourse = 0;
        }
        //Text ttext = Instantiate(Resources.Load("Prefab/Battle/Course") as GameObject, courseContent, false).GetComponent<Text>();
        //ttext.text = text;
    }
    #endregion
}
