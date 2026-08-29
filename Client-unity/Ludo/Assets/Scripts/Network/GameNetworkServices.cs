using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
public class GameNetworkServices
{

    private IClient _client;
    private ISocket _socket;
    private ISession _session;
    private IMatch _match;
    private CommandHandler commandHandler;
    public bool IsOnline
    {
        get
        {
            return _client != null &&
                   _session != null &&
                   _socket != null &&
                   _socket.IsConnected;
        }
    }
    public bool IsAuthenticated =>
    _client != null && _session != null;

    public bool IsConnected =>
        _socket != null && _socket.IsConnected;
    public GameNetworkServices()
    {

    }

    public void SetCommandHandler(CommandHandler _commandHandler)
    {
        commandHandler = _commandHandler;
    }

    public async Task<ISession> InitializeAsync()
    {
        try
        {
            Debug.Log("1. Create Nakama Client");

            _client = new Client(
     "http",
     "127.0.0.1",
     7350,
     "defaultkey"
 );

            Debug.Log("2. Authenticate user");

            _session = await _client.AuthenticateDeviceAsync(
                SystemInfo.deviceUniqueIdentifier
            );

            Debug.Log("3. Authenticate SUCCESS");

            _socket = _client.NewSocket();

            Debug.Log("4. Socket created");

            RegisterEvents();

            Debug.Log("5. Connecting socket");

            await _socket.ConnectAsync(_session);

            Debug.Log("6. Nakama Connected");
            return _session;
        }
        catch (Exception e)
        {
            Debug.LogError($"Nakama Initialize ERROR:\n{e}");
            return null;
        }
    }

    public ISession GetSession()
    {

        return _session;
    }
    private void RegisterEvents()
    {
        _socket.ReceivedMatchState += OnMatchState;
        _socket.ReceivedMatchPresence += OnMatchPresence;
        //_socket.Closed += OnSocketClosed;
        //_socket.Connected += OnSocketConnected;
    }
    private void OnMatchState(IMatchState message)
    {
        var command = Interpret(message);
        commandHandler.Enqueue(command);
    }

