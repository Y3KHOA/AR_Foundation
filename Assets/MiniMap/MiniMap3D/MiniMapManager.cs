// using UnityEngine;

// public class MiniMapManager : MonoBehaviour
// {
//     [Header("MiniMap Settings")]
//     public GameObject modelRoot;         // Gốc mô hình chính đã dựng
//     public Vector3 offset = new Vector3(0.4f, 0.4f, 0.8f); // Vị trí góc nhìn
//     public float scaleFactor = 0.1f;     // Tỉ lệ thu nhỏ

//     private GameObject miniMapInstance;
//     private Camera targetCamera;

//     void Start()
//     {
//         if (modelRoot == null)
//         {
//             Debug.LogWarning("MiniMapManager: Chưa gán modelRoot.");
//             return;
//         }

//         CreateMiniMap();
//     }

//     void LateUpdate()
//     {
//         if (miniMapInstance != null && targetCamera != null)
//         {
//             // Luôn hướng về cùng phương ngang (theo trục Y) với camera
//             float cameraY = targetCamera.transform.eulerAngles.y;
//             miniMapInstance.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);
//         }
//     }

//     void CreateMiniMap()
//     {
//         miniMapInstance = Instantiate(modelRoot);
//         miniMapInstance.name = "MiniMapModel";
//         miniMapInstance.transform.SetParent(targetCamera.transform, true);

//         // Vị trí MiniMap nằm trước camera một khoảng, lệch lên góc
//         miniMapInstance.transform.localPosition = offset;
//         miniMapInstance.transform.localRotation = Quaternion.identity;
//         miniMapInstance.transform.localScale = Vector3.one * scaleFactor;

//         // Ẩn collider (không tương tác)
//         foreach (var col in miniMapInstance.GetComponentsInChildren<Collider>())
//         {
//             Destroy(col);
//         }

//         // Gán layer nếu bạn muốn lọc riêng
//         SetLayerRecursively(miniMapInstance, LayerMask.NameToLayer("MiniMap"));

//         Debug.Log("MiniMap đã tạo xong.");
//     }

//     void SetLayerRecursively(GameObject obj, int layer)
//     {
//         obj.layer = layer;
//         foreach (Transform child in obj.transform)
//         {
//             SetLayerRecursively(child.gameObject, layer);
//         }
//     }
// }
