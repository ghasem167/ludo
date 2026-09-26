using System.Collections.Generic;
using System.Text;

/// <summary>
/// Editor-time Persian/Arabic text shaper.
/// The project's TextMeshPro build has no RTL support, so Persian strings are
/// pre-shaped: every letter is converted to its contextual presentation form
/// (U+FB50..FBFF / U+FE70..FEFF) and the RTL segments are reversed so TMP can
/// render them left-to-right and they READ correctly right-to-left.
/// Mixed strings (Latin words / western digits) are handled with a simple
/// bidi run split: LTR runs are kept intact and re-ordered as a block.
/// Usage in editor scripts: label.text = PersianText.Shape("بازی با دوستان");
/// </summary>
public static class PersianText
{
    private const char Zwnj = '\u200C';

    private struct Forms { public char Iso, Fin, Ini, Med; public bool Dual; }

    private static readonly Dictionary<char, Forms> Map = new Dictionary<char, Forms>();
    private static bool _init;

    private static void Add(char c, char iso, char fin, char ini, char med, bool dual)
    {
        Map[c] = new Forms { Iso = iso, Fin = fin, Ini = ini, Med = med, Dual = dual };
    }

    private static void Init()
    {
        if (_init) return;
        _init = true;
        Add('\u0621', '\uFE80', '\0', '\0', '\0', false); // ء
        Add('\u0622', '\uFE81', '\uFE82', '\0', '\0', false); // آ
        Add('\u0623', '\uFE83', '\uFE84', '\0', '\0', false); // أ
        Add('\u0624', '\uFE85', '\uFE86', '\0', '\0', false); // ؤ
        Add('\u0625', '\uFE87', '\uFE88', '\0', '\0', false); // إ
        Add('\u0626', '\uFE89', '\uFE8A', '\uFE8B', '\uFE8C', true); // ئ
        Add('\u0627', '\uFE8D', '\uFE8E', '\0', '\0', false); // ا
        Add('\u0628', '\uFE8F', '\uFE90', '\uFE91', '\uFE92', true); // ب
        Add('\u0629', '\uFE93', '\uFE94', '\0', '\0', false); // ة
        Add('\u062A', '\uFE95', '\uFE96', '\uFE97', '\uFE98', true); // ت
        Add('\u062B', '\uFE99', '\uFE9A', '\uFE9B', '\uFE9C', true); // ث
        Add('\u062C', '\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0', true); // ج
        Add('\u062D', '\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4', true); // ح
        Add('\u062E', '\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8', true); // خ
        Add('\u062F', '\uFEA9', '\uFEAA', '\0', '\0', false); // د
        Add('\u0630', '\uFEAB', '\uFEAC', '\0', '\0', false); // ذ
        Add('\u0631', '\uFEAD', '\uFEAE', '\0', '\0', false); // ر
        Add('\u0632', '\uFEAF', '\uFEB0', '\0', '\0', false); // ز
        Add('\u0633', '\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4', true); // س
        Add('\u0634', '\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8', true); // ش
        Add('\u0635', '\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC', true); // ص
        Add('\u0636', '\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0', true); // ض
        Add('\u0637', '\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4', true); // ط
        Add('\u0638', '\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8', true); // ظ
        Add('\u0639', '\uFEC9', '\uFECA', '\uFECB', '\uFECC', true); // ع
        Add('\u063A', '\uFECD', '\uFECE', '\uFECF', '\uFED0', true); // غ
        Add('\u0641', '\uFED1', '\uFED2', '\uFED3', '\uFED4', true); // ف
        Add('\u0642', '\uFED5', '\uFED6', '\uFED7', '\uFED8', true); // ق
        Add('\u0643', '\uFED9', '\uFEDA', '\uFEDB', '\uFEDC', true); // ك (arabic kaf)
        Add('\u06A9', '\uFB8E', '\uFB8F', '\uFB90', '\uFB91', true); // ک (persian keheh)
        Add('\u0644', '\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0', true); // ل
        Add('\u0645', '\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4', true); // م
        Add('\u0646', '\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8', true); // ن
        Add('\u0648', '\uFEED', '\uFEEE', '\0', '\0', false); // و
        Add('\u0647', '\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC', true); // ه
        Add('\u0649', '\uFEEF', '\uFEF0', '\0', '\0', false); // ى
        Add('\u064A', '\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4', true); // ي (arabic yeh)
        Add('\u06CC', '\uFBFC', '\uFBFD', '\uFBFE', '\uFBFF', true); // ی (persian yeh)
        Add('\u06AF', '\uFB92', '\uFB93', '\uFB94', '\uFB95', true); // گ
        Add('\u0686', '\uFB7A', '\uFB7B', '\uFB7C', '\uFB7D', true); // چ
        Add('\u067E', '\uFB56', '\uFB57', '\uFB58', '\uFB59', true); // پ
        Add('\u0698', '\uFB8A', '\uFB8B', '\0', '\0', false); // ژ
    }

