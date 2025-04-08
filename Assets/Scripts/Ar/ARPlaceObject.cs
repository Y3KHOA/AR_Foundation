// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// public class ARMeasureTool : MonoBehaviour
// {
//     [Header("Prefabs cần thiết")]
//     public GameObject checkpointPrefab;
//     public LineManager lineManager;

//     [Header("Cài đặt đo lường")]
//     public float closeThreshold = 0.2f; // 0.2m = 20cm

//     public ARRaycastManager raycastManager;
//     private List<Vector3> points = new List<Vector3>();
//     private List<GameObject> checkpoints = new List<GameObject>();

//     private GameObject selectedCheckpoint = null;
//     private float initialPinchDistance = 0f;
//     private const float minScale = 0.05f;
//     private const float maxScale = 2.0f;

//     private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>(); // Giảm allocation

//     void Update()
//     {
//         if (Input.touchCount == 1)
//         {
//             Touch touch = Input.GetTouch(0);

//             switch (touch.phase)
//             {
//                 case TouchPhase.Began:
//                     SelectOrPlaceCheckpoint(touch.position);
//                     break;
//                 case TouchPhase.Moved:
//                     MoveSelectedCheckpoint(touch.position);
//                     break;
//                 case TouchPhase.Ended:
//                     DeselectCheckpoint();
//                     break;
//             }
//         }
//         else if (Input.touchCount == 2 && selectedCheckpoint != null)
//         {
//             HandlePinchToResize();
//         }
//     }

//     void SelectOrPlaceCheckpoint(Vector2 touchPosition)
//     {
//         if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             Pose hitPose = hits[0].pose;

//             if (TrySelectCheckpoint(hitPose.position))
//             {
//                 return;
//             }

//             PlaceCheckpoint(hitPose.position);
//         }
//     }

//     bool TrySelectCheckpoint(Vector3 position)
//     {
//         float minDistance = closeThreshold; // Chỉ chọn nếu trong khoảng cách cho phép
//         GameObject nearestCheckpoint = null;

//         foreach (var checkpoint in checkpoints)
//         {
//             float distance = Vector3.Distance(checkpoint.transform.position, position);
//             if (distance < minDistance)
//             {
//                 minDistance = distance;
//                 nearestCheckpoint = checkpoint;
//             }
//         }

//         if (nearestCheckpoint != null)
//         {
//             selectedCheckpoint = nearestCheckpoint;
//             float newScale = Mathf.Clamp(selectedCheckpoint.transform.localScale.x + 0.1f, minScale, maxScale);
//             selectedCheckpoint.transform.localScale = Vector3.one * newScale;
//             return true;
//         }

//         return false;
//     }

//     void MoveSelectedCheckpoint(Vector2 touchPosition)
//     {
//         if (selectedCheckpoint == null) return;

//         if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             selectedCheckpoint.transform.position = hits[0].pose.position;
//             lineManager.UpdateLinesAndDistances(checkpoints);
//         }
//     }

//     void HandlePinchToResize()
//     {
//         Touch touch1 = Input.GetTouch(0);
//         Touch touch2 = Input.GetTouch(1);

//         float currentDistance = Vector2.Distance(touch1.position, touch2.position);
//         if (initialPinchDistance == 0f)
//         {
//             initialPinchDistance = currentDistance;
//             return;
//         }

//         float scaleMultiplier = currentDistance / initialPinchDistance;
//         float newScale = Mathf.Clamp(selectedCheckpoint.transform.localScale.x * scaleMultiplier, minScale, maxScale);
//         selectedCheckpoint.transform.localScale = Vector3.one * newScale;

//         initialPinchDistance = currentDistance;
//     }

//     void DeselectCheckpoint()
//     {
//         if (selectedCheckpoint != null)
//         {
//             float newScale = Mathf.Clamp(selectedCheckpoint.transform.localScale.x - 0.1f, minScale, maxScale);
//             selectedCheckpoint.transform.localScale = Vector3.one * newScale;
//         }
//         selectedCheckpoint = null;
//         initialPinchDistance = 0f;
//     }

//     void PlaceCheckpoint(Vector3 newPoint)
//     {
//         if (checkpointPrefab == null)
//         {
//             Debug.LogError("checkpointPrefab chưa được gán!");
//             return;
//         }

//         // Kiểm tra nếu newPoint gần điểm đầu tiên (P1) để đóng vùng
//         if (points.Count > 2 && Vector3.Distance(newPoint, points[0]) < closeThreshold * 1.5f) // Tăng một chút để tránh sai số
//         {
//             CloseMeasurement();
//             return;
//         }

//         GameObject checkpoint = Instantiate(checkpointPrefab, newPoint, Quaternion.identity);
//         checkpoints.Add(checkpoint);
//         points.Add(newPoint);

//         if (!checkpoint.GetComponent<Collider>())
//             checkpoint.AddComponent<SphereCollider>();

//         if (points.Count > 1)
//         {
//             lineManager.DrawLineAndDistance(points[^2], newPoint);
//         }
//     }

//     void CloseMeasurement()
//     {
//         lineManager.DrawLineAndDistance(points[^1], points[0]);
//         AreaCalculator.ShowAreaText(points[0], AreaCalculator.CalculateArea(points));
//         Debug.Log($"Dien tich vung: {AreaCalculator.CalculateArea(points):F2} m2");

//         foreach (var checkpoint in checkpoints)
//         {
//             Destroy(checkpoint);
//         }

//         checkpoints.Clear();
//         points.Clear();
//     }

//     public void ResetMeasurement()
//     {
//         foreach (var obj in checkpoints)
//         {
//             Destroy(obj);
//         }
//         checkpoints.Clear();

//         foreach (var obj in lineManager.lines)
//         {
//             Destroy(obj);
//         }
//         lineManager.lines.Clear();

//         foreach (var obj in lineManager.distanceTexts)
//         {
//             Destroy(obj);
//         }
//         lineManager.distanceTexts.Clear();

//         points.Clear();

//         Debug.Log("Reset đo lường hoàn tất!");
//     }
// }
