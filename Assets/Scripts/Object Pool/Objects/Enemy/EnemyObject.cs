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

    private void Start()
    {
        // �Ŀ� player�� �����ϴ� ��Ʈ�ѷ��� ���ؼ� �޾ƿ��� ���� ��.
        aIDestinationSetter.target = FindObjectOfType<Player>().transform;
    }

    public void Init(Enemy data)
    {
        hp = data.hp;
        aiPath.maxSpeed = speed = data.speed;
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;
    }
    /*
    private void FixedUpdate()
    {
        if (isMove)
        {
            rigidbody.MovePosition(rigidbody.position + (Vector2)direction * moveAmount);
        }
    }
    */
    #region IDamagedObject
    public int Hp { get { return hp; } }
    public void Damaged(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            PoolController.Push(gameObject.name, this);
            StopAllCoroutines();
        }
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
            spriteRenderer.color = Color.red;
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
            if (time > ADelay / 3) spriteRenderer.color = Color.yellow;
            time += Time.deltaTime;
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

        moveAmount = Time.fixedDeltaTime * speed;
        Vector2 path;
        if (MapGenerator.ObjectOnBoundary(rigidbody.position))
        {
            // ó�� �����Ǿ��� �� (���� ��ġ�� �� �ۿ� ������)
            // �̵��ؾ��� ��ġ�� ���� ����� ��ġ�� ������ ��.
            Vector2Int curPos = MapGenerator.RoundToInt(rigidbody.position);
            path = MapGenerator.GetNearestMapBoundary(curPos) - rigidbody.position;
        }
        else path = MapGenerator.Instance.FindPath(rigidbody.position);
        /*
        if (path != Astar.NoneVector) // ó������ �ڽ��� ��ġ�� �⺻������ ��.
        {
            direction = path;
            return true;
        }*/
        return false;

        /*
        if (path != Astar.NoneVector) 
        {
            // �켱 ���� ������������ ���� �Ÿ��� üũ��.
            // path[1] - curPos = ����.
            // path[1] - pos = ���� �Ÿ�
            // moveAmount = �����Ӵ� �̵��Ÿ�
            // ���� �Ÿ� < �̵��Ÿ� �� ��� ���� �̵� ��θ� üũ�ؾ���.

            int nextDestination = 1;
            float remainDistance;
            // path[0]�� �⺻������ �ڽ��� ��ġ��� ���õ� �ڸ���.
            // �ٸ�, ��ġ üũ ������� ���� �ش� ��ġ�� �������� ������ �� ����.
            // ���� path[1]������ ������ Ȯ���� ��
            // �ش� �̵� ������ path[0]������ ����� �ݴ��� path[1]�� �������ִ� ������� ����.

            Vector2 checkDirection = (path[0] - rigidbody.position);
            if (direction.x * checkDirection.x <= 0 && direction.y * checkDirection.y <= 0)
            {
                // �ݴ�����̶�� path[1]�� ������ ������ ��.
                remainDistance = Vector2.Distance(path[1], rigidbody.position);
            }
            else
            {
                // �ƴ϶�� path[0]�� ������ ������ ��.
                nextDestination = 0;
                direction = checkDirection;
                remainDistance = Vector2.Distance(path[0], rigidbody.position);
            }

            // �̵����� ���� �Ÿ����� ���� ��
            // ���� ���� ���õ� �������� ���õ� �̵�����ŭ �̵��ϸ� ��.
            if (remainDistance < moveAmount)
            {
                if (path.Count > nextDestination + 1) // ���� �������� ���� �������� �ƴ�
                {
                    // ���� �������� ���� ������ üũ
                    // �� �� ���������� ������ ���� Ȯ��
                    // ���� ���������� ���� �̵�����ŭ ���� ������ �������� �̵��� ��ġ
                    // �ش� ��ġ�� ���������� ������ �� �ֵ��� ����� �̵����� ����.
                    // �ش� ��ġ���� ���� ��ġ�� ���� ������ ���� ����.
                    // magnitude�� ���� �̵��� ����.
                    Vector2 nextDirection = path[nextDestination + 1] - path[nextDestination];
                    Vector2 finalDestination = path[nextDestination] + nextDirection.normalized * remainDistance;

                    direction = finalDestination - rigidbody.position;
                    moveAmount = direction.magnitude;
                }
                else // ���� �������� ���� ��������.
                {
                    // ���� �������� ������ �� �ֵ��� ����.
                    moveAmount = remainDistance;
                }
            }
            direction = direction.normalized;
        }
        // �̵��� ���� ���ٸ� Failure
        */
    }

    public void Move()
    {
        direction = direction.normalized;
        spriteRenderer.color = Color.green;
        rigidbody.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aiPath.canMove = true;
    }

    #endregion
    private void OnDrawGizmos()
    {
        if (MapGenerator.Instance == null) return;
        Vector2 path = MapGenerator.Instance.FindPath(transform.position);
        /*
        if (path != Astar.NoneVector)
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(path.x, path.y));
        }*/
    }
}
