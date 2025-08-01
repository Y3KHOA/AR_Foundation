using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearAllRoomsButton : MonoBehaviour
{
    [Header("References")]
    public Button clearAllButton;

    public CheckpointManager checkpointManager;

    private RoomInfoDisplay roomInfoDisplay; // Tham chiếu đến CheckpointManager để điều khiển vẽ
    [SerializeField] private ToggleGroupUI toggleGroupUI;
    [SerializeField] private PenManager penManager;
    [SerializeField] private DrawingTool drawingTool;


    private const string CLEAR_ALL_WARNING = "Bạn có chắc muốn xóa TẤT CẢ các Room?\nDữ liệu sẽ mất vĩnh viễn!";
    
    private bool isClearingAll = false;
    void Start()
    {
        penManager = FindFirstObjectByType<PenManager>();

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
        popup.Header = CLEAR_ALL_WARNING;
        popup.ClickYesEvent = () =>
        {
            Debug.Log("Người dùng xác nhận: Xóa tất cả!");
            ClearEverything();

            toggleGroupUI.ToggleOffAll();
            penManager.ChangeState(true );
            
            if (roomInfoDisplay != null)
            {
                roomInfoDisplay.ResetState();

            }
            // BackgroundUI.Instance.Hide();
        };
        // popup.EventWhenClickButtons = () =>
        // {
        //     BackgroundUI.Instance.Hide();
        //
        // };
        popup.autoClearWhenClick = true;
        
        // BackgroundUI.Instance.Show(popup.gameObject, null);
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

    public void ClearEverything(bool isCreateCommand = true)
    {
        if(!roomInfoDisplay)
            roomInfoDisplay = FindFirstObjectByType<RoomInfoDisplay>();
        // đảm bảo không tạo lệnh dư thừa
        if (RoomStorage.rooms.Count == 0)
        {
            isCreateCommand = false;
        }
        // 2) Xóa mesh floor
        // var floors = GameObject.FindObjectsOfType<RoomMeshController>();
        List<Delete_RoomData> deleteRoomDataList = new();
        var floors = GameObject.FindObjectsByType<RoomMeshController>(FindObjectsSortMode.None);    
        foreach (var floor in floors)
        {
            if (isCreateCommand)
            {
                var deleteRoomData = new Delete_RoomData(new Room(RoomStorage.GetRoomByID(floor.RoomID)),floor.transform.position);
                deleteRoomDataList.Add(deleteRoomData);
            }
            Destroy(floor.gameObject);
        }
        // 1) Xóa Room trong RoomStorage
        RoomStorage.rooms.Clear();

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
        drawingTool.currentLineType = LineType.Wall;

        if (isCreateCommand)
        {
            DeleteAllRoomCommand deleteAllRoomCommand = new DeleteAllRoomCommand(deleteRoomDataList);
            deleteAllRoomCommand.ClearAllRoom = this;
            UndoRedoController.Instance.AddToUndo(deleteAllRoomCommand);
        }
    }
}