using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dumps the whole MainMenu canvas tree to Temp/mainmenu_dump.txt (used while wiring the menu flow).
/// Nodes deeper than <see cref="DetailedDepth"/> are printed as one line (name + child count) unless
/// their name is interesting (Page / Header / Title / Btn / Button / Panel / Group / Layout).
/// </summary>
public static class DumpMainMenu
{
    private const int DetailedDepth = 2;

    /// <summary>Sub-trees that are always dumped in full detail (everything below them).</summary>
    private static readonly string[] DeepRoots =
    {
        "Home Page",
        "Classical Game Page",
        "Level Box",
        "Header",
        "User Data",
        "Btn Group",
        "Offline",
        "Select Menu Btn"
    };

    [MenuItem("Tools/Dump Main Menu")]
    public static void Dump()
    {
        var sb = new StringBuilder();
        var mm = GameObject.Find("MainMenu");
        if (mm == null)
        {
            File.WriteAllText("Temp/mainmenu_dump.txt", "MainMenu not found");
            return;
        }

        var canvas = mm.GetComponent<Canvas>();
        var scaler = mm.GetComponent<CanvasScaler>();
        sb.AppendLine($"screen={Screen.width}x{Screen.height} rootRect={((RectTransform)mm.transform).rect.size} " +
                      $"renderMode={(canvas != null ? canvas.renderMode.ToString() : "no canvas")} sortOrder={(canvas != null ? canvas.sortingOrder : -1)}");
        if (scaler != null)
            sb.AppendLine($"scaler mode={scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");
        sb.AppendLine();

        Dump(mm.transform, 0, sb, false);
        File.WriteAllText("Temp/mainmenu_dump.txt", sb.ToString());
        Debug.Log("[DumpMainMenu] written Temp/mainmenu_dump.txt");
    }

    private static bool Interesting(string name)
    {
        string n = name.ToLowerInvariant();
        return n.Contains("page") || n.Contains("header") || n.Contains("title") || n.Contains("btn") ||
               n.Contains("button") || n.Contains("panel") || n.Contains("layout") || n.Contains("group") ||
               n.Contains("back");
    }

    private static void Dump(Transform t, int depth, StringBuilder sb, bool deep)
    {
        string pad = new string(' ', depth * 2);
        if (System.Array.IndexOf(DeepRoots, t.name) >= 0) deep = true;
        bool detailed = deep || depth <= DetailedDepth || Interesting(t.name);

        if (!detailed)
        {
            sb.AppendLine($"{pad}{t.name} (childCount={t.childCount}) active={t.gameObject.activeSelf}");
            return;
        }

        var extra = new StringBuilder();

        var vlg = t.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) extra.Append($" VLG[pad={vlg.padding.left},{vlg.padding.top},{vlg.padding.right},{vlg.padding.bottom} sp={vlg.spacing} ctrlH={vlg.childControlHeight} expH={vlg.childForceExpandHeight}]");
        var hlg = t.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) extra.Append($" HLG[pad={hlg.padding.left},{hlg.padding.top},{hlg.padding.right},{hlg.padding.bottom} sp={hlg.spacing} ctrlH={hlg.childControlHeight} expH={hlg.childForceExpandHeight}]");

        var le = t.GetComponent<LayoutElement>();
        if (le != null) extra.Append($" LE[minW={le.minWidth} minH={le.minHeight} prefW={le.preferredWidth} prefH={le.preferredHeight} flexW={le.flexibleWidth} flexH={le.flexibleHeight}]");

        var cg = t.GetComponent<CanvasGroup>();
        if (cg != null) extra.Append($" CG[alpha={cg.alpha:F2} interactable={cg.interactable} blocks={cg.blocksRaycasts}]");

        var cs = t.GetComponents<MonoBehaviour>();
        var scriptNames = new StringBuilder();
        foreach (var c in cs)
        {
            if (c == null) { scriptNames.Append("<MISSING>,"); continue; }
            var type = c.GetType();
            if (type == typeof(VerticalLayoutGroup) || type == typeof(HorizontalLayoutGroup) ||
                type == typeof(GridLayoutGroup) || type == typeof(LayoutElement) ||
                type == typeof(CanvasGroup) || type == typeof(ContentSizeFitter) ||
                type == typeof(CanvasScaler) || type == typeof(GraphicRaycaster)) continue;
            scriptNames.Append(type.Name).Append(',');
        }
        string scripts = scriptNames.ToString().TrimEnd(',');

        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) extra.Append($" TMP[\"{Truncate(tmp.text, 40)}\" font={(tmp.font != null ? tmp.font.name : "null")} align={tmp.alignment}]");

        var img = t.GetComponent<Image>();
        if (img != null) extra.Append($" IMG[sprite={(img.sprite != null ? img.sprite.name : "null")}]");

        var btn = t.GetComponent<Button>();
        if (btn != null) extra.Append($" BTN[persistent={btn.onClick.GetPersistentEventCount()} interactable={btn.interactable}]");

        var rt = t as RectTransform;
        string rect = rt != null
            ? $" size={rt.sizeDelta} anchors=({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}) pos={rt.anchoredPosition} pivot=({rt.pivot.x:F2},{rt.pivot.y:F2})"
            : "";

        sb.AppendLine($"{pad}{t.name} [{scripts}] active={t.gameObject.activeSelf} sibling={t.GetSiblingIndex()}{rect}{extra}");
        for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, sb, deep);
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.Replace("\n", "\\n");
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
