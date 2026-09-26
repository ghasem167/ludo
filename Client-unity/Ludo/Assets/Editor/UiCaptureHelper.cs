using UnityEditor;
using UnityEngine;

public static class UiCaptureHelper
{
    [MenuItem("Tools/UI Capture Mode On")]
    public static void On()
    {
        var mm = GameObject.Find("MainMenu");
        if (mm == null) { Debug.LogError("[UiCapture] MainMenu not found"); return; }
        var canvas = mm.GetComponent<Canvas>();
        if (canvas == null) { Debug.LogError("[UiCapture] no Canvas on MainMenu"); return; }
        Undo.RecordObject(canvas, "ui capture on");
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        var cam = Camera.main;
        canvas.worldCamera = cam;
        canvas.planeDistance = 100;
        Debug.Log("[UiCapture] overlay -> screen space camera (temporary)");
    }

    [MenuItem("Tools/UI Capture Mode Off")]
    public static void Off()
    {
        var mm = GameObject.Find("MainMenu");
        if (mm == null) { Debug.LogError("[UiCapture] MainMenu not found"); return; }
        var canvas = mm.GetComponent<Canvas>();
        if (canvas == null) { Debug.LogError("[UiCapture] no Canvas on MainMenu"); return; }
        Undo.RecordObject(canvas, "ui capture off");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        Debug.Log("[UiCapture] back to screen space overlay");
    }
}