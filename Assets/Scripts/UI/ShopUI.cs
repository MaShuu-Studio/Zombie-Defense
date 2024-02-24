using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private RectTransform weaponScrollRectTransform;
    [SerializeField] private RectTransform turretScrollRectTransform;
    [SerializeField] private RectTransform otherItemScrollRectTransform;
    [SerializeField] private RectTransform magazineScrollRectTransform;
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

        // �켱�� Magazine�� Shop�� �����ϰ� �߰�
        // �� �Ŀ� UI�� ������ ���̱� ������ �켱 �۵��� Ȯ���� �� �ֵ��ϸ� ��.
        int count = 0;
        foreach (var weapon in WeaponManager.Weapons)
        {
            if (weapon.consumable) continue;
            var item = Instantiate(itemPrefab, weaponScrollRectTransform);
            item.Init(weapon);
            item.gameObject.SetActive(true);
            items.Add(item);
            count++;

            if (weapon.infmagazine) continue;
            var magazine = Instantiate(itemPrefab, magazineScrollRectTransform);
            magazine.Init(weapon, true);
            magazine.gameObject.SetActive(true);
            items.Add(magazine);
        }
        weaponScrollRectTransform.sizeDelta = new Vector2(150 * count, weaponScrollRectTransform.sizeDelta.y);
        magazineScrollRectTransform.sizeDelta = new Vector2(150 * count, magazineScrollRectTransform.sizeDelta.y);

        count = 0;
        foreach (var turret in TurretManager.Turrets)
        {
            var item = Instantiate(itemPrefab, turretScrollRectTransform);
            item.Init(turret);
            item.gameObject.SetActive(true);
            items.Add(item);
            count++;
        }
        turretScrollRectTransform.sizeDelta = new Vector2(150 * count, turretScrollRectTransform.sizeDelta.y);

        count = 0;
        foreach (var other in ItemManager.Items)
        {
            var item = Instantiate(itemPrefab, otherItemScrollRectTransform);
            item.Init(other);
            item.gameObject.SetActive(true);
            items.Add(item);
            count++;
        }
        // otherItemScrollRectTransform.sizeDelta = new Vector2(150 * count, otherItemScrollRectTransform.sizeDelta.y);
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
                if (shopItem.IsMagazine) Player.Instance.AddMagazine(weapon.key);
                else Player.Instance.AdjustItemAmount(weapon.key, 1);
            }
            // �Ҹ�ǰ�� �ƴѵ� ���Ⱑ ���� źâ�� �ƴ϶�� ���Ӱ� ȹ��
            else if (!shopItem.IsMagazine)
            {
                WeaponController.Instance.AddWeapon(weapon.key);
                UIController.Instance.AddItem(weapon.key);
            }
        }
        else
        {
            Player.Instance.AdjustItemAmount(shopItem.Item.key, 1);
        }
    }
}
