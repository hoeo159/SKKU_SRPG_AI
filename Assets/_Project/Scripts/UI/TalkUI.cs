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
    private Action<string> userSendAsync;
    private Action onClose;
    private string npcName = "NPC";
    private bool isBusy = false;

    public bool isOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if(root == null) root = gameObject;
        if(sendButton != null) sendButton.onClick.AddListener(OnClickSend);
        if(closeButton != null) closeButton.onClick.AddListener(OnClickClose);
    }

    private void Update()
    {
        if(!isOpen || isBusy) return;

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
        this.userSendAsync = null;
        this.onClose = onClose;

        OpenAndAppend();
    }

    public void OpenAsync(string npcName, Action<string> userSend, Action onClose = null)
    {
        this.npcName = string.IsNullOrEmpty(npcName) ? "NPC" : npcName;
        this.replyFunc = null;
        this.userSendAsync = userSend;
        this.onClose = onClose;

        OpenAndAppend();
    }

    private void OpenAndAppend()
    {
        if (root != null) root.SetActive(true);

        if (titleText != null) titleText.text = this.npcName;
        if (historyText != null) historyText.text = "";

        AppendLine($"{this.npcName}와 대화를 진행합니다.");

        SetBusy(false);

        if (inputField != null)
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

    public void AppendNPC(string text)
    {
        AppendLine($"{npcName}: {text}");
    }

    public void SetBusy(bool busy)
    {
        this.isBusy = busy;
        if(sendButton != null) sendButton.interactable = !busy;
        if(inputField != null) inputField.interactable = !busy;
    }

    public void OnClickSend()
    {
        if (isBusy) return;
        if (inputField == null) return;

        string user = inputField.text;
        if(string.IsNullOrEmpty(user)) return;

        inputField.text = "";
        AppendLine($"You: {user}");

        if(replyFunc != null)
        {
            string reply = (replyFunc != null) ? replyFunc.Invoke(user) : "......";
            AppendNPC(reply);
            inputField.ActivateInputField();
            inputField.Select();
        }
        if(userSendAsync != null)
        {
            SetBusy(true);
            AppendNPC("......");
            userSendAsync.Invoke(user);
            return;
        }

    }

    public void OnClickClose()
    {
        if(root != null) root.SetActive(false);
        replyFunc = null;
        userSendAsync = null;

        onClose?.Invoke();
        onClose = null;
    }
}