    private void OnMatchPresence(IMatchPresenceEvent message)
    {
        Debug.Log("Match Presence Event Received");
        // Handle match presence events here
    }
    public async Task<PlayerInventoryData> LoadInventoryAsync()
    {
        try
        {
            var result = await _socket.RpcAsync(
                "LoadInventory",
                "{}"
            );

            if (string.IsNullOrEmpty(result.Payload))
                return null;


            PlayerInventoryData dto =
                JsonConvert.DeserializeObject<PlayerInventoryData>(
                    result.Payload
                );


            return dto;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }
    public async Task<PlayerCustomizationData> LoadCustomizationAsync()
    {
        try
        {
            var result = await _socket.RpcAsync(
                "LoadCustomization",
                "{}"
            );

            if (string.IsNullOrEmpty(result.Payload))
                return null;


            PlayerCustomizationData dto =
                JsonConvert.DeserializeObject<PlayerCustomizationData>(
                    result.Payload
                );

            return dto;


        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public GameCommand Interpret(IMatchState message)
    {

        switch ((opcode)message.OpCode)
        {
            case opcode.LobbyStarted:
                Debug.Log("Lobby started");
                return new LobbyStartedCommand();

            case opcode.PlayerAdded:
                Debug.Log("player added");
                return BuildPlayerAdded(message);

            case opcode.Players:
                Debug.Log("players");
                return BuildPlayers(message);

            case opcode.MatchStarted:
                Debug.Log("match started");
                return new MatchStartedCommand();

            case opcode.PiecesPosition:
                Debug.Log("pieceposition");
                return BuildPiecePositionOnBoardCommand(message);

            case opcode.LightsChanged:
                Debug.Log("ligh changed");
                return BuildLightsChanged(message);

            case opcode.TurnStarted:
                Debug.Log("turn started");
                return BuildTurnStartedCommand(message);

            case opcode.Rolling:
                Debug.Log("Rolling");
                return new RollingCommand();


            case opcode.DiceValue:
                Debug.Log("dice value");
                return BuildDiceValueCommand(message);

            case opcode.AvailableActions:
                Debug.Log("available actions");
                return BuildAvailableActionCommand(message);

            case opcode.NewAction:
                Debug.Log("new action");
                return BuildNewActionCommand(message);

            case opcode.CapturePiece:
                return BuildCapturePieceCommand(message);

            case opcode.PlayerFinish:
                Debug.Log("player finished");
                return new PlayerFinishedCommand();

            case opcode.MatchFinish:
                Debug.Log("match finished");
                return BuildMatchFinished(message);



            default:
                Debug.Log("not impolement message");
                throw new NotImplementedException();
        }
    }

    public async Task<FindOrCreateMatchResult> FindOrCreateMatch(
    TeamMode teamMode,
    GameMode gameMode)
    {
        var matchId = LoadMatchId();
        var payload = new
        {
            _match = matchId,
            teamMode = (int)teamMode,
            gameMode = (int)gameMode
        };

        var response = await _client.RpcAsync(
            _session,
            "FindOrCreateMatch",
            JsonConvert.SerializeObject(payload)
        );

        var responsePayload = JsonConvert.DeserializeObject<FindOrCreateMatchResult>(
            response.Payload
        );
        _matchId = responsePayload.matchId;
        return responsePayload;
    }
    private string _matchId;
    public async Task<IMatch> JoinMatch()
    {
        if (!IsOnline)
        {
            Debug.LogWarning("Cannot join match. Client is offline.");
            return null;
        }

        if (string.IsNullOrEmpty(_matchId))
        {
            Debug.LogError("Match ID is empty.");
            return null;
        }

        try
        {
            _match = await _socket.JoinMatchAsync(_matchId);

            Debug.Log(
                $"Joined match successfully. Match ID: {_matchId}");

            return _match;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Failed to join match: {ex.Message}");

            return null;
        }
    }
    private const string SavedMatchIdKey = "SavedMatchId";

    public async Task LeaveMatchAsync()
    {
        if (_match == null)
            return;

        string matchId = _match.Id;

        PlayerPrefs.SetString(
            SavedMatchIdKey,
            matchId
        );

        PlayerPrefs.Save();

        try
        {
            await _socket.LeaveMatchAsync(matchId);
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Failed to leave match {matchId}: {e}"
            );
        }
        finally
        {
            _match = null;
        }
    }

    private string LoadMatchId()
    {
        if (!PlayerPrefs.HasKey(SavedMatchIdKey))
            return null;

        return PlayerPrefs.GetString(
            SavedMatchIdKey
        );
    }
    private GameCommand BuildLobbyStarted(IMatchState message)
    {
        var dto = Deserialize<LobbyStartedDto>(message);

        return new LobbyStartedCommand();
    }
    private GameCommand BuildPlayerAdded(IMatchState message)
    {
        var dto = Deserialize<PlayerDto>(message);

        return new PlayerAddedCommand(dto);
    }
    private GameCommand BuildPlayers(IMatchState message)
    {
        var dto = Deserialize<PlayersDto>(message);

        return new PlayersCommand(dto.Players);
    }
    private GameCommand BuildMatchStarted(IMatchState message)
    {
        Debug.Log("match started");
        var dto = Deserialize<MatchStartedDto>(message);

        return new MatchStartedCommand();
    }
    private GameCommand BuildPiecePositionOnBoardCommand(IMatchState message)
    {
        var dto = Deserialize<List<PiecePositionDto>>(message);
        return new PiecesPositionCommand(dto);

    }
    private GameCommand BuildMatchFinished(IMatchState message)
    {
        var dto = Deserialize<MatchFinishedDto>(message);

        return new MatchFinishedCommand(dto.WinnerList);
    }
    private GameCommand BuildLightsChanged(IMatchState message)
    {
        var dto = Deserialize<LightsChangedDto>(message);
        return new LightsChangedCommand(
         dto.Player,
         dto.numOfLights
     );


    }

    private GameCommand BuildTurnStartedCommand(IMatchState message)
    {
        var dto = Deserialize<TurnStartedDto>(message);

        return new TurnStartedCommand(dto.PlayerColor);
    }

    private GameCommand BuildDiceValueCommand(IMatchState message)
    {
        var diceValue = Deserialize<int>(message);

        return new DiceValueCommand(
            diceValue
        );
    }
    private GameCommand BuildAvailableActionCommand(IMatchState message)
    {
        var actions = Deserialize<List<GameActionDto>>(message);

        return new AvailableActionCommand(actions);
    }

    private GameCommand BuildNewActionCommand(IMatchState message)
    {
        var dto = Deserialize<GameActionDto>(message);

        return new NewActionCommand(dto);
    }
    private GameCommand BuildCapturePieceCommand(IMatchState message)
    {
        var dto = Deserialize<PiecePositionDto>(message);

        return new CapturePieceCommand(dto);
    }


    private T Deserialize<T>(IMatchState message)
    {
        string json = Encoding.UTF8.GetString(message.State);
        return JsonConvert.DeserializeObject<T>(json);
    }
    public async Task<string> BuyAssetAsync(string assetId)
    {
        var payload = new
        {
            assetId = assetId
        };

        string json = JsonUtility.ToJson(
            new BuyAssetRequest
            {
                AssetId = assetId
            });

        var response = await _client.RpcAsync(
            _session,
            "buy_asset",
            json);

        return response.Payload;
    }
    public async Task<string> SelectAssetAsync(
    string assetType,
    string assetId)
    {
        string json = JsonUtility.ToJson(
            new SelectAssetRequest
            {
                AssetType = assetType,
                AssetId = assetId
            });

        var response = await _client.RpcAsync(
            _session,
            "select_asset",
            json);

        return response.Payload;
    }

    public async Task SendRollDice()
    {
        if (!IsOnline)
            return;

        await _socket.SendMatchStateAsync(
            _match.Id,
            (long)ClientOpCode.RollDice,
            string.Empty);
    }
    public async Task SendActionSelected(int actionIndex)
    {
        if (!IsOnline)
            return;

        byte[] data = System.Text.Encoding.UTF8.GetBytes(
            actionIndex.ToString());

        await _socket.SendMatchStateAsync(
            _match.Id,
            (long)ClientOpCode.SelectAction,
            data);
    }
     public async Task SendDiceTouched(int actionIndex)
    {
        if (!IsOnline)
            return;

        await _socket.SendMatchStateAsync(
            _match.Id,
            (long)ClientOpCode.RollDice,""
            );
    }
}
