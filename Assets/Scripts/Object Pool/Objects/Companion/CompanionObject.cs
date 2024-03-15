using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionObject : BTPoolable,
    IMovingObject, IDamagedObject, IAttackObject
{
    [SerializeField] private SpriteRenderer gunSpriteRenderer;
    [SerializeField] private BoxCollider2D autoTargetCollider;

    private int maxhp;
    private int hp;
    private int def;
    private float speed = 5;

    private Weapon weapon;
    private bool reloading;

    public Weapon UsingWeapon { get { return weapon; } }

    public override void Init()
    {
        base.Init();
        hp = maxhp = 5;
        def = 0;
        reloading = false;
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

    #region IMovingObject

    public enum PatrolType { NARROWLY = 0, WIDELY, LEAD, BACK, HOLD, }
    private PatrolType patrolType;
    private bool move;
    private Vector2 targetPos;
    public float Speed { get { return speed; } }

    private List<Vector2> holdPatrolPosList;
    private int patrolIndex;
    private bool patrolForward;

    public void SetPatrolType(PatrolType type)
    {
        patrolType = type;
        if (type == PatrolType.HOLD)
            SetHoldPatrol(new List<Vector2>() { transform.position });
    }

    public void SetHoldPatrol(List<Vector2> list)
    {
        // ���̶� ��ġ�� ������ �ִٸ� �н�
        foreach (var pos in list)
        {
            if (MapGenerator.PosOnWall(pos)) return;
        }

        patrolType = PatrolType.HOLD;

        move = false;
        transform.position = list[0];
        holdPatrolPosList = list;
        patrolIndex = 0;
        patrolForward = true;
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

            // ȸ������ 0�� �� �������� �ٶ󺸰� ����.
            // �̸� �������� y���� ��ȯ���� ��ġ�� ��������.
            // �� �� Player�� �������� ȸ������ ��ġ�� ����.
            // ��, �׻� Player �տ� �����ؾ��ϹǷ� ĳ���Ͱ� ���� ��ȯ��Ű�� ������ �������־�� ��.
            case PatrolType.LEAD:
                x = 2f;
                y = Random.Range(-2.5f, 2.5f);
                targetPos += (Vector2)(Quaternion.Euler(Player.Instance.transform.rotation.eulerAngles) * new Vector2(x, y));
                break;
            case PatrolType.BACK:
                x = -2f;
                y = Random.Range(-2.5f, 2.5f);
                targetPos += (Vector2)(Quaternion.Euler(Player.Instance.transform.rotation.eulerAngles) * new Vector2(x, y));
                break;

            case PatrolType.HOLD:
                if (patrolForward) patrolIndex++;
                else patrolIndex--;

                if (patrolIndex >= holdPatrolPosList.Count)
                {
                    patrolIndex--;
                    patrolForward = false;
                }
                else if (patrolIndex < 0)
                {
                    patrolIndex++;
                    patrolForward = true;
                }
                targetPos = holdPatrolPosList[patrolIndex];
                break;
        }
        move = true;
        return true;
    }

    public void Move()
    {
        float moveAmount = Time.deltaTime * speed;
        Vector3 dir = (targetPos - (Vector2)transform.position).normalized;

        // �Ŀ� A*Path�� Ȱ���ϸ� ���� ��?
        LookAt(targetPos);
        transform.position += dir * moveAmount;

        // �̵��� ���� �����̸� �������� ����
        // Ȥ�� ���� �̵� ��ġ�� ���̶�� �������� ����
        if (Vector2.Distance(targetPos, transform.position) < moveAmount
            || OverlapWall(transform.position + dir * moveAmount))
        {
            // ��Ʈ�� �ʱ�ȭ
            move = false;
        }
    }

    private bool OverlapWall(Vector2 pos)
    {
        return Physics2D.OverlapCircle(pos, .5f, 1 << LayerMask.NameToLayer("Wall") | 1 << LayerMask.NameToLayer("Turret")) != null;
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
    public bool WaitAttack { get { return weapon != null ? weapon.Wait : false; } }

    public Collider2D TargetCollider { get { return targetCollider; } }
    private Collider2D targetCollider;

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

        if (!WaitAttack)
        {
            if (weapon.curammo <= 0)
            {
                Reload();
                return;
            }

            LookAt(target.transform.position);
            weapon.Fire(transform.position, target.position, transform.rotation.eulerAngles.z);

            StartCoroutine(weapon.AttackDelay());
        }
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
        reloadCoroutine = weapon.Reloading();
        StartCoroutine(reloadCoroutine);
    }
}

