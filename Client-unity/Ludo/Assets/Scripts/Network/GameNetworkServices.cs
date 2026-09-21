using Nakama;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;
public class GameNetworkServices : IDisposable
{
    public void Dispose()
    {
        _ = LeaveMatchAsync();
        _socket?.CloseAsync();
        _client = null;
        _session = null;
        _match = null;
    }


    private IClient _client;
    private ISocket _socket;
    private ISession _session;
    private IMatch _match;

    public ISocket Socket => _socket;
    public IMatch Match => _match;
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
        //_socket.ReceivedMatchState += OnMatchState;
        //_socket.ReceivedMatchPresence += OnMatchPresence;
        //_socket.Closed += OnSocketClosed;
        //_socket.Connected += OnSocketConnected;
    }

    public async Task<PlayerInventoryData> LoadInventoryAsync()
    {
        try
        {
            var result = await _client.RpcAsync(
                _session,
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
            var result = await _client.RpcAsync(
                _session,
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
    AssetType assetType,
    string assetId)
    {
        string json = JsonUtility.ToJson(
            new SelectAssetRequest
            {
                AssetType = assetType.ToString(),
                AssetId = assetId
            });

        var response = await _client.RpcAsync(
            _session,
            "select_asset",
            json);

        return response.Payload;
    }

    public async Task<DiamondBalanceData> LoadDiamondBalanceAsync()
    {
        try
        {
            var response = await _client.RpcAsync(
                _session,
                "get_diamond_balance",
                "{}"
            );

            if (string.IsNullOrEmpty(response.Payload))
                return null;

            return JsonConvert.DeserializeObject<DiamondBalanceData>(
                response.Payload
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }


}
