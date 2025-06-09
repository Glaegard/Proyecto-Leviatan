using System;
using UnityEngine;

public class AutoFrameRateSetter : MonoBehaviour
{
    void Awake()
    {
        // 1) Desactivar V-Sync para no quedar limitado al vertical sync
        QualitySettings.vSyncCount = 0;

        // 2) Detectar la tasa de refresco del dispositivo
        int refresh = GetDeviceRefreshRate();

        // 3) Fijar el targetFrameRate a esa tasa
        Application.targetFrameRate = refresh;

        Debug.Log($"[AutoFrameRateSetter] VSync={QualitySettings.vSyncCount}, targetFrameRate={Application.targetFrameRate}");
    }

    private int GetDeviceRefreshRate()
    {
        // Intentamos primero con Screen.currentResolution
        int rate = Screen.currentResolution.refreshRate;
        if (rate > 0) return rate;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Llama a la API de Android para obtener el refreshRate real
            using var up       = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = up.GetStatic<AndroidJavaObject>("currentActivity");
            using var wm       = activity.Call<AndroidJavaObject>("getWindowManager");
            using var display  = wm.Call<AndroidJavaObject>("getDefaultDisplay");
            float rr = display.Call<float>("getRefreshRate");
            if (rr > 0) return Mathf.RoundToInt(rr);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"AutoFrameRateSetter: no pude leer tasa de refresco Android: {e.Message}");
        }
#endif

        // Fallback razonable
        return 60;
    }
}
