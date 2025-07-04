using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này xử lý biểu ngữ tiêu đề nút trong trò chơi, cho phép người dùng mở cài đặt cấu hình cho vật phẩm hoặc mặt đất.
/// </summary>
public class ButtonHeaderBanner : MonoBehaviour
{
    [Header("Configuration")]
    public Configuration configuation;
    public ConfigurationButtonGroup configurationButtonGroup;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void OpenConfigOnClick()
    {
        if (!gameManager.guiCanvasManager.configCanvas.activeSelf && gameManager.itemIndex != null)
        {
            gameManager.itemIndex.CloneItemForConfig();

            gameManager.guiCanvasManager.configCanvas.SetActive(true);
            if (gameManager.itemIndex.item.CompareKindOfItem("Kết cấu"))
            {
                configuation.groundConfigCanvas.gameObject.SetActive(true);
                configuation.itemConfigCanvas.gameObject.SetActive(false);
                LoadInfomationGround();
            }
            else
            {
                configuation.itemConfigCanvas.gameObject.SetActive(true);
                configuation.groundConfigCanvas.gameObject.SetActive(false);
                LoadInfomationItem();
            }
        }
        else if (gameManager.guiCanvasManager.configCanvas.activeSelf)
        {
            gameManager.guiCanvasManager.configCanvas.SetActive(false);
            gameManager.guiCanvasManager.colorPickerCanvas.SetActive(false);
        }
    } 

    private void LoadInfomationItem()
    {
        ItemCreated itemCreated = gameManager.itemIndex;
        var temp = itemCreated.tempItem; // Dùng dữ liệu tạm

        configuation.itemConfigCanvas.itemNameInput.inputField.text = temp.itemName;
        configuation.itemConfigCanvas.lengthInput.inputField.text = temp.length.ToString();
        configuation.itemConfigCanvas.widthInput.inputField.text = temp.width.ToString();
        configuation.itemConfigCanvas.heightInput.inputField.text = temp.height.ToString();

        //
        configuation.itemConfigCanvas.itemNameInput.valueTemp = temp.itemName;
        configuation.itemConfigCanvas.lengthInput.valueTemp = temp.length.ToString();
        configuation.itemConfigCanvas.widthInput.valueTemp = temp.width.ToString();
        configuation.itemConfigCanvas.heightInput.valueTemp = temp.height.ToString();

        //Load checkbox
        configurationButtonGroup.UpdateInfomationCheckButton(itemCreated);
    }

    private void LoadInfomationGround()
    {
        ItemCreated itemCreated = gameManager.itemIndex;
        configuation.itemCreated = itemCreated;

        //Tạo inputfield theo từng cạnh
        configuation.groundConfigCanvas.InitSizeInputField(4);

        configuation.groundConfigCanvas.groundNameInput.inputField.text = itemCreated.item.itemName;
        configuation.groundConfigCanvas.groundNameInput.valueTemp = itemCreated.item.itemName;

        for (int i = 0; i < configuation.groundConfigCanvas.inputSizeList.Count; i++)
        {
            configuation.groundConfigCanvas.inputSizeList[i].inputField.text = itemCreated.item.edgeLengthList[i].ToString();
            configuation.groundConfigCanvas.inputSizeList[i].valueTemp = itemCreated.item.edgeLengthList[i].ToString();
        }

        //Load checkbox
        configurationButtonGroup.UpdateInfomationCheckButton(itemCreated);
    }
}
