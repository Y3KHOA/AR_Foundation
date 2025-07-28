using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveRectangularUndoRedoCommand : IUndoRedoCommand
{
    private CheckpointManager checkPointManager;
    private string ID = String.Empty;

    public MoveRectangularUndoRedoCommand(MoveRoomData moveRoomData)
    {
        this.ID = Guid.NewGuid().ToString();
        this._data = moveRoomData;
        checkPointManager = CheckpointManager.Instance;
    }

    private MoveRoomData _data;

    public void Undo()
    {
        Debug.Log("Undo");
        SnapObject(_data.OldRoom, _data.OldPosition, _data.OldCheckPointPos);
    }

    public void Redo()
    {
        SnapObject(_data.NewRoom, _data.CurrentPosition, _data.CurrentCheckPointPos);
    }

    private void SnapObject(Room room, Vector3 position, List<(Vector3, Vector3)> checkPointList)
    {
        var movingObject = checkPointManager.RoomFloorMap[room.ID].transform;
        movingObject.transform.position = position;

        RoomStorage.UpdateOrAddRoom(room);
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();

        UpdateCheckPoint(RoomStorage.GetRoomByID(room.ID));
        LoadCheckPointPositions(checkPointList, room.ID);

        movingObject.GetComponent<RoomMeshController>().GenerateMesh(room.checkpoints);
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

public class RectangularCreatingData
{
    public string RoomID;
    public Vector3 position;
    public float width;
    public float heigh;
}