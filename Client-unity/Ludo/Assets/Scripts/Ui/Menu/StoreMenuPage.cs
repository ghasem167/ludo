using UnityEngine;

/// <summary>
/// Store page: the carts are built once by <see cref="Store"/>; every time the page is opened the
/// owned/selected state is refreshed.
/// </summary>
public class StoreMenuPage : MenuPage
{
    [SerializeField] private Store store;

    protected override void OnPageAwake()
    {
        if (store == null) store = GetComponent<Store>();
    }

    public override void OnOpened()
    {
        if (store != null) store.RefreshStore();
    }
}
