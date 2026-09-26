using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public PlayerContext ThisContext { get; set; }

    /// <summary>Players the server put in the lobby we are waiting in (up to 4), shown by LobbyMenuPage.</summary>
    public LobbyState Lobby { get; private set; }

    /// <summary>True while a scene is loading: the command queue waits for it (see CommandHandler).</summary>
    public bool IsLoadingScene { get; private set; }

    /// <summary>Raised after the match request was accepted, so the menu can open the lobby page.</summary>
    public event Action LobbyEntered;
    public SceneService SceneServices { get; set; }
    public GameNetworkServices NetworkService { get; private set; }
    public GamePlayHandler GamePlayHandler { get; set; }
    public GameAssets GameAssets;
    public BoardFactory BoardFactory { get; set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        ThisContext = new PlayerContext();
        Lobby = new LobbyState();
        SceneServices = new SceneService();
        SceneServices.AfterSceneLoad += OnAfterSceneLoad;
        SceneServices.BeforeSceneUnload += OnBeforeSceneUnload;

        NetworkService = new GameNetworkServices();

        _ = InitializeNetwork();

    }


    private void OnAfterSceneLoad(GameScene scene)
    {
        switch (scene)
        {
            case GameScene.Match:
                Debug.Log("Scene Match Loaded");
                BoardFactory = new BoardFactory(GameAssets);
                BoardFactory.Build();

                // the lobby already created the online handler and joined the match:
                // creating another one would send a second FindOrCreateMatch / JoinMatch
                if (GamePlayHandler != null &&
                    GamePlayHandler.LastContext != null &&
                    GamePlayHandler.LastContext.CurrentPlayMode == PlayMode.Online)
                {
                    Debug.Log("[GameManager] keeping the online handler created by the lobby");
                    break;
                }

                GamePlayHandler?.Dispose();
                GamePlayHandler = CreateGamePlayHandler();
                break;

        }
    }

    /// <summary>
    /// Builds the game-play handler for the current menu selection
    /// (<see cref="PlayerContext.playMode"/>, game mode and team mode).
    /// </summary>
    private GamePlayHandler CreateGamePlayHandler()
    {
        var context = ThisContext;
        TeamMode teamMode = context != null ? context.teamMode : TeamMode.None;
        GameMode gameMode = context != null ? context.gameMode : GameMode.Modern;

        bool online = context != null &&
                      context.playMode == PlayMode.Online &&
                      NetworkService != null &&
                      NetworkService.IsOnline;

        if (online)
        {
            Debug.Log($"[GameManager] online match: mode={gameMode} team={teamMode}");

            var handler = new GamePlayHandler(NetworkService, teamMode, gameMode);
            _ = handler.JoinOnlineMatchAsync();   // fire and forget: the lobby page shows the roster
            return handler;
        }

        if (context != null && context.playMode == PlayMode.Online)
            Debug.LogWarning("[GameManager] server is not available - falling back to an offline match");

        Debug.Log($"[GameManager] offline match: mode={gameMode} team={teamMode} territory={(context != null ? context.territory.ToString() : "-")}");
        return new GamePlayHandler(teamMode, gameMode);
    }

    /// <summary>
    /// Starts a match with the selection stored in <see cref="ThisContext"/>:
    /// the menu pages call this once the player picked a mode / territory.
    /// </summary>
    public async Task StartMatchAsync()
    {
        if (SceneServices == null)
        {
            Debug.LogError("[GameManager] SceneServices is not ready yet");
            return;
        }

        var context = ThisContext;
        Debug.Log($"[GameManager] StartMatch: playMode={context.playMode} gameMode={context.gameMode} " +
                  $"teamMode={context.teamMode} territory={context.territory} entryCost={context.entryCost}");

        IsLoadingScene = true;
        try
        {
            await SceneServices.LoadSceneAsync(GameScene.Match);
        }
        finally
        {
            IsLoadingScene = false;
        }
    }

    /// <summary>
    /// Sends the match request for the selection made in the menu (territory / game mode / team mode)
    /// and keeps the menu scene open, so <c>LobbyMenuPage</c> can show the players the server adds.
    /// Falls back to an offline match when the server is not reachable.
    /// </summary>
    public async Task<bool> EnterLobbyAsync()
    {
        var context = ThisContext;
        if (context == null)
        {
            Debug.LogError("[GameManager] no PlayerContext available");
            return false;
        }

        context.playMode = PlayMode.Online;

        Debug.Log($"[GameManager] EnterLobby: gameMode={context.gameMode} teamMode={context.teamMode} " +
                  $"territory={context.territory} entryCost={context.entryCost}");

        if (NetworkService == null || !NetworkService.IsOnline)
        {
            Debug.LogWarning("[GameManager] server is not available - starting an offline match instead");
            await StartMatchAsync();
            return false;
        }

        bool reuseOnlineHandler = GamePlayHandler != null &&
                                  GamePlayHandler.LastContext != null &&
                                  GamePlayHandler.LastContext.CurrentPlayMode == PlayMode.Online;

        if (!reuseOnlineHandler)
        {
            GamePlayHandler?.Dispose();
            GamePlayHandler = new GamePlayHandler(NetworkService, context.teamMode, context.gameMode);
        }

        Lobby.Reset();

        // show ourselves right away; the server's Players message replaces this entry with the real color
        Lobby.SetLocalPlayer(new PlayerMatchDto
        {
            Player = new PlayerDto
            {
                Id = context.userId,
                Username = context.userName
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

            Color = PlayerColor.Blue
        });
        bool joined = await GamePlayHandler.JoinOnlineMatchAsync();
        if (!joined)
        {
            Debug.LogWarning("[GameManager] joining the lobby failed - starting an offline match instead");
            await StartMatchAsync();
            return false;
        }

        LobbyEntered?.Invoke();
        return true;
    }

    /// <summary>Leaves the lobby (cancel button / going back before the match started).</summary>
    public async Task LeaveLobbyAsync()
    {
        Lobby?.Reset();

        if (NetworkService != null) await NetworkService.LeaveMatchAsync();

        GamePlayHandler?.Dispose();
        GamePlayHandler = null;
    }

    /// <summary>"Players" message: the roster of the lobby we just joined.</summary>
    public void ApplyPlayers(IEnumerable<PlayerMatchDto> players)
    {
        Lobby?.SetPlayers(players);

        Debug.Log($"[GameManager] lobby roster: {(Lobby != null ? Lobby.Count : 0)}/{LobbyState.MaxPlayers}");

        // still in the menu: the lobby page shows the roster, the board does not exist yet
        if (BoardFactory == null || Lobby == null) return;

        foreach (var player in Lobby.Players)
            BoardFactory.UpdatePlayerDto(player);
    }

    /// <summary>"PlayerAdded" message: one more player joined the lobby.</summary>
    public void ApplyPlayerAdded(PlayerMatchDto player)
    {
        if (player == null) return;

        bool added = Lobby != null && Lobby.AddPlayer(player);

        Debug.Log($"[GameManager] player added: {player} (color={player.Color}) " +
                  $"added={added} roster={(Lobby != null ? Lobby.Count : 0)}/{LobbyState.MaxPlayers}");

        if (added && BoardFactory != null) BoardFactory.UpdatePlayerDto(player);
    }

    public void OnLobbyStarted()
    {
        Debug.Log("[GameManager] lobby started");
        Lobby?.SetStarted();
    }

    private void OnBeforeSceneUnload(GameScene scene)
    {
        switch (scene)
        {
            case GameScene.Match:
                GamePlayHandler?.Dispose();
                GamePlayHandler = null;
                break;

        }
    }


    private async Task InitializeNetwork()
    {
        try
        {
            var session = await NetworkService.InitializeAsync();

            ThisContext.userId = session?.UserId;
            ThisContext.userName = session?.Username;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        await GameAssets.Initialize(NetworkService);
    }

    public void OnApplicationQuit()
    {
        NetworkService?.Dispose();
        GamePlayHandler?.Dispose();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
