using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lớp đại diện cho giao diện cấu hình mục, cho phép người dùng nhập thông tin về mục như tên, kích thước và các thuộc tính khác.
/// </summary>
public class ItemConfigCanvas : MonoBehaviour
{
    [Header("Input field")]
    public InputConfig itemNameInput;
    public InputConfig lengthInput;
    public InputConfig widthInput;
    public InputConfig heightInput;

    private GameManager gameManager;
    private Configuration configuration;
    private List<Vector3> oldPosList = new List<Vector3>();

    private void Start()
    {
        gameManager = GameManager.instance;
        configuration = Configuration.instance;

        //Kết thúc nhập
        itemNameInput.inputField.onEndEdit.AddListener((value) => OnInputComplete(itemNameInput, 0));
        lengthInput.inputField.onEndEdit.AddListener((value) => OnInputComplete(lengthInput, 1));
        widthInput.inputField.onEndEdit.AddListener((value) => OnInputComplete(widthInput, 1));
        heightInput.inputField.onEndEdit.AddListener((value) => OnInputComplete(heightInput, 1));
    }

    public void OnInputComplete(InputConfig inputConfig, int kind)
    {
        if (inputConfig.inputField.text.Trim() == string.Empty)
        {
            inputConfig.inputField.text = inputConfig.valueTemp;
        }
        else
        {
            float temp = 0;
            if (kind == 0)
            {
                inputConfig.valueTemp = inputConfig.inputField.text;
            }
            else if (kind == 1)
            {
                float a;
                if (float.TryParse(inputConfig.inputField.text, out a))
                {
                    float value = float.Parse(inputConfig.inputField.text);
                    inputConfig.inputField.text = value.ToString();
                    temp = float.Parse(inputConfig.valueTemp);
                    inputConfig.valueTemp = value.ToString();
                }
                else
                {
                    inputConfig.inputField.text = inputConfig.valueTemp;
                }
            }

            UpdateInfomationItem();
            if (inputConfig.valueTemp != temp.ToString())
            {
                SavePreviousMoveAction();
                UpdateSize();
            }
        }
    }

    public void UpdateInfomationItem()
    {
        configuration.itemCreated.item.itemName = itemNameInput.inputField.text;
        configuration.itemCreated.item.length = float.Parse(lengthInput.inputField.text);
        configuration.itemCreated.item.width = float.Parse(widthInput.inputField.text);
        configuration.itemCreated.item.height = float.Parse(heightInput.inputField.text);

        gameManager.guiCanvasManager.infomationItemCanvas.UpdateInfomation(configuration.itemCreated.item);
    }

    private void UpdateSize()
    {
        configuration.itemCreated.sizePointManager.DrawOutline(configuration.itemCreated.item);
        configuration.itemCreated.sizePointManager.CreateSizePoints();
        configuration.itemCreated.sizePointManager.UpdateAreaText();
    }

    private void SavePreviousMoveAction()
    {
        oldPosList = new List<Vector3>();
        for (int i = 0; i < gameManager.itemIndex.sizePointManager.sizePointList.Count; i++)
        {
            oldPosList.Add(Vector3.zero);
            oldPosList[i] = gameManager.itemIndex.sizePointManager.sizePointList[i].transform.localPosition;
        }

        //Action
        PreviousAction previousAction = new PreviousAction();
        previousAction.itemId = gameManager.itemIndex.itemId;
        previousAction.action = "Change Size";
        previousAction.sizePointPosList = new List<Vector3>();
        for (int i = 0; i < gameManager.itemIndex.sizePointManager.sizePointList.Count; i++)
        {
            previousAction.sizePointPosList.Add(Vector3.zero);
            previousAction.sizePointPosList[i] = oldPosList[i];
        }
        gameManager.undoActionList.previousActions.Add(previousAction);
    }
}