    private static bool CanJoinNext(char c)
    {
        Forms f;
        return Map.TryGetValue(c, out f) && f.Ini != '\0'; // dual-joining letters have an initial form
    }

    private static bool CanJoinPrev(char c)
    {
        Forms f;
        return Map.TryGetValue(c, out f) && f.Fin != '\0'; // everything except hamza takes a final form
    }

    private static bool IsLtr(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
    }

    /// <summary>Shapes logical Persian text into render-ready pre-shaped, reversed text.</summary>
    public static string Shape(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        Init();

        // --- tokenize into bidi runs ---
        var runs = new List<StringBuilder>();
        var runRtl = new List<bool>();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool rtl;
            if (Map.ContainsKey(c) || c == Zwnj) rtl = true;
            else if (IsLtr(c)) rtl = false;
            else rtl = runs.Count == 0 || runRtl[runRtl.Count - 1]; // neutral: stick to current run (default RTL)
            if (runs.Count == 0 || rtl != runRtl[runRtl.Count - 1]) { runs.Add(new StringBuilder()); runRtl.Add(rtl); }
            runs[runs.Count - 1].Append(c);
        }

        // --- visual order: reverse run order, reverse chars inside RTL runs ---
        var outSb = new StringBuilder();
        for (int r = runs.Count - 1; r >= 0; r--)
        {
            if (runRtl[r]) outSb.Append(ShapeRtlRun(runs[r]));
            else outSb.Append(runs[r]);
        }
        return outSb.ToString();
    }

    private static string ShapeRtlRun(StringBuilder run)
    {
        var src = run.ToString();
        var outc = new List<char>();
        char prevLetter = '\0';
        bool broke = true; // joining disabled until a letter is processed
        for (int i = 0; i < src.Length; i++)
        {
            char c = src[i];
            if (c == Zwnj) { broke = true; continue; } // non-joiner: break joining, drop from output
            Forms f;
            if (Map.TryGetValue(c, out f))
            {
                bool jp = !broke && CanJoinNext(prevLetter) && f.Fin != '\0';
                bool jn = f.Ini != '\0' && CanConnectForward(src, i + 1);
                char o;
                if (jp && jn && f.Med != '\0') o = f.Med;
                else if (jp) o = f.Fin;
                else if (jn) o = f.Ini;
                else o = f.Iso;
                if (o == '\0') o = f.Iso;
                outc.Add(o);
                prevLetter = c;
                broke = false;
            }
            else
            {
                outc.Add(c); // spaces / neutral punctuation stay
                prevLetter = '\0';
                broke = true;
            }
        }
        var rev = new char[outc.Count];
        for (int i = 0; i < outc.Count; i++) rev[outc.Count - 1 - i] = outc[i];
        return new string(rev);
    }

    private static bool CanConnectForward(string src, int j)
    {
        while (j < src.Length)
        {
            char c = src[j];
            if (c == Zwnj) return false;
            Forms f;
            if (Map.TryGetValue(c, out f)) return f.Fin != '\0';
            return false; // space/neutral breaks the connection
        }
        return false;
    }
}