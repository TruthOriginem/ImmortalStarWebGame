using UnityEngine;
using System.Collections;

public class BackGroundMoveScript : MonoBehaviour {

    private static float m_radius = 350f;
    private static float m_agree = 0f;
	// Use this for initialization
	void Start () {

    }
    IEnumerator testRegister()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", "你去屎吧");
        form.AddField("password", "123456");
        WWW w = new WWW("localhost:85/register.php", form);
        yield return w;
        if (w.error != null)
        {
            Debug.LogWarning("有错");
            w.Dispose();
        }
        else
        {
            if(w.text == "true")
            {
                Debug.Log("注册成功！");
                
            }else if (w.text == "exists")
            {

            }
            w.Dispose();
        }
    }
	
	// Update is called once per frame
	void Update () {
        m_agree += Time.deltaTime * 3f;
        if(m_agree >= 360f)
        {
            m_agree -= 360f;
        }
        RectTransform trans = GetComponent<RectTransform>();
        float x = m_radius * Mathf.Cos(m_agree * Mathf.PI / 180f);
        float y = m_radius * Mathf.Sin(m_agree * Mathf.PI / 180f);
        trans.localPosition = new Vector3(x,y,0f);
    }
}
