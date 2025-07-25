using UnityEngine;
using System.Collections.Generic;

public class UndoRedoController : MonoBehaviour
{
    public static UndoRedoController Instance;
    private List<IUndoRedoCommand> undoStack;
    private List<IUndoRedoCommand> redoStack;

    private void Awake()
    {
        Instance = this;
        undoStack = new List<IUndoRedoCommand>();
        redoStack = new List<IUndoRedoCommand>();
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
    }

    public void AddToRedo(IUndoRedoCommand undoRedoCommand)
    {
        undoStack.Add(undoRedoCommand);
        redoStack.Clear();
    }

    public void Undo()
    {
        ExtractFunc(undoStack, redoStack, true);
    }

    public void Redo()
    {
        ExtractFunc(redoStack, undoStack, false);
    }

    private void ExtractFunc(List<IUndoRedoCommand> sourceStack, List<IUndoRedoCommand> takeStack, bool isUndo)
    {
        if (sourceStack.Count > 0)
        {
            IUndoRedoCommand undoRedoCommand = sourceStack[^1];
            if (isUndo)
            {
                Debug.Log("Undo");
                undoRedoCommand.Undo();
            }
            else
            {
                undoRedoCommand.Redo();
            }

            sourceStack.Remove(undoRedoCommand);
            takeStack.Add(undoRedoCommand);
        }
    }
}