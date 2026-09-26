using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Home page (the hub). It only fills the player data it owns; opening other pages is MainMenu's job.
/// </summary>
public class HomeMenuPage : MenuPage
{
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Image xpFill;

    protected override void OnPageAwake()
    {
        // nothing to wire yet: the page's big buttons are scene art, the nav bar is rewired by
        // 'Tools/Setup Main Menu Flow' -> MainMenu.OpenPage(...)
    }

    public override void OnOpened()
    {
        Refresh();
    }

    public void Refresh()
    {
        var manager = GameManager.Instance;
        if (manager == null || manager.ThisContext == null) return;

        var context = manager.ThisContext;

        if (userNameText != null && !string.IsNullOrEmpty(context.userName))
            userNameText.text = context.userName;

        if (xpText != null)
            xpText.text = xpText.text; // XP comes from the server payload later on
    }

    public void SetXp(int current, int max)
    {
        if (xpText != null) xpText.text = $"{current} / {max} XP";
        if (xpFill != null) xpFill.fillAmount = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
    }
    public void StoreClassicGameMode()
    {
        var manager = GameManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[MainMenu] game mode '{GameMode.Classic}' was not stored: no GameManager in the scene", this);
            return;
        }

        if (manager.ThisContext == null) manager.ThisContext = new PlayerContext();

        manager.ThisContext.gameMode = GameMode.Classic;
        Debug.Log($"[MainMenu] game mode stored: {GameMode.Classic}");
    }
    public void StoreModernGameMode()
    {
        var manager = GameManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[MainMenu] game mode '{GameMode.Modern}' was not stored: no GameManager in the scene", this);
            return;
        }

        if (manager.ThisContext == null) manager.ThisContext = new PlayerContext();

        manager.ThisContext.gameMode = GameMode.Modern;
        Debug.Log($"[MainMenu] game mode stored: {GameMode.Modern}");
    }
}
