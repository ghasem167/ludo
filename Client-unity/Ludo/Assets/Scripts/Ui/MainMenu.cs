using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Opens the menu pages, animates the transition between them and keeps the back stack.
///
/// Transition (forward): the new page slides in from the LEFT while the previous page slides
/// out to the RIGHT. Back is the mirror image: the previous page comes back from the RIGHT and
/// the current page leaves to the LEFT.
///
/// A page is never destroyed: the one we leave is hidden, kept alive and pushed on the back
/// stack, so going back restores exactly the same instance (and its state).
/// </summary>
public class MainMenu : MonoBehaviour
{
  
    [Header("Pages")]
    [SerializeField] private MenuPage[] pages;
    [SerializeField] private MenuPageId startPage = MenuPageId.Home;

    [Header("Header")]
    [SerializeField] private GameObject headerRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_FontAsset persianFont;
    [SerializeField] private TMP_FontAsset latinFont;
    [SerializeField] private Button backButton;
    [SerializeField] private UiAudioManager uiAudioManager;

    [Header("Navigation buttons")]
    //[Tooltip("Filled by 'Tools/Setup Main Menu Flow': every button that opens a page. " +
        //    "The listeners are (re)created in Awake so no stale scene listener can interfere.")]
    //[SerializeField] private PageButton[] pageButtons;


    [Header("Transition")]
    [SerializeField, Range(0.05f, 1.5f)] private float transitionDuration = 0.35f;
    [SerializeField, Range(0f, 1f)] private float incomingAlpha = 0.65f;
    [SerializeField, Range(0f, 1f)] private float outgoingAlpha = 0.35f;
    [SerializeField] private bool closeWithEscape = true;

    public readonly Dictionary<MenuPageId, MenuPage> _pagesById = new Dictionary<MenuPageId, MenuPage>();

    /// <summary>Pages we came from; the last entry is the page "Back" returns to.</summary>
    private readonly List<MenuPage> _backStack = new List<MenuPage>();

    private MenuPage _current;
    private bool _transitioning;

    public MenuPage CurrentPage => _current;
    public MenuPageId CurrentPageId => _current != null ? _current.Id : MenuPageId.None;
    public bool IsTransitioning => _transitioning;
    public IReadOnlyList<MenuPage> BackStack => _backStack;

    /// <summary>Raised after a page finished opening (the header/title are already updated).</summary>
    public event Action<MenuPageId> PageOpened;

