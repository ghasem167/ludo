using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for every menu page. Each page owns its own behaviour (see the concrete subclasses)
/// and is responsible for opening other pages. <see cref=""MainMenu""/> is responsible for the
/// slide transition and the back stack only.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuPage : MonoBehaviour
{
    [Serializable]
    public class PageButton
    {
        public Button button;
        public MenuPageId page;
    }

    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private MenuPageId id = MenuPageId.None;

    /// <summary>Shown in the shared header while the page is open.</summary>
    [SerializeField] private string title = string.Empty;

    /// <summary>Root pages are never pushed on the back stack (Home).</summary>
    [SerializeField] private bool root = false;

    [Tooltip("Filled by 'Tools/Setup Main Menu Flow': every button that opens a page. " +
             "The listeners are (re)created in Awake so no stale scene listener can interfere.")]
    [SerializeField] private PageButton[] pageButtons;


    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private MainMenu _mainMenuCached;
    private Vector2 _homePosition;
    private bool _homeCaptured;

    /// <summary>Reference to the main menu, lazily resolved.</summary>
    private MainMenu MainMenuRef
    {
        get
        {
            if (_mainMenuCached == null) _mainMenuCached = mainMenu ?? GetComponent<MainMenu>();
            return _mainMenuCached;
        }
    }

    public MenuPageId Id => id;
    public string Title => title;
    public bool IsRoot => root;

    public RectTransform Rect
    {
        get
        {
            if (_rect == null) _rect = (RectTransform)transform;
            return _rect;
        }
    }

    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }

    public Vector2 HomePosition
    {
        get
        {
            if (!_homeCaptured)
            {
                _homePosition = Rect.anchoredPosition;
                _homeCaptured = true;
            }
            return _homePosition;
        }
    }

    public virtual bool CanGoBack => !root;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _homePosition = _rect.anchoredPosition;
        _homeCaptured = true;
        OnPageAwake();
    }

    public void WireButtons()
    {
        if (pageButtons != null)
        {
            foreach (var binding in pageButtons)
            {
                if (binding == null || binding.button == null) continue;
                var source = binding;
                binding.button.onClick.RemoveAllListeners();
                binding.button.onClick.AddListener(() => OnPageButtonClicked(source));
            }
        }
    }

    /// <summary>Opens the target page. Each page owns this logic.</summary>
    public void OpenPage(MenuPageId targetPageId)
    {
        if (mainMenu._pagesById == null || targetPageId == MenuPageId.None) return;

        var target = mainMenu._pagesById.TryGetValue(targetPageId, out var page) ? page : null;
        if (target == null)
        {
            //Debug.LogWarning($""[MenuPage] page '{targetPageId}' not found"", this);
            return;
        }

        MainMenuRef.PlayClick();
        MainMenuRef.OpenPage(targetPageId);
    }

    private void OnPageButtonClicked(PageButton binding)
    {
        if (binding == null) return;
        OpenPage(binding.page);
    }

    /// <summary>Called once, before any open/close callback. Use it to wire the page's own buttons.</summary>
    protected virtual void OnPageAwake() { }

    /// <summary>Called when the page finished sliding in (or was shown instantly).</summary>
    public virtual void OnOpened() { }

    /// <summary>Called right after the page was closed (slide-out complete).</summary>
    public virtual void OnClosed() { }

    public void SetTitle(string value) => title = value;
}
