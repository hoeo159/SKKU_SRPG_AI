using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Canvas canvas;

    [Header("Button")]
    [SerializeField] private Button moveButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button cancelButton;

    public bool isOpen => panel != null && panel.gameObject.activeSelf;

    private void Awake()
    {
        if(panel == null) panel = GetComponent<RectTransform>();
        if(canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    public void SetButton(bool canMove, bool canTalk, bool canAttack)
    {
        if(moveButton != null) moveButton.interactable = canMove;
        if(talkButton != null) talkButton.interactable = canTalk;
        if(attackButton != null) attackButton.interactable = canAttack;
    }

    public void Open(Vector3 wpos, Camera cam, Vector2 offset)
    {
        if (panel == null || canvas == null || cam == null) return;
        panel.gameObject.SetActive(true);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, wpos);
        screenPos += offset;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Camera uicam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uicam, out Vector2 localPos))
        {
            panel.anchoredPosition = localPos;
        }
    }

    public void Close()
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
    }
}
