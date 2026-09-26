using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Fixes the font (and the shaping) of TMP texts in one pass.
///
///   Tools/Persian Fonts/Report (no changes)        - writes Temp/persian_font_report.txt only
///   Tools/Persian Fonts/Fix TMP Fonts (selection)  - the selection, else the whole open scene
///
/// Rules:
///   * Persian/Arabic letters (or already shaped text)  -> BRoyaBd SDF Dynamic
///   * Latin text that still uses the Persian font      -> LiberationSans SDF
///   * Persian strings are shaped with PersianText.Shape, one line at a time, unless they are
///     already shaped (U+FB50..FBFF / U+FE70..FEFF): running the tool twice never re-shapes.
///   * a TMP_InputField text component only gets the font (its string is typed at runtime).
///   * rich text is skipped by default (the tags would break the visual order); switch
///     ShapeRichText on to shape only the text parts of a line.
/// </summary>
public static class FixPersianFonts
{
    private const string PersianFontPath = "Assets/UI/Fonts/BRoyaBd SDF Dynamic.asset";
    private const string LatinFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string ReportPath = "Temp/persian_font_report.txt";

    /// <summary>BRoyaBd is a bold face already, Bold would be applied twice.</summary>
    private const bool ResetFontStyle = true;

    /// <summary>Shape the text parts of a line that contains rich text tags.</summary>
    private const bool ShapeRichText = false;

    /// <summary>Center the Persian texts (off: never touch a hand tuned layout).</summary>
    private const bool CenterPersianText = false;

    private static TMP_FontAsset _fa;
    private static TMP_FontAsset _latin;
    private static StringBuilder _log;
    private static readonly HashSet<UnityEngine.SceneManagement.Scene> _touchedScenes =
        new HashSet<UnityEngine.SceneManagement.Scene>();

    /// <summary>True while <see cref="FixPrefabs"/> edits prefab contents (no scene bookkeeping then).</summary>
    private static bool _prefabMode;

    private class Stats
    {
        public int total, persian, latinReset, shapes, alreadyShaped, richText, inputField, empty, untouched;
    }

    [MenuItem("Tools/Persian Fonts/Report (whole scene, no changes)")]
    public static void Report() => Run(false, true);

    [MenuItem("Tools/Persian Fonts/Fix TMP Fonts (selection or whole scene)")]
    public static void FixSelection() => Run(true, false);

    [MenuItem("Tools/Persian Fonts/Fix TMP Fonts (whole scene)")]
    public static void FixScene() => Run(true, true);

    /// <summary><paramref name="sceneWide"/> ignores the current selection and walks every loaded scene.</summary>
    private static void Run(bool apply, bool sceneWide)
    {
        _log = new StringBuilder();
        _log.AppendLine($"=== FixPersianFonts (apply={apply} sceneWide={sceneWide}) ===");
        _touchedScenes.Clear();
        _prefabMode = false;

        LoadFonts();
        if (_fa == null)
        {
            Debug.LogError($"[FixPersianFonts] '{PersianFontPath}' not found - run Tools/Test Persian Font first");
            WriteLog();
            return;
        }

        var texts = CollectTexts(sceneWide);
        var stats = new Stats { total = texts.Count };

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Fix Persian Fonts");
        int undoGroup = Undo.GetCurrentGroup();

        bool touchedPrefabAsset = false;
        foreach (var tmp in texts)
        {
            if (tmp == null) continue;
            touchedPrefabAsset |= Process(tmp, apply, stats);
        }

        Undo.CollapseUndoOperations(undoGroup);

        if (apply)
        {
            if (touchedPrefabAsset) AssetDatabase.SaveAssets();

            // same convention as SetupMainMenuFlow / FixFriendsPage: leave the scene saved, not dirty
            foreach (var scene in _touchedScenes)
            {
                if (!scene.IsValid() || !scene.isLoaded) continue;
                _log.AppendLine($"  saved '{scene.name}' = {EditorSceneManager.SaveScene(scene)}");
            }
        }

        _log.AppendLine($"texts={stats.total} persian={stats.persian} shaped={stats.shapes} " +
                        $"alreadyShaped={stats.alreadyShaped} richText={stats.richText} " +
                        $"latinReset={stats.latinReset} inputField={stats.inputField} " +
                        $"empty={stats.empty} untouched={stats.untouched}");
        WriteLog();

        Debug.Log($"[FixPersianFonts] {(apply ? "fix" : "report")} - persian={stats.persian} " +
                  $"shaped={stats.shapes} latinReset={stats.latinReset} untouched={stats.untouched} -> {ReportPath}");
    }

