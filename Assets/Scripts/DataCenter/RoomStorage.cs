using System.Collections.Generic;
using System.Linq;
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
                Debug.Log("[ROOM_STORAGE] RoomID da bi thay doi" + rooms[i].ID);
                rooms[i].checkpoints = new List<Vector2>(updatedRoom.checkpoints);
                // rooms[i].wallLines = new List<WallLine>(updatedRoom.wallLines);
                rooms[i].wallLines = new List<WallLine>(updatedRoom.wallLines.Select(w => new WallLine(w)));
                // rooms[i].heights = new List<float>(updatedRoom.heights);
                // rooms[i].Compass = updatedRoom.Compass;
                // rooms[i].headingCompass = updatedRoom.headingCompass;
                // rooms[i].area = updatedRoom.area;
                // rooms[i].ceilingArea = updatedRoom.ceilingArea;
                // rooms[i].perimeter = updatedRoom.perimeter;
                // Debug.Log($"[RoomStorage] Room {updatedRoom.ID} đã được cập nhật.");
                return;
            }
        }

        Debug.Log("[ROOM_STORAGE] Them room moi" + updatedRoom.ID);
        rooms.Add(updatedRoom);
    }

    public static Room GetRoomByID(string id)
    {
        foreach (var room in rooms)
        {
            if (room.ID == id)
                return room;
        }

        Debug.LogWarning($"RoomStorage: Không tìm thấy Room với ID: {id}");
        return null;
    }

    public static List<Room> GetRoomsByGroupID(string groupID)
    {
        return rooms.Where(r => r.groupID == groupID).ToList();
    }

    public static void CheckDuplicateRoomID()
    {
        Debug.Log("Room Count: " + rooms.Count);
        HashSet<string> roomIDCheck = new();

        foreach (var item in rooms)
        {
            if (roomIDCheck.Contains(item.ID))
            {
                Debug.Log("DuplicateRoomID: " + item.ID);
                continue;
            }

            roomIDCheck.Add(item.ID);
        }
    }
}