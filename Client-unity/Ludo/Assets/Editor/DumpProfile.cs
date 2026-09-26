using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DumpProfile
{
    [MenuItem("Tools/Dump Profile Hierarchy")]
    public static void Dump()
    {
        var sb = new StringBuilder();
        var mm = GameObject.Find("MainMenu");
        if (mm == null) { File.WriteAllText("Temp/profile_dump.txt", "MainMenu not found"); return; }

        var scaler = mm.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
            sb.AppendLine($"CanvasScaler mode={scaler.uiScaleMode} refRes={scaler.referenceResolution} match={scaler.matchWidthOrHeight} screen={Screen.width}x{Screen.height}");
        var canvas = mm.GetComponent<Canvas>();
        if (canvas != null) sb.AppendLine($"Canvas renderMode={canvas.renderMode}");

        Transform root = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Profile Page") { root = t; break; }
        if (root == null) { File.WriteAllText("Temp/profile_dump.txt", sb + "Profile Page not found under MainMenu"); return; }

        Dump(root, 0, sb);
        sb.AppendLine();
        sb.AppendLine("=== InsidePages context (Header + Pages) ===");
        Transform inside = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "InsidePages") { inside = t; break; }
        if (inside != null)
        {
            Dump(inside, 0, sb);
        }
        File.WriteAllText("Temp/profile_dump.txt", sb.ToString());
        Debug.Log("[DumpProfile] written Temp/profile_dump.txt");
    }

    private static void Dump(Transform t, int depth, StringBuilder sb)
    {
        var pad = new string(' ', depth * 2);
        string extra = "";
        var vlg = t.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vlg != null) extra += $" VLG[pad={vlg.padding.left},{vlg.padding.top},{vlg.padding.right},{vlg.padding.bottom} sp={vlg.spacing} ctrlW={vlg.childControlWidth} ctrlH={vlg.childControlHeight} expW={vlg.childForceExpandWidth} expH={vlg.childForceExpandHeight} align={vlg.childAlignment}]";
        var hlg = t.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (hlg != null) extra += $" HLG[pad={hlg.padding.left},{hlg.padding.top},{hlg.padding.right},{hlg.padding.bottom} sp={hlg.spacing} ctrlW={hlg.childControlWidth} ctrlH={hlg.childControlHeight} expW={hlg.childForceExpandWidth} expH={hlg.childForceExpandHeight} align={hlg.childAlignment}]";
        var le = t.GetComponent<UnityEngine.UI.LayoutElement>();
        if (le != null) extra += $" LE[minW={le.minWidth} minH={le.minHeight} prefW={le.preferredWidth} prefH={le.preferredHeight} flexW={le.flexibleWidth} flexH={le.flexibleHeight} ign={le.ignoreLayout}]";
        var arf = t.GetComponent<UnityEngine.UI.AspectRatioFitter>();
        if (arf != null) extra += $" ARF[{arf.aspectMode} r={arf.aspectRatio}]";
        var tmp = t.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null) extra += $" TMP[\"{tmp.text}\" fs={tmp.fontSize} auto={tmp.enableAutoSizing} align={tmp.alignment}]";
        var img = t.GetComponent<UnityEngine.UI.Image>();
        if (img != null) extra += $" IMG[type={img.type} fill={img.fillAmount} preserve={img.preserveAspect}]";
        var comps = t.GetComponents<Component>();
        var names = new StringBuilder();
        foreach (var c in comps) names.Append(c == null ? "<MISSING>" : c.GetType().Name).Append(",");
        string compsStr = names.ToString().TrimEnd(',');
        if (t is RectTransform rt)
            sb.AppendLine($"{pad}{t.name} [{compsStr}]{extra} active={t.gameObject.activeSelf} anchors=({rt.anchorMin.x:F3},{rt.anchorMin.y:F3})-({rt.anchorMax.x:F3},{rt.anchorMax.y:F3}) pos={rt.anchoredPosition} size={rt.sizeDelta} pivot=({rt.pivot.x:F2},{rt.pivot.y:F2}) ls={rt.localScale.x:F2}");
        else
            sb.AppendLine($"{pad}{t.name} [{compsStr}]{extra} active={t.gameObject.activeSelf}");
        for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, sb);
    }
}