    private void Awake()
    {
        CachePages();
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackButtonClicked);
        //WireButtons();
    }

    private void Start()
    {
        // the server accepted our match request -> GameManager raises LobbyEntered (see EnterLobbyAsync)
        if (GameManager.Instance != null) GameManager.Instance.LobbyEntered += OnLobbyEntered;

        foreach (var page in _pagesById.Values) HideImmediately(page);

        var start = GetPage(startPage) ?? FirstConfiguredPage();
        if (start == null)
        {
            Debug.LogError("[MainMenu] no page is configured - run 'Tools/Setup Main Menu Flow'");
            return;
        }

        ShowImmediately(start);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.LobbyEntered -= OnLobbyEntered;
    }

    /// <summary>Shows the lobby page as soon as the server accepted the match request.</summary>
    private void OnLobbyEntered()
    {
        OpenPage(MenuPageId.Lobby);
    }

    private void Update()
    {
        if (!closeWithEscape || _transitioning) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Back();
    }

    private void CachePages()
    {
        _pagesById.Clear();
        if (pages == null) return;

        foreach (var page in pages)
        {
            if (page == null) continue;
            if (page.Id == MenuPageId.None)
            {
                Debug.LogWarning($"[MainMenu] page '{page.name}' has no id", page);
                continue;
            }

          
            page.WireButtons();
            _pagesById[page.Id] = page;
        }
    }

    /// <summary>
    /// (Re)creates the onClick listeners from the binding tables. The scene listeners are cleared
    /// first: older versions of the scene drove the pages with SetActive() calls which would fight
    /// this navigator. Add new navigation in the inspector fields, not with a scene listener.
    /// </summary>


   
    private void OnBackButtonClicked()
    {
        Back();
        PlayBack();
    }

    public void PlayClick()
    {
        if (uiAudioManager != null) uiAudioManager.PlayClick();
    }

    public void PlayBack()
    {
        if (uiAudioManager != null) uiAudioManager.PlayBack();
    }

    /// <summary>The header back button plus the extra page local back buttons, without duplicates.</summary>
  

    #region public API (wired to the scene buttons)

    /// <summary>UnityEvent friendly entry point: pass (int)MenuPageId.</summary>
    public void OpenPage(int pageId) => OpenPage((MenuPageId)pageId);

    public void OpenHome() => OpenPage(startPage);

    /// <summary>Home page 'Classic' card: opens the Game page with the classic rules selected.</summary>
   

    /// <summary>
    /// Opens <see cref="MenuPageId.GamePage"/> and remembers <paramref name="mode"/> in the
    /// GameManager context, which is what GameManager.CreateGamePlayHandler() reads when the match
    /// scene is loaded. Usable straight from a UnityEvent (see also the parameterless
    /// <see cref="OpenClassicGame"/> / <see cref="OpenModernGame"/>).
    /// </summary>
   

    /// <summary>
    /// Keeps whether the player picked the classic or the modern rules. The value lives in
    /// <see cref="GameManager.ThisContext"/> so it survives the scene change into the match.
    /// </summary>
   

    public void OpenPage(MenuPageId id)
    {
        if (_transitioning) return;

        var target = GetPage(id);
        if (target == null)
        {
            Debug.LogWarning($"[MainMenu] page '{id}' is not configured", this);
            return;
        }

        if (_current == target) return;

        // Walking back to a page we already visited: drop the pages above it and slide back.
        int stackIndex = _backStack.IndexOf(target);
        if (stackIndex >= 0 || target.Id == startPage)
        {
            if (stackIndex >= 0) _backStack.RemoveRange(stackIndex, _backStack.Count - stackIndex);
            else _backStack.Clear();

            StartCoroutine(TransitionRoutine(target, false));
            return;
        }

        if (_current != null) _backStack.Add(_current);
        StartCoroutine(TransitionRoutine(target, true));
    }

    /// <summary>Returns to the page we came from.</summary>
    public void Back()
    {
        if (_transitioning) return;
        if (_backStack.Count == 0) return;

        var target = _backStack[_backStack.Count - 1];
        _backStack.RemoveAt(_backStack.Count - 1);
        StartCoroutine(TransitionRoutine(target, false));
    }

    #endregion

    #region internals

    private MenuPage GetPage(MenuPageId id)
    {
        MenuPage page;
        return _pagesById.TryGetValue(id, out page) ? page : null;
    }

    /// <summary>
    /// Helper: find the PageButton entry for a given page ID.
    /// </summary>
 
    private MenuPage FirstConfiguredPage()
    {
        foreach (var page in _pagesById.Values) return page;
        return null;
    }

    private void ShowImmediately(MenuPage page)
    {
        page.gameObject.SetActive(true);
        page.Rect.anchoredPosition = page.HomePosition;

        var group = page.CanvasGroup;
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        _current = page;
        ApplyChrome(page);
        page.OnOpened();
        PageOpened?.Invoke(page.Id);
    }

    private void HideImmediately(MenuPage page)
    {
        page.Rect.anchoredPosition = page.HomePosition;

        var group = page.CanvasGroup;
        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;

        page.gameObject.SetActive(false);
    }

    private IEnumerator TransitionRoutine(MenuPage target, bool forward)
    {
        _transitioning = true;

        var from = _current;

        target.gameObject.SetActive(true);
        target.Rect.SetAsLastSibling();

        float width = PageWidth(from != null ? from : target);

        var targetGroup = target.CanvasGroup;
        targetGroup.alpha = incomingAlpha;
        targetGroup.interactable = false;
        targetGroup.blocksRaycasts = false;

        CanvasGroup fromGroup = null;
        if (from != null)
        {
            fromGroup = from.CanvasGroup;
            fromGroup.interactable = false;
            fromGroup.blocksRaycasts = false;
        }

        Vector2 targetHome = target.HomePosition;
        Vector2 targetStart = targetHome + SlideOffset(width, 0f, true, forward);
        target.Rect.anchoredPosition = targetStart;

        Vector2 fromHome = from != null ? from.HomePosition : Vector2.zero;

        float time = 0f;
        while (time < transitionDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / transitionDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            target.Rect.anchoredPosition = targetHome + SlideOffset(width, t, true, forward);
            targetGroup.alpha = Mathf.Lerp(incomingAlpha, 1f, smooth);

            if (from != null)
            {
                from.Rect.anchoredPosition = fromHome + SlideOffset(width, t, false, forward);
                fromGroup.alpha = Mathf.Lerp(1f, outgoingAlpha, smooth);
            }

            yield return null;
        }

        target.Rect.anchoredPosition = targetHome;
        targetGroup.alpha = 1f;
        targetGroup.interactable = true;
        targetGroup.blocksRaycasts = true;

        if (from != null)
        {
            HideImmediately(from);
            from.OnClosed();
        }

        _current = target;
        ApplyChrome(target);
        target.OnOpened();
        PageOpened?.Invoke(target.Id);

        _transitioning = false;
    }

    /// <summary>Width the pages slide over (the page area, not the whole canvas).</summary>
    private float PageWidth(MenuPage reference = null)
    {
        var rect = reference != null
            ? reference.Rect
            : (_current != null ? _current.Rect : (RectTransform)transform);

        float width = rect.rect.width;
        if (width <= 1f) width = ((RectTransform)transform).rect.width;
        return width <= 1f ? Screen.width : width;
    }

    private void ApplyChrome(MenuPage page)
    {
        bool isHome = page.IsRoot || page.Id == startPage;

        if (headerRoot != null) headerRoot.SetActive(!isHome);
        if (backButton != null) backButton.gameObject.SetActive(!isHome && page.CanGoBack);

        if (titleText == null) return;

        titleText.text = page.Title;

        var font = NeedsPersianFont(page.Title) ? persianFont : latinFont;
        if (font != null) titleText.font = font;
    }

