using UnityEngine;

public class BoardFactory
{
    public Board Board { get; private set; }
    private GameAssets assets;

    public BoardFactory(GameAssets _assets)
    {
        assets = _assets;
        Debug.Log("new BoardFactory");
        Debug.Log("diceId" + assets.Customization.SelectedDiceId);

        Debug.Log("pieceId" + assets.Customization.SelectedPieceId);
    }
    public void Build()
    {
        Board = Object.FindAnyObjectByType<Board>();

        if (Board == null)
        {
            Debug.LogError("Board could not be found in the scene.");
            return;
        }

        var inventory = assets.Inventory;


        GameObject dicePrefab =
            assets.catalog.GetDicePrefab(inventory.OwnedDiceIds[assets.Customization.SelectedDiceId]);

        GameObject piecePrefab =
            assets.catalog.GetPiecePrefab(inventory.OwnedPieceIds[assets.Customization.SelectedPieceId]);

        BuildDice(dicePrefab);
        BuildPlayers(piecePrefab);
    }



    private void BuildPlayers(GameObject piecePrefab)
    {
        if (piecePrefab == null)
        {
            Debug.LogError("Selected piece prefab is null.");
            return;
        }

        if (Board.players == null)
        {
            Debug.LogError("Board players array is null.");
            return;
        }

        foreach (Player player in Board.players)
        {
            if (player == null)
                continue;

            BuildPlayer(player, piecePrefab);
        }
    }
    private void BuildPlayer(Player player, GameObject piecePrefab)
    {
        if (player == null || piecePrefab == null)
            return;

        for (int i = 0; i < player.pieces.Length; i++)
        {
            Piece oldPiece = player.pieces[i];

            Transform parent = oldPiece.transform.parent;


            GameObject newPieceObject =
                Object.Instantiate(piecePrefab, parent);
            
           

            Piece newPiece =
                newPieceObject.GetComponent<Piece>();

            if (newPiece == null)
            {
                Object.Destroy(newPieceObject);
                continue;
            }

            newPiece.index = i;
            //player.ApplyPieceModel(piecePrefab);
            newPiece.ApplyColor(player.playerColor);

            player.pieces[i] = newPiece;

            Object.Destroy(oldPiece.gameObject);
        }
    }
    private void BuildDice(GameObject dicePrefab)
    {
        if (dicePrefab == null)
        {
            Debug.LogError("Selected dice prefab is null.");
            return;
        }

        Dice oldDice = Board.dice;

        if (oldDice == null)
        {
            Debug.LogError("Board dice is null.");
            return;
        }

        Transform parent = oldDice.transform.parent;

        Vector3 localPosition = oldDice.transform.localPosition;
        Quaternion localRotation = oldDice.transform.localRotation;
        Vector3 localScale = oldDice.transform.localScale;

        GameObject newDiceObject =
            Object.Instantiate(dicePrefab, parent);

        newDiceObject.transform.localPosition = localPosition;
        newDiceObject.transform.localRotation = localRotation;
        newDiceObject.transform.localScale = localScale;

        Dice newDice = newDiceObject.GetComponent<Dice>();

        if (newDice == null)
        {
            Debug.LogError(
                "Selected Dice prefab does not contain Dice component.");

            Object.Destroy(newDiceObject);
            return;
        }

        Object.Destroy(oldDice.gameObject);

        Board.dice = newDice;
    }

    public void UpdatePlayerDto(PlayerDto dto)
    {
        Board.players[(int)dto.Color].userId = dto.Id;

        Board.players[(int)dto.Color].userName = dto.Username;
        if (dto.Id == GameManager.Instance.ThisContext.userId)
        {
            GameManager.Instance.ThisContext.color = dto.Color;
        }
    }
}