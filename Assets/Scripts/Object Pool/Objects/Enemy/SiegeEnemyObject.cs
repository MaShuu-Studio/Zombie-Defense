using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeEnemyObject : EnemyObject
{
    private static float meleeRange = 1.5f;
    private bool meleeAttack;
    private bool targetIsTurret;
    public override bool DetectTarget()
    {
        // �켱 �ٰŸ� ���� ���� ������ Ÿ���� �ִ��� üũ
        // �� �� ���Ÿ� ���� ���� ������ Ÿ���� �ִ��� üũ
        // �� �Ŀ����� �÷��̾� Ž��.
        // �� �� Ʈ���� ���� �������� ����. Ʈ���� ���Ÿ� ���� �������� �μ��� ����.

        // �������̶�� �ش� Ÿ���� ������ ���� ������ üũ
        // �װ� �ƴ϶�� 0.8������ �ȿ� �ֳ� üũ

        // ���� ��İ� ������ �������� üũ
        // layer�� �����ϴ� ���� ����� ����ϸ� ������ ��ĥ �� ���� �� ����.

        targetCollider = null;
        targetIsTurret = true;
        meleeAttack = true;
        FindTargets(meleeRange, .8f, 1 << LayerMask.NameToLayer("Turret"));

        // ���Ÿ� ������ ���� �� ���� ���� ����� ã�� ���ߴٸ�
        // ���Ÿ� ������� Ž�� ����
        if (data.siegeRadius > 0 && targetCollider == null
            && FindTargets(Range, .8f, 1 << LayerMask.NameToLayer("Turret")).Length > 0)
        {
            meleeAttack = false;
        }

        // �ͷ��� �ƿ� ã�� ���ߴٸ� ���� �������� �÷��̾� Ž��
        if (targetCollider == null
            && FindTargets(meleeRange, .75f, 1 << LayerMask.NameToLayer("Player")).Length > 0)
        {
            targetIsTurret = false;
        }

        if (targetCollider == null) isAttacking = false;
        else LookAt(targetCollider.transform.position);

        return targetCollider != null;
    }

    public override void Attack()
    {
        AdjustMove(false);
        // ���Ÿ� ���ݰ� �ٰŸ� ������ �����Ͽ� �۵���Ŵ.
        isAttacking = true;
        // �ش� �κ� ���� �Լ��� ��ġ ����.
        if (!WaitAttack)
        {
            IDamagedObject damagedObject = targetCollider.transform.parent.GetComponent<IDamagedObject>();
            if (meleeAttack)
            {
                int dmg = (int)(Dmg * (targetIsTurret ? 1.5f : 1f));
                damagedObject.Damaged(dmg);
            }
            else
            {
                // ���Ÿ� ������ ���� �������� ������ ����ü�� ������ ���.
                var proj = (Projectile)PoolController.Pop("Projectile");
                proj.SetProj(transform.position, targetCollider.transform.position, Dmg, data.siegeRadius, 10);
            }
            StartCoroutine(AttackTimer());
        }
    }
}
