using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    Resolution[] resolutions;
    int currentResolutionIndex = 0;
    
    void Start()
    {
        getResolution();
        getQuality();
    }

	public void MasterVolume (float vol)
    {
        audioMixer.SetFloat("MasterVolume", vol);
	}

    public void SFXVolume(float vol)
    {
        audioMixer.SetFloat("SFXVolume", vol);
    }

    public void BGMVolume(float vol)
    {
        audioMixer.SetFloat("BGMVolume", vol);
    }

    public void Mute(bool isMute)
    {
        isMute = !isMute;
        AudioListener.volume = isMute ? 1 : 0;
    }

    public void FullScreen (bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    private void getResolution()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> res = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string _res = resolutions[i].width + " x " + resolutions[i].height;
            res.Add(_res);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(res);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void getQuality()
    {
        int qualityLevel = QualitySettings.GetQualityLevel();
        qualityDropdown.value = qualityLevel;
    }

    public void Quality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
