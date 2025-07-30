using UnityEngine;
using UnityEngine.UI;

public class CustomGridLayoutCellsize : MonoBehaviour
{
    // [SerializeField] private float widthScale;
    //
    // [SerializeField] private GridLayoutGroup gridLayoutGroup;
    //
    // private int previousWidth;
    //
    // private void Awake()
    // {
    //     gridLayoutGroup = GetComponent<GridLayoutGroup>();
    //     gridLayoutGroup.cellSize.Set(Screen.width * widthScale, gridLayoutGroup.cellSize.y);
    //     previousWidth = Screen.width;
    //     RefreshCellSize();
    //
    // }
    //
    // private void RefreshCellSize()
    // {
    //     Debug.Log("Width scale change");
    //     var fitSize = Screen.width > Screen.height ? Screen.width : Screen.height;
    //     gridLayoutGroup.cellSize = new Vector2(fitSize, gridLayoutGroup.cellSize.y);
    //
    // }
    //
    // private void Update()
    // {
    //     if (Screen.width != previousWidth)
    //     {
    //         RefreshCellSize();
    //         previousWidth = Screen.width;
    //     }
    // }
}