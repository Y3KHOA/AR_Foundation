using System.Collections.Generic;
using UnityEngine;

public static class RoomStorage
{
    public static List<Room> rooms = new List<Room>();
    public static void UpdateOrAddRoom(Room updatedRoom)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].ID == updatedRoom.ID)
            {
                rooms[i].checkpoints = new List<Vector2>(updatedRoom.checkpoints);
                rooms[i].wallLines = new List<WallLine>(updatedRoom.wallLines);
                // rooms[i].heights = new List<float>(updatedRoom.heights);
                // rooms[i].Compass = updatedRoom.Compass;
                // rooms[i].headingCompass = updatedRoom.headingCompass;
                // rooms[i].area = updatedRoom.area;
                // rooms[i].ceilingArea = updatedRoom.ceilingArea;
                // rooms[i].perimeter = updatedRoom.perimeter;
                Debug.Log($"[RoomStorage] Room {updatedRoom.ID} đã được cập nhật.");
                return;
            }
        }
        rooms.Add(updatedRoom);
    }
}        
