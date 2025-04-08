using System.Collections.Generic;
using UnityEngine;

public static class SceneStateManager
{
    private static Dictionary<string, object> sceneStates = new Dictionary<string, object>();

    public static void SaveState(string sceneName, object state)
    {
        sceneStates[sceneName] = state;
        Debug.Log($"[SceneState] Đã lưu trạng thái cho scene: {sceneName}");
    }

    public static void LoadState(object state)
    {
        if (state is MeasurementState measurementState)
        {
            RestoreMeasurementState(measurementState);
        }
    }

    private static void RestoreMeasurementState(MeasurementState state)
    {
        // Xóa các điểm cũ
        foreach (var obj in GameObject.FindGameObjectsWithTag("Checkpoint"))
        {
            GameObject.Destroy(obj);
        }

        // Tạo lại các điểm
        foreach (Vector3 pos in state.basePoints)
        {
            GameObject newPoint = GameObject.Instantiate(state.pointPrefab, pos, Quaternion.identity);
        }
    }
}
