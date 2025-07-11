using System;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    private Stack<ActionPair> undoStack = new();
    private Stack<ActionPair> redoStack = new();

    public void AddAction(System.Action undoAction, System.Action redoAction)
    {
        undoStack.Push(new ActionPair(undoAction, redoAction));
        redoStack.Clear();
    }

    public void Do(Action doAction, Action undoAction)
    {
        doAction.Invoke();
        undoStack.Push(new ActionPair(undoAction, doAction));
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            var action = undoStack.Pop();
            action.UndoAction?.Invoke();
            redoStack.Push(action);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            var action = redoStack.Pop();
            action.RedoAction?.Invoke();
            undoStack.Push(action);
        }
    }

    public bool CanUndo() => undoStack.Count > 0;
    public bool CanRedo() => redoStack.Count > 0;
}

public class ActionPair
{
    public System.Action UndoAction;
    public System.Action RedoAction;

    public ActionPair(System.Action undo, System.Action redo)
    {
        UndoAction = undo;
        RedoAction = redo;
    }
}
