using UnityEngine;

public struct MoveRoomData
{
    public string RoomID;
    public Room OldRoom;
    public Room NewRoom;
    public Transform MovingObject;
    public Vector3 OldPosition;
    public Vector3 NewPosition;
}