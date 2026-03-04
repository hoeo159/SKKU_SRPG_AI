using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class TalkUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject     root;
    [SerializeField] private TMP_Text       titleText;
    [SerializeField] private TMP_Text       historyText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button         sendButton;
    [SerializeField] private Button         closeButton;
    [SerializeField] private ScrollRect     scrollRect;

    private Func<string, string> replyFunc;
    private Action onClose;
    private string npcName = "NPC";

    public bool isOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if(root == null) root = gameObject;
        if(sendButton != null) sendButton.onClick.AddListener(OnClickSend);
        if(closeButton != null) closeButton.onClick.AddListener(OnClickClose);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if(keyboard == null) return;

        if (keyboard.enterKey.wasPressedThisFrame)
        {
            OnClickSend();
        }
    }

    public void Open(string npcName, Func<string, string> replyFunc, Action onClose = null)
    {
        this.npcName = string.IsNullOrEmpty(npcName) ? "NPC" : npcName;
        this.replyFunc = replyFunc;
        this.onClose = onClose;

        if(root != null) root.SetActive(true);

        if(titleText != null) titleText.text = this.npcName;
        if(historyText != null) historyText.text = "";

        AppendLine($"{this.npcName}: Hello!");

        if(inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    public void AppendLine(string line)
    {
        if(historyText == null) return;

        historyText.text += line + "\n";

        if(scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }

    public void OnClickSend()
    {
        if (inputField == null) return;

        string user = inputField.text;
        if(string.IsNullOrEmpty(user)) return;

        inputField.text = "";
        AppendLine($"You: {user}");

        string reply = (replyFunc != null) ? replyFunc.Invoke(user) : "???";
        AppendLine($"{npcName}: {reply}");

        inputField.ActivateInputField();
        inputField.Select();
    }

    public void OnClickClose()
    {
        if(root != null) root.SetActive(false);
        replyFunc = null;

        onClose?.Invoke();
        onClose = null;
    }
}
