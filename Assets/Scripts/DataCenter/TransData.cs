using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TransData : MonoBehaviour
{

    public BtnController btnController; // Script chứa dữ liệu tọa độ

    public static TransData Instance { get; private set; }

    public List<List<Vector2>> allPoints;
    public List<List<float>> allHeights;
    private List<List<WallLine>> allWallLines = new List<List<WallLine>>();
    public List<Room> rooms; // Gán từ bên ngoài hoặc thông qua BtnController

    public float Area;
    public float Perimeter;
    public float Ceiling;

    private bool isDataChanged;  // Biến flag để theo dõi sự thay đổi của dữ liệu

    void Awake()
    {
        // Gán Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Nếu đã có một instance khác => hủy
        }
        else
        {
            Instance = this;
        }
        isDataChanged = false;  // Khởi tạo mặc định là chưa thay đổi
    }

    void Update()
    {
        // Kiểm tra nếu dữ liệu đã thay đổi và đồng bộ lại
        if (DataTransfer.Instance.IsDataChanged())
        {
            SyncDataToDataTransfer();
            ResetDataChangedFlag();  // Đặt lại flag sau khi đồng bộ
        }
    }

    // Cập nhật dữ liệu từ DataTransfer
    public void SyncDataFromDataTransfer()
    {
        allPoints = DataTransfer.Instance.GetAllPoints();
        allHeights = DataTransfer.Instance.GetAllHeights();
        allWallLines= DataTransfer.Instance.GetAllWallLines();
    }

    // Đồng bộ dữ liệu vào DataTransfer
    public void SyncDataToDataTransfer()
    {
        if (DataTransfer.Instance.IsDataChanged())
        {
            DataTransfer.Instance.SetAllPoints(allPoints);
            DataTransfer.Instance.SetAllHeights(allHeights);
            Debug.Log("Dữ liệu đã được đồng bộ");
        }
        else
        {
            Debug.Log("Dữ liệu chưa thay đổi, không cần đồng bộ");
        }
    }

    // Đặt lại flag sau khi đồng bộ
    private void ResetDataChangedFlag()
    {
        isDataChanged = false;
    }

    // Truyền dữ liệu từ BtnController sang DataTransfer và TransData
    public void TransferData()
    {
        //lấy dữ liệu từ RoomStorage
        List<List<Vector2>> allProjectedPoints = new List<List<Vector2>>();
        List<List<float>> allHeightsList = new List<List<float>>();
        List<List<WallLine>> allWallLines = new List<List<WallLine>>();

        foreach (Room room in RoomStorage.rooms)
        {
            // Lưu điểm 2D
            List<Vector2> path2D = new List<Vector2>(room.checkpoints);
            Debug.Log("Done Room checkpoint.count: " + room.checkpoints.Count);
            Debug.Log("Done Room checkpoints.position: " + room.checkpoints[room.checkpoints.Count - 1]);
            allProjectedPoints.Add(path2D);

            // Lưu chiều cao
            List<float> heightList = new List<float>(room.heights);
            Debug.Log("Done Room heights.count: " + room.heights.Count);
            Debug.Log("Done Room heights " + room.heights);
            allHeightsList.Add(heightList);

            // Lưu các WallLines đặc biệt
            List<WallLine> roomWallLines = new List<WallLine>(room.wallLines);
            allWallLines.Add(roomWallLines);
        }

        // Lưu vào DataTransfer
        DataTransfer.Instance.SetAllPoints(allProjectedPoints);
        DataTransfer.Instance.SetAllHeights(allHeightsList);
        DataTransfer.Instance.SetAllWallLines(allWallLines);

        DataTransfer.Instance.AreaValue = Area;
        DataTransfer.Instance.PerimeterValue = Perimeter;
        DataTransfer.Instance.CeilingValue = Ceiling;

        // Lưu vào TransData
        this.allPoints = allProjectedPoints;
        this.allHeights = allHeightsList;
        this.allWallLines = allWallLines;
    }

    int CountTotalPoints(List<List<Vector2>> data)
    {
        int total = 0;
        foreach (var list in data)
        {
            total += list.Count;
        }
        return total;
    }
}

public class DataTransfer
{
    private static DataTransfer instance;
    private List<List<Vector2>> allPoints;
    private List<List<float>> allHeights;
    private List<List<WallLine>> allWallLines;
    private bool isDataChanged;  // Biến flag để theo dõi sự thay đổi của dữ liệu

    public float AreaValue { get; set; }
    public float PerimeterValue { get; set; }
    public float CeilingValue { get; set; }

    private DataTransfer()
    {
        allPoints = new List<List<Vector2>>();
        allHeights = new List<List<float>>();
        allWallLines = new List<List<WallLine>>();
        isDataChanged = false;  // Khởi tạo flag là false
    }

    public static DataTransfer Instance
    {
        get
        {
            if (instance == null)
                instance = new DataTransfer();
            return instance;
        }
    }

    public void SetAllPoints(List<List<Vector2>> newPoints)
    {
        if (!DataEquals(allPoints, newPoints)) // Kiểm tra sự thay đổi
        {
            allPoints = newPoints;
            isDataChanged = true; // Đánh dấu là dữ liệu đã thay đổi
        }
    }

    public List<List<Vector2>> GetAllPoints()
    {
        return allPoints;
    }

    public void SetAllHeights(List<List<float>> newHeights)
    {
        allHeights = newHeights;
    }

    public List<List<float>> GetAllHeights()
    {
        return allHeights;
    }

    public void SetAllWallLines(List<List<WallLine>> newWallLines)
    {
        allWallLines = newWallLines;
    }

    public List<List<WallLine>> GetAllWallLines()
    {
        return allWallLines;
    }
    public bool IsDataChanged()
    {
        return isDataChanged;
    }

    private bool DataEquals<T>(List<List<T>> list1, List<List<T>> list2)
    {
        if (list1 == null || list2 == null)
            return list1 == list2;

        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].Count != list2[i].Count)
                return false;

            for (int j = 0; j < list1[i].Count; j++)
            {
                if (!list1[i][j].Equals(list2[i][j]))
                    return false;
            }
        }
        return true;
    }
}
