using UnityEngine;
using System;

[Serializable]
public struct PlayerProfile
{
    // [Range(0, 100)] except for delta profile can be negative
    public int mercy;
    public int greedy;
    public int curious;
    public int discipline;
    public int risk;
    public int social;
    public int cruel;
    public int caution;

    public static PlayerProfile Init()
    {
        return new PlayerProfile
        {
            mercy = 50,
            greedy = 50,
            curious = 50,
            discipline = 50,
            risk = 50,
            social = 50,
            cruel = 50,
            caution = 50
        };
    }

    public void Clamp()
    {
        mercy = Mathf.Clamp(mercy, 0, 100);
        greedy = Mathf.Clamp(greedy, 0, 100);
        curious = Mathf.Clamp(curious, 0, 100);
        discipline = Mathf.Clamp(discipline, 0, 100);
        risk = Mathf.Clamp(risk, 0, 100);
        social = Mathf.Clamp(social, 0, 100);
        cruel = Mathf.Clamp(cruel, 0, 100);
        caution = Mathf.Clamp(caution, 0, 100);
    }

    public static PlayerProfile Add(PlayerProfile p, PlayerProfile delta)
    {
        p.mercy += delta.mercy;
        p.greedy += delta.greedy;
        p.curious += delta.curious;
        p.discipline += delta.discipline;
        p.risk += delta.risk;
        p.social += delta.social;
        p.cruel += delta.cruel;
        p.caution += delta.caution;
        p.Clamp();
        return p;
    }

    public string ToReportForm()
    {
        return
            $"Mercy: {mercy}\n" +
            $"Greedy: {greedy}\n" +
            $"Curious: {curious}\n" +
            $"Discipline: {discipline}\n" +
            $"Risk: {risk}\n" +
            $"Social: {social}\n" +
            $"Cruel: {cruel}\n" +
            $"Caution: {caution}";
    }
}
