using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class HubEventUI : MonoBehaviour
{
    [SerializeField] private GameObject root;

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button optionAButton;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private Button optionBButton;
    [SerializeField] private TMP_Text optionBText;

    private Action onChooseA;
    private Action onChooseB;

    public bool isOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (root == null) root = this.gameObject;
    }

    public void Open(HubEventData eventData, Action chooseA, Action chooseB)
    {
        if (titleText != null) titleText.text = eventData.title;
        if (descText != null) descText.text = eventData.description;
        if (optionAText != null) optionAText.text = eventData.optionA;
        if (optionBText != null) optionBText.text = eventData.optionB;

        onChooseA = chooseA;
        onChooseB = chooseB;

        SetButton(true);
    }

    public void Close()
    {
        onChooseA = null;
        onChooseB = null;

        SetButton(false);
    }

    private void SetButton(bool enable)
    {
        if (optionAButton != null) optionAButton.interactable = enable;
        if (optionBButton != null) optionBButton.interactable = enable;
    }
}
