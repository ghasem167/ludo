using UnityEngine;

public class SpecialCellVisual : MonoBehaviour
{
    [SerializeField]
    private int cellIndex;

    public int CellIndex => cellIndex;

    public GameObject HighlightPrefab;
    public float HighlightYOffset = 0.1f;

    private GameObject currentHighlight;

    public void Highlight()
    {
        if (HighlightPrefab != null)
        {
            var highlight = Instantiate(HighlightPrefab, transform.position + Vector3.up * HighlightYOffset, Quaternion.identity, transform);
            currentHighlight = highlight;
        }
    }

    private void OnDestroy()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }

    

}