using UnityEngine;
using System.Collections.Generic;

public class UndoRedoController : MonoBehaviour
{
    public static UndoRedoController Instance;

    private Stack<IUndoRedoCommand> undoStack;
    private Stack<IUndoRedoCommand> redoStack;

    private void Awake()
    {
        Instance = this;
        undoStack = new Stack<IUndoRedoCommand>();
        redoStack = new Stack<IUndoRedoCommand>();
    }

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

    public void AddToUndo(IUndoRedoCommand command)
    {
        Debug.Log("Add to undo stack");
        undoStack.Push(command);
        redoStack.Clear(); // Clear redo khi có hành động mới
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;

        var command = undoStack.Pop();
        Debug.Log("Undo");
        command.Undo();
        redoStack.Push(command);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;

        var command = redoStack.Pop();
        Debug.Log("Redo");
        command.Redo();
        undoStack.Push(command);
    }
}