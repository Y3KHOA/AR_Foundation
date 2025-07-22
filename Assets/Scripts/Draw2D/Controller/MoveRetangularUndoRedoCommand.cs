public class MoveRetangularUndoRedoCommand : IUndoRedoCommand
{   
    private CheckpointManager checkpointManager;


    public MoveRetangularUndoRedoCommand(MoveRoomData moveRoomData)
    {

        this.data = moveRoomData;
        checkpointManager = CheckpointManager.Instance;
    }

    private MoveRoomData data;


    public void Redo()
    {
        data.MovingObject.position = data.NewPosition;
        RoomStorage.UpdateOrAddRoom(data.NewRoom);
        Refresh();
    }

    public void Undo()
    {
        data.MovingObject.position = data.OldPosition;
        RoomStorage.UpdateOrAddRoom(data.OldRoom);
        Refresh();
    }

    private void Refresh()
    {
        checkpointManager.DrawingTool.ClearAllLines();
        checkpointManager.RedrawAllRooms();
    }

}