    [MenuItem("Tools/Persian Fonts/Fix TMP Fonts (all prefabs under Assets/UI)")]
    public static void FixPrefabs()
    {
        const string root = "Assets/UI";

        _log = new StringBuilder();
        _log.AppendLine($"=== FixPersianFonts (prefabs under {root}) ===");
        _touchedScenes.Clear();
        _prefabMode = true;

        LoadFonts();
        if (_fa == null)
        {
            Debug.LogError($"[FixPersianFonts] '{PersianFontPath}' not found - run Tools/Test Persian Font first");
            WriteLog();
            _prefabMode = false;
            return;
        }

        var stats = new Stats();
        int scanned = 0, changed = 0;

        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { root }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var contents = PrefabUtility.LoadPrefabContents(path);
            if (contents == null) continue;

            scanned++;
            _log.AppendLine($"prefab '{path}'");

            bool dirty = false;
            foreach (var tmp in contents.GetComponentsInChildren<TMP_Text>(true))
            {
                stats.total++;
                dirty |= Process(tmp, true, stats);
            }

            if (dirty)
            {
                changed++;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                _log.AppendLine($"  SAVED '{path}'");
            }

            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        _prefabMode = false;

        _log.AppendLine($"prefabs scanned={scanned} changed={changed}");
        _log.AppendLine($"texts={stats.total} persian={stats.persian} shaped={stats.shapes} " +
                        $"alreadyShaped={stats.alreadyShaped} richText={stats.richText} " +
                        $"latinReset={stats.latinReset} inputField={stats.inputField} " +
                        $"empty={stats.empty} untouched={stats.untouched}");
        WriteLog();

        Debug.Log($"[FixPersianFonts] prefabs scanned={scanned} changed={changed} texts={stats.total} " +
                  $"persian={stats.persian} shaped={stats.shapes} latinReset={stats.latinReset} -> {ReportPath}");
    }

