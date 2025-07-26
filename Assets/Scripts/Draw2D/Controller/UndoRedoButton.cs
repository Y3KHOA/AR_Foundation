using UnityEngine;
using UnityEngine.UI;

public class UndoRedoButton : MonoBehaviour
{
    public bool isUndo = true;
    private Button btn;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        if (isUndo)
        {
            UndoRedoController.Instance.Undo();
        }
        else
        {
            UndoRedoController.Instance.Redo();
        }
    }
}