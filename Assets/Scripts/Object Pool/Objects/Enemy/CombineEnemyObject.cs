using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineEnemyObject : EnemyObject, ICombineObject
{
    public bool IsCombined { get { return isCombined; } }
    private bool isCombined;

    public bool CheckHPState { get { return Hp <= data.combineHp; } }

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
        isCombined = true;

        // �ش� ��ġ���� �켱 �̵�.
        aiDestinationSetter.target = combineTarget.transform;
        aiPath.canMove = true;

        combineTarget.Combined(true);
        // �� ���� ��� ��ü�������� �������־�� ��.
        StartCoroutine(Combining());
    }

    public IEnumerator Combining()
    {
        // ���� ���� ������ ���.
        while (Vector2.Distance(transform.position, combineTarget.transform.position) > 1f) yield return null;
        aiPath.canMove = false;
        combineTarget.LookAt(transform.position);
        LookAt(combineTarget.transform.position);
        // �����ٸ� ���� �ð����� ���
        // ���� �ð� ������ ��ü�� �غ��ϴ� �ܰ�.
        // �� �ð� �ȿ� ������ ������ ������ ���� �� ����.
        float time = 0f;
        bool combining = true;
        while (time < 1.5f && combining)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            // Ÿ���� �߰��� �װ� �Ǹ� �ٽ� Ž�� �ؾ���.
            if (combineTarget.gameObject.activeSelf == false)
            {
                combining = false;
                isCombined = false;
                combineTarget = null;
                aiDestinationSetter.target = Player.Instance.transform;
            }
            yield return null;
        }

        if (combining)
        {
            // Ÿ���� ü���� �ش� ������ �����ִ� ü�¸�ŭ ȸ�� �� ���
            combineTarget.ActivateBuff(new BuffInfo() { hp = Hp });
            Dead();
        }
    }

    public void Combined(bool b)
    {
        isCombined = b;
        if (b) aiPath.canMove = false;
    }

    public override bool DetectPath()
    {
        // ��ü ���̶�� �̵��� ����� ��.
        if (isCombined)
        {
            aiPath.canMove = false;
            return false;
        }
        return base.DetectPath();
    }

    public override void Dead()
    {
        base.Dead();
        if (combineTarget != null) combineTarget.Combined(false);
        combineTarget = null;
        isCombined = false;
        aiDestinationSetter.target = Player.Instance.transform;
    }
}
