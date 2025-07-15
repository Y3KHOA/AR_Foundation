using UnityEngine;
using UnityEngine.UI;

public class LimitScrollView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0f, 1f);
    }
}