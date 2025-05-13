using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_CompassBar is used as a compass bar, showing travel direction in 3D space and any important markers.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Compass Bar")]
    [DisallowMultipleComponent]
    public class QT_CompassBar : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private RectTransform barBackground = null;
        [SerializeField] private RectTransform markersTransform = null;
        [SerializeField] private RawImage image = null;

        [Header("Object References")]
        public Camera ReferenceCamera;

        public List<QT_MapObject> Objects = new List<QT_MapObject>();
        public QT_MapMarker MarkerPrefab;
        public List<QT_MapMarker> Markers { get; set; } = new List<QT_MapMarker>();

        [Header("Compass Bar Variables")]
        public Vector2 CompassSize = new Vector2(200, 25);
        public Vector2 ShownCompassSize = new Vector2(100, 25);
        public float MaxRenderDistance = 5;
        public float MarkerSize = 20;
        public float MinScale = 0.5f;
        public float MaxScale = 2f;

        private float currentHeading = 0f;
        private Vector2 currentPosition;
        private Vector2 currentScale= Vector2.one;


        private void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }

            // Delay khởi tạo layout đến khi camera sẵn sàng
            StartCoroutine(InitializeAfterDelay());
        }

        private IEnumerator InitializeAfterDelay()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);

                // Đợi user phản hồi (1 frame)
                yield return null;

                // Nếu vẫn chưa được cấp, thoát
                if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    Debug.LogError("Permission denied for FineLocation.");
                    yield break;
                }
            }

            if (!Input.location.isEnabledByUser)
            {
                Debug.LogError("Location services are not enabled by the user.");
                yield break;
            }

            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogError("Location service failed to start.");
                yield break;
            }

            // Bây giờ mới bật compass
            Input.compass.enabled = true;

            Debug.Log("Compass enabled.");

            // Đợi dữ liệu compass cập nhật
            int compassWait = 10;
            while (Input.compass.timestamp == 0 && compassWait > 0)
            {
                Debug.Log("Waiting for compass to be ready...");
                yield return new WaitForSeconds(1);
                compassWait--;
            }

            if (Input.compass.timestamp == 0)
            {
                Debug.LogError("Compass failed to initialize (timestamp = 0).");
                yield break;
            }

            Debug.Log("Compass and location initialized successfully.");

            SetupUI();
        }


        private void SetupUI()
        {
            if (rectTransform == null || barBackground == null || markersTransform == null || image == null)
            {
                Debug.LogError("QT_CompassBar is missing UI references.");
                enabled = false;
                return;
            }

            foreach (var obj in Objects)
                if (obj.Data.ShowOnCompass)
                    AddMarker(obj);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ShownCompassSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ShownCompassSize.y);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CompassSize.x);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CompassSize.y);
        }


        private void Update()
        {
            if (!Input.compass.enabled || Input.compass.timestamp == 0)
                return;

            float targetHeading = Input.compass.trueHeading;

            // Làm mượt góc quay bằng LerpAngle (xử lý wrap-around 0° <-> 360°)
            currentHeading = Mathf.LerpAngle(currentHeading, targetHeading, Time.deltaTime * 1f);

            // Cập nhật UV của hình ảnh compass
            image.uvRect = new Rect(currentHeading / 360f, 0, 1, 1);

            foreach (var marker in Markers)
            {
                SetPosition(CalculatePosition(marker));
                SetScale(CalculateScale(marker));
            }
        }

        public void SetPosition(Vector2 pos)
        {
            // Làm mượt vị trí (tuỳ chỉnh tốc độ nếu cần)
            currentPosition = Vector2.Lerp(currentPosition, pos, Time.deltaTime * 10f);
            transform.localPosition = currentPosition;
        }
        public void SetScale(Vector2 scale)
        {
            currentScale = Vector2.Lerp(currentScale, scale, Time.deltaTime * 10f);
            transform.localScale = currentScale;
        }


        private Vector2 CalculatePosition(QT_MapMarker marker)
        {
            float compassDegree = CompassSize.x / 360;

            Vector2 referencePosition = new Vector2(ReferenceCamera.transform.position.x, ReferenceCamera.transform.position.z);

            float heading = Input.compass.trueHeading;
            float headingRad = -heading * Mathf.Deg2Rad;

            Vector2 referenceForward = new Vector2(Mathf.Sin(headingRad), Mathf.Cos(headingRad));

            Vector2 targetPosition = marker.Object.Position(QT_MapType.Map3D);
            Vector2 dirToTarget = targetPosition - referencePosition;

            float angle = Vector2.SignedAngle(dirToTarget, referenceForward);

            Debug.Log($"Marker: {marker.name} | Angle: {angle}° | Pos: {dirToTarget} | Forward: {referenceForward}");

            return new Vector2(compassDegree * angle, 0);
        }

        private Vector2 CalculateScale(QT_MapMarker marker)
        {
            Vector2 referencePosition = new Vector2(ReferenceCamera.transform.position.x, ReferenceCamera.transform.position.z);
            float distance = Vector2.Distance(referencePosition, marker.Object.Position(QT_MapType.Map3D));

            float scale = 0f;

            if (distance < MaxRenderDistance)
                scale = Mathf.Clamp(1 - distance / MaxRenderDistance, MinScale, MaxScale);

            Debug.Log($"Marker: {marker.name} | Distance: {distance:F2} | Scale: {scale:F2}");

            return new Vector2(scale, scale);
        }


        /// <summary>
        /// Creates a new marker on the compass bar, based on the given object.
        /// </summary>
        /// <param name="obj">The GameObject with a QT_MapObject on it.</param>
        public void AddMarker(QT_MapObject obj)
        {
            QT_MapMarker marker = Instantiate(MarkerPrefab, markersTransform);
            marker.Initialize(obj, MarkerSize);
            Markers.Add(marker);
        }
    }
}