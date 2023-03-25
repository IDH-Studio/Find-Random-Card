using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _effectSlider;

    [Space(10)]
    [Header("Handle Objects")]
    [SerializeField] private Image _bgmHandleImage;
    [SerializeField] private Image _effectHandleImage;

    [Space(10)]
    [SerializeField] private List<Sprite> speakerSprites;
    private float soundStandard = 0.25f;

    public void ShowSetting()
    {
        _bgmSlider.value = GameManager._instance._soundManager.AudioSources[(int)AUDIO.BGM].volume;
        OnBGMSliderDown();
        _effectSlider.value = GameManager._instance._soundManager.AudioSources[(int)AUDIO.EFFECT].volume;
        OnEffectSliderDown();
    }

    public void Cancel()
    {
        GameManager._instance._soundManager.PlayEffectSound("ButtonPopSoundDown");
        GameManager._instance._screenManager.PrevScreen();
    }

    public void Apply()
    {
        GameManager._instance._soundManager.ChangeVolume(AUDIO.BGM, _bgmSlider.value);
        GameManager._instance._soundManager.ChangeVolume(AUDIO.EFFECT, _effectSlider.value);

        GameManager._instance._soundManager.PlayEffectSound("ButtonPopSound");
        GameManager._instance._screenManager.PrevScreen();
    }

    public void OnBGMSliderDown()
    {
        int spriteIndex;
        if (_bgmSlider.value == 0)
        {
            spriteIndex = 0;
        }
        else
        {
            spriteIndex = (int)(_bgmSlider.value / soundStandard) + 1;
            spriteIndex = spriteIndex > 4 ? 4 : spriteIndex;
        }

        _bgmHandleImage.sprite = speakerSprites[spriteIndex];
    }

    public void OnEffectSliderDown()
    {
        int spriteIndex;
        if (_effectSlider.value == 0)
        {
            spriteIndex = 0;
        }
        else
        {
            spriteIndex = (int)(_effectSlider.value / soundStandard) + 1;
            spriteIndex = spriteIndex > 4 ? 4 : spriteIndex;
        }

        _effectHandleImage.sprite = speakerSprites[spriteIndex];
    }

}
