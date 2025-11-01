using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Radial Gradient")]
public class UIRadialGradient : BaseMeshEffect
{
    [Header("Radial Gradient Settings")]
    public Color centerColor = new Color(168f / 255f, 143f / 255f, 122f / 255f, 1f); // #A88F7A
    public Color edgeColor = new Color(144f / 255f, 112f / 255f, 84f / 255f, 0.7f);   // rgba(144, 112, 84, 0.7)

    [Range(0f, 1f)]
    public float centerX = 0.5f; // 50% 水平位置

    [Range(0f, 1f)]
    public float centerY = 1f;   // 0% 轉換為Unity座標系統 = 1f (頂部)

    [Range(0.1f, 3f)]
    public float radiusMultiplier = 1.86f; // 185.62% ≈ 1.86

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        var vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        ApplyRadialGradient(vertexList);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    private void ApplyRadialGradient(List<UIVertex> vertexList)
    {
        if (vertexList.Count == 0) return;

        // 計算邊界
        Vector2 min = vertexList[0].position;
        Vector2 max = vertexList[0].position;

        foreach (var vertex in vertexList)
        {
            min = Vector2.Min(min, vertex.position);
            max = Vector2.Max(max, vertex.position);
        }

        Vector2 size = max - min;
        Vector2 center = min + new Vector2(size.x * centerX, size.y * centerY);
        float maxRadius = Mathf.Max(size.x, size.y) * radiusMultiplier;

        // 套用漸層
        for (int i = 0; i < vertexList.Count; i++)
        {
            var vertex = vertexList[i];
            float distance = Vector2.Distance(vertex.position, center);
            float normalizedDistance = Mathf.Clamp01(distance / maxRadius);

            vertex.color = Color.Lerp(centerColor, edgeColor, normalizedDistance);
            vertexList[i] = vertex;
        }
    }
}