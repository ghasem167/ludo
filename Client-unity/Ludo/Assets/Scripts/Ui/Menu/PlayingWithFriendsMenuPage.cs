using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Playing with friends" page: builds the match with the invited friends.
/// The invite rows are still placeholders, only the create-game button is wired.
/// </summary>
public class PlayingWithFriendsMenuPage : MenuPage
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button[] inviteButtons;

    protected override void OnPageAwake()
    {
        if (createGameButton != null)
        {
            createGameButton.onClick.RemoveAllListeners();
            createGameButton.onClick.AddListener(OnCreateGameClicked);
        }

        if (inviteButtons != null)
        {
            for (int i = 0; i < inviteButtons.Length; i++)
            {
                if (inviteButtons[i] == null) continue;
                int index = i;
                inviteButtons[i].onClick.RemoveAllListeners();
                inviteButtons[i].onClick.AddListener(() => OnInviteClicked(index));
            }
        }
    }

    public void OnCreateGameClicked()
    {
        var manager = GameManager.Instance;
        if (manager == null || manager.ThisContext == null) return;

        // a friends match is always an online match
        manager.ThisContext.playMode = PlayMode.Online;
        manager.ThisContext.gameMode = GameMode.Classic;

        Debug.Log("[PlayingWithFriendsMenuPage] create game -> lobby");
        _ = manager.EnterLobbyAsync();
    }

    private void OnInviteClicked(int index)
    {
        // TODO: send the invite through the network service; the UI is already in place.
        Debug.Log($"[PlayingWithFriendsMenuPage] invite row {index} clicked");
    }
}
