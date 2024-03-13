using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;

[AddComponentMenu("Poolable/Attacking Turret (Poolable)")]
public class AttackTurretObject : TurretObject, IAttackObject
{
    [SerializeField] private LocalizeStringEvent mountedWeapon;
    private Collider2D targetCollider;
    private int dmg;
    private float range;
    private float aDelay;
    private int speed;
    private bool reloading;
    private IEnumerator reloadCoroutine;

    private Weapon weapon;

    public Collider2D TargetCollider { get { return targetCollider; } }
    public int Dmg { get { return dmg; } }
    public float Range { get { return range; } }
    public float ADelay { get { return aDelay; } }
    public bool WaitAttack { get { return weapon != null ? weapon.Wait : false; } }

    public override void SetData(Turret data, Vector2 pos)
    {
        base.SetData(data, pos);
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;
        speed = data.speed;
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
            dmg = weapon.dmg;
            range = weapon.range;
            aDelay = weapon.adelay;
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

        targets = Physics2D.OverlapCircleAll(transform.position, range / 2, 1 << LayerMask.NameToLayer("Enemy"));
        return targets != null && targets.Length > 0;
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
