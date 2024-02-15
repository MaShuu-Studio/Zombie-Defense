using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private RectTransform weaponScrollRectTransform;
    [SerializeField] private RectTransform turretScrollRectTransform;
    [SerializeField] private ShopItem itemPrefab;
    private List<ShopItem> items = new List<ShopItem>();

    private void Awake()
    {
        itemPrefab.gameObject.SetActive(false);
    }

    public void Init()
    {
        items.ForEach(item => Destroy(item.gameObject));
        items.Clear();

        foreach (var weapon in WeaponManager.Weapons)
        {
            var item = Instantiate(itemPrefab, weaponScrollRectTransform);
            item.Init(weapon);
            item.gameObject.SetActive(true);
            items.Add(item);
        }
        weaponScrollRectTransform.sizeDelta = new Vector2(weaponScrollRectTransform.sizeDelta.x, 150 * items.Count);

        foreach (var turret in TurretManager.Turrets)
        {
            var item = Instantiate(itemPrefab, turretScrollRectTransform);
            item.Init(turret);
            item.gameObject.SetActive(true);
            items.Add(item);
        }
        turretScrollRectTransform.sizeDelta = new Vector2(turretScrollRectTransform.sizeDelta.x, 150 * items.Count);
    }

    public void Open(bool b)
    {
        gameObject.SetActive(b);
    }

    public void BuyItem(ShopItem shopItem)
    {
        Weapon weapon = shopItem.Item as Weapon;
        if (weapon != null)
        {
            // ���Ⱑ �ְų� �Ҹ�ǰ�̸� ����ǰ�� �߰�
            if (weapon.consumable || WeaponController.Instance.HasWeapon(weapon.key))
            {
                Player.Instance.AdjustItemAmount(weapon.key, 1);
            }
            // �Ҹ�ǰ�� �ƴѵ� ���Ⱑ ���ٸ� ���Ӱ� ȹ��
            else
            {
                WeaponController.Instance.AddWeapon(weapon);
                UIController.Instance.AddItem(weapon.key);
            }
        }

        Turret turret = shopItem.Item as Turret;
        if (turret != null)
        {
            Player.Instance.AdjustItemAmount(turret.key, 1);
        }
    }
}
