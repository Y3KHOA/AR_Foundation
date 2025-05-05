using System.Collections.Generic;
using UnityEngine;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_MapType determines how map marker positions are calculated. Map3D uses the X and Z coordinates, whereas Map2D uses the X and Y coordinates of map objects.
    /// </summary>
    [System.Serializable]
    public enum QT_MapType
    { Map3D, Map2D }

    /// <summary>
    /// QT_Map represents any map of the world, and is the base for the minimap.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Map")]
    [DisallowMultipleComponent]
    public class QT_Map : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform = null;
        [SerializeField] protected RectTransform markersTransform = null;

        [Header("Object References")]
        public QT_MapObject ReferenceObject;
        public List<QT_MapObject> Objects = new List<QT_MapObject>();
        public QT_MapMarker MarkerPrefab;
        protected QT_MapMarker ReferenceMarker;
        public List<QT_MapMarker> Markers { get; set; } = new List<QT_MapMarker>();

        [Header("Map Variables")]
        public QT_MapType Type;
        public Vector2 WorldSize = new Vector2(5, 5);
        public float MarkerSize = 20;
        public float MinZoom = 1;
        public float MaxZoom = 2;
        [HideInInspector] public float Zoom = 1;

        protected void Awake()
        {
            foreach (var obj in Objects)
                AddMarker(obj, false);
            AddMarker(ReferenceObject, true);
        }

        protected virtual void Update()
        {
            foreach (var marker in Markers)
            {
                marker.SetPosition(CalculatePosition(marker));
                if (marker.Data.ShowRotation)
                    marker.transform.localEulerAngles = new Vector3(0, 0, Type == QT_MapType.Map3D ? -marker.Object.transform.eulerAngles.y : marker.Object.transform.eulerAngles.z);
            }

            if (ReferenceMarker.Data.ShowRotation)
                ReferenceMarker.transform.localEulerAngles = new Vector3(0, 0, Type == QT_MapType.Map3D ? -ReferenceMarker.Object.transform.eulerAngles.y : ReferenceMarker.Object.transform.eulerAngles.z);
        }

        protected virtual Vector2 CalculatePosition(QT_MapMarker marker)
        {
            Vector2 mapSize = rectTransform.rect.size;
            Vector2 worldToMap = new Vector2(mapSize.x / WorldSize.x, mapSize.y / WorldSize.y);
            Vector2 difference = marker.Object.Position(Type) - ReferenceObject.Position(Type);
            Zoom = Mathf.Clamp(Zoom, MinZoom, MaxZoom);

            return new Vector2(difference.x * worldToMap.x * Zoom, difference.y * worldToMap.y * Zoom);
        }

        /// <summary>
        /// Creates a new marker on the map, based on the given object.
        /// </summary>
        /// <param name="obj">The GameObject with a QT_MapObject on it.</param>
        /// <param name="reference">Whether or not this is the reference object.</param>
        public void AddMarker(QT_MapObject obj, bool reference)
        {
            QT_MapMarker marker = Instantiate(MarkerPrefab, reference ? transform : markersTransform);
            marker.Initialize(obj, MarkerSize);

            if (!reference)
                Markers.Add(marker);
            else
                ReferenceMarker = marker;
        }

        /// <summary>
        /// Sets the zoom of the map.
        /// </summary>
        /// <param name="zoom">The new zoom level.</param>
        public void SetZoom(float zoom)
        { Zoom = Mathf.Clamp(zoom, MinZoom, MaxZoom); }

        /// <summary>
        /// Changes the zoom of the map by a certain amount.
        /// </summary>
        /// <param name="amount">The amount to change by.</param>
        public void ChangeZoom(float amount)
        { Zoom = Mathf.Clamp(Zoom + amount, MinZoom, MaxZoom); }
    }
}