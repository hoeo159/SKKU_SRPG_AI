using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class APIKeyAskUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private OpenAIResponseClient openAIClient;

    private Action<string> onSubmitCallback;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (submitButton != null) submitButton.onClick.AddListener(OnClickSubmit);

        if (inputField != null)
        {
            inputField.contentType = TMP_InputField.ContentType.Password;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
        }
    }

    public void Show(Action<string> onSubmit)
    {
        onSubmitCallback = onSubmit;
        if (errorText != null) errorText.text = "";
        if (inputField != null) inputField.text = "";
        if (panel != null) panel.SetActive(true);

        if (inputField != null) inputField.ActivateInputField();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void OnClickSubmit()
    {
        string key = inputField != null ? inputField.text.Trim() : "";

        if (string.IsNullOrEmpty(key))
        {
            ShowError("API 키를 입력해주세요.");
            return;
        }

        StartCoroutine(Co_ValidateAndSubmit(key));
    }
    private IEnumerator Co_ValidateAndSubmit(string key)
    {
        SetBusy(true);
        ShowMessage("키 검사 중...", false);

        bool isValid = false;
        string errMsg = null;

        if (openAIClient != null)
        {
            yield return openAIClient.ValidateApiKey(key, (ok, msg) =>
            {
                isValid = ok;
                errMsg = msg;
            });
        }
        else
        {
            isValid = true;
        }

        SetBusy(false);

        if (isValid)
        {
            Hide();
            onSubmitCallback?.Invoke(key);
        }
        else
        {
            ShowError(errMsg ?? "키 검사에 실패했습니다.");
        }
    }

    private void SetBusy(bool busy)
    {
        if (submitButton != null) submitButton.interactable = !busy;
        if (inputField != null) inputField.interactable = !busy;
    }

    private void ShowError(string msg)
    {
        ShowMessage(msg, true);
    }

    private void ShowMessage(string msg, bool isError)
    {
        if (errorText == null) return;
        errorText.text = msg;
    }
}