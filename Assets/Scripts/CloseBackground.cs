using UnityEngine;

public class CloseBackground : MonoBehaviour
{
    public void Close()
    {
        BackgroundUI.Instance.Hide();
    }
}