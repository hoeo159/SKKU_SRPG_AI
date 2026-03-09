using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIRaycast : MonoBehaviour
{
    GraphicRaycaster raycaster;
    EventSystem es;

    void Awake()
    {
        raycaster = FindFirstObjectByType<GraphicRaycaster>();
        es = EventSystem.current;
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (raycaster == null || es == null)
        {
            Debug.Log("[UIRaycast] Missing GraphicRaycaster or EventSystem.");
            return;
        }

        var ped = new PointerEventData(es);
        ped.position = Mouse.current.position.ReadValue();

        var results = new List<RaycastResult>();
        raycaster.Raycast(ped, results);

        if (results.Count == 0)
        {
            Debug.Log("[UIRaycast] nothing hit");
        }
        else
        {
            Debug.Log("[UIRaycast] top hit: " + results[0].gameObject.name);
        }
    }
}