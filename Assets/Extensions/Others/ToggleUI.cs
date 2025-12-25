using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    public Button btnToggle;
    public Sprite onSprite;
    public Sprite offSprite;
    public Sprite onSpriteBg;
    public Sprite offSpriteBg;
    public Image image;
    public Image imageBG;
    public Transform toggleTrans;
    public Transform offTrans;
    public Transform onTrans;

    public bool isOn = false;
    Action<bool> toogleAction;
    bool anim = false;

    void Awake()
    {
        btnToggle.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateUI();
        toogleAction?.Invoke(isOn);
    }
    public void SetToggle(bool value)
    {
        isOn = value;
        UpdateUI();
        anim = true;
    }
    public void AddListener(Action<bool> action)
    {
        toogleAction += action;
    }
    private void UpdateUI()
    {
        if (isOn)
        {
            image.sprite = onSprite;
            imageBG.sprite = onSpriteBg;
            if (anim)
                toggleTrans.DOMove(onTrans.position, 0.2f);
            else
                toggleTrans.position = onTrans.position;
        }
        else
        {
            image.sprite = offSprite;
            imageBG.sprite = offSpriteBg;
            if (anim)
                toggleTrans.DOMove(offTrans.position, 0.2f);
            else
                toggleTrans.position = offTrans.position;
        }
    }
}
