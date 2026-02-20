using UnityEngine;
using System;
using UnityEngine.Rendering.LookDev;

public enum  EventEffectType
{
    AddGold,
    AddGuardAlert,
    AddMerchantAlert,
    AddEnemyAgressive
}

[Serializable]
public struct EventEffect
{
    public EventEffectType  type;
    public int              value;
}

[Serializable]
public struct EventOption
{
    [TextArea(1, 10)]
    public string           optionText;
    [TextArea(2, 10)]
    public string           optionValue;
    public EventEffect[]    effects;
}

[CreateAssetMenu(fileName = "EventCard", menuName = "Scriptable Objects/EventCard")]
public class EventCardSO : ScriptableObject
{
    public string id;
    public string title;
    [TextArea(4, 10)]
    public string description;

    [Header("Weight")]
    public float baseWeight = 10.0f;

    [Header("profile influence")]
    public float wMercy      = 0.0f;
    public float wGreedy     = 0.0f;
    public float wCurious    = 0.0f;
    public float wDiscipline = 0.0f;
    public float wRisk       = 0.0f;
    public float wSocial     = 0.0f;
    public float wCruel      = 0.0f;
    public float wCaution    = 0.0f;

    [Header("Options")]
    public EventOption option;
}
