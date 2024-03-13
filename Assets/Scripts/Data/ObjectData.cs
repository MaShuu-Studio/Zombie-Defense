using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectData
{
    public string key;
}

public abstract class BuyableData : ObjectData
{
    public int price;
}

public class Weapon : BuyableData
{
    public bool infAmount;
    public bool usable;

    public int dmg;
    public float adelay;
    public float dmgdelay = 0;
    public float range;
    public int bulletspreadangle;

    public float bulletSize = 1;
    public float bulletSpeed = 20;
    public int bullets = 1;
    public float radius = 0;

    public int ammo;
    public int curammo;
    public float reload;

    public bool pierce = false;
    public bool point = false;
    public bool autotarget = false;
    public bool consumable = false;

    public Weapon() { }

    public Weapon(Weapon w)
    {
        infAmount = w.infAmount;

        key = w.key;
        price = w.price;

        dmg = w.dmg;
        adelay = w.adelay;
        dmgdelay = w.dmgdelay;
        range = w.range;
        bulletspreadangle = w.bulletspreadangle;

        ammo = curammo = w.ammo;
        reload = w.reload;

        bulletSize = w.bulletSize;
        bulletSpeed = w.bulletSpeed;
        bullets = w.bullets;
        radius = w.radius;

        pierce = w.pierce;
        point = w.point;
        autotarget = w.autotarget;
        consumable = w.consumable;
    }

    public Collider2D[] DetectEnemyTargets(Vector2 pos, float angle)
    {
        if (autotarget == false) return null;
        // Ư�� ��ġ�� ��������
        // range��ŭ ���� ����
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        return Physics2D.OverlapBoxAll(
            pos + new Vector2(range / 2 + .5f, 0),
            Vector2.one * range, angle, layerMask);
    }

    public bool Wait { get; private set; }

    public void Put()
    {
        Wait = false;
    }

    public void Fire(Vector2 pos, Vector2 dest, float angle)
    {
        Collider2D[] autoTargets = DetectEnemyTargets(pos, angle);
        Vector2 dir = dest - pos;
        int spread = bulletspreadangle;
        for (int i = 0; i < bullets; i++)
        {
            int spreadAngle = Random.Range(-spread / 2, spread / 2 + 1);
            Vector3 newDir = Quaternion.Euler(0, 0, spreadAngle) * dir;
            // ����Ÿ���̸� ���� ����ŭ �ڵ�Ÿ�����Ͽ� ����.
            if (autotarget)
            {
                if (autoTargets.Length > i) pos = autoTargets[i].transform.position;
                // ���� ���� Ÿ���� ������ ���ٸ� ��ŵ
                else break;
            }
            ((Bullet)PoolController.Pop("Bullet")).SetBullet(pos, dest, newDir, this, bulletSpeed);
        }
        SoundController.Instance.PlaySFX(pos, key);
        curammo--;
    }

    public IEnumerator AttackDelay()
    {
        Wait = true;

        float time = adelay;
        while (time > 0)
        {
            if (!GameController.Instance.Pause) time -= Time.deltaTime;
            yield return null;
        }
        Wait = false;
    }

    public IEnumerator Reloading(bool player = false)
    {
        Wait = true;
        if (player) UIController.Instance.Reloading(true);

        float pReload = (100 + Player.Instance.ReloadTime) / 100f;
        float time = reload / pReload;
        if (player) SoundController.Instance.PlaySFX(Player.Instance.transform.position, "RELOAD");
        while (time > 0)
        {
            if (!GameController.Instance.Pause) time -= Time.deltaTime;
            yield return null;
        }
        curammo = ammo;
        if (player) UIController.Instance.Reloading(false);
        Wait = false;
    }
}
public class Turret : BuyableData
{
    public int hp;
    public int range;
    public int dmg;
    public float adelay;
    public int speed;
}

public class OtherItem : BuyableData
{

}

public class Enemy : ObjectData
{
    public bool inv;
    public bool fly;

    public int hp;
    public int dmg;
    public int speed;
    public float range;
    public float adelay;

    public BuffInfo buff;
    public string summonUnit;
    public float summonCD;
    public int summonAmount;

    public int separate;

    public int exp;
    public int money;
}

