using System.Collections.Generic;
using UnityEngine;

public struct MoveRoomData
{
    public string RoomID;
    public Room OldRoom;
    public Transform MovingObject;
    public Vector3 OldPosition;
    public List<(Vector3,Vector3)> oldCheckPointPos;
}