using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using TMPro;

public static class PersianFontTest
{
    [MenuItem("Tools/Test Persian Font")]
    public static void Run()
    {
        var sb = new StringBuilder();
        string sample = "بازی با دوستان ورودی 100 جایزه 250 ساخت بازی اشرافی حرفه\u200Cای مبتدی لیست دوستان دعوت شده اعلائین WARIOR_02 1650 بعد از ساخت بازی دعوتنامه";
        string shaped = PersianText.Shape(sample);
        var cps = new StringBuilder();
        foreach (char c in shaped) cps.Append(((int)c).ToString("X4")).Append(' ');
        sb.AppendLine($"shaped sample len={shaped.Length}");
        sb.AppendLine(cps.ToString());

        var bhoma = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/BHoma.asset");
        if (bhoma == null) sb.AppendLine("BHoma asset not found");
        else
        {
            int glyphCount = bhoma.glyphTable != null ? bhoma.glyphTable.Count : -1;
            var srcF = bhoma.sourceFontFile;
            sb.AppendLine($"BHoma mode={bhoma.atlasPopulationMode} glyphs={glyphCount} srcFont={(srcF != null ? srcF.name : "null")}");
            var missingStatic = Check(bhoma, shaped, false);
            sb.AppendLine(missingStatic.Count == 0 ? "BHoma static: FULL coverage" : $"BHoma static MISSING {missingStatic.Count}");
            var missingTry = Check(bhoma, shaped, true);
            sb.AppendLine(missingTry.Count == 0 ? "BHoma tryAdd: FULL coverage" : $"BHoma tryAdd MISSING {missingTry.Count}");
        }

        TMP_FontAsset chosen = EnsureDynamicFont(sb);
        if (chosen != null)
        {
            var missing = Check(chosen, shaped, true);
            sb.AppendLine(missing.Count == 0 ? "BRoyaBdDyn: FULL coverage" : $"BRoyaBdDyn MISSING {missing.Count}: {List(missing)} (Latin covered via fallback)");
        }

        int fb = TMP_Settings.fallbackFontAssets != null ? TMP_Settings.fallbackFontAssets.Count : -1;
        sb.AppendLine($"TMP global fallbacks: {fb}");
        if (TMP_Settings.fallbackFontAssets != null)
            foreach (var f in TMP_Settings.fallbackFontAssets) sb.AppendLine($"  fallback: {f.name}");

        ApplyVisualTest(chosen, sb);
        File.WriteAllText("Temp/persian_font_test.txt", sb.ToString());
        Debug.Log($"[PersianFontTest] done. chosen={(chosen != null ? chosen.name : "NONE")}");
    }

    private static TMP_FontAsset EnsureDynamicFont(StringBuilder sb)
    {
        const string path = "Assets/UI/Fonts/BRoyaBd SDF Dynamic.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null)
        {
            AddLiberationFallback(existing, sb);
            sb.AppendLine("BRoyaBd dynamic asset already exists");
            return existing;
        }
        var ttf = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/BRoyaBd.ttf");
        if (ttf == null) { sb.AppendLine("BRoyaBd.ttf not found"); return null; }
        var fa = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        fa.name = "BRoyaBd SDF Dynamic";
        AssetDatabase.CreateAsset(fa, path);
        if (fa.atlasTexture != null) AssetDatabase.AddObjectToAsset(fa.atlasTexture, path);
        if (fa.material != null) AssetDatabase.AddObjectToAsset(fa.material, path);
        AddLiberationFallback(fa, sb);
        AssetDatabase.SaveAssets();
        sb.AppendLine("created BRoyaBd dynamic font asset");
        return fa;
    }

    private static void AddLiberationFallback(TMP_FontAsset fa, StringBuilder sb)
    {
        var lib = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (lib == null) { sb.AppendLine("LiberationSans SDF not found for fallback"); return; }
        if (fa.fallbackFontAssetTable == null) fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
        if (!fa.fallbackFontAssetTable.Contains(lib)) fa.fallbackFontAssetTable.Add(lib);
        EditorUtility.SetDirty(fa);
        sb.AppendLine("LiberationSans SDF fallback ready");
    }

    private static List<char> Check(TMP_FontAsset fa, string shaped, bool tryAdd)
    {
        var missing = new List<char>();
        foreach (char c in shaped)
        {
            if (c == ' ' || c == '\n' || c == '\u200C') continue;
            if (!fa.HasCharacter(c, false, tryAdd)) missing.Add(c);
        }
        return missing;
    }

    private static string List(List<char> l)
    {
        var s = new StringBuilder();
        foreach (var c in l) s.Append(((int)c).ToString("X4")).Append(' ');
        return s.ToString();
    }

    private static void ApplyVisualTest(TMP_FontAsset font, StringBuilder sb)
    {
        var mm = GameObject.Find("MainMenu");
        if (mm == null || font == null) { sb.AppendLine("visual test skipped (no font or no MainMenu)"); return; }

        var all = mm.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in all)
        {
            if (t.text == "Tittle")
            {
                Undo.RecordObject(t, "fa test");
                t.font = font;
                t.text = PersianText.Shape("بازی با دوستان");
                t.enableAutoSizing = true; t.fontSizeMin = 20; t.fontSizeMax = 34;
                sb.AppendLine("header title set");
            }
            else if (t.text == "Friends List")
            {
                Undo.RecordObject(t, "fa test");
                t.font = font;
                t.text = PersianText.Shape("لیست دوستان");
                sb.AppendLine("friends list title set");
            }
            else if (t.text != null && t.text.StartsWith("xxx"))
            {
                Undo.RecordObject(t, "fa test");
                t.font = font;
                t.text = PersianText.Shape("بعد از ساخت بازی دعوتنامه در قسمت پیام ها برای دوستان شما قابل مشاهده است.");
                t.alignment = TextAlignmentOptions.Center;
                t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 26;
                sb.AppendLine("bottom text set");
            }
        }
    }
}