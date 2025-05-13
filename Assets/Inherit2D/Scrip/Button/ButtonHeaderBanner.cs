using UnityEngine;
using UnityEngine.UI;

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
        if(!gameManager.guiCanvasManager.configCanvas.activeSelf && gameManager.itemIndex != null)
        {
            gameManager.guiCanvasManager.configCanvas.SetActive(true);
            if(gameManager.itemIndex.item.CompareKindOfItem("Kết cấu"))
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
        else if(gameManager.guiCanvasManager.configCanvas.activeSelf)
        {
            gameManager.guiCanvasManager.configCanvas.SetActive(false);
            gameManager.guiCanvasManager.colorPickerCanvas.SetActive(false);
        }    
    }    

    private void LoadInfomationItem()
    {
        ItemCreated itemCreated = gameManager.itemIndex;
        configuation.itemCreated = itemCreated;

        configuation.itemConfigCanvas.itemNameInput.inputField.text = itemCreated.item.itemName;
        configuation.itemConfigCanvas.lengthInput.inputField.text = itemCreated.item.length.ToString();
        configuation.itemConfigCanvas.widthInput.inputField.text = itemCreated.item.width.ToString();
        configuation.itemConfigCanvas.heightInput.inputField.text = itemCreated.item.height.ToString();

        //
        configuation.itemConfigCanvas.itemNameInput.valueTemp = itemCreated.item.itemName;
        configuation.itemConfigCanvas.lengthInput.valueTemp = itemCreated.item.length.ToString();
        configuation.itemConfigCanvas.widthInput.valueTemp = itemCreated.item.width.ToString();
        configuation.itemConfigCanvas.heightInput.valueTemp = itemCreated.item.height.ToString();

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
