using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Poolable/Enemy (Poolable)")]
public class EnemyObject : BTPoolable, IDamagedObject, IAttackObject, IMovingObject
{
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Pathfinding.AIPath aiPath;
    [SerializeField] private Pathfinding.AIDestinationSetter aIDestinationSetter;

    private int hp;
    private Collider2D targetCollider;
    private int dmg;
    private float range;
    private float aDelay;
    private bool isAttacking;
    private int speed;

    private int exp;
    private int money;

    private void Start()
    {
        // �Ŀ� player�� �����ϴ� ��Ʈ�ѷ��� ���ؼ� �޾ƿ��� ���� ��.
        aIDestinationSetter.target = Player.Instance.transform;
    }

    public void Init(Enemy data)
    {
        hp = data.hp;
        aiPath.maxSpeed = speed = data.speed;
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;

        exp = data.exp;
        money = data.money;
    }

    public override void Update()
    {
        base.Update();
        if (GameController.Instance.Pause) aiPath.canMove = false;
    }

    #region IDamagedObject
    public int Hp { get { return hp; } }
    public void Damaged(int dmg)
    {
        hp -= dmg;
        StartCoroutine(ChangeColor());
        if (hp <= 0)
        {
            Player.Instance.Reward(exp, money);
            PoolController.Push(gameObject.name, this);
            spriteRenderer.color = Color.green;
            StopAllCoroutines();
        }
    }

    IEnumerator ChangeColor()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.green;
    }
    #endregion

    #region IAttackObject
    public Collider2D TargetCollider { get { return targetCollider; } }
    public int Dmg { get { return dmg; } }
    public float Range { get { return range; } }
    public float ADelay { get { return aDelay; } }
    public bool WaitAttack { get; set; }

    public bool DetectTarget()
    {
        aiPath.canMove = false;
        // ���� ������ �������� �� Failure�� ����� ��. (���� ��� ���̳� �÷��̾�)
        // �� �ܿ� ������ �پ����� ���� �̵��ϴ� ��ĵ� ����� �ʿ䰡 ����.
        // ���� ��� ������ �پ��ְ� ���� ���� ���� ���̶�� �̵��� �� �ʿ䰡 ����.
        // �ٸ� �� ���� �������ݿ� ����. ���Ÿ����� �����̶�� �տ� �ٰŸ� ������ ���� ���� �����Ƿ�
        // ������ ������ ���� ������ �ؾ��� ���� ����.
        // �̷� ��� ��Ȳ�� ���� ó���� �ʿ���.

        // ���� ���� ������ ���� Player�� �ֺ��� �ִ��� üũ
        // �ִٸ� �ش� ������ Ÿ������ ����
        // ���ٸ� Turret�̶� �ִ��� üũ
        // �ִٸ� ���� ù �ͷ��� Ÿ������ ����.

        // �������̶�� �ش� Ÿ���� ������ ���� ������ üũ
        // �װ� �ƴ϶�� 0.75������ �ȿ� �ֳ� üũ

        float range = this.range;
        if (!isAttacking) range *= .75f;

        targetCollider = Physics2D.OverlapCircle(transform.position, range, 1 << LayerMask.NameToLayer("Player"));
        if (targetCollider == null)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, 1 << LayerMask.NameToLayer("Turret"));
            if (cols != null && cols.Length > 0) targetCollider = cols[0];
        }

        if (targetCollider == null) isAttacking = false;

        return targetCollider != null;
    }

    public void Attack()
    {
        isAttacking = true;
        if (!WaitAttack)
        {
            IDamagedObject damagedObject = targetCollider.GetComponent<IDamagedObject>();
            damagedObject.Damaged(dmg);
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

    #region IMovingObject
    public int Speed { get { return speed; } }
    private Vector3 direction;
    private float moveAmount;
    private bool isMove;
    public bool DetectPath()
    {
        // �켱 path�� ������ ã�´ٰ� ����.
        return true;
    }

    public void Move()
    {
        direction = direction.normalized;
        rigidbody.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aiPath.canMove = true;
    }

    #endregion
}
