using UnityEngine;
using UnityEngine.Rendering;

public class SelectableOutline : MonoBehaviour
{
    [SerializeField] private RenderingLayerMask outlineLayer;

    private Renderer[] _renderers;
    private uint[] _originalLayers;


    public void Initialize()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        _originalLayers = new uint[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalLayers[i] = _renderers[i].renderingLayerMask;
        }
    }


    public void SetOutline(bool enable)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (enable)
            {
                _renderers[i].renderingLayerMask =
                    _originalLayers[i] | outlineLayer;
            }
            else
            {
                _renderers[i].renderingLayerMask =
                    _originalLayers[i];
            }
        }
    }
     
}