using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VisualBoard : MonoBehaviour
{
    private Dictionary<int, SpecialCellVisual> _specialCells;
    public GameObject HighlightSpecialCellPrefab;
    public float HighlightSpecialCellYOffset = 0.1f;
    void Awake()
    {
        _specialCells = new Dictionary<int, SpecialCellVisual>();

        foreach (var visual in GetComponentsInChildren<SpecialCellVisual>())
        {
            _specialCells[visual.CellIndex] = visual;
            _specialCells[visual.CellIndex].HighlightPrefab = HighlightSpecialCellPrefab;
            _specialCells[visual.CellIndex].HighlightYOffset = HighlightSpecialCellYOffset;
        }
    }


    public SpecialCellVisual GetSpecialCellVisual(int cellIndex)
    {
        _specialCells.TryGetValue(cellIndex, out var visual);
        return visual;
    }
    public IEnumerable<SpecialCellVisual> GetSpecialCellVisual()
    {
        return _specialCells.Values;
    }


}