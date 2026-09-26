using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lobby page: shows the players the server put in the lobby (one row per player, at most 4) while
/// the countdown to the match runs. The roster lives in <see cref="LobbyState"/>
/// (GameManager.Lobby), so the page can be opened before OR after the messages arrived - it only
/// reads the cache (see LobbyPlayerSlot for the slot visuals).
/// </summary>
public class LobbyMenuPage : MenuPage
{
    [Tooltip("The 4 slots of the lobby grid (filled by 'Tools/Setup Lobby Page').")]
    [SerializeField] private LobbyPlayerSlot[] slots;

    [Tooltip("Message shown while the lobby is not full yet ('waitingtext' in the scene).")]
    [SerializeField] private TMP_Text waitingText;

    [Tooltip("Optional countdown text ('timer' in the scene).")]
    [SerializeField] private TMP_Text timerText;

    [Tooltip("Optional button that leaves the lobby and goes back to the territory page.")]
    [SerializeField] private Button cancelButton;

    [Tooltip("The server does not send its remaining lobby time yet, so the countdown is approximate.")]
    [SerializeField] private bool showTimer = false;

    [SerializeField] private float countdownSeconds = 20f;
    [SerializeField] private float fullLobbyCountdownSeconds = 3f;

    [Tooltip("Page opened when the player cancels the lobby.")]
    [SerializeField] private MenuPageId cancelPage = MenuPageId.GamePage;

    private LobbyState _lobby;
    private float _remaining;
    private bool _subscribed;

    private static LobbyState Lobby
    {
        get
        {
            var manager = GameManager.Instance;
            return manager != null ? manager.Lobby : null;
        }
    }

    private static string LocalUserId
    {
        get
        {
            var manager = GameManager.Instance;
            return manager != null && manager.ThisContext != null ? manager.ThisContext.userId : null;
        }
    }

    protected override void OnPageAwake()
    {
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (timerText != null) timerText.gameObject.SetActive(false);
        Refresh();
    }

    public override void OnOpened()
    {
        Subscribe();
        RestartCountdown();
        Refresh();
    }

    public override void OnClosed()
    {
        Unsubscribe();

        // the page is closed when the player backs out of the lobby: leave the match on the server
        // (when the match starts the page is not closed, the whole menu scene is unloaded instead)
        var manager = GameManager.Instance;
        if (manager != null) _ = manager.LeaveLobbyAsync();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (!showTimer || timerText == null || !timerText.gameObject.activeSelf) return;
        if (_remaining <= 0f) return;

        _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
        timerText.text = Mathf.CeilToInt(_remaining).ToString();
    }

    private void Subscribe()
    {
        if (_subscribed) return;

        var lobby = Lobby;
        if (lobby == null) return;

        _lobby = lobby;
        _lobby.PlayersChanged += Refresh;
        _lobby.PlayerJoined += OnPlayerJoined;
        _lobby.LobbyStarted += OnLobbyStarted;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _lobby == null) return;

        _lobby.PlayersChanged -= Refresh;
        _lobby.PlayerJoined -= OnPlayerJoined;
        _lobby.LobbyStarted -= OnLobbyStarted;
        _subscribed = false;
    }

    private void OnPlayerJoined(PlayerDto player)
    {
        Debug.Log($"[LobbyMenuPage] player joined the lobby: {(player != null ? player.DisplayName : "-")}");
        Refresh();
    }

    private void OnLobbyStarted()
    {
        RestartCountdown();
        Refresh();
    }

    /// <summary>Fills the 4 slots from the cached roster: safe to call at any time.</summary>
    public void Refresh()
    {
        var lobby = Lobby;
        var players = lobby != null ? lobby.Players : null;
        int count = players != null ? players.Count : 0;
        string localId = LocalUserId;

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].Clear();

            // the slots are the four pieces (Blue/Red/Yellow/Green): a player is shown in the slot
            // of its own color, extra players take the first free slot
            var used = new bool[slots.Length];

            for (int i = 0; i < count; i++)
            {
                var player = players[i];
                if (player == null) continue;

                int index = (int)player.Color;
                if (index < 0 || index >= slots.Length || used[index])
                {
                    index = -1;
                    for (int s = 0; s < slots.Length; s++)
                        if (!used[s]) { index = s; break; }
                }

                if (index < 0) break;   // no free slot left

                used[index] = true;

                var slot = slots[index];
                if (slot == null) continue;

                slot.Bind(player, !string.IsNullOrEmpty(localId) && player.Player.Id == localId);
            }
        }

        if (waitingText != null) waitingText.gameObject.SetActive(count < LobbyState.MaxPlayers);

        if (timerText != null) timerText.gameObject.SetActive(showTimer);
        if (showTimer && _remaining <= 0f) RestartCountdown();
    }

    private void RestartCountdown()
    {
        var lobby = Lobby;
        bool full = lobby != null && lobby.IsFull;

        _remaining = full ? fullLobbyCountdownSeconds : countdownSeconds;

        if (timerText != null && showTimer)
            timerText.text = Mathf.CeilToInt(_remaining).ToString();
    }

    private void OnCancelClicked()
    {
        var manager = GameManager.Instance;
        if (manager != null) _ = manager.LeaveLobbyAsync();

        OpenPage(cancelPage);
    }
}