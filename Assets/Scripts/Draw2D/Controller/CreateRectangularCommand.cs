using UnityEngine;

public class CreateRectangularCommand : IUndoRedoCommand
{
    private CheckpointManager checkPointManager;
    private RectangularCreatingData data;

    public CreateRectangularCommand(RectangularCreatingData data)
    {
        checkPointManager = CheckpointManager.Instance;
        this.data = data;
    }

    public void Undo()
    {
        // get data
        var roomID = data.RoomID;
        var room = RoomStorage.GetRoomByID(roomID);
        
        // get list
        var roomFloorMap = checkPointManager.RoomFloorMap;
        var rooms = RoomStorage.rooms;
        
        // remove in data
        var roomMesh = roomFloorMap[data.RoomID];
        GameObject.Destroy(roomMesh.gameObject);
        ClearCheckPoint(room.ID);

        rooms.Remove(room);
        roomFloorMap.Remove(roomID);
        
        checkPointManager.RedrawAllRooms();
        checkPointManager.DrawingTool.ClearAllLines();
        
    }

    public void Redo()
    {
        // create new room
        CheckpointManager.Instance.
            CreateRectangleRoom(data.width, data.heigh, data.position, data.RoomID, false);

    }
    
    private void ClearCheckPoint(string RoomID)
    {
        
        // delete check point is game object
        var mapping =
            checkPointManager.AllCheckpoints.Find(loop => checkPointManager.FindRoomIDForLoop(loop) == RoomID);
        if (mapping != null)
        {
            for (int index = 0; index < mapping.Count; index++)
            {
                GameObject.Destroy(mapping[index].gameObject);
            }

            checkPointManager.AllCheckpoints.Remove(mapping);
        }
    }
}