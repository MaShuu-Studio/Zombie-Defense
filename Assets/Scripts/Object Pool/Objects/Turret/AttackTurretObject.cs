using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;

[AddComponentMenu("Poolable/Attacking Turret (Poolable)")]
public class AttackTurretObject : TurretObject, IAttackObject
{
    [SerializeField] private LocalizeStringEvent mountedWeapon;
    private Collider2D targetCollider;
    private bool reloading;
    private IEnumerator reloadCoroutine;

    private Weapon weapon;

    public Collider2D TargetCollider { get { return targetCollider; } }
    public int Dmg { get { return weapon != null ? weapon.dmg : 0; } }
    public float Range { get { return weapon != null ? weapon.range : 0; } }
    public float ADelay { get { return weapon != null ? weapon.adelay : 0; } }
    public bool WaitAttack { get { return weapon != null ? weapon.Wait : false; } }

    public override void SetData(Turret data, Vector2 pos)
    {
        base.SetData(data, pos);
        weapon = null;
        mountedWeapon.SetEntry("WEAPON.NONE");
    }

    public void Mount(Weapon w, bool b)
    {
        // ����Ʈ �ϴ� ���
        if (b)
        {
            if (w.infAmount == false && Player.Instance.ItemAmount(w.key) <= 0) return;

            // �̹� ���Ⱑ ������� ���
            if (weapon != null)
            {
                // ������ �ִ� ����� ����ǰ����
                Player.Instance.AdjustItemAmount(weapon.key, 1);
            }
            weapon = new Weapon(w);
            mountedWeapon.SetEntry(w.key);
            Player.Instance.AdjustItemAmount(w.key, -1);
        }
        // �𸶿�Ʈ �ϴ� ���
        else
        {
            // ������ �ִ� ����� ����ǰ����
            Player.Instance.AdjustItemAmount(weapon.key, 1);
            weapon = null;
            mountedWeapon.SetEntry("WEAPON.NONE");
        }
    }

    Collider2D[] targets;
    Transform target;
    public bool DetectTarget()
    {
        if (weapon == null || reloading) return false;

        targets = Physics2D.OverlapCircleAll(transform.position, Range / 2, 1 << LayerMask.NameToLayer("Enemy"));
        return targets.Length > 0;
    }

    public void Attack()
    {
        // �켱������ ���� ���� �����ϴ� �ڵ尡 �� ����
        target = targets[0].transform;

        if (!WaitAttack)
        {
            if (weapon.curammo <= 0)
            {
                Reload();
                return;
            }
            weapon.Fire(transform.position, target.position, transform.rotation.eulerAngles.z);
            StartCoroutine(weapon.AttackDelay());
        }
    }

    public void Reload()
    {
        if (reloading || !Player.Instance.HasMagazine(weapon.key)) return;
        reloadCoroutine = weapon.Reloading();
        StartCoroutine(reloadCoroutine);
    }

    public override void DestroyTurret()
    {
        base.DestroyTurret();
        StopAllCoroutines();

        // ���Ⱑ �־��ٸ� �𸶿�Ʈ
        if (weapon != null) Mount(weapon, false);
        weapon = null;
        reloading = false;
    }
}
