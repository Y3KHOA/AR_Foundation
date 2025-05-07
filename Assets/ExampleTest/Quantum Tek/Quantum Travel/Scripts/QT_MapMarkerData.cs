using UnityEngine;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_MapMarkerData is used to create marker templates.
    /// </summary>
    [CreateAssetMenu(menuName = "Quantum Tek/Quantum Travel/Map Marker", fileName = "New Map Marker")]
    public class QT_MapMarkerData : ScriptableObject
    {
        [Header("Data")]
        public string Name;
        public Sprite Icon;

        [Header("Rules")]
        public bool ShowOnCompass = true;
        public bool HugBorder;
        public bool ShowRotation = true;
    }
}