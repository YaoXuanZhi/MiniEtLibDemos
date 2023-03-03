using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Describe:响应
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class RayCastArea :  MaskableGraphic
{
    protected RayCastArea()
    {
        useLegacyMeshGeneration = false;
        raycastTarget = true;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
    public override void Rebuild(CanvasUpdate update)
    {

    }
}
