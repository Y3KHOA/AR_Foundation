using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControlButton : MonoBehaviour
{
    public float percentSize = 0.85f;
    public RectTransform rectTransformParent;

    [Header("Different type of button")]
    public List<RectTransform> buttonRectDiffList = new List<RectTransform>();

    private RectTransform rectTransform;
    private List<RectTransform> buttonRectList = new List<RectTransform>();

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonRectList = GetComponentsInChildren<RectTransform>().Where(rt => rt != rectTransform).ToList();
    }

    private void FixedUpdate()
    {
        UpdateSize();
    }

    private void UpdateSize()
    {
        if(rectTransformParent == null)
        {
            float size;
            size = rectTransform.rect.height * percentSize;

            foreach (RectTransform rt in buttonRectList)
            {
                rt.sizeDelta = new Vector2(size, size);
            }

            foreach(RectTransform rt in buttonRectDiffList)
            {
                rt.sizeDelta = new Vector2(size * 1.35f, size);
            }    
        }     
        else
        {
            float size;
            size = rectTransformParent.rect.height * percentSize;

            foreach (RectTransform rt in buttonRectList)
            {
                rt.sizeDelta = new Vector2(size, size);
            }
        }    
    }    
}
