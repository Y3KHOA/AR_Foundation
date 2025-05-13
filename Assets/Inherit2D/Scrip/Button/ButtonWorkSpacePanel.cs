using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWorkSpacePanel : MonoBehaviour
{
    private GameManager gameManager;

    [Header("3D Button")]
    public Sprite sprite3D;
    public Sprite sprite2D;
    public Button button3D;

    [Header("Canvas")]
    public List<GameObject> canvasList;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void TurnOn3DView()
    {
        if (!gameManager.isOn3DView)
        {
            gameManager.guiCanvasManager.boardCanvas.SetActive(false);
            for(int i = 0; i < canvasList.Count; i++)
            {
                canvasList[i].gameObject.SetActive(false);
            }    
            gameManager.guiCanvasManager.view3dCanvas.SetActive(true);
            gameManager.isOn3DView = true;
            gameManager.hasItem = false;

            //Button
            button3D.image.sprite = sprite2D;
        }
        else
        {
            //Camrera
            Camera.main.transform.position = new Vector3(0, 0, -150);
            Camera.main.transform.rotation = new Quaternion(0, 0, 0, 0);
            Camera.main.orthographicSize = 80;

            gameManager.guiCanvasManager.boardCanvas.SetActive(true);
            for (int i = 0; i < canvasList.Count; i++)
            {
                canvasList[i].gameObject.SetActive(true);
            }
            gameManager.guiCanvasManager.view3dCanvas.SetActive(false);
            gameManager.isOn3DView = false;
            gameManager.guiCanvasManager.view3dCanvas.transform.eulerAngles = Vector3.zero;

            //Button
            button3D.image.sprite = sprite3D;
        }
    }
}
