using System.Collections.Generic;
using UnityEngine;

public class DeleteAllRoomCommand : IUndoRedoCommand
{
    private List<Delete_RoomData> datas;

    public DeleteAllRoomCommand(List<Delete_RoomData> datas)
    {
        this.datas = datas;
    }

    public ClearAllRoomsButton ClearAllRoom { get; set; }

    public void Undo()
    {
        // spawn lại
        foreach (var item in datas)
        {
            CheckpointManager.Instance.CreateRoomByRoomData(item.room,item.position);
        }
    }

    public void Redo()
    {
        // xóa hết
        ClearAllRoom.ClearEverything(false);
    }
}