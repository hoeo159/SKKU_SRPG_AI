using System.Collections.Generic;
using UnityEngine;
using static GridPath;

public class EnemyUtilityAI
{
    public struct SelectedAction
    {
        public Vector2Int moveTo;
        public CombatUnit target;
        public float score;
    }

    public static SelectedAction Select(GridManager gridManager, CombatUnit enemy, List<CombatUnit> players)
    {
        ReachResult reach = GridPath.BFS_Reachable(gridManager, enemy.coord, enemy.UnitData.moveRange);

        if(!reach.dist.ContainsKey(enemy.coord)) reach.dist[enemy.coord] = 0;

        SelectedAction best = new SelectedAction
        {
            moveTo = enemy.coord,
            target = null,
            score = float.NegativeInfinity
        };

        foreach(var i in reach.dist)
        {
            Vector2Int candidate = i.Key;

            CombatUnit bestTarget = null;
            float bestTargetScore = float.NegativeInfinity;

            foreach(var player in players)
            {
                if(player == null || player.isDead) continue;

                int dist = GridPath.Manhattan(candidate, player.coord);
                if(dist <= enemy.UnitData.attackRange)
                {
                    // 가치 평가
                    int damage = enemy.DamageTo(player);
                    bool canKill = damage >= player.HP;

                    float curScore = 0f;
                    curScore += damage * 10f;
                    if (canKill) curScore += 200f;
                    //if(player.UnitData != null) value += player.UnitData.aiMoveWeight;

                    if(curScore > bestTargetScore)
                    {
                        bestTargetScore = curScore;
                        bestTarget = player;
                    }
                }
            }

            float score = 0f;

            // 1) 공격 가능하면 점수 추가
            if(bestTarget != null) score += bestTargetScore;

            // 2) 거리 점수(공격 못하면 접근 선호)
            int bestDist = int.MaxValue;
            foreach(var player in players)
            {
                if(player == null || player.isDead) continue;
                int dist = GridPath.Manhattan(candidate, player.coord);
                if(dist < bestDist) bestDist = dist;
            }

            if (bestDist < int.MaxValue) score += (-bestDist) * 5f;

            // 3) 플레이어가 이 타일을 공격 가능한지 휴리스틱
            int danger = 0;
            foreach(var player in players)
            {
                if (player == null || player.isDead) continue;
                int dist = GridPath.Manhattan(candidate, player.coord);
                if(player.UnitData != null && dist <= player.UnitData.attackRange)
                {
                    danger += 1;
                }
            }
            score += (-danger) * 20f;

            // 4) 너무 먼 이동은 감점
            score += (-i.Value) * 2f;

            // 5) 동점 방지
            score += Random.Range(-0.5f, 0.5f);

            if (score > best.score)
            {
                best.score = score;
                best.moveTo = candidate;
                best.target = bestTarget;
            }
        }

        return best;
    }
}
