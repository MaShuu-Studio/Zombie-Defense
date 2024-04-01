using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestEnemyObject : EnemyObject, IRestObject
{
    public bool CheckHPState { get { return Hp <= data.thresholdHp; } }
    // ���� ���ٸ� ������ ����.
    public bool IsArrived { get { return !DetectPath(); } }
    public bool IsRunningAway { get { return isRunningAway; } }
    public bool IsHealed { get { return isHealed; } }

    private bool isRunningAway;
    private bool isHealed;

    private Poolable runawayPoint;

    public override void SetData(Enemy data, int remainSep = -1)
    {
        base.SetData(data, remainSep);
        runawayPoint = PoolController.Pop("Movepoint");
        moveTarget = Player.Instance.transform;
    }

    public void Rest()
    {
        // �̵� ����, ȸ�� ����
        isRunningAway = false;
        isHealed = true;
        AdjustMove(false);
        StartCoroutine(Resting());
    }

    public void Runaway()
    {
        // ����ġ�� ��ġ�� �÷��̾�κ��� �ݴ��� ��ġ�� �̵�.
        Vector3 dir = transform.position - Player.Instance.transform.position;
        dir = dir.normalized;
        runawayPoint.transform.position = transform.position;
        for (int i = 0; i < 10; i++)
        {
            // �ݴ���ġ�� �̵��� �� �� ���̶�� �ش���ġ���� ��ž.
            Vector2Int nextPos = MapGenerator.RoundToInt(runawayPoint.transform.position + dir);
            if (MapGenerator.PosOnMap(MapGenerator.ConvertToMapPos(nextPos))) runawayPoint.transform.position += dir;
            else break;
        }
        moveTarget = runawayPoint.transform;
        SetPath();
        isRunningAway = true;
    }

    IEnumerator Resting()
    {
        float time = 0;
        while (Hp < data.hp)
        {
            while (time < 1f)
            {
                if (!GameController.Instance.Pause) time += Time.deltaTime;
                yield return null;
            }
            time -= 1f;
            Heal(data.restHealAmount);
        }
        // ȸ���� �����ٸ� �ٽ� �÷��̾ ã���� ��.
        moveTarget = Player.Instance.transform;
        isHealed = false;
    }

    public override void Dead()
    {
        base.Dead();
        PoolController.Push("Movepoint", runawayPoint);
        isHealed = false;
        isRunningAway = false;
    }
}
