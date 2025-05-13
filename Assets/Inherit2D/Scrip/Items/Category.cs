using System;
using UnityEngine;

[Serializable]
public class Category
{
    public string categoryName;
    public int numberOfItem;

    public Category()
    {
        categoryName = string.Empty;
        numberOfItem = 0;
    }    

    public void CountNumberOfItem()
    {
        this.numberOfItem++;
    }    
}
