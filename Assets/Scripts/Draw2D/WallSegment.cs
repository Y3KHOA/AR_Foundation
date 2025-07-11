using UnityEngine;

public class WallSegment
{
    public Vector3 start;
    public Vector3 end;
    public bool hasDoor;

    public void Draw(GameObject parent, Material solidMat, Material dashedMat)
    {
        var go = new GameObject("WallSegment");
        go.transform.parent = parent.transform;
        var lr = go.AddComponent<LineRenderer>();
        lr.material = hasDoor ? dashedMat : solidMat;
        lr.textureMode = LineTextureMode.Tile;
        lr.widthMultiplier = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        float len = Vector3.Distance(start, end);
        lr.material.mainTextureScale = new Vector2(len * 2f, 1f); // Điều chỉnh độ lặp nét đứt
    }
}
