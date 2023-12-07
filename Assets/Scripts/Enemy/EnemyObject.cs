using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Poolable/Enemy (Poolable)")]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyObject : Poolable, IDamagedObject
{
    [SerializeField] private GameObject target;

    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private BehaviourTree bt;

    private int hp;
    private int speed;
    private int dmg;
    private float radius;
    private float adelay;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
        bt = new BehaviourTree(SetBT());
    }

    public void Init(Enemy data)
    {
        hp = data.hp;
        speed = data.speed;
        dmg = data.dmg;
        radius = data.range;
        adelay = data.adelay;
    }

    private void Update()
    {
        bt.Operate();
    }

    public void Damaged(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
            PoolController.Push(gameObject.name, this);
    }

    #region BT
    private IBTNode SetBT()
    {
        return new SelectorNode(
            new List<IBTNode>()
            {
                new SequenceNode(new List<IBTNode>()
                {
                    new ActionNode(Detect),
                    new ActionNode(Attack),
                }
                ),
                new SequenceNode(new List<IBTNode>()
                {
                    new ActionNode(CheckMove),
                    new ActionNode(Move),
                }
                ),
            }
            );
    }
    #region Attack

    bool isAttacking;
    Collider2D targetCollider;
    bool waitAttack;
    private IBTNode.NodeState Detect()
    {
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

        float range = radius;
        if (!isAttacking) range *= .75f;

        targetCollider = Physics2D.OverlapCircle(transform.position, range, 1 << LayerMask.NameToLayer("Player"));
        if (targetCollider != null) return IBTNode.NodeState.Success;
        else
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, 1 << LayerMask.NameToLayer("Turret"));
            if (cols != null && cols.Length > 0)
            {
                targetCollider = cols[0];
                return IBTNode.NodeState.Success;
            }
        }

        isAttacking = false;
        return IBTNode.NodeState.Failure;
    }

    private IBTNode.NodeState Attack()
    {
        isAttacking = true;
        if (!waitAttack)
        {
            IDamagedObject damagedObject = targetCollider.GetComponent<IDamagedObject>();
            damagedObject.Damaged(dmg);
            StartCoroutine(AttackTimer());
        }
        return IBTNode.NodeState.Success;
    }

    IEnumerator AttackTimer()
    {
        spriteRenderer.color = Color.red;
        waitAttack = true;
        float time = 0;
        while (time < adelay)
        {
            time += Time.deltaTime;
            yield return null;
            spriteRenderer.color = Color.yellow;
        }
        waitAttack = false;
    }
    #endregion
    #region Move
    private Vector3 direction;
    private float moveAmount;
    private IBTNode.NodeState CheckMove()
    {
        List<Vector2Int> path = MapGenerator.Instance.FindPath(transform.position);
        moveAmount = Time.deltaTime * speed;

        if (path.Count > 1) // ó������ �ڽ��� ��ġ�� �⺻������ ��.
        {
            /* �켱 ���� ������������ ���� �Ÿ��� üũ��.
             * path[1] - curPos = ����.
             * path[1] - pos = ���� �Ÿ�
             * moveAmount = �����Ӵ� �̵��Ÿ�
             * ���� �Ÿ� < �̵��Ÿ� �� ��� ���� �̵� ��θ� üũ�ؾ���.
             */

            int nextDestination = 1;
            float remainDistance;
            direction = (path[nextDestination] - (Vector2)transform.position);

            /* path[0]�� �⺻������ �ڽ��� ��ġ��� ���õ� �ڸ���.
             * �ٸ�, ��ġ üũ ������� ���� �ش� ��ġ�� �������� ������ �� ����.
             * ���� path[1]������ ������ Ȯ���� ��
             * �ش� �̵� ������ path[0]������ ����� �ݴ��� path[1]�� �������ִ� ������� ����.
             */

            Vector2 checkDirection = (path[0] - (Vector2)transform.position);
            if (direction.x * checkDirection.x <= 0 && direction.y * checkDirection.y <= 0)
            {
                // �ݴ�����̶�� path[1]�� ������ ������ ��.
                direction = direction.normalized;
                remainDistance = Vector2.Distance(path[1], transform.position);
            }
            else
            {
                // �ƴ϶�� path[0]�� ������ ������ ��.
                nextDestination = 0;
                direction = checkDirection.normalized;
                remainDistance = Vector2.Distance(path[0], transform.position);
            }

            // �̵����� ���� �Ÿ����� ���� ��
            // ���� ���� ���õ� �������� ���õ� �̵�����ŭ �̵��ϸ� ��.
            if (remainDistance < moveAmount)
            {
                if (path.Count > nextDestination + 1) // ���� �������� ���� �������� �ƴ�
                {
                    /* ���� �������� ���� ������ üũ
                     * �� �� ���������� ������ ���� Ȯ��
                     * ���� ���������� ���� �̵�����ŭ ���� ������ �������� �̵��� ��ġ
                     * �ش� ��ġ�� ���������� ������ �� �ֵ��� ����� �̵����� ����.
                     * �ش� ��ġ���� ���� ��ġ�� ���� ������ ���� ����.
                     * magnitude�� ���� �̵��� ����.
                     */
                    Vector2 nextDirection = path[nextDestination + 1] - path[nextDestination];
                    Vector2 finalDestination = path[nextDestination] + nextDirection.normalized * remainDistance;

                    direction = finalDestination - (Vector2)transform.position;
                    moveAmount = direction.magnitude;
                    direction = direction.normalized;
                }
                else // ���� �������� ���� ��������.
                {
                    // ���� �������� ������ �� �ֵ��� ����.
                    moveAmount = remainDistance;
                }
            }
            return IBTNode.NodeState.Success;
        }
        // �̵��� ���� ���ٸ� Failure
        return IBTNode.NodeState.Failure;
    }

    private IBTNode.NodeState Move()
    {
        spriteRenderer.color = Color.green;
        transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward);
        rigidbody.MovePosition(transform.position + direction * moveAmount);
        return IBTNode.NodeState.Success;
    }
    #endregion
    #endregion

    private void OnDrawGizmos()
    {
        if (MapGenerator.Instance == null) return;
        List<Vector2Int> path = MapGenerator.Instance.FindPath(transform.position);

        if (path.Count > 0)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y));
            }
        }
    }
}
