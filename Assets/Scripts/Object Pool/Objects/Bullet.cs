using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Poolable/Bullet (Poolable)")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bullet : Poolable
{
    [SerializeField] TrailRenderer trail;
    [SerializeField] ParticleSystem particle;

    private Rigidbody2D rigidbody;
    private CircleCollider2D hitbox;

    private Weapon weapon;

    private Vector2 destination;
    private Vector2 direction;

    private float distance;
    private float remainTime;
    private float speed;

    private float originRadius;
    private float radius;
    private float bulletSize;

    private bool stop;
    private bool point;
    private float dmgDelay;

    ParticleSystem.MainModule pmain;

    public override void Init()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<CircleCollider2D>();
    }

    public void SetBullet(Vector2 start, Vector2 dest, Vector2 dir, float angle,
        Weapon w, float spd)
    {
        stop = false;

        transform.position = rigidbody.position = start;
        destination = dest;
        direction = dir.normalized;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        bulletSize = w.bulletSize;
        transform.localScale = Vector3.one * bulletSize;
        if (particle != null)
        {
            pmain = particle.main;
            float lifetime = Vector2.Distance(start, dest) / (Time.fixedDeltaTime * speed);
            pmain.startSize = new ParticleSystem.MinMaxCurve(pmain.startSize.constant * bulletSize);
            particle.Play();
        }

        weapon = w;
        dmgDelay = w.dmgdelay;
        distance = weapon.range;
        speed = spd;

        originRadius = hitbox.radius;
        radius = w.radius / 2;

        point = w.point;
        if (remainTime == 0) remainTime = Time.fixedDeltaTime * 2;

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
            Push();
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

        hitbox.radius = radius;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius, 1 << LayerMask.NameToLayer("Enemy"));
        foreach (var col in cols) Damage(col);

        yield return null;
        Push();
    }

    private void Damage(Collider2D collision)
    {
        var enemy = collision.transform.parent.GetComponent<EnemyObject>();

        ActionController.AddAction(gameObject, () =>
        {
            enemy.Damaged(weapon.dmg, weapon.attribute);
            if (weapon.pierce == false && !point)
            {
                Push();
                StopAllCoroutines();
            }
        });
    }

    private void Push()
    {
        PoolController.Push(gameObject.name, this);
        trail.Clear();

        if (particle != null)
        {
            pmain.startSize = new ParticleSystem.MinMaxCurve(pmain.startSize.constant / bulletSize);
        }

        string particleName = gameObject.name.Replace("WEAPON", "PARTICLE");
        if (particleName != gameObject.name)
        {
            var p = PoolController.Pop(particleName);
            if (p != null)
            {
                p.transform.position = transform.position;
                ((ParticleObject)p).Play(0, radius / originRadius);
            }
        }

        hitbox.radius = originRadius;
    }
}
