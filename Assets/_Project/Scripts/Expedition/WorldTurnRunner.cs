using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldTurnRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    public bool busy { get; private set; } = false;

    private readonly List<CombatUnit> playerUnits = new List<CombatUnit>();

    private void Awake()
    {
        if(gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    void RefreshPlayerUnits()
    {
        playerUnits.Clear();

        var units = FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        foreach (var u in units)
        {
            if (u == null || u.isDead) continue;
            if (u.Faction != Faction.Player) continue;
            playerUnits.Add(u);
        }
    }


    public IEnumerator RunWorldTurn()
    {
        if (busy) yield break;
        busy = true;

        RefreshPlayerUnits();

        GameStateSO state = GameManager.gameManager != null ? GameManager.gameManager.state : null;

        // enemy turn
        EnemyController[] enemyControllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach(var enemy in enemyControllers)
        {
            if (enemy == null) continue;
            yield return enemy.TakeTurn(state, playerUnits);
        }

        // npc turn
        //if (isNeutral)
        //{
        //    NpcController[] npcs = FindObjectsByType<NpcController>(FindObjectsSortMode.None);
        //    foreach (var npc in npcs)
        //    {
        //        if (npc == null) continue;
        //        yield return npc.TakeTurn(state, playerUnits);
        //    }
        //}

        busy = false;
    }
}
