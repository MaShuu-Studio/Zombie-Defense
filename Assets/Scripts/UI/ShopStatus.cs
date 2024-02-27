using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using TMPro;

public class ShopStatus : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    // ����, ���� �� ��Ȳ�� ���� �ٸ� �ɷ��� ǥ��� �� �ֵ��� ����
    // �켱�� ���⿡�� ���������� ���
    [SerializeField] private LocalizeStringEvent[] statusNames;

    // �ɷ�ġ ��ġ�� �Ŀ� �����̴��� ���������� ǥ��
    [SerializeField] private Slider[] sliders;
    [SerializeField] private TextMeshProUGUI[] stats;

    public void UpdateStatus(Weapon weapon)
    {
        string uiKey = weapon.key.Replace("WEAPON.", "UI.");
        itemImage.sprite = SpriteManager.GetSprite(uiKey);
        if (itemImage.sprite != null)
            itemImage.rectTransform.sizeDelta = new Vector2(itemImage.sprite.rect.width / itemImage.sprite.rect.height * 100, 100);

        float wRatio = itemImage.rectTransform.sizeDelta.x / ShopUI.ITEM_IMAGE_MAX_WIDTH;
        float hRatio = itemImage.rectTransform.sizeDelta.y / ShopUI.ITEM_IMAGE_MAX_HEIGHT;
        // �̹����� ������ ũ�⸦ ����� ��� ũ�⸦ ������.
        if (wRatio > 1 || hRatio > 1)
        {
            if (wRatio > hRatio) itemImage.rectTransform.sizeDelta /= wRatio;
            else itemImage.rectTransform.sizeDelta /= hRatio;
        }

        // ���� ���� �������� �ɷ�ġ��
        string dmg = weapon.bullets > 1 ? $"{weapon.dmg}x{weapon.bullets}" : $"{weapon.dmg}";
        string aspeed = $"{string.Format("{0:0.0}", 1 / weapon.adelay)}/s";
        string ammo = $"{weapon.ammo}";

        statusNames[0].SetEntry("GAME.SHOP.STAT.DMG");
        statusNames[1].SetEntry("GAME.SHOP.STAT.ASPEED");
        statusNames[2].SetEntry("GAME.SHOP.STAT.AMMO");

        // ���� Ư���ɷ� �߰� ǥ��
        stats[0].text = dmg;
        stats[1].text = aspeed;
        stats[2].text = ammo;
    }
}
