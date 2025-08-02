using System.Collections.Generic;
using UnityEngine;

public struct MoveRoomData
{
    public string RoomID;
    public Transform MovingObject;
    // snapshoot room data
    public Room OldRoom;
    public Room NewRoom;
    // snapshoot room position
    public Vector3 OldPosition;
    public Vector3 CurrentPosition;
    // snapshoot check point of room
    public List<(Vector3,Vector3)> OldCheckPointPos;
    public List<(Vector3,Vector3)> CurrentCheckPointPos;
}