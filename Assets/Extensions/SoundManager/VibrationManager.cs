using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static void Vibrate(long milliseconds = 100, int amplitude = 255)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Clamp amplitude between 1 and 255 (Android vibration amplitude range)
            amplitude = Mathf.Clamp(amplitude, 1, 255);
            
            Debug.Log($"VibrationManager Triggering vibration for {milliseconds} ms with amplitude {amplitude}");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator != null)
            {
                Debug.Log($"VibrationManager Vibration service obtained");
                
                // Check if device supports amplitude control (API level 26+)
                AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
                int sdkVersion = buildVersion.GetStatic<int>("SDK_INT");
                
                if (sdkVersion >= 26)
                {
                    // Use VibrationEffect for amplitude control (Android 8.0+)
                    AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    AndroidJavaObject vibrationPattern = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
                    vibrator.Call("vibrate", vibrationPattern);
                }
                else
                {
                    // Fallback to basic vibration for older Android versions
                    vibrator.Call("vibrate", milliseconds);
                    Debug.Log("VibrationManager Using basic vibration (amplitude not supported on this Android version)");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Vibration failed: " + e.Message);
        }
#else
        Debug.Log("Vibration not supported in Editor");
#endif
    }
}
