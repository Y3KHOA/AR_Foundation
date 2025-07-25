using System.Collections.Generic;
using UnityEngine;

public class MoveRetangularUndoRedoCommand : IUndoRedoCommand
{
    private CheckpointManager checkPointManager;


    public MoveRetangularUndoRedoCommand(MoveRoomData moveRoomData)
    {
        this._data = moveRoomData;
        checkPointManager = CheckpointManager.Instance;
    }

    private MoveRoomData _data;

    public void Undo()
    {
        Debug.Log("Undo");
        var data = _data;
        data.MovingObject.position = data.OldPosition;
        RoomStorage.UpdateOrAddRoom(data.OldRoom);
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();
        UpdateCheckPoint(RoomStorage.GetRoomByID(data.RoomID));
        LoadCheckPointPositions(data.OldCheckPointPos, data.RoomID);
        data.MovingObject.GetComponent<RoomMeshController>().GenerateMesh(data.OldRoom.checkpoints);
    }

    public void Redo()
    {
        var data = _data;
        data.MovingObject.position = data.CurrentPosition;
        RoomStorage.UpdateOrAddRoom(data.NewRoom);
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();
        UpdateCheckPoint(RoomStorage.GetRoomByID(data.RoomID));
        LoadCheckPointPositions(data.CurrentCheckPointPos, data.RoomID);
        data.MovingObject.GetComponent<RoomMeshController>().GenerateMesh(data.NewRoom.checkpoints);
    }

    private void SnapObject(Room room, List<(Vector3,Vector3)> checkPointList)
    {
        RoomStorage.UpdateOrAddRoom(room);
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();
        
        UpdateCheckPoint(RoomStorage.GetRoomByID(room.ID));
        LoadCheckPointPositions(checkPointList, room.ID);
        
        _data.MovingObject.GetComponent<RoomMeshController>().GenerateMesh(room.checkpoints);
    }

    private void UpdateCheckPoint(Room room)
    {
        var mapping =
            checkPointManager.AllCheckpoints.Find(loop => checkPointManager.FindRoomIDForLoop(loop) == room.ID);
        if (mapping != null)
        {
            var checkPoints = room.checkpoints;
            for (int index = 0; index < mapping.Count; index++)
            {
                var childPoint = mapping[index];
                var checkPointPosition = checkPoints[index];
                childPoint.transform.position = new Vector3(checkPointPosition.x, 0, checkPointPosition.y);
            }
        }
    }
   
    private void LoadCheckPointPositions(List<(Vector3, Vector3)> positions, string roomID)
    {
        if (checkPointManager.tempDoorWindowPoints.TryGetValue(roomID, out var doorsInRoom))
        {
            for (int index = 0; index < doorsInRoom.Count; index++)
            {
                var item = doorsInRoom[index];
                item.p1.transform.position = positions[index].Item1;
                item.p2.transform.position = positions[index].Item2;
            }
        }
    }
    
}