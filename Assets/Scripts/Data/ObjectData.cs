using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectData
{
    public string key;
    public enum Attribute { NONE = 0, BULLET, EXPLOSION, FIRE, ELECTRIC }
}

public abstract class BuyableData : ObjectData
{
    public int price;
    public int magprice;
}

public class Weapon : BuyableData
{
    public bool infAmount;
    public bool usable;

    public Attribute attribute;

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
    public bool singleBulletReload;

    public int pierce = 0;
    public bool point = false;
    public bool autotarget = false;
    public bool consumable = false;

    public Weapon() { }

    public Weapon(Weapon w)
    {
        infAmount = w.infAmount;

        attribute = w.attribute;

        key = w.key;
        price = w.price;

        dmg = w.dmg;
        adelay = w.adelay;
        dmgdelay = w.dmgdelay;
        range = w.range;
        bulletspreadangle = w.bulletspreadangle;

        ammo = curammo = w.ammo;
        reload = w.reload;
        singleBulletReload = w.singleBulletReload;

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
            pos + (Vector2)(Quaternion.AngleAxis(angle - 90, Vector3.forward) * new Vector3(range / 2 + 0.3f, 0)),
            Vector2.one * range, angle - 90, layerMask);
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
                if (autoTargets.Length > i) dest = autoTargets[i].transform.position;
                // ���� ���� Ÿ���� ������ ���ٸ� ��ŵ
                else break;
            }
            var bullet = PoolController.Pop(key);
            if (bullet == null) bullet = PoolController.Pop("Bullet");
            ((Bullet)bullet).SetBullet(pos, dest, newDir, angle, this, bulletSpeed);
        }
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
        if (player && !singleBulletReload) UIController.Instance.Reloading(true);

        float pReload = (100 + Player.Instance.ReloadTime) / 100f;

        while (curammo < ammo)
        {
            float time = reload / pReload;
            while (time > 0)
            {
                if (!GameController.Instance.Pause) time -= Time.deltaTime;
                yield return null;
            }
            
            // ������ �ϳ� �ϰ� ���� ���� ����
            Wait = false;
            SoundController.Instance.PlaySFX(Player.Instance.transform, key + ".RELOAD");

            int refillAmmo = Player.Instance.UseMagazine(key, curammo);
            if (refillAmmo > 0)
            {
                curammo += refillAmmo;
                if (player) UIController.Instance.UpdateAmmo(curammo);
            }
            else break;
        }
        Wait = false;
        if (player && !singleBulletReload) UIController.Instance.Reloading(false);
    }
}
public class Building : BuyableData
{
    public int hp;
    public int range;
    public int dmg;
    public float adelay;
    public int speed;

    public BuffInfo buff;
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
    public float speed;
    public float range;
    public float projSpeed = 0;
    public float adelay;

    public Dictionary<Attribute, float> resistances = new Dictionary<Attribute, float>();

    public BuffInfo buff = null;
    public BuffInfo debuff = null;
    public Item dropItem = null;

    public bool projSummon;
    public int summonProb = 100;
    public string summonUnit;
    public float summonCD;
    public int summonAmount;

    public int separate;
    public int thresholdHp;
    public int restHealAmount;
    public bool isSiege = false;

    public int money;
}

public class Companion : ObjectData
{
    public int hp;
    public int def;
    public float speed = 5;
}

