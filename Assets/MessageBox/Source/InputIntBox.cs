﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Events;

public class InputIntBox : ModalBox
{
    /// <summary>
    /// Set this to the name of the prefab that should be loaded when a menu box is shown.
    /// </summary>
    [Tooltip("Set this to the name of the prefab that should be loaded when a menu box is shown.")]
    public static string PrefabResourceName = "InputInt Box";

    /// <summary>
    /// Set this to a custom function that will be used to localize the button texts.
    /// </summary>
    /// <remarks>
    /// You can hook into your existing localization system here to lookup and return a translated string using the original as a key.
    /// For instance using "Localization package" from the asset store you would do this:
    /// MessageBox.Localize = (originalString) => { return Language.Get(originalString); };
    /// 
    /// Current text strings that need to be localized are "OK", "Yes", "No", "Cancel", "Abort", "Retry", "Ignore"
    /// 
    /// This function only needs to be set once at game startup.
    /// 
    /// See MessageBoxExample.TestLocalization for an example.
    /// </remarks>
    [Tooltip("Set this to a custom function that will be used to localize the button texts.")]
    public static Func<string, string> Localize = (sourceString) => { return sourceString; };

    /// <summary>
    /// Set to true to send the title and message of message boxes and menus thru the Localize function.
    /// </summary>
    [Tooltip("Set to true to send the title and message of message boxes and menus thru the Localize function.")]
    public static bool LocalizeTitleAndMessage = false;

    public InputField inputField;

    int inputValue;
    [HideInInspector]
    public int maxInputValue;
    DialogResult result;
    Action<DialogResult,int> onFinish;

    /// <summary>
    /// Display a message box.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="onFinished">An action to be called when the dialog is closed.</param>
    /// <param name="buttons">Selects what buttons are shown on the dialog.</param>
    /// <returns>The message box game object.</returns>
    public static InputIntBox Show(string message, Action<DialogResult,int> onFinished, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return Show(message, null,int.MaxValue, onFinished, buttons);
    }

    /// <summary>
    /// Display a message box.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">A title for the message box.</param>
    /// <param name="onFinished">An action to be called when the dialog is closed.</param>
    /// <param name="buttons">Selects what buttons are shown on the dialog.</param>
    /// <returns>The message box game object.</returns>
    public static InputIntBox Show(string message, string title = null,int maxInputValue = int.MaxValue, Action<DialogResult,int> onFinished = null, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        var box = (Instantiate(Resources.Load<GameObject>(PrefabResourceName)) as GameObject).GetComponent<InputIntBox>();

        box.onFinish = onFinished;
        box.maxInputValue = maxInputValue;
        box.SetUpButtons(buttons);
        box.SetText(message, title);
        box.inputField.placeholder.GetComponent<Text>().text += maxInputValue == int.MaxValue ? "" : "(最高" + maxInputValue + ")";
        return box;
    }

    void SetUpButtons(MessageBoxButtons buttons)
    {
        var button = Button.gameObject;
        switch (buttons)
        {
            case MessageBoxButtons.OK:
                button.GetComponentInChildren<Text>().text = Localize("好的");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.OK; Close(); });
                break;
            case MessageBoxButtons.OKCancel:
                button.GetComponentInChildren<Text>().text = Localize("好的");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.OK; Close(); });

                CreateButton(button, Localize("取消"), () => { result = DialogResult.Cancel; Close(); });
                break;
            case MessageBoxButtons.YesNo:
                button.GetComponentInChildren<Text>().text = Localize("是的");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.Yes; Close(); });

                CreateButton(button, Localize("不"), () => { result = DialogResult.No; Close(); });
                break;
            case MessageBoxButtons.RetryCancel:
                button.GetComponentInChildren<Text>().text = Localize("重试");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.Retry; Close(); });

                CreateButton(button, Localize("取消"), () => { result = DialogResult.Cancel; Close(); });
                break;
            case MessageBoxButtons.YesNoCancel:
                button.GetComponentInChildren<Text>().text = Localize("是的");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.Yes; Close(); });

                CreateButton(button, Localize("不"), () => { result = DialogResult.No; Close(); });
                CreateButton(button, Localize("取消"), () => { result = DialogResult.Cancel; Close(); });
                break;
            case MessageBoxButtons.AbortRetryIgnore:
                button.GetComponentInChildren<Text>().text = Localize("中止");
                button.GetComponent<Button>().onClick.AddListener(() => { result = DialogResult.Abort; Close(); });

                CreateButton(button, Localize("重试"), () => { result = DialogResult.Retry; Close(); });
                CreateButton(button, Localize("忽略"), () => { result = DialogResult.Ignore; Close(); });
                break;
        }
    }

    GameObject CreateButton(GameObject buttonToClone, string label, UnityAction target)
    {
        var button = Instantiate(buttonToClone) as GameObject;

        button.transform.SetParent(buttonToClone.transform.parent, false);

        button.GetComponentInChildren<Text>().text = label;
        button.GetComponent<Button>().onClick.AddListener(target);

        return button;
    }

    public void OnValueChange(string text)
    {
        int number;
        if (Int32.TryParse(text,out number))
        {
            if (maxInputValue != int.MaxValue)
            {
                inputValue = number > maxInputValue ? maxInputValue : number;
                inputField.text = inputValue + "";
            }else
            {
                inputValue = number;
            }
        }
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public override void Close()
    {
        if (onFinish != null)
            onFinish(result,inputValue);
        base.Close();
    }
}
