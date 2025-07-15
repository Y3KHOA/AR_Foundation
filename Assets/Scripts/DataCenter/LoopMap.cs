using System.Collections.Generic;
using UnityEngine;

public class LoopMap
{
    public string RoomID;
    public List<GameObject> CheckpointsGO;

    public LoopMap(string id, List<GameObject> checkpoints)
    {
        RoomID = id;
        CheckpointsGO = checkpoints;
    }
}

