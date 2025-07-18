using UnityEngine;
using UnityEngine.UI;

public class ClearAllRoomsButton : MonoBehaviour
{
    [Header("References")]
    public Button clearAllButton;

    public CheckpointManager checkpointManager;

    private RoomInfoDisplay roomInfoDisplay; // Tham chiếu đến CheckpointManager để điều khiển vẽ
    [SerializeField] private ToggleGroupUI toggleGroupUI;
    private bool isClearingAll = false;
    void Start()
    {


        if (clearAllButton != null)
            clearAllButton.onClick.AddListener(OnClearAllClicked);
        else
            Debug.LogError("Chưa gán ClearAllButton!");

        if (checkpointManager == null)
            Debug.LogError("Chưa gán CheckpointManager!");
    }

    void OnClearAllClicked()
    {
        var popup = Instantiate(ModularPopup.Prefab);
        popup.AutoFindCanvasAndSetup();
        popup.Header = "Bạn có chắc muốn xóa TẤT CẢ các Room?\nDữ liệu sẽ mất vĩnh viễn!";
        popup.ClickYesEvent = () =>
        {
            Debug.Log("Người dùng xác nhận: Xóa tất cả!");
            ClearEverything();

            if (roomInfoDisplay != null)
            {
                roomInfoDisplay.ResetState();
                toggleGroupUI.ToggleOffAll();
            }
            BackgroundUI.Instance.Hide();
        };
        popup.EventWhenClickButtons = () =>
        {
            BackgroundUI.Instance.Hide();

        };
        popup.autoClearWhenClick = true;
        
        BackgroundUI.Instance.Show(popup.gameObject, null);
        // PopupController.Show(
        //     "Bạn có chắc muốn xóa TẤT CẢ các Room?\nDữ liệu sẽ mất vĩnh viễn!",
        //     onYes: () =>
        //     {
        //         Debug.Log("Người dùng xác nhận: Xóa tất cả!");
        //         ClearEverything();
        //     },
        //     onNo: () => { Debug.Log("Người dùng hủy bỏ xóa tất cả."); }
        // );
    }

    void ClearEverything()
    {
        roomInfoDisplay= FindFirstObjectByType<RoomInfoDisplay>();

        // 1) Xóa Room trong RoomStorage
        RoomStorage.rooms.Clear();

        // 2) Xóa mesh floor
        // var floors = GameObject.FindObjectsOfType<RoomMeshController>();
        var floors = GameObject.FindObjectsByType<RoomMeshController>(FindObjectsSortMode.None);
        foreach (var floor in floors)
        {
            Destroy(floor.gameObject);
        }

        // 3) Xóa checkpoints prefab
        foreach (var loop in checkpointManager.AllCheckpoints)
        {
            foreach (var cp in loop)
            {
                if (cp != null) Destroy(cp);
            }
        }

        checkpointManager.AllCheckpoints.Clear();

        // 4) Xóa dữ liệu tạm
        checkpointManager.DeleteCurrentDrawingData();

        // 5) Clear line trong DrawingTool
        checkpointManager.DrawingTool.ClearAllLines();

        // roomInfoDisplay.ClearText();

        Debug.Log("Đã xóa toàn bộ Room, checkpoint, mesh, line!");
    }
}