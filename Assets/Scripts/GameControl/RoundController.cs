using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    public static RoundController Instance { get { return instance; } }
    private static RoundController instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private int round;
    private List<IEnumerator> spawnEnemyCoroutines;

    private void Start()
    {
        spawnEnemyCoroutines = new List<IEnumerator>();
    }

    public void StartRound()
    {
        // ���忡 ���� ���� �ð� �ο�
        StartCoroutine(ProgressRound(20));
    }

    private IEnumerator ProgressRound(float time)
    {
        // �� �� ���� �����Ͱ� ������ �Ǹ� ��Ȳ�� �°� ���õ� ��.
        spawnEnemyCoroutines.Add(EnemyController.Instance.SpawnEnemy(EnemyManager.Enemies[0], 1.5f));
        spawnEnemyCoroutines.Add(EnemyController.Instance.SpawnEnemy(EnemyManager.Enemies[1], 4.0f));
        spawnEnemyCoroutines.Add(EnemyController.Instance.SpawnEnemy(EnemyManager.Enemies[2], 2.5f));

        spawnEnemyCoroutines.ForEach(coroutine => StartCoroutine(coroutine));

        while (time > 0)
        {
            if (!GameController.Instance.Pause)
            {
                time -= Time.deltaTime;
                UIController.Instance.UpdateRoundTime(Mathf.CeilToInt(time));
            }
            yield return null;
        }

        GameController.Instance.EndRound();
        spawnEnemyCoroutines.ForEach(coroutine => StopCoroutine(coroutine));
    }
}
