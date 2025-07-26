using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Show2DModel : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject checkpointPrefab;
    public Drawing2D Drawing2D;

    //parent of all checkpoints
    [Header("Parent")]
    public Transform modelRoot;

    private List<LoopMap> loopMappings = new List<LoopMap>();
    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();
    public List<List<GameObject>> AllCheckpoints =>
        allCheckpoints; // Truy cập danh sách tất cả các checkpoint từ bên ngoài

    [SerializeField] private List<ToggleButtonUI> togglesButtonList = new();
    
    private ToggleButtonUI currentButton;

    private CheckpointManager checkPointManager;

    void Start()
    {
        // InitToggleButton();

        LoadPointsFromRoomStorage();
    }

    private void OnClickBtn(ToggleButtonUI toggleButtonUI)
    {
        foreach (var item in togglesButtonList)
        {
            var state = item == toggleButtonUI ? ToggleButtonUI.State.Active : ToggleButtonUI.State.DeActive;
            if (item == toggleButtonUI)
            {
                if (item == currentButton) break;
                // set current button
                currentButton = toggleButtonUI;
                currentButton.OnActive();
            }
            
            item.ChangeState(state);
        }
    }

    // === Load points from RoomStorage
    void LoadPointsFromRoomStorage()
    {
        modelRoot.gameObject.layer = LayerMask.NameToLayer("PreviewModel"); //add layer to modelRoot

        checkPointManager = FindFirstObjectByType<CheckpointManager>();

        var rooms = RoomStorage.rooms;
        if (rooms.Count == 0)
        {
            Debug.Log("Không có Room nào để hiển thị.");
            return;
        }

        foreach (var room in rooms)
        {
            // === Tạo lại checkpoint GameObject từ room.checkpoints
            List<GameObject> loopGO = new List<GameObject>();
            foreach (var pt in room.checkpoints)
            {
                Vector3 worldPos = new Vector3(pt.x, 0.01f, pt.y); // Nâng nhẹ lên để đè line
                GameObject cp = Instantiate(checkpointPrefab, worldPos, Quaternion.identity, modelRoot);
                cp.name = $"Checkpoint_{pt.x}_{pt.y}";
                SetLayerRecursively(cp, modelRoot.gameObject.layer);

                loopGO.Add(cp);
            }

            // === Lưu vào ánh xạ checkpoint<->RoomID
            allCheckpoints.Add(loopGO);
            loopMappings.Add(new LoopMap(room.ID, loopGO));

            // === Vẽ lại các wallLines
            foreach (var wl in room.wallLines)
            {
                Drawing2D.currentLineType = wl.type;
                Drawing2D.DrawLineAndDistance(wl.start, wl.end); // Nếu có tạo GameObject line, hãy gán parent = modelRoot trong hàm này

                // Nếu là cửa hoặc cửa sổ: tạo 2 điểm đầu/cuối riêng
                if (wl.type == LineType.Door || wl.type == LineType.Window)
                {
                    GameObject p1 = Instantiate(checkpointPrefab, wl.start + new Vector3(0, 0.01f, 0), Quaternion.identity, modelRoot);
                    GameObject p2 = Instantiate(checkpointPrefab, wl.end + new Vector3(0, 0.01f, 0), Quaternion.identity, modelRoot);
                    p1.name = $"{wl.type}_P1_{room.ID}";
                    p2.name = $"{wl.type}_P2_{room.ID}";

                    SetLayerRecursively(p1, modelRoot.gameObject.layer);
                    SetLayerRecursively(p2, modelRoot.gameObject.layer);

                    if (!checkPointManager.tempDoorWindowPoints.ContainsKey(room.ID))
                        checkPointManager.tempDoorWindowPoints[room.ID] = new List<(WallLine, GameObject, GameObject)>();

                    checkPointManager.tempDoorWindowPoints[room.ID].Add((wl, p1, p2));
                }
            }
        }

        Debug.Log($"[LoadPointsFromRoomStorage] Đã load lại {rooms.Count} phòng, {allCheckpoints.Count} loop.");
    }

    // === add layer cho các object trong modelRoot
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}