    /// <summary>Returns true when a prefab asset (not a scene object) was modified.</summary>
    private static bool Process(TMP_Text tmp, bool apply, Stats stats)
    {
        string text = tmp.text ?? string.Empty;
        string path = PathOf(tmp);
        bool persian = HasArabicScript(text);
        bool shaped = IsShaped(text);
        bool prefabAsset = false;

        // --- input fields: the string is typed at runtime, only the font can be fixed here ---
        var input = tmp.GetComponentInParent<TMP_InputField>();
        if (input != null && input.textComponent == tmp)
        {
            stats.inputField++;
            if (persian && apply && tmp.font != _fa) { Record(tmp, out prefabAsset); tmp.font = _fa; }
            _log.AppendLine($"  [input]      {path} font={(tmp.font != null ? tmp.font.name : "null")} '{Preview(text)}'");
            return prefabAsset;
        }

        // --- Latin text that kept the Persian font from an earlier pass ---
        if (!persian)
        {
            // an empty string is normally filled at runtime (page messages, timers, scores):
            // its font is left alone so the runtime Persian text keeps a font that can render it
            if (string.IsNullOrEmpty(text))
            {
                stats.empty++;
                _log.AppendLine($"  [empty]      {path} font={(tmp.font != null ? tmp.font.name : "null")} (left alone)");
            }
            else if (tmp.font == _fa && _latin != null)
            {
                stats.latinReset++;
                if (apply) { Record(tmp, out prefabAsset); tmp.font = _latin; }
                _log.AppendLine($"  [latin]      {path} -> {_latin.name} '{Preview(text)}'");
            }
            else stats.untouched++;

            return prefabAsset;
        }

        stats.persian++;

        bool rich = HasRichText(text);
        string newText = text;

        if (shaped)
        {
            stats.alreadyShaped++;
            _log.AppendLine($"  [shaped ok]  {path} '{Preview(text)}'");
        }
        else if (rich && !ShapeRichText)
        {
            stats.richText++;
            _log.AppendLine($"  [rich skip]  {path} (font only, shaping skipped) '{Preview(text)}'");
        }
        else
        {
            newText = ShapeText(text, rich);
            stats.shapes++;
            _log.AppendLine($"  [shaped]     {path} '{Preview(text)}' -> '{Preview(newText)}'");
        }

        if (HasPersianDigits(text))
            _log.AppendLine($"  [digits]     {path} Persian/Arabic digits inside the run - verify the visual order");

        bool fontOk = tmp.font == _fa;
        bool styleOk = !ResetFontStyle || tmp.fontStyle == FontStyles.Normal;
        bool alignOk = !CenterPersianText || tmp.alignment == TextAlignmentOptions.Center;
        bool textOk = shaped || (rich && !ShapeRichText);

        if (!fontOk)
            _log.AppendLine($"  [font]       {path} {(tmp.font != null ? tmp.font.name : "null")} -> {_fa.name}");
        if (!styleOk)
            _log.AppendLine($"  [style]      {path} {tmp.fontStyle} -> Normal");
        if (!alignOk)
            _log.AppendLine($"  [align]      {path} {tmp.alignment} -> Center");

        if (fontOk && styleOk && alignOk && textOk)
        {
            stats.untouched++;
            return false;
        }

        if (!apply) return false;

        Record(tmp, out prefabAsset);

        if (!fontOk) tmp.font = _fa;
        if (ResetFontStyle && !styleOk) tmp.fontStyle = FontStyles.Normal;
        if (CenterPersianText && !alignOk) tmp.alignment = TextAlignmentOptions.Center;
        if (!textOk && newText != text) tmp.text = newText;

        return prefabAsset;
    }

    // ---------- text tools ----------

    /// <summary>Shapes every line on its own: word wrap must never split a pre-shaped run.</summary>
    private static string ShapeText(string text, bool rich)
    {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var sb = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(rich ? ShapeSegments(lines[i]) : PersianText.Shape(lines[i]));
        }

