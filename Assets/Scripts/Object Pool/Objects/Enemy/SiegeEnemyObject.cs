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

        targetCollider = null;

        int turretLayer = 1 << LayerMask.NameToLayer("Turret");
        targetIsTurret = true;
        float range = meleeRange;
        if (!isAttacking) range *= .8f;
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, turretLayer);
        if (cols != null && cols.Length > 0)
        {
            targetCollider = cols[0];
            meleeAttack = true;
        }

        if (data.siegeRadius > 0)
        {
            // �и� ������ ã�� ����.
            if (targetCollider == null)
            {
                range = Range;
                if (!isAttacking) range *= .8f;
                cols = Physics2D.OverlapCircleAll(transform.position, range, turretLayer);
                if (cols != null && cols.Length > 0)
                {
                    targetCollider = cols[0];
                    meleeAttack = false;
                }
            }
        }

        // ���� �ͷ��� ������ ã�� ����.
        if (targetCollider == null)
        {
            range = meleeRange;
            if (!isAttacking) range *= .8f;
            targetCollider = Physics2D.OverlapCircle(transform.position, range, 1 << LayerMask.NameToLayer("Player"));
            if (targetCollider != null) targetIsTurret = false;
        }

        if (targetCollider == null) isAttacking = false;
        else
        {
            LookAt(targetCollider.transform.position);
            aiPath.canMove = false;
        }

        return targetCollider != null;
    }

    public override void Attack()
    {
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
