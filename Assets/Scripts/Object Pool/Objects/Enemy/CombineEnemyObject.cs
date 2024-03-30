using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineEnemyObject : EnemyObject, ICombineObject
{
    public bool IsCombined { get { return isCombined; } }
    private bool isCombined;
    public bool IsCombining { get { return isCombining; } }
    private bool isCombining;

    public bool CheckHPState { get { return Hp <= data.thresholdHp; } }

    CombineEnemyObject combineTarget = null;
    public bool DetectOtherObject()
    {
        // ��� 1. Ư�� ���� ���� ��ü�� ������ �ֳ� üũ.
        // ��� 2. EnemyController�κ��� enemy�� ���� �޾ƿ� �� ICombineObject�� ������ �ִ��� üũ. ����� ���ֿ��� ����

        // �� �� �̹� �ش� ������ ��ü�� �ϰ��ִ� ���¶�� Detecting�� ���� �ʾƾ���.

        if (combineTarget != null) return true;

        foreach (var enemy in EnemyController.Instance.SpawnEnemies)
        {
            if (data.key != enemy.Data.key || enemy == this) continue;
            // ĳ��Ʈ�� �ؼ� ĳ��Ʈ�� �ȴٸ� combine�� �� �ִ� ������.
            CombineEnemyObject ce = (CombineEnemyObject)enemy;
            if (ce && !ce.IsCombined)
            {
                // ����ְ� �Ÿ��� 10f ������ ���
                // ��������� ������ ���� ã�� ������ �� ����� ���
                if ((combineTarget == null && Vector2.Distance(transform.position, ce.transform.position) < 7.5f)
                    || (combineTarget != null && Vector2.Distance(transform.position, combineTarget.transform.position)
                         > Vector2.Distance(transform.position, ce.transform.position)))
                {
                    combineTarget = ce;
                }
            }
        }

        return combineTarget != null;
    }

    public void Combine()
    {
        // �ش� ��ġ���� �켱 �̵�.
        moveTarget = combineTarget.transform;
        SetPath();
        isCombined = true;

        combineTarget.Combined(true);
        // �� ���� ��� ��ü�������� �������־�� ��.
        StartCoroutine(Combining());
    }

    public IEnumerator Combining()
    {
        // ���� ���� ������ ���.
        while (Vector2.Distance(transform.position, combineTarget.transform.position) > 1f) yield return null;
        combineTarget.LookAt(transform.position);
        LookAt(combineTarget.transform.position);
        isCombining = true;
        // �����ٸ� ���� �ð����� ���
        // ���� �ð� ������ ��ü�� �غ��ϴ� �ܰ�.
        // �� �ð� �ȿ� ������ ������ ������ ���� �� ����.
        float time = 0f;
        while (time < 1.5f && IsCombining)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            // Ÿ���� �߰��� �װ� �Ǹ� �ٽ� Ž�� �ؾ���.
            if (combineTarget.gameObject.activeSelf == false)
            {
                isCombining = false;
                isCombined = false;
                combineTarget = null;
                moveTarget = Player.Instance.transform;
                SetPath();
            }
            yield return null;
        }

        if (isCombining)
        {
            // Ÿ���� ü���� �ش� ������ �����ִ� ü�¸�ŭ ȸ�� �� ���
            combineTarget.ActivateBuff(new BuffInfo() { hp = Hp });
            Dead();
        }
    }

    public void Combined(bool b)
    {
        isCombined = b;
        isCombining = b;
    }

    public override void Dead()
    {
        base.Dead();
        if (combineTarget != null) combineTarget.Combined(false);
        combineTarget = null;
        isCombined = false;
        moveTarget = Player.Instance.transform;
    }
}
