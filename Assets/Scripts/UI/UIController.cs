using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get { return instance; } }
    private static UIController instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        setting.Init();
        InitItemInfo();
        OpenSetting(false);
        OpenShop(false);
        levelUpView.gameObject.SetActive(false);
    }

    [Header("Scene")]
    [SerializeField] private GameObject[] scenes;
    [SerializeField] private Canvas canvas;

    public void ChangeScene(int index)
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i].SetActive(i == index);
        }

        if (index == 2) canvas.worldCamera = CameraController.Instance.Cam;
    }

    [Header("Game")]
    [SerializeField] private SettingUI setting;
    [SerializeField] private ShopUI shop;

    public void StartGame()
    {
        shop.Init();
        foreach (var weapon in WeaponManager.Weapons)
        {
            if (weapon.consumable) continue;
            itemInfos[weapon.key].gameObject.SetActive(WeaponController.Instance.HasWeapon(weapon.key));
        }
    }

    public void OpenShop(bool b)
    {
        shop.Open(b);
    }

    public void BuyItem(ShopItem shopItem)
    {
        shop.BuyItem(shopItem);
    }

    public void OpenSetting(bool b)
    {
        setting.gameObject.SetActive(b);
    }

    public void LoadResolutionInfo()
    {
        setting.LoadResolutionInfo();
    }

    #region Status
    [Header("Status")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider ammoSlider;
    [SerializeField] private LocalizeStringEvent weaponLocalizeString;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private GameObject reloadingObj;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI granadeAmountText;

    [Header("Item")]
    [SerializeField] private Transform weaponInfoParent;
    [SerializeField] private Transform turretInfoParent;
    [SerializeField] private ItemInfoUI itemInfoPrefab;
    private Dictionary<string, ItemInfoUI> itemInfos;

    void Update()
    {
        if (GameController.Instance.GameStarted == false) return;

        hpSlider.maxValue = Player.Instance.MaxHp;
        hpSlider.value = Player.Instance.Hp;
        expSlider.maxValue = Player.Instance.MaxExp;
        expSlider.value = Player.Instance.Exp;
        lvText.text = Player.Instance.Lv.ToString();
        ammoSlider.value = WeaponController.Instance.CurWeapon.curammo;
        granadeAmountText.text = Player.Instance.ItemAmount("WEAPON.GRANADE").ToString();
        moneyText.text = $"$ {Player.Instance.Money}";
    }

    public void SwitchWeapon()
    {
        ammoSlider.maxValue = WeaponController.Instance.CurWeapon.ammo;
        ammoSlider.value = WeaponController.Instance.CurWeapon.curammo;
        weaponLocalizeString.SetEntry(WeaponController.Instance.CurWeapon.key);
        Reloading(false);
    }

    public void Reloading(bool b)
    {
        reloadingObj.SetActive(b);
    }

    private void InitItemInfo()
    {
        itemInfoPrefab.gameObject.SetActive(false);

        itemInfos = new Dictionary<string, ItemInfoUI>();
        foreach (var weapon in WeaponManager.Weapons)
        {
            if (weapon.consumable) continue;
            var weaponInfoUI = Instantiate(itemInfoPrefab, weaponInfoParent);
            weaponInfoUI.SetInfo(SpriteManager.GetSprite(weapon.key));
            itemInfos.Add(weapon.key, weaponInfoUI);
            weaponInfoUI.gameObject.SetActive(false);
        }

        foreach (var turret in TurretManager.Turrets)
        {
            var turretInfoUI = Instantiate(itemInfoPrefab, turretInfoParent);
            turretInfoUI.SetInfo(SpriteManager.GetSprite(turret.key));
            itemInfos.Add(turret.key, turretInfoUI);
            turretInfoUI.gameObject.SetActive(true);
        }
    }

    public void GetItem(string key)
    {
        if (itemInfos.ContainsKey(key))
            itemInfos[key].gameObject.SetActive(true);
    }

    public void UpdateItemAmount(string key, int amount)
    {
        if (itemInfos.ContainsKey(key))
            itemInfos[key].UpdateInfo(amount);
    }
    #endregion

    #region Level
    [Header("Level Up")]
    [SerializeField] private GameObject levelUpView;
    [SerializeField] private TextMeshProUGUI[] upgradeInfos;

    public void LevelUp()
    {
        upgradeInfos[0].text = $"{Player.Instance.MaxHp} �� {Player.Instance.MaxHp + 5}";
        upgradeInfos[1].text = $"{Player.Instance.Speed} �� {Player.Instance.Speed + 1}";
        upgradeInfos[2].text = $"{Player.Instance.Reload}% �� {Player.Instance.Reload + 25}%";
        upgradeInfos[3].text = $"{Player.Instance.Reward}% �� {Player.Instance.Reward + 25}%";

        levelUpView.gameObject.SetActive(true);
        GameController.Instance.LevelUpPause(true);
    }

    public void UpgradeStat(int index)
    {
        Player.Instance.Upgrade((Player.StatType)index);
        levelUpView.gameObject.SetActive(false);
        GameController.Instance.LevelUpPause(false);
    }
    #endregion

    #region Round
    [Space]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private GameObject startRoundButton;

    public void UpdateRoundTime(int time)
    {
        roundTimeText.text = time.ToString();
    }

    public void StartRound()
    {
        roundTimeText.gameObject.SetActive(true);
        startRoundButton.SetActive(false);
    }

    public void EndRound()
    {
        roundTimeText.gameObject.SetActive(false);
        startRoundButton.SetActive(true);
    }
    #endregion
}
