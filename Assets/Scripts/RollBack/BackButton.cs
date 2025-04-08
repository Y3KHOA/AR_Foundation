using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button backButton;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                Debug.Log("Button Back Clecked!");
                SceneHistoryManager.LoadPreviousScene();
            });
        }
        else
            Debug.LogError("BackButton: Chua gan Button!");
    }
}
