using UnityEngine;

/// <summary>
/// Page used by everything that has no behaviour of its own yet (Store, Friends, Messages,
/// Lucky wheel, VIP wheel, Profile). Replace it with a dedicated script when the page needs
/// real logic - it simply exposes the <see cref="MenuPage"/> hooks.
/// </summary>
public class SimpleMenuPage : MenuPage
{
    [Tooltip("Optional: refreshed every time the page is opened.")]
    [SerializeField] private bool logOpen = false;

    public override void OnOpened()
    {
        if (logOpen) Debug.Log($"[SimpleMenuPage] opened '{name}' ({Id})", this);
    }
}
