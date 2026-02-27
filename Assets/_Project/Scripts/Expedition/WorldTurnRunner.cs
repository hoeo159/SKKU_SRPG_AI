using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldTurnRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CombatUnit playerUnit;
    [SerializeField] private bool isNeutral = true;

    public bool busy { get; private set; } = false;

    private readonly List<CombatUnit> playerUnits = new List<CombatUnit>();

    private void Awake()
    {
        if(gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    public void SetPlayer(CombatUnit player)
    {
        playerUnit = player;

        playerUnits.Clear();
        if (playerUnit != null) playerUnits.Add(playerUnit);
    }

    public IEnumerator RunWorldTurn()
    {
        if (busy) yield break;
        busy = true;

        GameStateSO state = GameManager.gameManager != null ? GameManager.gameManager.state : null;

        // enemy turn
        EnemyController[] enemyControllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach(var enemy in enemyControllers)
        {
            if (enemy == null) continue;
            yield return enemy.TakeTurn(state, playerUnits);
        }

        // npc turn
        if (isNeutral)
        {
            NpcController[] npcs = FindObjectsByType<NpcController>(FindObjectsSortMode.None);
            foreach (var npc in npcs)
            {
                if (npc == null) continue;
                yield return npc.TakeTurn(state, playerUnit);
            }
        }

        busy = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