#if UNITY_EDITOR
    /// <summary>Editor tooling: applies the header/title of a page without running the transition.</summary>
    public void EditorApplyChrome(MenuPageId id)
    {
        CachePages();
        var page = GetPage(id);
        if (page != null) ApplyChrome(page);
    }

    /// <summary>Editor tooling: hides every page (used before capturing a single page).</summary>
    public void EditorHideAllPages()
    {
        CachePages();
        foreach (var page in _pagesById.Values)
        {
            page.gameObject.SetActive(false);
            page.Rect.anchoredPosition = page.HomePosition;
        }
    }

    /// <summary>Editor tooling: places the two pages exactly where the transition would put them at <paramref name="t"/>.</summary>
    public void EditorPreviewTransition(MenuPageId fromId, MenuPageId toId, float t, bool forward)
    {
        CachePages();

        var target = GetPage(toId);
        var from = GetPage(fromId);

        if (target != null) target.gameObject.SetActive(true);

        float width = PageWidth(target ?? from);

        if (target != null)
        {
            target.gameObject.SetActive(true);
            target.Rect.SetAsLastSibling();
            target.Rect.anchoredPosition = target.HomePosition + SlideOffset(width, t, true, forward);
            target.CanvasGroup.alpha = Mathf.Lerp(incomingAlpha, 1f, Mathf.SmoothStep(0f, 1f, t));
        }

        if (from != null && from != target)
        {
            from.gameObject.SetActive(true);
            from.Rect.anchoredPosition = from.HomePosition + SlideOffset(width, t, false, forward);
            from.CanvasGroup.alpha = Mathf.Lerp(1f, outgoingAlpha, Mathf.SmoothStep(0f, 1f, t));
        }

        if (target != null) ApplyChrome(target);
    }
#endif

    private static bool NeedsPersianFont(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (char c in value) if (c > 0x7F) return true;
        return false;
    }

    /// <summary>
    /// Slide offset of a page during a transition.
    /// The incoming page comes from the left (forward) / right (back) and ends at zero,
    /// the outgoing page leaves to the opposite side. Shared with the editor capture tool.
    /// </summary>
    public static Vector2 SlideOffset(float width, float t, bool incoming, bool forward)
    {
        float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        float fromX = incoming ? (forward ? -width : width) : 0f;
        float toX = incoming ? 0f : (forward ? width : -width);
        return new Vector2(Mathf.Lerp(fromX, toX, smooth), 0f);
    }

    #endregion
}