        return sb.ToString();
    }

    /// <summary>Shapes the text parts of a line and leaves every "&lt;tag&gt;" untouched.</summary>
    private static string ShapeSegments(string line)
    {
        var sb = new StringBuilder();
        int i = 0;

        while (i < line.Length)
        {
            int open = line.IndexOf('<', i);
            if (open < 0) { sb.Append(PersianText.Shape(line.Substring(i))); break; }
            if (open > i) sb.Append(PersianText.Shape(line.Substring(i, open - i)));

            int close = line.IndexOf('>', open);
            if (close < 0) { sb.Append(line.Substring(open)); break; }

            sb.Append(line.Substring(open, close - open + 1));
            i = close + 1;
        }

        return sb.ToString();
    }

    private static bool HasArabicScript(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (char c in value)
            if ((c >= '\u0600' && c <= '\u06FF') || (c >= '\u0750' && c <= '\u077F') ||
                (c >= '\u08A0' && c <= '\u08FF') || (c >= '\uFB50' && c <= '\uFDFF') ||
                (c >= '\uFE70' && c <= '\uFEFF')) return true;

        return false;
    }

    /// <summary>True when the string already contains contextual presentation forms.</summary>
    private static bool IsShaped(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (char c in value)
            if ((c >= '\uFB50' && c <= '\uFBFF') || (c >= '\uFE70' && c <= '\uFEFF')) return true;

        return false;
    }

    private static bool HasRichText(string value) =>
        !string.IsNullOrEmpty(value) && value.IndexOf('<') >= 0 && value.IndexOf('>') > value.IndexOf('<');

    private static bool HasPersianDigits(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (char c in value)
            if ((c >= '\u0660' && c <= '\u0669') || (c >= '\u06F0' && c <= '\u06F9')) return true;

        return false;
    }

    // ---------- plumbing ----------

    private static void LoadFonts()
    {
        _fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PersianFontPath);
        _latin = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LatinFontPath);
        if (_latin == null) _latin = TMP_Settings.defaultFontAsset;

        if (_fa != null)
        {
            if (_fa.fallbackFontAssetTable == null) _fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (_latin != null && !_fa.fallbackFontAssetTable.Contains(_latin))
            {
                _fa.fallbackFontAssetTable.Add(_latin);
                EditorUtility.SetDirty(_fa);
            }
        }

        _log.AppendLine($"fonts: persian={(_fa != null ? _fa.name : "null")} latin={(_latin != null ? _latin.name : "null")}");
    }

    private static List<TMP_Text> CollectTexts(bool sceneWide)
    {
        var result = new List<TMP_Text>();
        var seen = new HashSet<TMP_Text>();

        void Add(IEnumerable<TMP_Text> list)
        {
            foreach (var text in list)
                if (text != null && seen.Add(text)) result.Add(text);
        }

        if (!sceneWide && Selection.gameObjects != null && Selection.gameObjects.Length > 0)
        {
            _log.AppendLine($"scope: selection ({Selection.gameObjects.Length} root object(s))");
            foreach (var go in Selection.gameObjects) Add(go.GetComponentsInChildren<TMP_Text>(true));
            return result;
        }

        _log.AppendLine("scope: every loaded scene");

        // Scene.GetRootGameObjects walks scene objects only: prefab assets / other assets are skipped
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            int before = result.Count;
            foreach (var root in scene.GetRootGameObjects()) Add(root.GetComponentsInChildren<TMP_Text>(true));
            _log.AppendLine($"  scene '{scene.name}' roots={scene.rootCount} texts={result.Count - before}");
        }

        return result;
    }

    private static void Record(Object target, out bool isPrefabAsset)
    {
        isPrefabAsset = false;

        // prefab contents (FixPrefabs): dirty the object, the caller saves the prefab asset
        if (_prefabMode)
        {
            isPrefabAsset = true;
            EditorUtility.SetDirty(target);
            return;
        }

        Undo.RecordObject(target, "Fix Persian Fonts");

        if (PrefabUtility.IsPartOfPrefabInstance(target))
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        else if (PrefabUtility.IsPartOfPrefabAsset(target))
        {
            isPrefabAsset = true;
            EditorUtility.SetDirty(target);
        }

        var component = target as Component;
        var go = target as GameObject ?? (component != null ? component.gameObject : null);
        if (go == null || !go.scene.IsValid()) return;

        EditorSceneManager.MarkSceneDirty(go.scene);
        _touchedScenes.Add(go.scene);
    }

    private static string PathOf(Component component)
    {
        var sb = new StringBuilder(component.gameObject.name);
        var parent = component.transform.parent;
        while (parent != null) { sb.Insert(0, parent.name + "/"); parent = parent.parent; }
        return sb.ToString();
    }

    private static string Preview(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        string one = value.Replace("\r", string.Empty).Replace("\n", "\\n");
        return one.Length <= 48 ? one : one.Substring(0, 45) + "...";
    }

    private static void WriteLog() => File.WriteAllText(ReportPath, _log.ToString());
}
