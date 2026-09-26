using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class DumpFriendsPage
{
    [MenuItem("Tools/Dump Friends Page")]
    public static void Dump()
    {
        var sb = new StringBuilder();
        var mm = GameObject.Find("MainMenu");
        if (mm == null) { File.WriteAllText("Temp/friends_dump.txt", "MainMenu not found"); return; }

        var scaler = mm.GetComponent<CanvasScaler>();
        if (scaler != null)
            sb.AppendLine($"CanvasScaler mode={scaler.uiScaleMode} refRes={scaler.referenceResolution} match={scaler.matchWidthOrHeight} screen={Screen.width}x{Screen.height}");
        var canvas = mm.GetComponent<Canvas>();
        if (canvas != null) sb.AppendLine($"Canvas renderMode={canvas.renderMode}");
        sb.AppendLine($"TMP default font: {(TMP_Settings.defaultFontAsset != null ? TMP_Settings.defaultFontAsset.name : "null")}");

        var mmComp = mm.GetComponent<MainMenu>();
        if (mmComp != null)
        {
            var so = new SerializedObject(mmComp);
            var arr = so.FindProperty("pages");
            if (arr != null)
            {
                sb.AppendLine("=== MainMenu.pages array ===");
                for (int i = 0; i < arr.arraySize; i++)
                {
                    var o = arr.GetArrayElementAtIndex(i).objectReferenceValue;
                    sb.AppendLine($"  [{i}] {(o != null ? o.name : "<null>")}");
                }
            }
        }

        Transform pages = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Pages") { pages = t; break; }
        if (pages != null)
        {
            sb.AppendLine("=== Pages children ===");
            for (int i = 0; i < pages.childCount; i++)
                sb.AppendLine($"  [{i}] {pages.GetChild(i).name} active={pages.GetChild(i).gameObject.activeSelf}");
        }

        Transform root = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Playing with Friends Page") { root = t; break; }
        if (root == null)
            foreach (var t in mm.GetComponentsInChildren<Transform>(true))
                if (t.name.IndexOf("Playing", System.StringComparison.OrdinalIgnoreCase) >= 0 && t.GetComponent<RectTransform>() != null) { root = t; break; }
        if (root == null) sb.AppendLine("Playing with Friends Page not found");
        else Dump(root, 0, sb);

        sb.AppendLine();
        sb.AppendLine("=== Header (InsidePages/Header) ===");
        Transform inside = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "InsidePages") { inside = t; break; }
        if (inside != null)
        {
            var header = inside.Find("Header");
            if (header != null) Dump(header, 0, sb);
            else sb.AppendLine("Header not found under InsidePages");
        }
        File.WriteAllText("Temp/friends_dump.txt", sb.ToString());
        Debug.Log("[DumpFriendsPage] written Temp/friends_dump.txt");
    }

    private static void Dump(Transform t, int depth, StringBuilder sb)
    {
        var pad = new string(' ', depth * 2);
        string extra = "";
        var vlg = t.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) extra += $" VLG[pad={vlg.padding.left},{vlg.padding.top},{vlg.padding.right},{vlg.padding.bottom} sp={vlg.spacing} ctrlW={vlg.childControlWidth} ctrlH={vlg.childControlHeight} expW={vlg.childForceExpandWidth} expH={vlg.childForceExpandHeight} align={vlg.childAlignment}]";
        var hlg = t.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) extra += $" HLG[pad={hlg.padding.left},{hlg.padding.top},{hlg.padding.right},{hlg.padding.bottom} sp={hlg.spacing} ctrlW={hlg.childControlWidth} ctrlH={hlg.childControlHeight} expW={hlg.childForceExpandWidth} expH={hlg.childForceExpandHeight} align={hlg.childAlignment}]";
        var le = t.GetComponent<LayoutElement>();
        if (le != null) extra += $" LE[minW={le.minWidth} minH={le.minHeight} prefW={le.preferredWidth} prefH={le.preferredHeight} flexW={le.flexibleWidth} flexH={le.flexibleHeight} ign={le.ignoreLayout}]";
        var csf = t.GetComponent<ContentSizeFitter>();
        if (csf != null) extra += $" CSF[h={csf.verticalFit} w={csf.horizontalFit}]";
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            var rtlProp = typeof(TMP_Text).GetProperty("isRightToLeft");
            string rtl = rtlProp != null ? $" rtl={rtlProp.GetValue(tmp)}" : "";
            extra += $" TMP[\"{tmp.text}\" font={(tmp.font != null ? tmp.font.name : "null")} fs={tmp.fontSize} auto={tmp.enableAutoSizing} align={tmp.alignment} color=#{ColorUtility.ToHtmlStringRGBA(tmp.color)} bold={tmp.fontStyle}{rtl}]";
        }
        var img = t.GetComponent<Image>();
        if (img != null) extra += $" IMG[sprite={(img.sprite != null ? img.sprite.name : "null")} type={img.type} fill={img.fillAmount.ToString("F2")} preserve={img.preserveAspect} color=#{ColorUtility.ToHtmlStringRGBA(img.color)}]";
        var btn = t.GetComponent<Button>();
        if (btn != null) extra += $" BTN[target={(btn.targetGraphic != null ? btn.targetGraphic.name : "null")}]";
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