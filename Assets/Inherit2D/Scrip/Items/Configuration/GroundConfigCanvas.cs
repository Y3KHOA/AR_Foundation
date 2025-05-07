using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroundConfigCanvas : MonoBehaviour
{
    [Header("Input size")]
    public GameObject sizeInputPrefab;
    public GridLayoutGroup gridLayoutGroup;
    public InputConfig groundNameInput;
    public List<InputConfig> inputSizeList = new List<InputConfig>();

    private Configuration configuration;
    private GameManager gameManager;
    private RectTransform groundInputRect;
    private int t = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.instance;
        configuration = Configuration.instance;

        groundInputRect = groundNameInput.GetComponent<RectTransform>();
        ControlGridLayout();
    }

    private void ControlGridLayout()
    {
        //Cell size
        float cellX = groundInputRect.rect.width * 0.9f;
        float cellY = groundInputRect.rect.height;
        gridLayoutGroup.cellSize = new Vector2(cellX, cellY);

        //Spacing
        float spacingY = cellY * 0.14f;
        gridLayoutGroup.spacing = new Vector2 (0, spacingY);
    }

    public void InitSizeInputField(int numberOfInputField)
    {
        groundNameInput.inputField.onEndEdit.AddListener((value) => OnInputGroundComplete(groundNameInput, 0));

        for (int i = inputSizeList.Count; i < numberOfInputField; i++)
        {
            InputConfig newInput = Instantiate(sizeInputPrefab, gridLayoutGroup.transform).GetComponent<InputConfig>();
            inputSizeList.Add(newInput);

            newInput.inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Cạnh " + (i + 1);

            // Tạo một biến cục bộ để tránh lỗi closure
            int index = i;
            newInput.inputField.onEndEdit.AddListener((value) => OnInputGroundComplete(inputSizeList[index], 1));
        }
    }

    public void OnInputGroundComplete(InputConfig inputConfig, int kind)
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
                //UpdateSize();
            }
        }
    }

    private void UpdateInfomationItem()
    {
        configuration.itemCreated.item.itemName = groundNameInput.inputField.text;
        for (int i = 0; i < configuration.itemCreated.item.edgeLengthList.Count; i++)
        {
            configuration.itemCreated.item.edgeLengthList[i] = float.Parse(inputSizeList[i].inputField.text);
        }    

        gameManager.guiCanvasManager.infomationItemCanvas.UpdateInfomation(configuration.itemCreated.item);
    }

    private void UpdateSize()
    {
        configuration.itemCreated.sizePointManager.DrawOutline(configuration.itemCreated.item);
        configuration.itemCreated.sizePointManager.CreateSizePoints();
        configuration.itemCreated.sizePointManager.UpdateAreaText();
    }
}
