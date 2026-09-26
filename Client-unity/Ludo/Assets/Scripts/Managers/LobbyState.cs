using System;
using System.Collections.Generic;

/// <summary>
/// Client side view of the lobby the server put us in: at most <see cref="MaxPlayers"/> players
/// (the server's <c>MatchLabel.maxPlayers</c>).
///
/// It is filled by <see cref="PlayersCommand"/> (the roster sent to the player who just joined)
/// and <see cref="PlayerAddedCommand"/> (one player per join) and read by <c>LobbyMenuPage</c>,
/// so the page can be opened before OR after the messages arrived - it simply re-reads the cache.
/// </summary>
public class LobbyState
{
    public const int MaxPlayers = 4;

    private readonly List<PlayerMatchDto> _players = new List<PlayerMatchDto>();

    public IReadOnlyList<PlayerMatchDto> Players => _players;
    public int Count => _players.Count;
    public int Capacity => MaxPlayers;
    public bool IsFull => _players.Count >= MaxPlayers;

    /// <summary>True after the server's "LobbyStarted" message (the countdown to the match runs).</summary>
    public bool Started { get; private set; }

    /// <summary>Raised on every roster change (also by <see cref="Reset"/>).</summary>
    public event Action PlayersChanged;

    /// <summary>Raised once per player that was really added.</summary>
    public event Action<PlayerDto> PlayerJoined;

    public event Action LobbyStarted;

    public bool Contains(string playerId) => IndexOf(playerId) >= 0;

    /// <summary>
    /// Shows the local player before the server answers (the "Players" message then replaces the
    /// entry with the real color). It also covers a reconnect, where the server sends no roster.
    /// </summary>
    public void SetLocalPlayer(PlayerMatchDto player)
    {
        if (player == null) return;

        Upsert(player, 0);
        PlayersChanged?.Invoke();
    }

    /// <summary>Adds or refreshes one player. False when the lobby is full and the player is new.</summary>
    public bool AddPlayer(PlayerMatchDto player)
    {
        if (player == null) return false;

        bool isNew = IndexOf(player.Player.Id) < 0;
        if (isNew && _players.Count >= MaxPlayers) return false;

        Upsert(player, _players.Count);

        if (isNew) PlayerJoined?.Invoke(player.Player);
        PlayersChanged?.Invoke();
        return true;
    }

    /// <summary>Replaces the whole roster (the "Players" message sent to the player who joined).</summary>
    public void SetPlayers(IEnumerable<PlayerMatchDto> players)
    {
        _players.Clear();

        if (players != null)
        {
            foreach (var player in players)
            {
                if (player == null) continue;
                if (_players.Count >= MaxPlayers) break;

                Upsert(player, _players.Count);
            }
        }

        PlayersChanged?.Invoke();
    }

    private int IndexOf(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return -1;

        for (int i = 0; i < _players.Count; i++)
            if (_players[i] != null && _players[i].Player.Id == playerId) return i;

        return -1;
    }

    /// <summary>Inserts the player, or refreshes the entry of the same id, at <paramref name="index"/>.</summary>
    private void Upsert(PlayerMatchDto player, int index)
    {
        int existing = IndexOf(player.Player.Id);
        if (existing >= 0)
        {
            _players[existing] = player;
            return;
        }

        if (_players.Count >= MaxPlayers) return;
        if (index < 0) index = 0;
        if (index > _players.Count) index = _players.Count;

        _players.Insert(index, player);
    }

    public void SetStarted()
    {
        if (Started) return;

        Started = true;
        LobbyStarted?.Invoke();
    }

    public void Reset()
    {
        _players.Clear();
        Started = false;

        PlayersChanged?.Invoke();
    }
}
