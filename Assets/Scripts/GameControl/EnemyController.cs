using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get { return instance; } }
    private static EnemyController instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        spawnedEnemies = new List<EnemyObject>();
    }

    private List<EnemyObject> spawnedEnemies;

    public void DeadEnemy(EnemyObject enemy)
    {
        spawnedEnemies.Remove(enemy);
    }

    public void EndRound()
    {
        while (spawnedEnemies.Count > 0) spawnedEnemies[0].Dead();
    }

    public IEnumerator SpawnEnemy(Enemy enemy, float time)
    {
        float t = 0;
        while (true)
        {
            if (!GameController.Instance.Pause) t += Time.deltaTime;

            if (t >= time)
            {
                t -= time;
                if (MapGenerator.Instance.Map == null) yield return null;

                // �Ŀ��� ���� �̸��� �ϳ��� �������� �� ����. 
                // ����� �׽�Ʈ �뵵�� �������� �з�����.


                EnemyObject enemyObject = (EnemyObject)PoolController.Pop(GetEnemyPrefabName(enemy));
                enemyObject.SetData(enemy);
                AddEnemy(enemyObject, MapGenerator.Instance.GetEnemySpawnPos());
            }
            yield return null;
        }
    }

    public void AddEnemy(EnemyObject enemyObject, Vector3 pos)
    {
        enemyObject.transform.position = pos; 
        spawnedEnemies.Add(enemyObject);
    }

    public static string GetEnemyPrefabName(Enemy enemy)
    {
        string prefabname = "Enemy";
        if (enemy.fly) prefabname = "Flight " + prefabname;
        if (enemy.inv) prefabname = "Invisible " + prefabname;
        if (enemy.buff != null) prefabname = "Buff Enemy";
        if (enemy.summonAmount != 0) prefabname = "Summon Enemy";

        return prefabname;
    }
}
