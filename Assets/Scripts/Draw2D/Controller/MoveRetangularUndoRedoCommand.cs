using System.Collections.Generic;
using UnityEngine;

public class MoveRetangularUndoRedoCommand : IUndoRedoCommand
{
    private CheckpointManager checkPointManager;


    public MoveRetangularUndoRedoCommand(MoveRoomData moveRoomData)
    {
        this.data = moveRoomData;
        checkPointManager = CheckpointManager.Instance;
    }

    private MoveRoomData data;

    public void Undo()
    {
        data.MovingObject.position = data.OldPosition;
        RoomStorage.UpdateOrAddRoom(data.OldRoom);
        Refresh();
        UpdateCheckPoint(RoomStorage.GetRoomByID(data.RoomID));
        LoadCheckPointPositions(data.oldCheckPointPos, data.RoomID);
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
    
    private void Refresh()
    {
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();
    }
}