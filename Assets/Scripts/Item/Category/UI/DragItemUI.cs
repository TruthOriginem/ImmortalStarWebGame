using UnityEngine;
using System.Collections;

public class DragItemUI : ItemUI {
    private void Update()
    {
        itemText.text = "";
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        BreakLink();
        gameObject.SetActive(false);
    }

    public void SetLocalPosition(Vector2 position)
    {
        transform.localPosition = position;
    }
    public void BreakLink()
    {
        linkedItem = null;
    }
}
