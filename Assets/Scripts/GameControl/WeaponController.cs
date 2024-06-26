using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public static WeaponController Instance { get { return instance; } }
    private static WeaponController instance;

    private List<Weapon> weapons;
    private int curIndex;
    public Weapon CurWeapon { get { return weapons[curIndex]; } }

    private int WeaponAmount
    {
        get
        {
            int amount = 0;
            foreach (var weapon in weapons)
            {
                if (weapon.usable) amount++;
            }
            return amount;
        }
    }

    public List<int> UsingWeaponIndexes
    {
        get
        {
            List<int> list = new List<int>();

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].usable) list.Add(i);
            }
            return list;
        }
    }

    public string[] PrevCurNextWeaponKeys
    {
        get
        {
            string[] str = new string[3] { "", "", "" };

            // 0부터 2까지 순서대로 prev, cur, next
            str[1] = weapons[curIndex].key;
            // 무기가 하나라면 prev와 next는 비움.
            if (WeaponAmount > 1)
            {
                // prev부터 채워줌.
                // 무기가 둘이라면 prev와 next가 같으나 next를 비우기 위함.
                int index = curIndex;
                do
                {
                    index--;
                    if (index < 0) index = weapons.Count - 1;
                } while (!weapons[index].usable);
                str[0] = weapons[index].key;

                if (WeaponAmount > 2)
                {
                    index = curIndex;
                    do
                    {
                        index++;
                        if (index >= weapons.Count) index = 0;
                    } while (!weapons[index].usable);
                    str[2] = weapons[index].key;
                }
            }
            return str;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void StartGame()
    {
        weapons = new List<Weapon>();
        foreach (var weapon in WeaponManager.GetWeapons())
            weapons.Add(new Weapon(weapon));

        foreach (var weapon in weapons)
        {
            if (weapon.infAmount) weapon.usable = true;
        }
        curIndex = 0;
        Player.Instance.SwitchWeapon((float)curIndex / (weapons.Count - 1));
        UIController.Instance.SwitchWeapon();
    }

    #region ManageWeapon
    public void AddWeapon(string key)
    {
        Weapon weapon = FindWeapon(key);
        if (weapon == null || weapon.consumable || HasWeapon(key)) return;

        weapon.usable = true;
        UIController.Instance.UpdateWeaponImage();
    }

    public bool HasWeapon(string key)
    {
        Weapon weapon = FindWeapon(key);
        return (weapon != null) ? weapon.usable : false;
    }

    private Weapon FindWeapon(string key)
    {
        return weapons.Find(weapon => weapon.key == key);
    }
    #endregion


    private void Update()
    {
        if (GameController.Instance.GameProgress == false
            || GameController.Instance.Pause
            || BuildingController.Instance.BuildMode) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.R))
            Reload();

        if (Input.GetKeyDown(KeyCode.G))
            Grenade(Player.Instance.transform.position, mouseWorldPos);

        if (UIController.PointOverUI()) return;
        int move = (scroll > 0) ? 1 : -1;
        if (scroll == 0) move = 0;
        Switch(move);

        if (Input.GetMouseButton(0))
            Fire(Player.Instance.FirePoint, mouseWorldPos);
    }

    public void Switch(int move)
    {
        int index = curIndex;
        do
        {
            if (move > 0) index--;
            if (move < 0) index++;

            if (index < 0) index = weapons.Count - 1;
            if (index >= weapons.Count) index = 0;
        } while (!weapons[index].usable);

        if (move != 0)
        {
            if (adelayCoroutine != null)
            {
                StopCoroutine(adelayCoroutine);
                adelayCoroutine = null;
            }
            CancelReload();
            CurWeapon.Put();
            curIndex = index;
            Player.Instance.SwitchWeapon((float)curIndex / (weapons.Count - 1));
            UIController.Instance.SwitchWeapon();
        }
    }

    #region Attack

    IEnumerator adelayCoroutine;
    IEnumerator reloadCoroutine;
    public void Reload()
    {
        if (CurWeapon.Wait || !Player.Instance.HasMagazine(CurWeapon.key)) return;
        CancelReload();
        reloadCoroutine = CurWeapon.Reloading(true);
        StartCoroutine(reloadCoroutine);
    }

    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
    }

    public void Fire(Vector3 pos, Vector3 dest)
    {
        if (CurWeapon.Wait) return;
        if (CurWeapon.curammo == 0)
        {
            Reload();
            return;
        }
        else CancelReload();

        CurWeapon.Fire(pos, dest, Player.Instance.transform.rotation.eulerAngles.z);
        SoundController.Instance.PlaySFX(Player.Instance.transform, CurWeapon.key + ".SHOT");
        Player.Instance.Fire();
        UIController.Instance.UpdateAmmo(CurWeapon.curammo);
        adelayCoroutine = CurWeapon.AttackDelay();
        StartCoroutine(adelayCoroutine);
    }

    public void Grenade(Vector2 start, Vector2 dest)
    {
        if (Player.Instance.ItemAmount("WEAPON.GRENADE") <= 0) return;

        Weapon granade = WeaponManager.GetWeapon("WEAPON.GRENADE");

        Vector2 dir = (dest - start).normalized;
        ((Bullet)PoolController.Pop("WEAPON.GRENADE")).SetBullet(start, dest, dir, Player.Instance.transform.rotation.eulerAngles.z, granade, granade.bulletSpeed);
        Player.Instance.AdjustItemAmount("WEAPON.GRENADE", -1);
    }
    #endregion
}
