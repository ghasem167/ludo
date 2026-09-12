using System.Collections.Generic;

namespace Ludo.Offline
{
    public class Player
    {
        public readonly PlayerColor Color;

        public string UserId;
        public string UserName;
        public string UserNickName;

        public List<Piece> Pieces;

        public Player Friend;

        public PlayerState PlayerState;

        public Player(
            PlayerColor color,
            string userId = "",
            string userName = "",
            string userNickName = "",
            List<Piece> pieces = null,
            Player friend = null)
        {
            Color = color;
            UserId = userId;
            UserName = userName;
            UserNickName = userNickName;

            Pieces = pieces ?? new List<Piece>();

            Friend = friend;

            PlayerState = new PlayerState();
        }

        public static Player CreatePlayer(
        PlayerColor color,
        Board board)
        {
            var player = new Player(color);


            if (GameManager.Instance.GamePlayHandler.LastContext.thisPlayerColor == color)
            {
                player.UserName = "You";
                player.UserNickName = "You";

                player.PlayerState.IsBot = false;
            }
            else
            {
                player.UserName = "Bot " + color.ToString();
                player.UserNickName = "Bot " + color.ToString();
                player.PlayerState.IsBot = true;
            }
            var path = board.Config.PlayerPath[color];

            for (int i = 0; i < 3; i++)
            {
                var initialCell =
                    board.Cells[path.InitialCells[i]];

                var piece = new Piece(
                    i,
                    initialCell,
                    player);

                player.Pieces.Add(piece);
            }

            return player;
        }
    }
}

