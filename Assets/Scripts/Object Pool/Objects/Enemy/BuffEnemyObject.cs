using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEnemyObject : EnemyObject, IBuffObject
{
    public BuffInfo Buff { get { return buff; } }
    private BuffInfo buff;

    private float buffRange;

    public bool WaitBuff { get; set; }
    public float BuffRange { get { return buffRange; } }
    public float BDelay { get { return Buff.delay; } }

    private List<IBuffTargetObject> buffTargets = new List<IBuffTargetObject>();

    public override void SetData(Enemy data)
    {
        base.SetData(data);
        buff = data.buff;
        buffRange = 3; // �ӽü���
        WaitBuff = false;
    }

    public bool DetectBuffTarget()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, buffRange, 1 << LayerMask.NameToLayer("Enemy"));
        buffTargets.Clear();
        foreach (var col in cols)
        {
            var target = col.transform.parent.GetComponent<IBuffTargetObject>();
            if (target != null)
            {
                // ���������� ��� ����� ������ �ʰ� ���� �ο�
                if (Buff.area) buffTargets.Add(target);
                // ���Ϲ����� ��� ������ ���� ���ֿ��Ը� ���� �ο� �� �ݺ��� Ż��
                else if (!target.BuffIsActivated)
                {
                    buffTargets.Add(target);
                    break;
                }
            }
        }
        return buffTargets.Count > 0;
    }

    public void GiveBuff()
    {
        foreach (var target in buffTargets)
        {
            target.ActivateBuff(buff);
        }
        StartCoroutine(GiveBuffTimer());
    }

    public IEnumerator GiveBuffTimer()
    {
        WaitBuff = true;
        float time = 0;
        while (time < BDelay)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            yield return null;
        }
        WaitBuff = false;
    }
}
