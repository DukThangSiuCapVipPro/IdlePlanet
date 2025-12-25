using System;
using UnityEngine;

public class LoadingScreenManager : Singleton<LoadingScreenManager>
{
    private Animator _animatorComponent;

    Action OnOpenEvt;
    Action OnCloseEvt;

    public Action OnHidedEvt;
    
    private void Start()
    {
        _animatorComponent = transform.GetComponent<Animator>();

        // Remove it if you don't want to hide it in the Start function and call it elsewhere
        // HideLoadingScreen();
    }

    public void RevealLoadingScreen(Action callback = null)
    {
        Debug.Log("Show Loading Screen");
        OnOpenEvt = callback;
        _animatorComponent.SetTrigger("Reveal");
    }

    public void HideLoadingScreen(Action callback = null)
    {
        Debug.Log("Hide Loading Screen");
        // Call this function, if you want start hiding the loading screen
        OnCloseEvt = callback;
        _animatorComponent.SetTrigger("Hide");
    }

    public void OnFinishedReveal()
    {
        Debug.Log("Showed Loading Screen");
        OnOpenEvt?.Invoke();
    }

    public void OnFinishedHide()
    {
        Debug.Log("Hided Loading Screen");
        OnCloseEvt?.Invoke();
        OnHidedEvt?.Invoke();
    }

}
