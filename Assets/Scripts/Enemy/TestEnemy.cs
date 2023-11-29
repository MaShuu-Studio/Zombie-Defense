using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [Range(1, 10)] [SerializeField] private float speed;
    [SerializeField] private float radius;
    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private BehaviourTree bt;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
        bt = new BehaviourTree(SetBT());
        speed = Random.Range(1, 10);
        hp = 3;
    }

    private int hp;

    private void Update()
    {
        bt.Operate();
    }

    public void Damaged(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
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
    Collider2D targetCollider;
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
        targetCollider = Physics2D.OverlapCircle(transform.position, .7f, 1 << LayerMask.NameToLayer("Player"));
        if (targetCollider != null) return IBTNode.NodeState.Success;
        else
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, .7f, 1 << LayerMask.NameToLayer("Turret"));
            if (cols != null && cols.Length > 0)
            {
                targetCollider = cols[0];
                return IBTNode.NodeState.Success;
            }

            return IBTNode.NodeState.Failure;
        }
    }

    private IBTNode.NodeState Attack()
    {
        Debug.Log($"Attack {targetCollider.name}");
        spriteRenderer.color = Color.red;
        return IBTNode.NodeState.Success;
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

            direction = (path[1] - (Vector2)transform.position).normalized;
            float remainDistance = Vector2.Distance(path[1], transform.position);
            
            // �̵����� ���� �Ÿ����� ���� ��
            // ���� ���� ���õ� �������� ���õ� �̵�����ŭ �̵��ϸ� ��.
            if (remainDistance < moveAmount) 
            {
                if (path.Count > 2) // ���� �������� ���� �������� �ƴ�
                {
                    /* ���� �������� ���� ������ üũ
                     * �� �� ���������� ������ ���� Ȯ��
                     * ���� ���������� ���� �̵�����ŭ ���� ������ �������� �̵��� ��ġ
                     * �ش� ��ġ�� ���������� ������ �� �ֵ��� ����� �̵����� ����.
                     * �ش� ��ġ���� ���� ��ġ�� ���� ������ ���� ����.
                     * magnitude�� ���� �̵��� ����.
                     */
                    Vector2 nextDirection = path[2] - path[1];
                    Vector2 finalDestination = path[1] + nextDirection.normalized * remainDistance;

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
