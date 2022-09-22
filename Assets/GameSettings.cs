using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class GameSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        List<string> listRes = new List<string>();
        for (int i = 0; i < resolutions.Length; i++) {
            Resolution r = resolutions[i];

            listRes.Add(r.width + "x" + r.height);
            if(r.width == Screen.currentResolution.width 
                && r.height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(listRes);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVolume(float amount) {
        audioMixer.SetFloat("MasterVolume", amount);
    }

    public void SetGraphics(int index) {
        Debug.Log("SetGraphics " + index);
        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullscreen(bool set) {
        Screen.fullScreen = set;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
