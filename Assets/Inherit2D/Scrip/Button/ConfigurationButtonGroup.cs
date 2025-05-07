using UnityEngine;
using UnityEngine.UI;

public class ConfigurationButtonGroup : MonoBehaviour
{
    [Header("Image")]
    public Sprite uncheck;
    public Sprite check;

    [Header("Button")]
    public Button displayName;
    public Button displayArea;
    public Button displayRuler;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void DisplayArea()
    {
        if(gameManager.itemIndex != null)
        {
            if(displayArea.image.sprite == check)
            {
                displayArea.image.sprite = uncheck;
                gameManager.itemIndex.sizePointManager.areaText.gameObject.SetActive(false);
            }    
            else
            {
                displayArea.image.sprite = check;
                gameManager.itemIndex.sizePointManager.areaText.gameObject.SetActive(true);
            }    
        }    
    }   
    
    public void DisplayRuler()
    {
        if (gameManager.itemIndex != null)
        {
            if (displayRuler.image.sprite == check)
            {
                displayRuler.image.sprite = uncheck;
                gameManager.itemIndex.sizePointManager.EnableEdgeText(false);
            }
            else
            {
                displayRuler.image.sprite = check;
                gameManager.itemIndex.sizePointManager.EnableEdgeText(true);
            }
        }
    }    

    public void UpdateInfomationCheckButton(ItemCreated itemCreated)
    {
        if(itemCreated.sizePointManager.areaText.gameObject.activeSelf)
        {
            displayArea.image.sprite = check;
        }    
        else
        {
            displayArea.image.sprite = uncheck;
        }

        if (itemCreated.sizePointManager.edgeLengthTextObjects[0].activeSelf)
        {
            displayRuler.image.sprite = check;
        }
        else
        {
            displayRuler.image.sprite = uncheck;
        }
    }    
}
