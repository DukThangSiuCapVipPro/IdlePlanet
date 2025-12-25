using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarHandler : MonoBehaviour
{
    [Tooltip("Quantity you have / quantity you need")]
    public TextMeshProUGUI ProgressText;
    [Tooltip("value = % version of ProgressText")]
    public Slider Slider;
    [Tooltip("Enable when progress is full")]
    public GameObject ProgressFull;


    public void Set(int current, int needed)
    {
        ProgressText.text = current + "/" + needed;
        Slider.value = current / (float)needed;
        if (ProgressFull)
            ProgressFull.SetActive(current >= needed);
    }

    public void SetMaxed(string s)
    {
        ProgressText.text = s;
        Slider.value = 1f;
        if (ProgressFull)
            ProgressFull.SetActive(true);
    }
}
