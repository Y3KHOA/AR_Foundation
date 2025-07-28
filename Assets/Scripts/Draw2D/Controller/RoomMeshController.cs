using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class RoomMeshController : MonoBehaviour
{
    private static readonly Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
    private static Camera mainCam;
    public string RoomID;
    private Vector3 dragStartWorldPos;
    private Vector3 roomOriginalOffset;
    public bool isDragging = false;
    private Room oldRoom;
    private CheckpointManager checkPointManager;

    [Header("Floor Material (optional)")]
    [SerializeField] private Material floorMaterial;

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        checkPointManager = CheckpointManager.Instance;
    }

#if UNITY_STANDALONE
    // PC: vẫn dùng OnMouseDown/Drag/Up
#else
    void Update()
    {
        if (!PenManager.isPenActive) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnStartDrag(touch.position);

                    break;

                case TouchPhase.Moved:
                    if (!isDragging) return;
                    DragRoom(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnEndDrag();
                    break;
            }
        }
    }
#endif
    private bool CheckTouchHitThisObject(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == this.transform;
        }

        return false;
    }

    // Hàm di chuyển Room theo vị trí chạm for Android
    void DragRoom(Vector2 screenPos)
    {
        if (IsClickingOnBackgroundBlackUI(Input.mousePosition))
        {
            Debug.Log("Đang nhấn Background Black ➜ Không move Mesh");
            return;
        }

        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (floorPlane.Raycast(ray, out float distance))
        {
            Vector3 currentPos = ray.GetPoint(distance);
            Vector3 delta = currentPos - dragStartWorldPos;
            dragStartWorldPos = currentPos;

            // Di chuyển chính GameObject Mesh sàn
            transform.position += delta;

            // Update tất cả checkpoint và wallLine theo
            Room room = RoomStorage.GetRoomByID(RoomID);
            if (room != null)
            {
                for (int i = 0; i < room.checkpoints.Count; i++)
                {
                    Vector2 old = room.checkpoints[i];
                    Vector2 moved = new Vector2(old.x + delta.x, old.y + delta.z);
                    room.checkpoints[i] = moved;
                }

                for (int i = 0; i < room.extraCheckpoints.Count; i++)
                {
                    Vector2 pt = room.extraCheckpoints[i];
                    room.extraCheckpoints[i] = new Vector2(pt.x + delta.x, pt.y + delta.z);
                }

                for (int i = 0; i < room.wallLines.Count; i++)
                {
                    room.wallLines[i].start += delta;
                    room.wallLines[i].end += delta;
                }

                RoomStorage.UpdateOrAddRoom(room);

                // Cập nhật checkpoint GameObjects bên ngoài
                // CheckpointManager checkpointMgr = FindObjectOfType<CheckpointManager>();

                if (checkPointManager != null)
                {
                    // === Move checkpoint phụ (extraCheckpoints) ===
                    if (checkPointManager.RoomFloorMap.TryGetValue(RoomID, out var floorGO))
                    {
                        foreach (Transform child in floorGO.transform)
                        {
                            if (child.CompareTag("CheckpointExtra")) // <-- tag riêng cho point phụ
                            {
                                Debug.Log($"[MoveCheck] Child: {child.name}, Tag: {child.tag}");
                                // child.position += delta;
                            }
                        }
                    }

                    var mapping =
                        checkPointManager.AllCheckpoints.Find(loop =>
                            checkPointManager.FindRoomIDForLoop(loop) == RoomID);
                    if (mapping != null)
                    {
                        foreach (var cp in mapping)
                        {
                            cp.transform.position += delta;
                        }
                    }

                    // === di chuyển point door/ window theo room
                    if (checkPointManager.tempDoorWindowPoints.TryGetValue(RoomID, out var doorsInRoom))
                    {
                        foreach (var (line, p1GO, p2GO) in doorsInRoom)
                        {
                            p1GO.transform.position += delta;
                            p2GO.transform.position += delta;
                        }
                    }

                    checkPointManager.DrawingTool.ClearAllLines();
                    checkPointManager.RedrawAllRooms();
                }
            }
        }
    }

    public void Initialize(string roomID)
    {
        RoomID = roomID;

        Room room = RoomStorage.GetRoomByID(RoomID);
        if (room == null)
        {
            Debug.LogError($"RoomMeshController: Không tìm thấy Room với ID {RoomID}");
            return;
        }

        GenerateMesh(room.checkpoints);

        // Tạo MeshRenderer nếu chưa có
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Nếu chưa có material, tạo material đỏ để dễ nhìn
        if (floorMaterial == null)
        {
            // floorMaterial = new Material(Shader.Find("Standard"));
            floorMaterial = new Material(Shader.Find("Unlit/Color"));
            // floorMaterial.color = Color.red; // Đổi sang đỏ
        }

        meshRenderer.material = floorMaterial;

        // Tùy chọn: Thêm collider để click sàn
        if (GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();

        // Đảm bảo đăng ký lại RoomFloorMap
        var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
        if (checkpointMgr != null && !checkpointMgr.RoomFloorMap.ContainsKey(RoomID))
        {
            checkpointMgr.RoomFloorMap[RoomID] = this.gameObject;
            Debug.Log($"Đã tự động đăng ký RoomFloorMap[{RoomID}] = {gameObject.name}");
        }
    }

    public void GenerateMesh(List<Vector2> checkpoints)
    {
        Debug.Log($"[RoomMeshController] GenerateMesh: RoomID={RoomID}, checkpoints={checkpoints.Count}");

        Vector2 pivot = GetCentroid(checkpoints); // <== dùng pivot thật

        // Dịch điểm về local-space để tạo mesh
        List<Vector2> offsetPoints = checkpoints
            .Select(p => new Vector2(p.x - pivot.x, p.y - pivot.y))
            .ToList();

        Mesh mesh = MeshGenerator.CreateRoomMesh(offsetPoints);

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        GetComponent<MeshFilter>().mesh = mesh;

        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;

        // Đặt lại transform để khớp world-space
        transform.position = new Vector3(pivot.x, 0, pivot.y);
    }

    private Vector2 GetCentroid(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return Vector2.zero;

        float sumX = 0f, sumY = 0f;
        foreach (var p in points)
        {
            sumX += p.x;
            sumY += p.y;
        }

        return new Vector2(sumX / points.Count, sumY / points.Count);
    }

    private void OnStartDrag(Vector3 startDragPosition)
    {
        if (!CheckTouchHitThisObject(startDragPosition))
        {
            return;
        }

        if (checkPointManager != null)
        {
            PenManager.isRoomFloorBeingDragged = true;
            checkPointManager.IsDraggingRoom = true;
        }

        Ray ray = Camera.main.ScreenPointToRay(startDragPosition);
        if (floorPlane.Raycast(ray, out float distance))
        {
            dragStartWorldPos = ray.GetPoint(distance);
            isDragging = true;
        }

        oldRoom = new Room(RoomStorage.GetRoomByID(RoomID));
        oldPosition = transform.position;
        oldCheckPointList = SaveCheckPointPosition(RoomID);
    }

    private List<(Vector3, Vector3)> SaveCheckPointPosition(string RoomID)
    {
        var checkPointList = new List<(Vector3, Vector3)>();
        if (checkPointManager.tempDoorWindowPoints.TryGetValue(RoomID, out var doorsInRoom))
        {
            foreach (var (line, p1GO, p2GO) in doorsInRoom)
            {
                checkPointList.Add((p1GO.transform.position, p2GO.transform.position));
            }
        }

        return checkPointList;
    }

    private void OnEndDrag()
    {
        isDragging = false;

        if (checkPointManager != null)
        {
            PenManager.isRoomFloorBeingDragged = false;
            checkPointManager.IsDraggingRoom = false;
        }

        CreateUndoCommand();
    }

    private void OnMouseDown()
    {
        if (!PenManager.isPenActive) return;

        OnStartDrag(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        OnEndDrag();
    }

    private void OnMouseDrag()
    {
        if (!PenManager.isPenActive) return;
        if (!isDragging) return;
        DragRoom(Input.mousePosition);
    }

    private Vector2 oldPosition;
    private List<(Vector3, Vector3)> oldCheckPointList = new List<(Vector3, Vector3)>();

    private void CreateUndoCommand()
    {
        if (oldRoom == null) return;
        MoveRoomData moveObject = new MoveRoomData();
        moveObject.RoomID = RoomID;
        moveObject.MovingObject = transform;

        moveObject.OldPosition = oldPosition;
        moveObject.OldRoom = new Room(oldRoom);
        moveObject.oldCheckPointPos = new List<(Vector3, Vector3)>(oldCheckPointList);

        var command = new MoveRetangularUndoRedoCommand(moveObject);

        UndoRedoController.Instance.AddToRedo(command);
    }

    // === Hàm ko cho move trên UI
    private bool IsClickingOnBackgroundBlackUI(Vector2 screenPosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == "Background Black")
            {
                Debug.Log("Click UI trên Background Black ➜ Không cho move point");
                return true;
            }
        }

        return false;
    }
}