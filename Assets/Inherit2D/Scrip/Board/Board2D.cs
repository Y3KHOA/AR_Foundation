using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board2D : MonoBehaviour
{
    [Header("Box")]
    public GameObject boxPrefab;
    public int numberOfBox = 0;

    private List<GameObject> boxsList = new List<GameObject>();
    private Camera mainCamera; 

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        if(numberOfBox <= 0)
        {
            numberOfBox = 1;
        }

        InitBox();
    }

    private void InitBox()
    {
        for (int i = 0; i < numberOfBox; i++)
        {
            boxsList.Add(Instantiate(boxPrefab, transform));
        }
    }
}
