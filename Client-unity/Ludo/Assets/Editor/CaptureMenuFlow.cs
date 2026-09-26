using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Renders the menu pages without touching the open scene: the MainMenu canvas is duplicated into
/// a temporary additive scene, one page is activated at a time (header/title included) and the
/// result is written to Temp/captures/menu/*.png. It also renders mid-transition frames using the
/// same slide math as <see cref="MainMenu"/>.
/// </summary>
public static class CaptureMenuFlow
{
    private static readonly int[][] Sizes =
    {
        new[] { 720, 1080 },   // reference resolution
        new[] { 1080, 2340 }   // tall phone
    };

    private static readonly MenuPageId[] Pages =
    {
        MenuPageId.Home,
        MenuPageId.Store,
        MenuPageId.Friends,
        MenuPageId.Levels,
        MenuPageId.GamePage,
        MenuPageId.PlayingWithFriends,
        MenuPageId.Messages,
        MenuPageId.LuckyWheel,
        MenuPageId.VipWheel,
        MenuPageId.Profile,
        MenuPageId.Lobby
    };

    private const string OutDir = "Temp/captures/menu";

    [MenuItem("Tools/Capture Menu Flow")]
    public static void Capture()
    {
        var src = GameObject.Find("MainMenu");
        if (src == null) { Debug.LogError("[CaptureMenuFlow] MainMenu not found"); return; }

        var menu = src.GetComponent<MainMenu>();
        if (menu == null) { Debug.LogError("[CaptureMenuFlow] MainMenu component not found"); return; }

        Directory.CreateDirectory(OutDir);
        var report = new StringBuilder();

        var originalScene = SceneManager.GetActiveScene();

        var temp = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        if (originalScene.IsValid()) SceneManager.SetActiveScene(originalScene);

        var copy = Object.Instantiate(src);
        copy.name = "MainMenu (capture copy)";
        SceneManager.MoveGameObjectToScene(copy, temp);

        var copyMenu = copy.GetComponent<MainMenu>();
        var canvas = copy.GetComponent<Canvas>();
        var scaler = copy.GetComponent<CanvasScaler>();

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

        foreach (var size in Sizes)
        {
            int w = size[0], h = size[1];
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            report.AppendLine($"=== {w}x{h} ===");

            foreach (var id in Pages)
            {
                copyMenu.EditorHideAllPages();
                ShowSingle(copyMenu, id, report);
                Render(cam, rt, w, h, $"{id}_{w}x{h}.png");
            }

            copyMenu.EditorHideAllPages();
            foreach (float t in new[] { 0f, 0.35f, 0.65f })
            {
                copyMenu.EditorPreviewTransition(MenuPageId.Home, MenuPageId.GamePage, t, true);
                Render(cam, rt, w, h, $"transition_forward_t{Mathf.RoundToInt(t * 100)}_{w}x{h}.png");
            }

            copyMenu.EditorHideAllPages();
            copyMenu.EditorPreviewTransition(MenuPageId.GamePage, MenuPageId.Home, 0.5f, false);
            Render(cam, rt, w, h, $"transition_back_t50_{w}x{h}.png");

            cam.targetTexture = null;
            rt.Release();
            Object.DestroyImmediate(rt);
        }

        File.WriteAllText($"{OutDir}/report.txt", report.ToString());

        Object.DestroyImmediate(copy);
        Object.DestroyImmediate(camGo);
        EditorSceneManager.CloseScene(temp, true);

        Debug.Log($"[CaptureMenuFlow] done -> {OutDir}\n" + report);
    }

    private static void ShowSingle(MainMenu menu, MenuPageId id, StringBuilder report)
    {
        menu.EditorApplyChrome(id);

        var page = FindPage(menu.transform, id);
        if (page == null) { report.AppendLine($"  {id}: MISSING"); return; }

        var rect = page.Rect;
        page.gameObject.SetActive(true);
        rect.anchoredPosition = page.HomePosition;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Canvas.ForceUpdateCanvases();

        report.AppendLine($"  {id,-20} rect={rect.rect.width:F0}x{rect.rect.height:F0} " +
                          $"pos={rect.anchoredPosition.x:F0},{rect.anchoredPosition.y:F0}");
    }

    private static MenuPage FindPage(Transform root, MenuPageId id)
    {
        foreach (var page in root.GetComponentsInChildren<MenuPage>(true))
            if (page.Id == id) return page;

        return null;
    }

    private static void Render(Camera cam, RenderTexture rt, int w, int h, string fileName)
    {
        Canvas.ForceUpdateCanvases();
        cam.Render();
        Canvas.ForceUpdateCanvases();
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        File.WriteAllBytes($"{OutDir}/{fileName}", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }
}
