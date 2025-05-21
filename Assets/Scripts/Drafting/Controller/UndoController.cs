// using System.Collections.Generic;
// using UnityEngine;

// public class UndoController : MonoBehaviour
// {
//     public CheckpointManager checkpointManager;
//     public DrawingTool drawingTool;
//     public GameObject checkpointPrefab;
//     public Transform parentTransform;

//     private List<CheckpointState> undoStates = new();
//     private const int maxUndoSteps = 10;

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.U))
//         {
//             Debug.Log("Manual U key pressed → Undo");
//             OnUndoButtonClick();
//         }
//     }

//     public void SaveState()
//     {
//         var state = new CheckpointState();

//         foreach (var cp in checkpointManager.CurrentCheckpoints)
//             state.checkpointPositions.Add(cp.transform.position);

//         foreach (var line in checkpointManager.wallLines)
//             state.wallLines.Add((line.start, line.end, line.type));

//         if (undoStates.Count >= maxUndoSteps)
//             undoStates.RemoveAt(0);

//         undoStates.Add(state.Clone());
//     }

//     public void OnUndoButtonClick()
//     {
//         Debug.Log("Undo button clicked llll");
//         if (undoStates.Count == 0) return;

//         CheckpointState last = undoStates[^1];
//         undoStates.RemoveAt(undoStates.Count - 1);

//         // Xóa checkpoint hiện tại
//         foreach (var cp in checkpointManager.CurrentCheckpoints)
//             DestroyImmediate(cp);
//         checkpointManager.CurrentCheckpoints.Clear();

//         // Xóa line hiện tại
//         checkpointManager.wallLines.Clear();
//         drawingTool.ClearAllLines();

//         // Khôi phục checkpoint
//         foreach (var pos in last.checkpointPositions)
//         {
//             GameObject cp = Instantiate(checkpointPrefab, pos, Quaternion.identity, parentTransform);
//             checkpointManager.CurrentCheckpoints.Add(cp);
//         }

//         // Khôi phục line
//         foreach (var line in last.wallLines)
//         {
//             checkpointManager.wallLines.Add(new WallLine(line.start, line.end, line.type));
//             drawingTool.DrawLineAndDistance(line.start, line.end);
//         }
//     }
// }
