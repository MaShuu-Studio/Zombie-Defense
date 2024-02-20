using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private DescriptionIcon itemIcon;
    [SerializeField] private TextMeshProUGUI itemAmount;
    [SerializeField] private TextMeshProUGUI itemPrice;

    public BuyableData Item { get; private set; }
    public void Init(BuyableData data)
    {
        itemImage.sprite = SpriteManager.GetSprite(data.key);
        itemPrice.text = $"${data.price}";
        Item = data;
        itemIcon.SetIcon(Item.key);
    }

    private void Update()
    {
        if (Player.Instance != null && Item != null)
        {
            itemAmount.text = Player.Instance.ItemAmount(Item.key).ToString();
            // ������ ��쿡�� ������ �ִ��� �ƴ����� üũ���־�� ��.
            if (Item as Weapon != null && !WeaponController.Instance.HasWeapon(Item.key))
                itemAmount.text = "NONE";
        }
    }

    public void BuyItem()
    {
        UIController.Instance.BuyItem(this);
    }
}
