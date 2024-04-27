using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Poolable/Bullet (Poolable)")]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : Poolable
{
    private Rigidbody2D rigidbody;

    private Weapon weapon;

    private Vector2 destination;
    private Vector2 direction;

    private float distance;
    private float remainTime;
    private float speed;

    private float radius;

    private bool stop;
    private bool point;
    private float dmgDelay;

    public override void Init()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetBullet(Vector2 start, Vector2 dest, Vector2 dir, Weapon w, float spd)
    {
        stop = false;

        transform.position = rigidbody.position = start;
        destination = dest;
        direction = dir.normalized;

        weapon = w;
        dmgDelay = w.dmgdelay;
        distance = weapon.range;
        speed = spd;

        radius = w.radius;

        point = w.point;
        if (remainTime == 0) remainTime = Time.fixedDeltaTime * 2;

        transform.localScale = Vector3.one * w.bulletSize;
    }

    private void FixedUpdate()
    {
        if (GameController.Instance.Pause) return;

        // ����Ÿ���� ��� Ư�� ��ġ�� ����ٰ� ������� ��.
        if (weapon.autotarget)
            remainTime -= Time.fixedDeltaTime;
        // �� ���� ���� Ư�� �������� �̵��ؾ� ��.
        else if (!stop)
        {
            rigidbody.MovePosition(rigidbody.position + direction * Time.fixedDeltaTime * speed);
            distance -= Time.fixedDeltaTime * speed;
        }

        // ����Ʈ ������ ��쿡�� Ư�� ��ġ�� �������� ��츸 �����ؾ���.
        if (point)
        {
            // ������ ���� Ư���� ����� Ȱ���ϱ�� ��.
            // ���� ��ġ�κ��� dest, dest + moveAmount 3���� ������ �Ÿ��� å����.
            // dest�� �����ٸ� ���� �������� ���� ���̰� dest + moveAmount�� �����ٸ� ������ ����.
            // �������� �� ��ġ�� dest�� �������ְ� ���߽�����.
            float dist1 = Vector2.Distance(transform.position, destination);
            float dist2 = Vector2.Distance(transform.position, destination + direction * Time.fixedDeltaTime);
            if (dist1 == 0 || dist1 >= dist2)
            {
                transform.position = destination;
                StartCoroutine(RangeDamage());
            }
        }
        // �� ���� ��쿡�� Ư�� ��Ȳ�� �Ǹ� �����.
        else if (MapGenerator.Instance.MapBoundary.Contains(rigidbody.position) == false
                || distance < 0 || remainTime <= 0)
        {
            PoolController.Push("Bullet", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (point)
            {
                dmgDelay = 0;
                StartCoroutine(RangeDamage());
            }
            else Damage(collision);
        }
    }

    IEnumerator RangeDamage()
    {
        stop = true;

        while (dmgDelay > 0)
        {
            if (!GameController.Instance.Pause) dmgDelay -= Time.deltaTime;
            yield return null;
        }

        // ���� ������ ���� ���� �뵵
        transform.localScale = Vector3.one * radius * 2;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius, 1 << LayerMask.NameToLayer("Enemy"));
        foreach (var col in cols) Damage(col);

        yield return null;
        PoolController.Push("Bullet", this);
    }

    private void Damage(Collider2D collision)
    {
        var enemy = collision.transform.parent.GetComponent<EnemyObject>();

        ActionController.AddAction(gameObject, () =>
        {
            enemy.Damaged(weapon.dmg, weapon.attribute);
            if (weapon.pierce == false && !point)
            {
                PoolController.Push("Bullet", this);
                StopAllCoroutines();
            }
        });
    }
}
