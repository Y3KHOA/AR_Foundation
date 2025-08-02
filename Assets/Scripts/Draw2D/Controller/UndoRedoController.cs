using UnityEngine;
using System.Collections.Generic;

public class UndoRedoController : MonoBehaviour
{
    public static UndoRedoController Instance;

    [SerializeField] private int maxStackCount = 20;
    
    private List<IUndoRedoCommand> undoList;
    private List<IUndoRedoCommand> redoList;
    
    private void Awake()
    {
        Instance = this;

        undoList = new List<IUndoRedoCommand>();
        redoList = new List<IUndoRedoCommand>();
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Undo();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Redo();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            RoomStorage.CheckDuplicateRoomID();
        }
    }
#endif
    public void AddToUndo(IUndoRedoCommand command)
    {
        Debug.Log("Add to undo stack");

        undoList.Add(command);
        
        redoList.Clear(); // Clear redo khi có hành động mới
     
        if (undoList.Count > maxStackCount)
        {
            Debug.Log("Số lượng command vượt quá số lượng tối đa, đã xóa command trễ nhất");
            undoList.RemoveAt(0);
        }
        
    }

    public void Undo()
    {
        if (undoList.Count == 0) return;

        IUndoRedoCommand command = undoList[^1];
        undoList.Remove(command);
        command.Undo();
        redoList.Add(command);
    }

    public void Redo()
    {
        if (redoList.Count == 0) return;

        IUndoRedoCommand command = redoList[^1];
        redoList.Remove(command);
        command.Redo();
        undoList.Add(command);
    }

    public void ClearData()
    {
        undoList.Clear();
        redoList.Clear();
    }
}