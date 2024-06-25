using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Projectile : Poolable
{
    protected Rigidbody2D rigidbody;

    protected Vector2 destination;
    protected Vector2 direction;

    protected float remainTime;
    protected float speed;

    protected bool stop;

    protected IEnumerator rangeDamageCoroutine;
    public override void Init()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (GameController.Instance.Pause) return;

        if (!stop)
        {
            Move();
        }
    }

    protected void Move()
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
            RangeDamage();
        }
    }

    protected void RangeDamage()
    {
        // �̹� �������̶�� �������� ����.
        if (rangeDamageCoroutine == null)
        {
            rangeDamageCoroutine = RangeDamaging();
            StartCoroutine(rangeDamageCoroutine);
        }
    }

    protected abstract IEnumerator RangeDamaging();

    protected virtual void Push()
    {
        rangeDamageCoroutine = null;
        PoolController.Push(gameObject.name, this);

        string particleName = gameObject.name.Replace("PROJECTILE", "PARTICLE");
        if (particleName != gameObject.name)
        {
            var particle = PoolController.Pop(particleName);
            if (particle != null)
            {
                particle.transform.position = transform.position;
                ((ParticleObject)particle).Play(transform.rotation.eulerAngles.z);
            }
        }
    }
}
