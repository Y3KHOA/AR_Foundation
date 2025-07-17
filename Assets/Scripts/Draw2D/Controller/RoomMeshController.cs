using UnityEngine;
using System.Collections.Generic;

public class RoomMeshController : MonoBehaviour
{
    public string RoomID;
    private Vector3 dragStartWorldPos;
    private Vector3 roomOriginalOffset;
    public bool isDragging = false;

    [Header("Floor Material (optional)")]
    [SerializeField] private Material floorMaterial;

#if UNITY_EDITOR || UNITY_STANDALONE
    // PC: vẫn dùng OnMouseDown/Drag/Up
#else
void Update()
{
    if (!PenManager.isPenActive) return;

    if (Input.touchCount == 1)
    {
        Touch touch = Input.GetTouch(0);

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                    {
                        dragStartWorldPos = hit.point;
                        isDragging = true;
                        Debug.Log($"[RoomMeshController] TOUCH Began: RoomID={RoomID}");

                        var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
                        if (checkpointMgr != null) checkpointMgr.IsDraggingRoom = true;
                    }
                }
                break;

            case TouchPhase.Moved:
                if (!isDragging) return;
                DragRoom(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                var mgr = FindFirstObjectByType<CheckpointManager>();
                if (mgr != null) mgr.IsDraggingRoom = false;
                break;
        }
    }
}
#endif

    void DragRoom(Vector2 screenPos)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 currentPos = ray.GetPoint(distance);
            Vector3 delta = currentPos - dragStartWorldPos;
            dragStartWorldPos = currentPos;

            transform.position += delta;

            // Cập nhật Room + checkpoint + redraw
            Room room = RoomStorage.GetRoomByID(RoomID);
            if (room != null)
            {
                for (int i = 0; i < room.checkpoints.Count; i++)
                {
                    Vector2 old = room.checkpoints[i];
                    room.checkpoints[i] = new Vector2(old.x + delta.x, old.y + delta.z);
                }

                for (int i = 0; i < room.wallLines.Count; i++)
                {
                    room.wallLines[i].start += delta;
                    room.wallLines[i].end += delta;
                }

                RoomStorage.UpdateOrAddRoom(room);

                var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
                if (checkpointMgr != null)
                {
                    var mapping = checkpointMgr.AllCheckpoints.Find(loop => checkpointMgr.FindRoomIDForLoop(loop) == RoomID);
                    if (mapping != null)
                    {
                        foreach (var cp in mapping) cp.transform.position += delta;
                    }
                    checkpointMgr.DrawingTool.ClearAllLines();
                    checkpointMgr.RedrawAllRooms();
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
    }

    public void GenerateMesh(List<Vector2> checkpoints)
    {
        Debug.Log($"[RoomMeshController] GenerateMesh: RoomID={RoomID}, checkpoints={checkpoints.Count}");

        // Offset: cộng thêm transform.position để vẽ đúng vị trí hiện tại
        Vector3 offset = transform.position;

        List<Vector2> offsetPoints = new List<Vector2>();
        foreach (var pt in checkpoints)
        {
            offsetPoints.Add(new Vector2(pt.x - offset.x, pt.y - offset.z)); // dùng offset ngược lại
        }

        Mesh mesh = MeshGenerator.CreateRoomMesh(offsetPoints);
        Debug.Log($"[RoomMeshController] Mesh vertices: {mesh.vertexCount}, triangles: {mesh.triangles.Length}");

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        GetComponent<MeshFilter>().mesh = mesh;

        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
    }

    private void OnMouseDown()
    {
        if (!PenManager.isPenActive) return;

        var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
        if (checkpointMgr != null)
        {
            PenManager.isRoomFloorBeingDragged = true;
            checkpointMgr.IsDraggingRoom = true;
        }

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            dragStartWorldPos = ray.GetPoint(distance);
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        if (!PenManager.isPenActive) return;
        if (!isDragging) return;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
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

                for (int i = 0; i < room.wallLines.Count; i++)
                {
                    room.wallLines[i].start += delta;
                    room.wallLines[i].end += delta;
                }

                RoomStorage.UpdateOrAddRoom(room);

                // Cập nhật checkpoint GameObjects bên ngoài
                // CheckpointManager checkpointMgr = FindObjectOfType<CheckpointManager>();
                var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
                if (checkpointMgr != null)
                {
                    var mapping = checkpointMgr.AllCheckpoints.Find(loop => checkpointMgr.FindRoomIDForLoop(loop) == RoomID);
                    if (mapping != null)
                    {
                        foreach (var cp in mapping)
                        {
                            cp.transform.position += delta;
                        }
                    }
                    checkpointMgr.DrawingTool.ClearAllLines();
                    checkpointMgr.RedrawAllRooms();
                }
            }
        }
    }
    private void OnMouseUp()
    {
        isDragging = false;

        var checkpointMgr = FindFirstObjectByType<CheckpointManager>();
        if (checkpointMgr != null)
        {
            PenManager.isRoomFloorBeingDragged = false;
            checkpointMgr.IsDraggingRoom = false;
        }
    }
}
