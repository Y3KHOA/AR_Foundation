using UnityEngine;

public class AutoToggleBackground : MonoBehaviour
{
    // Easy conflict with each other, using carefully

    private void OnEnable()
    {
        BackgroundUI.Instance.Show(gameObject, null);
    }

    private void OnDisable()
    {
        BackgroundUI.Instance.Hide();
    }
    
}