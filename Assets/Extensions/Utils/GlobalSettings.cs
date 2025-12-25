using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSettings : Singleton<GlobalSettings>
{
    public void ChangeScene(Scene scene, bool autoHide = true)
    {
        StartCoroutine(IChangeScene(scene, autoHide));
    }

    IEnumerator IChangeScene(Scene scene, bool autoHide = true)
    {
        LoadingScreenManager.Instance.RevealLoadingScreen();
        yield return new WaitForSeconds(1f);
        var change = SceneManager.LoadSceneAsync(scene.ToString());
        while (change.isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }
        if (autoHide)
            LoadingScreenManager.Instance.HideLoadingScreen();
    }
}

[Serializable]
public enum Scene
{
    Loading,
    Game
}
