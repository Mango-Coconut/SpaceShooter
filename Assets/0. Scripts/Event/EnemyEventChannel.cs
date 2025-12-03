using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyEventChannel")]
public class EnemyEventChannel : ScriptableObject
{
    public event Action<string, int> OnEnemyKilled;
    // string = enemyId, int = killCount(보통 1)

    public void RaiseEnemyKilled(string enemyId, int amount = 1)
    {
        OnEnemyKilled?.Invoke(enemyId, amount);
    }
}
