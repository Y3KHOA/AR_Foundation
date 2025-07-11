using UnityEngine;
using UnityEngine.UI;

public class UndoRedoUI : MonoBehaviour
{
    public Button undoButton;
    public Button redoButton;
    public UndoRedoManager undoRedoManager;

    void Start()
    {
        Debug.Log("UndoRedoUI Start called");
        undoButton.onClick.AddListener(() =>
        {
            Debug.Log("Undo button clicked!");
            undoRedoManager.Undo();
        });
        redoButton.onClick.AddListener(() =>
        {
            Debug.Log("Redo button clicked!");
            undoRedoManager.Redo();
        });
    }
}
