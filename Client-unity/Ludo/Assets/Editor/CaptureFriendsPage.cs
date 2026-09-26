using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Renders the "Playing with Friends" page at several phone screen sizes to Temp/captures/*.png
/// without touching the open scene: the canvas is duplicated into a temporary additive scene.
/// Also writes a numeric layout report (content height vs available height) per size.
/// </summary>
public static class CaptureFriendsPage
{
    private static readonly int[][] Sizes =
    {
        new[] { 720, 1080 },   // reference resolution (2:3)
        new[] { 1080, 1920 },  // 9:16
        new[] { 1080, 2340 },  // 9:19.5
        new[] { 1440, 3200 },  // 9:20
        new[] { 1080, 1080 },  // square (extreme)
    };

    private const string OutDir = "Temp/captures";

    [MenuItem("Tools/Capture Friends Page")]
    public static void Capture()
    {
        var src = GameObject.Find("MainMenu");
        if (src == null) { Debug.LogError("[CaptureFriends] MainMenu not found"); return; }

        Directory.CreateDirectory(OutDir);
        var report = new StringBuilder();

        var originalScene = SceneManager.GetActiveScene();

        // ---- isolated copy in a temporary additive scene ----
        var temp = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        if (originalScene.IsValid()) SceneManager.SetActiveScene(originalScene);

        var copy = Object.Instantiate(src);
        copy.name = "MainMenu (capture copy)";
        SceneManager.MoveGameObjectToScene(copy, temp);

        var canvas = copy.GetComponent<Canvas>();
        var scaler = copy.GetComponent<CanvasScaler>();
        var page = FindByName(copy.transform, "Playing with Friends Page");
        if (canvas == null) { Debug.LogError("[CaptureFriends] copy has no Canvas"); }
        if (page == null) { Debug.LogError("[CaptureFriends] page not found in copy"); }

        var camGo = new GameObject("__capture_cam", typeof(Camera));
        SceneManager.MoveGameObjectToScene(camGo, temp);
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.06f, 0.13f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 5000f;
        cam.depth = -100;
        camGo.transform.SetPositionAndRotation(new Vector3(0f, 0f, -1000f), Quaternion.identity);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 500f;
        }
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        var fitter = page != null ? page.GetComponent<ResponsivePageFitter>() : null;
        if (fitter == null) Debug.LogWarning("[CaptureFriends] page has no ResponsivePageFitter");

        foreach (var s in Sizes)
        {
            int w = s[0], h = s[1];
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            Canvas.ForceUpdateCanvases();
            if (fitter != null) fitter.Refresh(); // edit mode: apply the fit factor for this viewport
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)page);
            Canvas.ForceUpdateCanvases();
            cam.Render();
            Canvas.ForceUpdateCanvases();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes($"{OutDir}/friends_{w}x{h}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            cam.targetTexture = null;
            rt.Release();
            Object.DestroyImmediate(rt);

            report.AppendLine(Measure(page, copy.transform, w, h));
        }

        File.WriteAllText($"{OutDir}/report.txt", report.ToString());
        Object.DestroyImmediate(copy);
        Object.DestroyImmediate(camGo);
        EditorSceneManager.CloseScene(temp, true);
        Debug.Log($"[CaptureFriends] done -> {OutDir}");
    }

    private static string Measure(Transform page, Transform canvasRoot, int w, int h)
    {
        var sb = new StringBuilder();
        var canvasRt = (RectTransform)canvasRoot;
        sb.AppendLine($"=== {w}x{h}  canvas={canvasRt.rect.width:F0}x{canvasRt.rect.height:F0} ===");
        if (page == null) { sb.AppendLine("  page missing"); return sb.ToString(); }

        var pr = (RectTransform)page;
        LayoutRebuilder.ForceRebuildLayoutImmediate(pr);

        var vlg = page.GetComponent<VerticalLayoutGroup>();
        float content = 0f;
        int n = 0;
        for (int i = 0; i < page.childCount; i++)
        {
            var c = page.GetChild(i);
            if (!c.gameObject.activeSelf) continue;
            content += LayoutUtility.GetPreferredHeight((RectTransform)c);
            n++;
        }
        float pad = vlg != null ? vlg.padding.top + vlg.padding.bottom : 0f;
        float sp = vlg != null ? vlg.spacing * Mathf.Max(0, n - 1) : 0f;
        float need = content + pad + sp;

        float minSum = 0f;
        for (int i = 0; i < page.childCount; i++)
        {
            var c = page.GetChild(i);
            if (!c.gameObject.activeSelf) continue;
            minSum += LayoutUtility.GetMinHeight((RectTransform)c);
        }
        float minNeed = minSum + pad + sp;

        float have = pr.rect.height;
        sb.AppendLine($"  pageRect={pr.rect.width:F0}x{have:F0}" +
                      $" preferred={need:F0} min={minNeed:F0} -> " +
                      (minNeed > have + 0.5f ? $"CLIPPED by {minNeed - have:F0}px" : "fits"));

        for (int i = 0; i < page.childCount; i++)
        {
            var c = page.GetChild(i);
            if (!c.gameObject.activeSelf) continue;
            var cr = (RectTransform)c;
            sb.AppendLine($"    {c.name,-22} h={cr.rect.height:F0} w={cr.rect.width:F0} " +
                          $"(minH={LayoutUtility.GetMinHeight(cr):F0} prefH={LayoutUtility.GetPreferredHeight(cr):F0})");
        }
        return sb.ToString();
    }

    private static Transform FindByName(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}
