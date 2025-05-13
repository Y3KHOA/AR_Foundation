using UnityEngine;
using UnityEngine.UI;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_MapMarker is the UI map marker representing a map object.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Map Marker")]
    [DisallowMultipleComponent]
    public class QT_MapMarker : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private Image image = null;

        [HideInInspector] public QT_MapMarkerData Data;
        [HideInInspector] public QT_MapObject Object;

        public void Initialize(QT_MapObject obj, float size)
        {
            Data = obj.Data;
            Object = obj;
            image.sprite = Data.Icon;
            if (Data.Icon == null)
                image.enabled = false;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }

        public void SetPosition(Vector2 position)
        { rectTransform.anchoredPosition = position; }

        public void SetScale(Vector2 scale)
        { rectTransform.localScale = scale; }
    }
}