using System;
using DG.Tweening;
using ThangDD;
using UnityEngine;
using UnityEngine.UI;

public class CloudLoad : Singleton<CloudLoad>
{
    protected override bool DontDestroyOnLoadEnabled => false;

    public Animator animator;


    Action OnOpenEvt;
    Action OnCloseEvt;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Open(Action callback = null)
    {
        OnOpenEvt = callback;
        animator.Play("CloudLoadOpen");

    }
    public void Close(Action callback = null)
    {
        OnCloseEvt = callback;
        animator.Play("CloudLoadClose");
    }

    public void OnOpen()
    {
        OnOpenEvt?.Invoke();
    }
    public void OnClose()
    {
        OnCloseEvt?.Invoke();
    }
}
