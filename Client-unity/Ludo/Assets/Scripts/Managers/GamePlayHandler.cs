using System;
using System.Threading.Tasks;
using UnityEngine;

public class GamePlayHandler : IDisposable
{

    public GamePlayEvents GamePlayEvents { get; set; }

    public GamePlayContext LastContext { get; private set; }
    public CommandHandler CommandHandler;
    private GameNetworkServices NetworkServices;
    private ServerMatchHandler serverMatchHandler;
    private Ludo.Offline.MatchHandler matchHandler;
    private TeamMode _teamMode = TeamMode.None;
    private GameMode _gameMode = GameMode.Modern;

    public SelectionManager SelectionManager { get; private set; } = new SelectionManager();
    public GamePlayHandler()
        : this(TeamMode.None, GameMode.Modern)
    {
    }

    /// <summary>Offline match with the mode / team mode picked on the main menu.</summary>
    public GamePlayHandler(TeamMode teamMode, GameMode gameMode)
    {
        LastContext = new GamePlayContext();
        LastContext.CurrentPlayMode = PlayMode.Offline;
        GamePlayEvents = new GamePlayEvents();

        GameObject commandHandlerObject =
            new GameObject("CommandHandler");

        CommandHandler =
            commandHandlerObject.AddComponent<CommandHandler>();

        CreateOfflineMatch(teamMode, gameMode);
        SubscribeGamePlayEvents();
    }

    public GamePlayHandler(GameNetworkServices networkServices)
        : this(networkServices, TeamMode.None, GameMode.Modern)
    {
    }

    public GamePlayHandler(GameNetworkServices networkServices, TeamMode teamMode, GameMode gameMode)
    {
        LastContext = new GamePlayContext();
        LastContext.CurrentPlayMode = PlayMode.Online;
        GamePlayEvents = new GamePlayEvents();

        GameObject commandHandlerObject =
            new GameObject("CommandHandler");

        // the online match is created while the lobby page is still open (a different scene),
        // so the command queue has to survive the scene load that follows
        UnityEngine.Object.DontDestroyOnLoad(commandHandlerObject);

        CommandHandler =
            commandHandlerObject.AddComponent<CommandHandler>();

        NetworkServices = networkServices;
        _teamMode = teamMode;
        _gameMode = gameMode;
        SubscribeGamePlayEvents();
    }

    /// <summary>
    /// Sends FindOrCreateMatch for the mode picked in the menu and joins the match the server
    /// puts us in. The <see cref="ServerMatchHandler"/> is registered BEFORE the join, otherwise
    /// the Players / PlayerAdded messages the server sends right after joining would be lost.
    /// Returns false when the client is offline or the join failed (the caller falls back).
    /// </summary>
    public async Task<bool> JoinOnlineMatchAsync()
    {
        if (NetworkServices == null)
        {
            Debug.LogWarning("[GamePlayHandler] cannot join: no network service");
            return false;
        }

        // right after start-up the socket may still be connecting: give it a moment
        for (int i = 0; i < 20 && !NetworkServices.IsOnline; i++)
            await Task.Delay(250);

        if (!NetworkServices.IsOnline)
        {
            Debug.LogWarning("[GamePlayHandler] cannot join: the network service is offline");
            return false;
        }

        if (serverMatchHandler == null)
            serverMatchHandler = new ServerMatchHandler(NetworkServices, CommandHandler);

        var response = await NetworkServices.FindOrCreateMatch(_teamMode, _gameMode);
        Debug.Log("Response of Match Create Requeste: " + response);

        var match = await NetworkServices.JoinMatch();
        if (match == null)
        {
            Debug.LogError("[GamePlayHandler] joining the match failed");
            return false;
        }

        Debug.Log($"[GamePlayHandler] joined online match {match.Id} gameMode={_gameMode} teamMode={_teamMode}");
        return true;
    }
    private void CreateOfflineMatch(TeamMode teamMode = TeamMode.None, GameMode gameMode = GameMode.Modern, PlayerColor playerColor = PlayerColor.Blue)
    {
        Debug.Log("Creating Offline Match");

        matchHandler = new Ludo.Offline.MatchHandler(
            CommandHandler,
            gameMode,
            teamMode
        );
        CommandHandler.Enqueue(new PlayerAddedCommand(
        new PlayerMatchDto
        {
            Player = new PlayerDto
            {
                Id = "offline_player_1",
                Username = "Player 1"
            },

            Stat = new PlayerStatDto
            {
                Xp = 0,
                Trophies = 0,
                Level = PlayerLevel.Beginner
            },

            Custom = new PlayerCustomDto
            {
                AvatarId = "avatar_default",
                LogoId = "logo_default",
                PieceId = "piece_default"
            },

            Color = playerColor
        }
    ));
        _ = matchHandler.RunMatchLoop();
    }


    private void SubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected += OnActionSelected;

        GamePlayEvents.DiceSelected += OnDiceSelected;
    }
    private void UnsubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected -= OnActionSelected;

        GamePlayEvents.DiceSelected -= OnDiceSelected;
    }
    private void OnActionSelected(int actionIndex)
    {
        if (LastContext.CurrentPlayMode == PlayMode.Online)
        {
            _ = serverMatchHandler
                .SendActionSelected(actionIndex);
        }
        else
        {
            HandleLocalActionSelected(actionIndex);
        }
    }
    private async void OnDiceSelected()
    {
        if (LastContext.CurrentPlayMode == PlayMode.Online)
        {
            await serverMatchHandler.SendRollDice();
            return;
        }

        HandleOfflineDiceRoll();
    }
    private void HandleLocalActionSelected(int actionIndex)
    {
        // ساخت همان Commandای که در حالت آنلاین
        // بعد از دریافت پیام سرور ساخته می‌شود
    }
    private void HandleOfflineDiceRoll()
    {
        // منطق Roll در حالت آفلاین
    }
    public void Dispose()
    {
        UnsubscribeGamePlayEvents();
        matchHandler?.Dispose();
        matchHandler = null;
        serverMatchHandler = null;

        // the command queue object is kept alive across scene loads (see the online constructor)
        if (CommandHandler != null)
        {
            UnityEngine.Object.Destroy(CommandHandler.gameObject);
            CommandHandler = null;
        }
    }

}