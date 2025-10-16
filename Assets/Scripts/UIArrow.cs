using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIArrow : Graphic
{
    [SerializeField] private float lineWidth = 5f;
    [SerializeField] private float arrowLength = 100f;
    [SerializeField] private float wingLength = 30f;
    [SerializeField] private float wingAngle = 45f;
    [SerializeField] private float cornerRadius = 5f;
    [SerializeField] private int cornerSegments = 8;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Draw main body (vertical line)
        DrawLine(vh, Vector2.zero, new Vector2(0, -arrowLength));

        // Calculate wing endpoints
        float angleRad = wingAngle * Mathf.Deg2Rad;
        float wingX = wingLength * Mathf.Sin(angleRad);
        float wingY = wingLength * Mathf.Cos(angleRad);

        Vector2 bodyEnd = new Vector2(0, -arrowLength);
        Vector2 leftWingEnd = bodyEnd + new Vector2(-wingX, -wingY);
        Vector2 rightWingEnd = bodyEnd + new Vector2(wingX, -wingY);

        // Draw left wing
        DrawLine(vh, bodyEnd, leftWingEnd);

        // Draw right wing
        DrawLine(vh, bodyEnd, rightWingEnd);
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * lineWidth * 0.5f;

        int baseIndex = vh.currentVertCount;

        // Create 4 vertices for the line rectangle
        vh.AddVert(CreateVertex(start - perpendicular));
        vh.AddVert(CreateVertex(start + perpendicular));
        vh.AddVert(CreateVertex(end + perpendicular));
        vh.AddVert(CreateVertex(end - perpendicular));

        // Create 2 triangles
        vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 2);
        vh.AddTriangle(baseIndex + 2, baseIndex + 3, baseIndex + 0);

        // Add rounded caps at both ends
        DrawRoundedCap(vh, start, -direction, perpendicular);
        DrawRoundedCap(vh, end, direction, perpendicular);
    }

    private void DrawRoundedCap(VertexHelper vh, Vector2 center, Vector2 direction, Vector2 perpendicular)
    {
        if (cornerRadius <= 0 || cornerSegments < 1) return;

        float radius = Mathf.Min(cornerRadius, lineWidth * 0.5f);
        int baseIndex = vh.currentVertCount;

        // Center vertex
        vh.AddVert(CreateVertex(center));

        // Starting angle is perpendicular to the line
        float startAngle = Mathf.Atan2(perpendicular.y, perpendicular.x);

        // Create arc vertices (180 degrees)
        for (int i = 0; i <= cornerSegments; i++)
        {
            float angle = startAngle + (Mathf.PI * i / cornerSegments);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            vh.AddVert(CreateVertex(center + offset));
        }

        // Create triangles for the cap
        for (int i = 0; i < cornerSegments; i++)
        {
            vh.AddTriangle(baseIndex, baseIndex + i + 1, baseIndex + i + 2);
        }
    }

    private UIVertex CreateVertex(Vector2 position)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        return vertex;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
}