using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadRoomScan : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform contentParent;
    public GameObject dataItemPrefab;
    public GameObject panelLoadRooom;

    public void OnEnable()
    {

        if (panelLoadRooom != null)
        {
            panelLoadRooom.SetActive(true);
            LoadAllRooms();
        }
    }

    public void LoadAllRooms()
    {
        // Xóa UI cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        for (int index = 0; index < RoomStorage.rooms.Count; index++)
        {
            Room room = RoomStorage.rooms[index];

            GameObject item = Instantiate(dataItemPrefab, contentParent);

            string displayName = "Room " + index;
            string areaText = "Diện tích: " + GetRoomAreaString(room);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var txt in texts)
            {
                if (txt.name.Contains("Name"))
                    txt.text = displayName;
                else if (txt.name.Contains("Area") || txt.name.Contains("Date"))
                    txt.text = areaText;
            }

            Button btn = item.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string capturedID = room.ID; // tránh closure
                btn.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetString("SelectedRoomID", capturedID);
                    SceneManager.LoadScene("AR");
                });
            }
        }
    }

    private string GetRoomAreaString(Room room)
    {
        float area = CalculatePolygonArea(room.checkpoints);
        return area.ToString("F2") + " m²";
    }

    private float CalculatePolygonArea(List<Vector2> points)
    {
        float area = 0f;
        int n = points.Count;
        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % n];
            area += (p1.x * p2.y) - (p2.x * p1.y);
        }
        return Mathf.Abs(area * 0.5f);
    }
}
