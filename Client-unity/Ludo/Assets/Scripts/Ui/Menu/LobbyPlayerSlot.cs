using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of the 4 lobby slots (the children of the grid under LobbyPage).
/// Each slot owns two visuals: the "empty" placeholder (e.g. Null User.prefab) and the filled row
/// (e.g. Loded User.prefab). Both are optional, so a slot with a single row object still works.
/// </summary>
public class LobbyPlayerSlot : MonoBehaviour
{
    [Tooltip("Shown while the slot has no player (e.g. an instance of 'Null User').")]
    [SerializeField] private GameObject emptyVisual;

    [Tooltip("Shown while the slot holds a player (e.g. an instance of 'Loded User').")]
    [SerializeField] private GameObject filledVisual;

    [Tooltip("Nickname of the player in this slot.")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("Optional avatar image of the row.")]
    [SerializeField] private Image avatarImage;

    [Tooltip("Optional small graphics tinted with the player color.")]
    [SerializeField] private Image colorIcon;

    [Tooltip("Tint of the color icon: Blue, Red, Yellow, Green - the order of PlayerColor.")]
    [SerializeField]
    private Color[] colorTints =
    {
        new Color(0.20f, 0.45f, 1.00f, 1f),
        new Color(1.00f, 0.25f, 0.25f, 1f),
        new Color(1.00f, 0.85f, 0.20f, 1f),
        new Color(0.25f, 0.85f, 0.35f, 1f)
    };

    [Tooltip("Name color of the local player (the others use white).")]
    [SerializeField] private Color localNameColor = new Color(0.42f, 1f, 0.55f, 1f);

    /// <summary>Empty slot: the placeholder is visible, the row is hidden.</summary>
    public void Clear()
    {
        if (emptyVisual != null) emptyVisual.SetActive(true);
        if (filledVisual != null) filledVisual.SetActive(false);
    }

    /// <summary>Fills the slot with a player from the lobby roster.</summary>
    public void Bind(PlayerMatchDto player, bool isLocal)
    {
        if (player == null)
        {
            Clear();
            return;
        }

        if (emptyVisual != null) emptyVisual.SetActive(false);
        if (filledVisual != null) filledVisual.SetActive(true);

        if (nameText != null)
        {
            nameText.text = player.Player.DisplayName;
            nameText.color = isLocal ? localNameColor : Color.white;
        }

        var tint = TintFor(player.Color);

        if (colorIcon != null)
            colorIcon.color = tint;

        if (avatarImage != null && avatarImage.color.a <= 0f)
            avatarImage.color = Color.white;
    }

    private Color TintFor(PlayerColor color)
    {
        int index = (int)color;
        if (colorTints == null || index < 0 || index >= colorTints.Length) return Color.white;

        return colorTints[index];
    }
}
