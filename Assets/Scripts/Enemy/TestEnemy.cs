using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [Range(1, 20)] [SerializeField] private float speed;
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

    private BehaviourTree bt;
    [SerializeField] private Rigidbody2D rigidbody;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        bt = new BehaviourTree(SetBT());
        speed = Random.Range(1, 20);
    }

    private void Update()
    {
        bt.Operate();
    }

    private IBTNode SetBT()
    {
        return new SelectorNode(
            new List<IBTNode>()
            {
                new SequenceNode(new List<IBTNode>()
                {
                    new ActionNode(CheckMove),
                    new ActionNode(Move),
                }
                ),
                new SequenceNode(new List<IBTNode>()
                {
                    new ActionNode(Detect),
                    new ActionNode(Attack),
                }
                ),
            }
            );
    }

    private Vector2 direction;
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
        // Ȥ�� ���� ������ �����ִ��� Failure�� ����� ��. (���� ��� ��)
        return IBTNode.NodeState.Failure;
    }

    private IBTNode.NodeState Move()
    {
        transform.position += (Vector3)(direction * moveAmount);
        return IBTNode.NodeState.Success;
    }

    private IBTNode.NodeState Detect()
    {
        return IBTNode.NodeState.Success;
    }

    private IBTNode.NodeState Attack()
    {
        return IBTNode.NodeState.Success;
    }
}
