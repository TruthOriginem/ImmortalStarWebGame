using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text;

public class LoginManager : MonoBehaviour
{

    private static string SCENE_TO_LOAD = "demo";
    [SerializeField]
    private InputField m_InputNameField;
    [SerializeField]
    private InputField m_InputPwField;
    [SerializeField]
    private Transform m_LoadingBar;

    public GameObject loginWindow;

    private Image m_BarContent;
    private float m_ShowProgress = 0f;
    private AsyncOperation m_SceneLoadAysnc;
    private const int shouldLoad = 7;
    private int nowLoad = -1;
    private bool loadCompleted = false;

    void Awake()
    {

        m_LoadingBar.gameObject.SetActive(false);
        m_BarContent = m_LoadingBar.Find("Fill").GetComponent<Image>();
    }
    void Start()
    {
        Debug.Log(ConnectUtils.server);
#if UNITY_WEBGL && !UNITY_EDITOR
        ConnectUtils.server = Application.absoluteURL;
        ConnectUtils.port = "";
#endif
    }

    void Update()
    {

        if (nowLoad >= 0)
        {
            m_ShowProgress = Mathf.Lerp(m_ShowProgress, nowLoad / (float)shouldLoad, Time.deltaTime * 15f);
            m_BarContent.fillAmount = m_ShowProgress;
            //Debug.Log(m_SceneLoadAysnc.progress);
        }
        if (loadCompleted)
        {
            if (Input.GetMouseButton(0))
            {
                m_SceneLoadAysnc.allowSceneActivation = true;
            }
        }
    }

    public void PressLoginButton()
    {
        StartCoroutine(RequestLogin());
    }
    /// <summary>
    /// 协程:请求登录
    /// </summary>
    /// <returns></returns>
    IEnumerator RequestLogin()
    {
        WWWForm form = new WWWForm();
        string id = m_InputNameField.text;
        form.AddField("name", id);
        form.AddField("password", m_InputPwField.text);
        WWW w = new WWW(ConnectUtils.ParsePath("login.php"), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        ConnectUtils.HideConnectingUI();
        if (w.error == null)
        {
            if (w.text == "fail")
            {
                MessageBox.Show("密码错误！", null, MessageBoxButtons.OK);
                m_InputPwField.text = "";
            }
            else if (w.text == "empty")
            {
                MessageBox.Show("不存在该用户！", null, MessageBoxButtons.OK);

            }
            else
            {
                LoginData data = JsonUtility.FromJson<LoginData>(w.text);
                m_LoadingBar.gameObject.SetActive(true);
                loginWindow.SetActive(false);
                PlayerInfoInGame.Id = id;
                PlayerInfoInGame.NickName = data.nickname;
                PlayerInfoInGame.uid = data.uid;
                PlayerInfoInGame.OnlineKey = data.onlineKey;
                GlobalSettings.SetEncryptPerset(PlayerInfoInGame.OnlineKey);
                StartCoroutine(LoadScene());
            }
            w.Dispose();
        }
        else
        {
            Debug.LogWarning("出错!");
            w.Dispose();
        }
    }

    /// <summary>
    /// 协程:载入游戏主界面，
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadScene()
    {
        nowLoad = 0;
        loadCompleted = false;
        //道具
        yield return ItemDataManager.InitAllItems();
        nowLoad++;
        //怪物
        yield return EnemyDataManager.InitAllEnemiesInList();
        nowLoad++;
        //词缀
        yield return ModDataManager.InitEquipmentFactory();
        nowLoad++;
        //远征怪物信息
        yield return EnemyExpeditionManager.InitEnemyExpeditionInfos();
        nowLoad++;
        //关卡信息
        yield return ScenarioManager.InitAllScenarioData();
        nowLoad++;
        //称号信息
        yield return DesignationManager.InitDesignationDatas();
        nowLoad++;
        //场景
        StartCoroutine(AysncLoadScene());
        while (m_SceneLoadAysnc != null && m_SceneLoadAysnc.progress < 0.8f)
        {
            yield return 0;
        }
        nowLoad++;
        loadCompleted = true;
        m_LoadingBar.Find("Text").GetComponent<Text>().text = "按鼠标左键进入游戏";
    }
    /// <summary>
    /// 初始化场景
    /// </summary>
    /// <returns></returns>
    IEnumerator AysncLoadScene()
    {
        m_SceneLoadAysnc = SceneManager.LoadSceneAsync(SCENE_TO_LOAD);
        m_SceneLoadAysnc.allowSceneActivation = false;
        //BattleInstanceManager.idsToGrid.Clear();
        //BattleInstanceManager.battleStages.Clear();
        yield return m_SceneLoadAysnc;
    }

    public void PressRegisterButton(RegisterWindow register)
    {
        if (register.inputIdField.text == "" ||
            register.inputPassWordField.text == "" ||
            register.inputNickNameField.text == "")
        {
            MessageBox.Show("请不要填为空！", "!");
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("用户名:" + register.inputIdField.text);
            sb.AppendLine("密码:" + register.inputPassWordField.text);
            sb.AppendLine("昵称:" + register.inputNickNameField.text);
            sb.Append("引领人编号:" + (register.inputLeaderUIDField.text == "" ? "无" : register.inputLeaderUIDField.text));
            MessageBox.Show("您的注册信息为" + sb.ToString(), "温馨提示", (result) =>
              {
                  if (result == DialogResult.OK)
                  {
                      register.gameObject.SetActive(false);
                      StartCoroutine(Register(register));
                  }
              }, MessageBoxButtons.OKCancel);
        }
    }
    IEnumerator Register(RegisterWindow register)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", register.inputIdField.text);
        form.AddField("password", register.inputPassWordField.text);
        form.AddField("nickname", register.inputNickNameField.text);
        form.AddField("leader_uid", register.inputLeaderUIDField.text);
        WWW w = new WWW(ConnectUtils.ParsePath("register.php"), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            if (w.text == "exists")
            {
                MessageBox.Show("该用户已存在！", "警告");
            }
            else
            {
                MessageBox.Show("注册成功!", "恭喜你！");
            }
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }
    [System.Serializable]
    public class LoginData
    {
        public string id;
        public string nickname;
        public int uid;
        public string onlineKey;
    }
}
