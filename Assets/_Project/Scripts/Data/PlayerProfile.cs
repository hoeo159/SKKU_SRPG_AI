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

    public static PlayerProfile Add(PlayerProfile profile, PlayerProfile delta)
    {
        profile.mercy += delta.mercy;
        profile.greedy += delta.greedy;
        profile.curious += delta.curious;
        profile.discipline += delta.discipline;
        profile.risk += delta.risk;
        profile.social += delta.social;
        profile.cruel += delta.cruel;
        profile.caution += delta.caution;
        profile.Clamp();
        return profile;
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
