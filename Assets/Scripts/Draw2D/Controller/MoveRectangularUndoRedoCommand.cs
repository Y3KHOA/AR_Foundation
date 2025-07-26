using System.Collections.Generic;
using UnityEngine;

public class MoveRectangularUndoRedoCommand : IUndoRedoCommand
{
    private CheckpointManager checkPointManager;


    public MoveRectangularUndoRedoCommand(MoveRoomData moveRoomData)
    {
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
        _data.MovingObject.transform.position = position;
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
        var roomID = data.RoomID;
        var room = RoomStorage.GetRoomByID(roomID);
        ClearCheckPoint(room.ID);
        RoomStorage.rooms.Remove(room);
        checkPointManager.DrawingTool.ClearAllLines();
        checkPointManager.RedrawAllRooms();
    }

    public void Redo()
    {
        CheckpointManager.Instance.
        CreateRectangleRoom(data.width, data.heigh, data.position, data.RoomID, false);

        var roomMesh = CheckpointManager.Instance.storedRoomMeshControllers[data.RoomID];
        GameObject.Destroy(roomMesh.gameObject);
    }

    private void ClearCheckPoint(string RoomID)
    {
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

public class RectangularCreatingData
{
    public string RoomID;
    public Vector3 position;
    public float width;
    public float heigh;
}