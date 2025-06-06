using System;
using UnityEngine;

/// <summary>
/// Lớp đại diện cho một danh mục trong trò chơi, chứa thông tin về tên danh mục và số lượng mục trong danh mục đó.
/// </summary>
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
