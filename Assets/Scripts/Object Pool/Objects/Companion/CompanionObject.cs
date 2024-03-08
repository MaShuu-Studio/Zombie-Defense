using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionObject : BTPoolable,
    IMovingObject, IDamagedObject, IAttackObject
{
    [SerializeField] private SpriteRenderer gunSpriteRenderer;

    private int maxhp;
    private int hp;
    private int def;
    private int speed = 5;

    private Weapon weapon;
    private bool reloading;

    public Weapon UsingWeapon { get { return weapon; } }

    public override void Init()
    {
        base.Init();
        hp = maxhp = 5;
        def = 0;
        reloading = false;
        WaitAttack = false;
        SetBasicWeapon();
    }

    public void SetBasicWeapon()
    {
        ChangeWeapon("WEAPON.PISTOL");
    }

    public bool ChangeWeapon(string key)
    {
        Weapon w = WeaponManager.GetWeapon(key);
        if (w.infAmount || Player.Instance.ItemAmount(key) > 0)
        {
            // �ٸ��ٸ� ������ ���� ����� ��ü
            if (weapon != null && key != weapon.key)
            {
                // ����ǰ���� ���ư�.
                Player.Instance.AdjustItemAmount(weapon.key, 1);
            }
            weapon = new Weapon(w);
            gunSpriteRenderer.sprite = SpriteManager.GetSprite(w.key);
            if (w.infAmount == false) Player.Instance.AdjustItemAmount(key, -1);
            return true;
        }
        return false;
    }

    /*
    public List<Collider2D> DetectEnemyTargets(float range)
    {
        // ��ġ�� ĳ���� ������ ����
        // ũ��� range�� ����
        autoTargetCollider.offset = new Vector2(range / 2 + .5f, 0);
        autoTargetCollider.size = Vector2.one * range;

        List<Collider2D> cols = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = 1 << LayerMask.NameToLayer("Enemy");
        // ���� �簢�� ���
        autoTargetCollider.OverlapCollider(filter, cols);
        return cols;
    }
    */

    #region IMovingObject

    public enum PatrolType { NARROWLY = 0, WIDELY, LEAD, BACK, }
    private PatrolType patrolType;
    private bool move;
    private Vector2 targetPos;
    public int Speed { get { return speed; } }

    public void SetPatrolType(PatrolType type)
    {
        patrolType = type;
    }

    public bool DetectPath()
    {
        if (move) return true;

        targetPos = Player.Instance.transform.position;
        float x, y;
        switch (patrolType)
        {
            case PatrolType.NARROWLY:
                x = Random.Range(1, 2f);
                y = Random.Range(1, 2f);

                x *= (Random.Range(1, 3) % 2 == 0) ? 1 : -1;
                y *= (Random.Range(1, 3) % 2 == 0) ? 1 : -1;

                targetPos += new Vector2(x, y);
                break;
            case PatrolType.WIDELY:
                x = Random.Range(3, 4f);
                y = Random.Range(3, 4f);

                x *= (Random.Range(1, 3) % 2 == 0) ? 1 : -1;
                y *= (Random.Range(1, 3) % 2 == 0) ? 1 : -1;

                targetPos += new Vector2(x, y);
                break;
            case PatrolType.LEAD:
                break;
            case PatrolType.BACK:
                break;
        }
        move = true;
        return true;
    }

    public void Move()
    {
        float moveAmount = Time.deltaTime * speed;
        Vector3 dir = (targetPos - (Vector2)transform.position).normalized;

        LookAt(targetPos);
        transform.position += dir * moveAmount;

        // �̵��� ���� �����̸� �������� ����
        if (Vector2.Distance(targetPos, transform.position) < moveAmount)
        {
            // ��Ʈ�� �ʱ�ȭ
            move = false;
        }
    }

    #endregion
    #region IDamagedObject
    public int Hp { get { return hp; } }
    public int MaxHp { get { return maxhp; } }
    public int Def { get { return def; } }

    public void Heal()
    {
        hp = maxhp;
    }

    public void Damaged(int dmg)
    {
        hp -= dmg;
        if (hp < 0)
        {
            StopAllCoroutines();
            CompanionController.Instance.RemoveCompanion(this);
        }
    }
    #endregion

    #region IAttackObject
    public int Dmg { get { return (weapon != null) ? weapon.dmg : 0; } }
    public float Range { get { return (weapon != null) ? weapon.range : 0; } }
    public float ADelay { get { return (weapon != null) ? weapon.adelay : 0; } }
    public bool WaitAttack { get; set; }

    private Collider2D targetCollider;
    public Collider2D TargetCollider { get { return targetCollider; } }

    Collider2D[] targets;
    Transform target;
    public bool DetectTarget()
    {
        if (weapon == null || reloading) return false;

        targets = Physics2D.OverlapCircleAll(transform.position, Range / 2, 1 << LayerMask.NameToLayer("Enemy"));
        return targets != null && targets.Length > 0;
    }

    public void LookAt(Vector3 target)
    {
        Vector2 dir = target - transform.position;
        float degree = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        transform.rotation = Quaternion.Euler(0, 0, degree);
    }

    public void Attack()
    {
        // ��Ʈ�� �ʱ�ȭ
        move = false;
        // �켱������ ���� ���� �����ϴ� �ڵ尡 �� ����
        target = targets[0].transform;

        if (!WaitAttack && !reloading)
        {
            if (weapon.curammo <= 0)
            {
                Reload();
                return;
            }
            LookAt(target.transform.position);
            Vector2 dir = target.position - transform.position;
            int spread = weapon.bulletspreadangle;
            for (int i = 0; i < weapon.bullets; i++)
            {
                int angle = Random.Range(-spread / 2, spread / 2 + 1);
                Vector3 newDir = Quaternion.Euler(0, 0, angle) * dir;
                ((Bullet)PoolController.Pop("Bullet")).SetBullet(transform.position, target.position, newDir, weapon, weapon.bulletSpeed);
            }
            SoundController.Instance.PlaySFX(Player.Instance.gameObject, weapon.key);
            weapon.curammo--;

            StartCoroutine(AttackTimer());
        }
    }

    public IEnumerator AttackTimer()
    {
        WaitAttack = true;
        float time = 0;
        while (time < ADelay)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            yield return null;
        }
        WaitAttack = false;
    }
    #endregion

    private IEnumerator reloadCoroutine;

    public void Reload()
    {
        // Magazine�� �� �������ٸ� �������� ��ȯ
        if (!Player.Instance.HasMagazine(weapon.key))
        {
            SetBasicWeapon();
            return;
        }
        if (reloading) return;
        reloadCoroutine = Reloading();
        StartCoroutine(reloadCoroutine);
    }

    IEnumerator Reloading()
    {
        reloading = true;
        UIController.Instance.Reloading(true);

        float time = weapon.reload;
        while (time > 0)
        {
            if (!GameController.Instance.Pause) time -= Time.deltaTime;
            yield return null;
        }
        weapon.Reload();
        reloading = false;
    }
}

