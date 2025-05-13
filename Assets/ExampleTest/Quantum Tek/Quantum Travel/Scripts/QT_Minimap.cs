using UnityEngine;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_MapColliderType determines if markers are allowed to "hug" the border of the map when offscreen. Either none (not allowed), rectangle, or circle.
    /// </summary>
    [System.Serializable]
    public enum QT_MapColliderType
    { None, Rectangle, Circle }

    /// <summary>
    /// QT_Minimap represents a small version of the map, usually used in the corner of the screen.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Minimap")]
    [DisallowMultipleComponent]
    public class QT_Minimap : QT_Map
    {
        [Header("Minimap Variables")]
        public QT_MapColliderType MapCollider;
        public bool RotateMap = true;

        protected override void Update()
        {
            foreach (var marker in Markers)
            {
                marker.SetPosition(CalculatePosition(marker));
                if (marker.Data.ShowRotation)
                    marker.transform.localEulerAngles = new Vector3(0, 0, Type == QT_MapType.Map3D ? -marker.Object.transform.eulerAngles.y : marker.Object.transform.eulerAngles.z);
            }

            if (RotateMap)
                markersTransform.localEulerAngles = new Vector3(0, 0, Type == QT_MapType.Map3D ? ReferenceObject.transform.eulerAngles.y : -ReferenceObject.transform.eulerAngles.z);
        }

        protected override Vector2 CalculatePosition(QT_MapMarker marker)
        {
            Vector2 mapSize = rectTransform.rect.size;
            Vector2 worldToMap = new Vector2(mapSize.x / WorldSize.x, mapSize.y / WorldSize.y);
            Vector2 difference = marker.Object.Position(Type) - ReferenceObject.Position(Type);
            Zoom = Mathf.Clamp(Zoom, MinZoom, MaxZoom);
            Vector2 position = new Vector2(difference.x * worldToMap.x * Zoom, difference.y * worldToMap.y * Zoom);

            if (MapCollider == QT_MapColliderType.Rectangle)
            {
                position.x = Mathf.Clamp(position.x, -mapSize.x / 2, mapSize.x / 2);
                position.y = Mathf.Clamp(position.y, -mapSize.y / 2, mapSize.y / 2);
            }
            else if (MapCollider == QT_MapColliderType.Circle)
            {
                float magnitude = Mathf.Clamp(position.magnitude, 1, Mathf.Min(mapSize.x, mapSize.y) / 2);
                position.Normalize();
                position *= magnitude;
            }

            return position;
        }
    }
}