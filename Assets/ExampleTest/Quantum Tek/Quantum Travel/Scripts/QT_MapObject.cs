using UnityEngine;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_MapObject represents a physical object in the world that corresponds to a map marker.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Map Object")]
    [DisallowMultipleComponent]
    public class QT_MapObject : MonoBehaviour
    {
        public QT_MapMarkerData Data;

        public Vector2 Position (QT_MapType type) => new Vector2(transform.position.x, type == QT_MapType.Map3D ? transform.position.z : transform.position.y);
    }
}