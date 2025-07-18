using UnityEngine;

public class OpenBackground : MonoBehaviour
{
    public void Open(GameObject target)
    {
        BackgroundUI.Instance.Show(target, null);
    }
}