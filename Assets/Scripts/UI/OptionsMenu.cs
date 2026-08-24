using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float maxBrightnessValue;
    [SerializeField] private float minBrightnessValue;

    [Header("References")]
    [SerializeField] private GameObject uiHolder;
    [SerializeField] private GameObject controlsMenuUI;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Volume brightnessVolume;
    
    // Used by the controls button in the options menu
    public void LoadControls()
    {
        uiHolder.SetActive(false);
        controlsMenuUI.SetActive(true);
    }

    // Used by the return button in the controls menu
    public void ReturnFromControls()
    {
        controlsMenuUI.SetActive(false);
        uiHolder.SetActive(true);
    }

    public void SetBrightnessPercent(float percent)
    {
        ColorAdjustments colr;
        brightnessVolume.profile.TryGet(out colr);
        if (colr == null)
        {
            throw new Exception("No ColorAdjustments profile found on brightness volume!");
        }

        float brightnessFinal = ((maxBrightnessValue - minBrightnessValue) * percent) + minBrightnessValue;
        colr.colorFilter.value = new Color(brightnessFinal, brightnessFinal, brightnessFinal);
        SaveOptions();
    }

    public void SetMusicVolume(float percent)
    {
        AudioManager.Instance.SetVCAVolume(FmodEvents.Instance.musicVCAPath, percent);
        SaveOptions();
    }

    public void SetSFXVolume(float percent)
    {
        AudioManager.Instance.SetVCAVolume(FmodEvents.Instance.sfxVCAPath, percent);
        SaveOptions();
    }

    public void SaveOptions()
    {
        OptionsData optionsData = new OptionsData(brightnessSlider.value, musicSlider.value, sfxSlider.value);
        //SaveSystem.SaveOptions(optionsData);
    }

    public void LoadOptions()
    {
        OptionsData optionsData = null;// = SaveSystem.GetOptions();
        if (optionsData == null)
        {
            optionsData = new OptionsData(1f, 1f, 1f);
        }

        brightnessSlider.value = optionsData.brightness;
        SetBrightnessPercent(optionsData.brightness);

        musicSlider.value = optionsData.musicVolume;
        SetMusicVolume(optionsData.musicVolume);

        sfxSlider.value = optionsData.sfxVolume;
        SetSFXVolume(optionsData.sfxVolume);
    }

    [System.Serializable]
    public class OptionsData
    {
        public float brightness;
        public float musicVolume;
        public float sfxVolume;

        public OptionsData(float brightness, float musicVolume, float sfxVolume)
        {
            this.brightness = brightness;
            this.musicVolume = musicVolume;
            this.sfxVolume = sfxVolume;
        }
    }
}