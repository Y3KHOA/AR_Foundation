using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Security.Cryptography;
using NUnit;
using System;

public class RoomDivider : MonoBehaviour
{
    private GameManager gameManager;

    public LineRenderer lineRenderer;

    private bool isDrawing = false;
    private SizePointManager sizePointManager;
    private List<Vector3> points = new List<Vector3>();
    private List<int> hitIndexList = new List<int>();
    private Camera mainCamera;
    private float angleThreshold = 45f;
    private GameObject roomParent;
    private int indexLineRenderer = 0;
    private Vector3 firstTouchPos = new Vector3();
    private Vector3 lastTouchPos = new Vector3();
    public int firstWallIndex = -1;
    public int lastWallIndex = -1;
    private bool hasMovedPastFirstTouch = false;

    private void Start()
    {
        gameManager = GameManager.instance;
        mainCamera = Camera.main;
        lineRenderer.useWorldSpace = true; // Đảm bảo LineRenderer sử dụng World Space
        lineRenderer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameManager.GetDrawingStatus())
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing();
            }
            else if (Input.GetMouseButton(0) && isDrawing)
            {
                UpdateDrawing();
            }
            else if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                EndDrawing();
                gameManager.DeactivateDrawing();
            }
        }
    }

    private void StartDrawing()
    {
        //Components
        isDrawing = true;
        sizePointManager = gameManager.itemIndex.sizePointManager;
        roomParent = gameManager.itemIndex.roomParent;
        points.Clear();
        lineRenderer.gameObject.SetActive(true);
        Vector3 startPoint = GetMouseWorldPosition();
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(indexLineRenderer, startPoint);
    }

    private void UpdateDrawing()
    {
        Vector3 newPoint = GetMouseWorldPosition();

        // Cập nhật LineRenderer
        indexLineRenderer++;
        lineRenderer.positionCount = indexLineRenderer + 1;
        lineRenderer.SetPosition(indexLineRenderer, newPoint);

        bool hasHitWall = false;
        Vector3 hitPoint = Vector3.zero;
        Wall hitWall = null;
        float radius = 0.3f;

        Ray ray = new Ray(newPoint + Vector3.forward * 0.1f, Vector3.back);
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, 50f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Wall"))
            {
                hitPoint = hit.point;
                hasHitWall = true;
                hitWall = hit.collider.GetComponent<Wall>();
                break; // Lấy va chạm đầu tiên
            }
        }

        // Ghi nhận lần chạm đầu tiên vào tường
        if (hasHitWall)
        {
            if (firstTouchPos == Vector3.zero)
            {
                firstTouchPos = hitPoint;
                points.Add(hitPoint);
                firstWallIndex = hitWall.index;
                hasMovedPastFirstTouch = false; // Reset trạng thái di chuyển
            }
            else if (lastTouchPos == Vector3.zero && hasMovedPastFirstTouch)
            {
                float distanceToFirst = Vector3.Distance(hitPoint, firstTouchPos);

                // Chỉ ghi nhận lastTouchPos nếu đã di chuyển xa khỏi firstTouchPos
                if (distanceToFirst > 1f)
                {
                    lastTouchPos = hitPoint;
                    points.Add(hitPoint);
                    lastWallIndex = hitWall.index;
                }
            }
        }

        if (firstTouchPos != Vector3.zero && lastTouchPos == Vector3.zero)
        {
            float moveDistance = Vector3.Distance(points[points.Count - 1], newPoint);

            if (moveDistance > 0.5f) // Đảm bảo có sự di chuyển trước khi đánh dấu
            {
                hasMovedPastFirstTouch = true;
            }

            if (moveDistance > 1f)
            {
                points.Add(newPoint);
            }
        }
    }

    private void EndDrawing()
    {
        isDrawing = false;

        if(firstWallIndex == -1 || lastWallIndex == -1)
        {
            ResetData();
            return;
        }  
        CheckWallIntersections();
        ProcessLineSegments();

        List<Vector3> finalPoints = new List<Vector3>();
        Vector3 startPoint = points[0];
        Vector3 endPoint = points[points.Count - 1];
        Vector3 direction = endPoint - startPoint;
        Vector3 normalizedDirection = direction.normalized;
        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        string drawDirection = "Khong xac dinh ?"; // Mặc định nếu không rơi vào vùng nào

        if (angle >= -2f && angle < 2f)
            drawDirection = "Right →";
        else if (angle >= 2f && angle < 38f)
        {
            drawDirection = "Top-Right ↗";

            finalPoints.Add(points[points.Count - 1]);

            int begin = lastWallIndex;
            int end = firstWallIndex + 1;

            if (begin > end)
            {
                for (int i = end; i >= 0; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }

                for (int i = sizePointManager.sizePointList.Count - 1; i >= begin; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }
            }
            else
            {
                for (int i = lastWallIndex; i >= firstWallIndex + 1; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }

            }

            //Khép kín points
            for (int i = 0; i < points.Count - 1; i++)
            {
                finalPoints.Add(points[i]);
            }
        }
        else if (angle >= 38f && angle < 88f)  // Mở rộng vùng Lên-Phải
            drawDirection = "Top-right ⬈";
        else if (angle >= 88f && angle < 92f)  // Thu hẹp vùng Lên
            drawDirection = "Top ↑";
        else if (angle >= 92f && angle < 142f) // Mở rộng vùng Lên-Trái
            drawDirection = "Top-left ⬉";
        else if (angle >= 142f && angle < 178f)
            drawDirection = "Top-left ↖";
        else if (angle >= 178f || angle < -178f)
            drawDirection = "Left ←";
        else if (angle >= -178f && angle < -142f)
            drawDirection = "Down-left ↙";
        else if (angle >= -142f && angle < -92f) // Mở rộng vùng Xuống-Trái
            drawDirection = "Down-left ⬋";
        else if (angle >= -92f && angle < -88f)  // Thu hẹp vùng Xuống
            drawDirection = "Down ↓";
        else if (angle >= -88f && angle < -38f)  // Mở rộng vùng Xuống-Phải
        {
            drawDirection = "Down-right ⬊";
        }
        else if (angle >= -38f && angle < -2f)
        {
            drawDirection = "Down-right ↘";

            finalPoints.Add(points[points.Count - 1]);

            int begin = lastWallIndex;
            int end = firstWallIndex + 1;

            if (begin < end)
            {
                for (int i = end; i >= 0; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }

                for (int i = sizePointManager.sizePointList.Count - 1; i >= begin; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }
            }
            else
            {
                Debug.Log("123");
                for (int i = lastWallIndex; i >= firstWallIndex + 1; i--)
                {
                    if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
                    {
                        finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
                    }
                }

            }

            //Khép kín points
            for (int i = 0; i < points.Count - 1; i++)
            {
                finalPoints.Add(points[i]);
            }
        }

        Debug.Log("Người dùng vẽ theo hướng: " + drawDirection);

        ////Tìm sizepoint corner theo hướng
        //if (lastWallIndex > firstWallIndex)
        //{
        //    finalPoints.Add(points[points.Count - 1]);
        //    for (int i = lastWallIndex; i >= firstWallIndex + 1; i--)
        //    {
        //        if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
        //        {
        //            finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
        //        }
        //    }

        //    //Khép kín points
        //    for (int i = 0; i < points.Count - 1; i++)
        //    {
        //        finalPoints.Add(points[i]);
        //    }
        //}
        //else
        //{
        //    finalPoints.Add(points[points.Count - 1]);
        //    for (int i = lastWallIndex + 1; i <= firstWallIndex; i++)
        //    {
        //        if (sizePointManager.sizePointList[i].pointType == Shared.SizePointType.Corner)
        //        {
        //            finalPoints.Add(sizePointManager.sizePointList[i].transform.position);
        //        }
        //    }

        //    //Khép kín points
        //    for (int i = 0; i < points.Count - 1; i++)
        //    {
        //        finalPoints.Add(points[i]);
        //    }
        //}
        //if (IsClockwise(finalPoints))
        //{
        //    finalPoints.Reverse();
        //}

        UpdateLineRenderer(finalPoints);
        DrawWallBetweenCutPoints(finalPoints);
        ResetData();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPoint = Vector3.zero;
        Vector3 screenPoint = Input.mousePosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(gameManager.guiCanvasManager.board2dRect, screenPoint, mainCamera, out worldPoint);
        return worldPoint;
    }

    private void CheckWallIntersections()
    {
        if (hitIndexList.Count > 1)
        {
            // Tạo danh sách mới chỉ chứa các điểm hợp lệ
            List<Vector3> trimmedPoints = new List<Vector3>();
            for (int i = hitIndexList[0] + 1; i <= hitIndexList[1] + 1.5f; i++)
            {
                trimmedPoints.Add(points[i]);
            }

            points = trimmedPoints;
        }
    }

    private void ProcessLineSegments()
    {
        if (points.Count < 3) return;

        List<Vector3> finalPoints = new List<Vector3> { points[0] }; // Luôn giữ điểm đầu tiên
        int i = 1; // Bắt đầu từ điểm thứ hai

        while (i < points.Count - 1)
        {
            Vector3 prev = finalPoints[finalPoints.Count - 1]; // Lấy điểm cuối trong danh sách đã chọn
            Vector3 current = points[i];
            Vector3 next = points[i + 1];

            float angle = Vector3.Angle(next - current, current - prev);

            if (angle > angleThreshold) // Nếu có góc nhọn, giữ lại điểm này
            {
                finalPoints.Add(current);
            }

            i++; // Tiếp tục kiểm tra điểm tiếp theo
        }

        finalPoints.Add(points[points.Count - 1]); // Luôn giữ điểm cuối cùng

        // Cập nhật danh sách điểm vẽ
        points = finalPoints;
    }

    private void UpdateLineRenderer(List<Vector3> corners)
    {
        lineRenderer.positionCount = corners.Count;
        for (int i = 0; i < corners.Count; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }
    }

    private void DrawWallBetweenCutPoints(List<Vector3> corners)
    {
        if (corners.Count < 2) return; // Cần ít nhất 2 điểm để tạo hình

        GameObject newItemObj = Instantiate(gameManager.itemHasChosen.itemCreatedPrefab, roomParent.transform);
        Vector3 pos = newItemObj.transform.position;
        pos.z = -1;
        newItemObj.transform.position = pos;

        ItemCreated newItem = newItemObj.GetComponent<ItemCreated>();
        Item item = new Item();
        item.itemName = "Phòng";
        item.kindsOfItem.Add("Kết cấu");
        item.kindsOfItem.Add("Thao tác");
        newItem.item = item;

        List<Vector3> localPoints = new List<Vector3>();
        for(int i = 0; i < corners.Count; i++) 
        {
            Vector3 vector3 = newItemObj.transform.InverseTransformPoint(corners[i]);
            vector3.z = 0;
            localPoints.Add(vector3);
        }

        newItem.sizePointManager.item = item;
        //newItem.sizePointManager.CreateBackgroundMeshh(localPoints.ToArray());
        newItem.sizePointManager.DrawOutline(localPoints);
        newItem.sizePointManager.CreateSizePoints();
        newItem.sizePointManager.EnableSizePoint(false);
        newItem.sizePointManager.EnableEdgeText(false);
        gameManager.createdItems2DList.Add(newItem);
    }
    
    private void ResetData()
    {
        points = new List<Vector3>();
        hitIndexList = new List<int>();
        indexLineRenderer = 0;
        lineRenderer.positionCount = 0;
        lineRenderer.gameObject.SetActive(false);
        firstTouchPos = Vector3.zero;
        lastTouchPos = Vector3.zero;
        firstWallIndex = -1;
        lastWallIndex = -1;
    }

    private bool IsClockwise(List<Vector3> points)
    {
        float sum = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];

            sum += (p2.x - p1.x) * (p2.y + p1.y);
        }
        return sum < 0; // Trả về true nếu là chiều kim đồng hồ
    }

}