using UnityEngine;

public static class EventApplier
{
    public static void ApplyOption(GameStateSO state, EventOption option)
    {
        if (state == null) return;
        if (option.effects == null) return;

        foreach(var effect in option.effects)
        {
            switch(effect.type)
            {
                case EventEffectType.AddGold:
                    state.gold += effect.value;
                    break;

                case EventEffectType.AddGuardAlert:
                    state.guardAlert = Mathf.Clamp(state.guardAlert + effect.value, 0, 100);
                    break;

                case EventEffectType.AddMerchantTrust:
                    state.merchantTrust = Mathf.Clamp(state.merchantTrust + effect.value, 0, 100);
                    break;

                case EventEffectType.AddEnemyAgressive:
                    state.enemyAgressive = Mathf.Clamp(state.enemyAgressive + effect.value, 0, 100);
                    break;

                case EventEffectType.AddShelterComfort:
                    state.shelterComfort = Mathf.Clamp(state.shelterComfort + effect.value, 0, 100);
                    break;

                case EventEffectType.AddRadiation:
                    state.radiation = Mathf.Clamp(state.radiation + effect.value, 0, 100);
                    break;
            }
        }
    }
}
