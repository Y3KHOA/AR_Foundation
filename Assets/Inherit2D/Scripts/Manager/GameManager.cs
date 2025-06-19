using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý các hành động trước đó trong trò chơi, bao gồm thông tin về ID mục, hành động đã thực hiện, vị trí, xoay, bộ chọn màu và kích thước.
/// </summary>
[Serializable]
public class PreviousAction
{
    public int itemId;
    public string action;
    public Vector3 position;
    public Quaternion rotation;
    public ColorPicker colorPicker;
    public Texture texture;
    public List<Vector3> sizePointPosList;    
    public Room roomBackup;
}

[Serializable]
public class ListPreviousAction
{
    public List<PreviousAction> previousActions = new List<PreviousAction>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("User")]
    public string userZodiac;
    public int userZodiacIndex;

    [Header("Item")]
    public bool hasItem = false;
    public ItemHasChosen itemHasChosen;
    public ItemCreated itemIndex;
    [HideInInspector] public List<ItemCreated> createdItems2DList = new List<ItemCreated>();
    [HideInInspector] public List<ItemCreated> deleteItems2DList = new List<ItemCreated>();
    [HideInInspector] public List<ItemCreated> createdGroudList = new List<ItemCreated>();
    [HideInInspector] public List<Box3D> createdItems3DList = new List<Box3D>();

    [Header("Item - Redo")]
    public List<ItemCreated> redoItemsList = new List<ItemCreated>();
    public ListPreviousAction redoActionList = new ListPreviousAction();

    [Header("Item - Undo")]
    public ListPreviousAction undoActionList = new ListPreviousAction();

    [Header("Item - Parents")]
    public GameObject createdItems2DParent;
    public GameObject deleteItems2DParent;
    public GameObject createdItems3DParent;
    public GameObject redoItemsParent;
    [HideInInspector] public int numberOfItemsCreated = 0;
    [HideInInspector] public Vector3 itemCreatedOldPosition = new Vector3();
    [HideInInspector] public Vector3 itemCreatedOldRotation = new Vector3();
    [HideInInspector] public Vector3 itemCreatedOldPositionRedo = new Vector3();
    [HideInInspector] public Vector3 itemCreatedOldRotationRedo = new Vector3();
    [HideInInspector] public bool sizePointClick = false;
    [HideInInspector] public bool isLock = false;

    [Header("View")]
    public bool isOn3DView = false;

    [Header("Object pool")]
    public ObjectPool itemCanvasPool;

    //
    [HideInInspector] public CameraController cameraController;
    [HideInInspector] public GUICanvasManager guiCanvasManager;
    public bool manualStartDrawing = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        cameraController = CameraController.instance;
        guiCanvasManager = GUICanvasManager.instance;

        //Item
        itemHasChosen.gameObject.SetActive(false);
    }

    public void ActivateDrawing()
    {
        manualStartDrawing = true;
    }

    public void DeactivateDrawing()
    {
        manualStartDrawing = false;
    }

    public bool GetDrawingStatus()
    {
        return manualStartDrawing;
    }
}
