using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{
    public static List <OtherItem> Items { get { return items; } }
    private static List<OtherItem> items;
    public static void Init()
    {
        items = new List<OtherItem>()
        {
            new OtherItem()
            {
                key = "WEAPON.GRENADE",
                price = 0,
            },
            new OtherItem()
            {
                key = "HEAL.HP",
                price = 100,
            },
            new OtherItem()
            {
                key = "HEAL.ARMOR",
                price = 100,
            },
            new OtherItem()
            {
                key = "COMPANION.COMPANION1",
                price = 0,
            },
            new OtherItem()
            {
                key = "COMPANION.COMPANION2",
                price = 0,
            },
            new OtherItem()
            {
                key = "COMPANION.COMPANION3",
                price = 0,
            },
        };
    }

    public static OtherItem GetItem(string key)
    {
        foreach (var item in items)
        {
            if (key == item.key) return item;
        }
        return null;
    }
}
