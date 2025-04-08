using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeasurementState
{
    public List<Vector3> basePoints = new List<Vector3>(); // Danh sách tọa độ các điểm cơ sở
    public GameObject pointPrefab; // Prefab dùng để tạo lại điểm

    public MeasurementState(List<Vector3> points, GameObject prefab)
    {
        basePoints = points;
        pointPrefab = prefab;
    }
}
