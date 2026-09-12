using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ludo.Offline
{
    public class MatchHandler:IDisposable
    {
        private readonly MatchContext _context;

        private readonly GameFlowManager _gameFlowManager;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public MatchHandler(
            CommandHandler commandHandler,
            GameMode gameMode,
            TeamMode teamMode)
        {
            var state = CreateMatch(gameMode, teamMode);
            _context = new MatchContext(state, commandHandler);

            _gameFlowManager = new GameFlowManager();


        }

        public void Dispose()
        {
            Stop();
        }

        public async Task RunMatchLoop()
        {
            int tickDelay = 1000 / MatchConstants.MatchTickRate;

            try
            {
                while (!_cts.Token.IsCancellationRequested &&
                       !_context.State.MatchEnd)
                {
                    _context.Tick++;

                    _gameFlowManager.Update(_context);

                    await Task.Delay(tickDelay, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // بازی متوقف شده؛ طبیعی است.
            }
        }
        public void Stop()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private MatchState CreateMatch(
            GameMode gameMode,
            TeamMode teamMode)
        {
            var board = new Board(BoardConfig.ClassicLudo());

            var players = new Player[]
            {
            Player.CreatePlayer(PlayerColor.Blue, board),
            Player.CreatePlayer(PlayerColor.Red, board),
            Player.CreatePlayer(PlayerColor.Yellow, board),
            Player.CreatePlayer(PlayerColor.Green, board)
            };

            var label = new MatchLabel(gameMode, teamMode);

            if (label.TeamMode == TeamMode.TwoVsTwo)
            {
                players[0].Friend = players[2];
                players[2].Friend = players[0];

                players[1].Friend = players[3];
                players[3].Friend = players[1];
            }

            return new MatchState(
                board,
                new TurnState(),
                new DiceState(),
                players,
                label
            );
        }
    }
}