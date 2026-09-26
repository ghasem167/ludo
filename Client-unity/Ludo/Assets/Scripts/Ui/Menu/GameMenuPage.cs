using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Classical Game" page: the player picks a territory (قلمرو) and the match starts with that
/// selection stored in <see cref="PlayerContext"/> (GameManager.ThisContext).
/// </summary>
public class GameMenuPage : MenuPage
{
    [Serializable]
    public class TerritoryCard
    {
        public Button button;
        public Territory territory;
        public TeamMode teamMode;
        public int entryCost;

        /// <summary>Card background that gets tinted while selected (optional).</summary>
        public Graphic highlight;
    }

    [SerializeField] private TerritoryCard[] cards;

    [Tooltip("How long the selected card stays highlighted before the match loads.")]
    [SerializeField, Range(0f, 2f)] private float startMatchDelay = 0.35f;

    [SerializeField] private Color selectedColor = new Color(0.42f, 1f, 0.55f, 1f);

    private readonly List<Color> _baseColors = new List<Color>();
    private Coroutine _startRoutine;

    private static PlayerContext Context =>
        GameManager.Instance != null ? GameManager.Instance.ThisContext : null;

    protected override void OnPageAwake()
    {
        _baseColors.Clear();
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            int index = i;

            if (card == null)
            {
                _baseColors.Add(Color.white);
                continue;
            }

            if (card.button != null)
            {
                card.button.onClick.RemoveAllListeners();
                card.button.onClick.AddListener(() => OnCardClicked(index));
            }

            _baseColors.Add(card.highlight != null ? card.highlight.color : Color.white);
        }

        ApplyHighlight(SelectedIndex());
    }

    public override void OnOpened()
    {
        ApplyHighlight(SelectedIndex());
    }

    public override void OnClosed()
    {
        StopPendingStart();
    }

    /// <summary>Called by each card's button.</summary>
    public void OnCardClicked(int index)
    {
        if (cards == null || index < 0 || index >= cards.Length) return;

        var card = cards[index];
        if (card == null) return;

        Select(card.territory, card.teamMode, card.entryCost);
        ApplyHighlight(index);

        StopPendingStart();
        if (isActiveAndEnabled) _startRoutine = StartCoroutine(StartMatchRoutine());
    }

    /// <summary>Stores the pick in the game context (used by the match creation later on).</summary>
    public void Select(Territory territory, TeamMode teamMode, int entryCost)
    {
        var context = Context;
        if (context == null)
        {
            Debug.LogWarning("[ClassicalGameMenuPage] no GameManager/PlayerContext available");
            return;
        }

       
        context.ApplyTerritory(territory, teamMode, entryCost);

        Debug.Log($"[ClassicalGameMenuPage] territory={territory} teamMode={teamMode} entryCost={entryCost}");
    }

    /// <summary>
    /// Sends the match request for the current selection: the server answers with the lobby roster
    /// (shown by <c>LobbyMenuPage</c>) and opens the board scene once the match starts.
    /// </summary>
    public void StartSelectedMatch()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;

        _ = manager.EnterLobbyAsync();
    }

    private IEnumerator StartMatchRoutine()
    {
        yield return new WaitForSecondsRealtime(startMatchDelay);
        _startRoutine = null;
        StartSelectedMatch();
    }

    private void StopPendingStart()
    {
        if (_startRoutine == null) return;
        StopCoroutine(_startRoutine);
        _startRoutine = null;
    }

    private int SelectedIndex()
    {
        var context = Context;
        if (context == null || cards == null) return -1;

        for (int i = 0; i < cards.Length; i++)
            if (cards[i] != null && cards[i].territory == context.territory) return i;

        return -1;
    }

    private void ApplyHighlight(int selectedIndex)
    {
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            if (card == null || card.highlight == null) continue;

            Color baseColor = i < _baseColors.Count ? _baseColors[i] : Color.white;
            card.highlight.color = i == selectedIndex ? selectedColor : baseColor;
        }
    }
}
