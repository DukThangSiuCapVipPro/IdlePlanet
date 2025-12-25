using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SmoothButton : MonoBehaviour, IPointerDownHandler
{
    public bool animEnabled = true;
    public bool vibrateEnabled = true;
    public GameObject objectDisable;
    public UnityEvent onClick;

    Button button;
    public bool interactable
    {
        get
        {
            return button.interactable;
        }
        set
        {
            button.interactable = value;
            objectDisable.SetActive(!value);
        }
    }

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnClickAnimate()
    {
        if (!animEnabled) return;
        if (vibrateEnabled)
            SoundManager.Instance.Vibrate(100, 100);
        transform.DOScale(Vector3.one * 1.15f, 0.1f).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.05f).OnComplete(() =>
            {
                
            });
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(SFXType.Click);
        OnClickAnimate();
        if (!interactable)
            return;
        onClick?.Invoke();
    }
}
