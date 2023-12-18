using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Poolable/Enemy (Poolable)")]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyObject : BTPoolable, IDamagedObject, IAttackObject, IMovingObject
{
    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;

    private int hp;
    private Collider2D targetCollider;
    private int dmg;
    private float range;
    private float aDelay;
    private bool isAttacking;
    private int speed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Init(Enemy data)
    {
        hp = data.hp;
        speed = data.speed;
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;
    }

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
    public bool DetectPath()
    {
        moveAmount = Time.deltaTime * speed;
        List<Vector2Int> path;
        if (MapGenerator.ObjectOnBoundary(transform.position))
        {
            // ó�� �����Ǿ��� �� (���� ��ġ�� �� �ۿ� ������)
            // �̵��ؾ��� ��ġ�� ���� ����� ��ġ�� ������ ��.
            Vector2Int curPos = MapGenerator.RoundToInt(transform.position);
            path = new List<Vector2Int>();
            path.Add(curPos);
            path.Add(MapGenerator.GetNearestMapBoundary(curPos));
        }
        else path = MapGenerator.Instance.FindPath(transform.position);

        if (path.Count > 1) // ó������ �ڽ��� ��ġ�� �⺻������ ��.
        {
            // �켱 ���� ������������ ���� �Ÿ��� üũ��.
            // path[1] - curPos = ����.
            // path[1] - pos = ���� �Ÿ�
            // moveAmount = �����Ӵ� �̵��Ÿ�
            // ���� �Ÿ� < �̵��Ÿ� �� ��� ���� �̵� ��θ� üũ�ؾ���.

            int nextDestination = 1;
            float remainDistance;
            direction = (path[nextDestination] - (Vector2)transform.position);

            // path[0]�� �⺻������ �ڽ��� ��ġ��� ���õ� �ڸ���.
            // �ٸ�, ��ġ üũ ������� ���� �ش� ��ġ�� �������� ������ �� ����.
            // ���� path[1]������ ������ Ȯ���� ��
            // �ش� �̵� ������ path[0]������ ����� �ݴ��� path[1]�� �������ִ� ������� ����.

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
                    // ���� �������� ���� ������ üũ
                    // �� �� ���������� ������ ���� Ȯ��
                    // ���� ���������� ���� �̵�����ŭ ���� ������ �������� �̵��� ��ġ
                    // �ش� ��ġ�� ���������� ������ �� �ֵ��� ����� �̵����� ����.
                    // �ش� ��ġ���� ���� ��ġ�� ���� ������ ���� ����.
                    // magnitude�� ���� �̵��� ����.
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
            return true;
        }
        // �̵��� ���� ���ٸ� Failure
        return false;
    }
    public void Move()
    {
        spriteRenderer.color = Color.green;
        transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward);
        rigidbody.MovePosition(transform.position + direction * moveAmount);
    }
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
