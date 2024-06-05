using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private GameObject goToTitle;
    public void Init()
    {
        masterVolumeSlider.minValue = bgmVolumeSlider.minValue = sfxVolumeSlider.minValue = 1;
        masterVolumeSlider.maxValue = bgmVolumeSlider.maxValue = sfxVolumeSlider.maxValue = 100;
        masterVolumeSlider.wholeNumbers = bgmVolumeSlider.wholeNumbers = sfxVolumeSlider.wholeNumbers = true;

        masterVolumeSlider.onValueChanged.AddListener(v =>
        {
            masterVolumeText.text = v.ToString();
            GameSetting.Instance.AdjustVolume(GameSetting.SoundType.MASTER, v);
        });

        bgmVolumeSlider.onValueChanged.AddListener(v =>
        {
            bgmVolumeText.text = v.ToString();
            GameSetting.Instance.AdjustVolume(GameSetting.SoundType.BGM, v);
        });

        sfxVolumeSlider.onValueChanged.AddListener(v =>
        {
            sfxVolumeText.text = v.ToString();
            GameSetting.Instance.AdjustVolume(GameSetting.SoundType.SFX, v);
        });

        masterVolumeSlider.value = GameSetting.Instance.SettingInfo.options["volume"][0];
        bgmVolumeSlider.value = GameSetting.Instance.SettingInfo.options["volume"][1];
        sfxVolumeSlider.value = GameSetting.Instance.SettingInfo.options["volume"][2];
    }

    public void Title(bool b)
    {
        goToTitle.SetActive(!b);
    }

    public void GoToTitle()
    {
        GameController.Instance.GoTo(SceneController.Scene.TITLE);
    }
}
