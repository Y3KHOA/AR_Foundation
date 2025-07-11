using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CheckpointState
{
    public List<Vector3> checkpointPositions = new(); // Danh sách vị trí checkpoint
    public List<(Vector3 start, Vector3 end, LineType type)> wallLines = new(); // Danh sách tường

    public CheckpointState Clone()
    {
        var copy = new CheckpointState();
        copy.checkpointPositions.AddRange(checkpointPositions);
        copy.wallLines.AddRange(wallLines);
        return copy;
    }
}
