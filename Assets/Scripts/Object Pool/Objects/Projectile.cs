using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Poolable
{
    private Rigidbody2D rigidbody;

    private Vector2 destination;
    private Vector2 direction;

    private float remainTime;
    private float speed;

    private int dmg;
    private float radius;

    private bool stop;

    public override void Init()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetProj(Vector2 start, Vector2 dest, int dmg, float radius, float speed)
    {
        stop = false;

        transform.position = rigidbody.position = start;
        destination = dest;
        direction = (dest - start).normalized;

        this.dmg = dmg;
        this.radius = radius;
        this.speed = speed;

        if (remainTime == 0) remainTime = Time.fixedDeltaTime * 2;
        transform.localScale = Vector3.one * radius * 2;
    }

    private void FixedUpdate()
    {
        if (GameController.Instance.Pause) return;

        if (!stop)
        {
            rigidbody.MovePosition(rigidbody.position + direction * Time.fixedDeltaTime * speed);

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
    }

    IEnumerator RangeDamage()
    {
        stop = true;

        int layerMask = 1 << LayerMask.NameToLayer("Turret") | 1 << LayerMask.NameToLayer("Trap") | 1 << LayerMask.NameToLayer("Player");
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius, layerMask);
        foreach (var col in cols) Damage(col);

        yield return null;
        PoolController.Push("Projectile", this);
    }

    private void Damage(Collider2D collision)
    {
        ActionController.AddAction(gameObject, () =>
        {
            int dmg = this.dmg;
            // Player�� �ƴ϶� �ͷ��̶�� 1.5���
            if (collision.transform.parent.gameObject != Player.Instance.gameObject) dmg = (int)(this.dmg * 1.5f);

            var target = collision.transform.parent.GetComponent<IDamagedObject>();
            target.Damaged(dmg);
        });
    }
}
