using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps a menu page usable on any viewport shape.
/// The page is expected to be a stretched RectTransform driven by a VerticalLayoutGroup whose
/// children carry LayoutElement min/preferred heights. When the viewport is tall enough nothing
/// changes (factor = 1). When it is too short, every cached min/preferred height and every layout
/// padding/spacing is scaled by the same factor so the whole design compresses uniformly instead
/// of overflowing the screen. Fonts are all auto-sizing, so labels shrink with their boxes.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ResponsivePageFitter : MonoBehaviour
{
    /// <summary>Never compress below this factor; below that the page is simply too short.</summary>
    [SerializeField, Range(0.4f, 1f)] private float minScale = 0.65f;

    private struct GroupBase
    {
        public HorizontalOrVerticalLayoutGroup group;
        public int left, right, top, bottom;
        public float spacing;
    }

    private struct ElementBase
    {
        public LayoutElement element;
        public float minHeight;
        public float preferredHeight;
    }

    private readonly List<GroupBase> _groups = new List<GroupBase>();
    private readonly List<ElementBase> _elements = new List<ElementBase>();
    private readonly Dictionary<LayoutElement, ElementBase> _byElement = new Dictionary<LayoutElement, ElementBase>();

    private VerticalLayoutGroup _selfLayout;
    private int _selfPadTop, _selfPadBottom, _selfPadLeft, _selfPadRight;
    private float _selfSpacing;

    private bool _cached;
    private int _cachedChildCount = -1;
    private float _applied = -1f;

    private void OnEnable()
    {
        Cache();
        if (!Application.isPlaying) return; // never rewrite the authored values in edit mode
        Apply(ComputeFactor());
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying || !isActiveAndEnabled) return;
        Apply(ComputeFactor());
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !isActiveAndEnabled) return;
        if (!_cached || transform.childCount != _cachedChildCount) Cache();
        Apply(ComputeFactor());
    }

    /// <summary>
    /// Re-caches the authored (factor = 1) values and re-applies them right away.
    /// Editor tools may call this in edit mode (that is the only path allowed to touch serialized values there).
    /// </summary>
    public void Refresh()
    {
        Cache();
        Apply(1f);
        Apply(ComputeFactor());
    }

    private void Cache()
    {
        _groups.Clear();
        _elements.Clear();
        _byElement.Clear();

        foreach (var g in GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
        {
            var pad = g.padding;
            _groups.Add(new GroupBase
            {
                group = g,
                left = pad.left,
                right = pad.right,
                top = pad.top,
                bottom = pad.bottom,
                spacing = g.spacing
            });
        }

        foreach (var e in GetComponentsInChildren<LayoutElement>(true))
        {
            var b = new ElementBase
            {
                element = e,
                minHeight = e.minHeight,
                preferredHeight = e.preferredHeight
            };
            _elements.Add(b);
            _byElement[e] = b;
        }

        _selfLayout = GetComponent<VerticalLayoutGroup>();
        if (_selfLayout != null)
        {
            var p = _selfLayout.padding;
            _selfPadLeft = p.left; _selfPadRight = p.right;
            _selfPadTop = p.top; _selfPadBottom = p.bottom;
            _selfSpacing = _selfLayout.spacing;
        }

        // base values were just re-read: force the next Apply() to rewrite every value
        _applied = -1f;
        _cached = true;
        _cachedChildCount = transform.childCount;
    }

    private float ComputeFactor()
    {
        var rect = (RectTransform)transform;
        float available = rect.rect.height;
        float required = RequiredHeight();
        if (available <= 1f || required <= 1f) return 1f;
        return Mathf.Clamp(available / required, minScale, 1f);
    }

    /// <summary>Authored minimum height: section minimums + own padding + spacing.</summary>
    private float RequiredHeight()
    {
        float sum = 0f;
        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            float h = 0f;
            var le = child.GetComponent<LayoutElement>();
            if (le != null)
            {
                ElementBase b;
                if (_byElement.TryGetValue(le, out b) && b.minHeight > 0f) h = b.minHeight;
                else if (le.minHeight > 0f) h = le.minHeight;
            }
            if (h <= 0f) h = LayoutUtility.GetMinHeight((RectTransform)child);

            sum += h;
            count++;
        }

        sum += _selfPadTop + _selfPadBottom;
        if (count > 1) sum += _selfSpacing * (count - 1);
        return sum;
    }

    private void Apply(float factor)
    {
        if (!_cached) Cache();
        factor = Mathf.Clamp(factor, minScale, 1f);
        if (Mathf.Abs(factor - _applied) < 0.0001f) return;
        _applied = factor;

        foreach (var gb in _groups)
        {
            if (gb.group == null) continue;
            gb.group.padding = new RectOffset(
                Mathf.RoundToInt(gb.left * factor),
                Mathf.RoundToInt(gb.right * factor),
                Mathf.RoundToInt(gb.top * factor),
                Mathf.RoundToInt(gb.bottom * factor));
            gb.group.spacing = gb.spacing * factor;
        }

        foreach (var eb in _elements)
        {
            if (eb.element == null) continue;
            eb.element.minHeight = eb.minHeight > 0f ? eb.minHeight * factor : eb.minHeight;
            eb.element.preferredHeight = eb.preferredHeight > 0f ? eb.preferredHeight * factor : eb.preferredHeight;
        }

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }
}
