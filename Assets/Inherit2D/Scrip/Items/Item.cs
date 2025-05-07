using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

[Serializable]
public class Item
{
    [Header("Info")]
    public string itemId;
    public string itemName;
    public string imageName;
    public List<string> kindsOfItem;
    public float width; //chiều rộng
    public float height; //chiều cao
    public float length; //chiều dài
    public float distance;
    public List<float> edgeLengthList;
    public List<Vector3> directionOfEdges;
    public ColorPicker colorPicker;
    public List<string> goodDirection;
    public List<string> badDirection;

    public Item()
    {
        itemId = "";
        itemName = "";
        imageName = "";
        kindsOfItem = new List<string>();
        width = 0;
        height = 0;
        length = 0;
        distance = 0;
        edgeLengthList = new List<float>();
        directionOfEdges = new List<Vector3>();
        colorPicker = new ColorPicker();
        colorPicker.alpha = 1;
        goodDirection = new List<string>();
        badDirection = new List<string>();
    }

    public Item(string itemId, string itemName, string imageName, List<string> kindOfItem, string width, string height, string length, string distance, List<float> edgeLengthList, List<Vector3> directionOfEdges, ColorPicker colorPicker, List<string> goodDir, List<string> badDir)
    {
        this.itemId = itemId;
        this.itemName = itemName;
        this.imageName = imageName;
        this.kindsOfItem = kindOfItem;
        this.width = float.Parse(width);
        this.height = float.Parse(height);
        this.length = float.Parse(length);
        this.distance = float.Parse(distance);
        this.edgeLengthList = edgeLengthList;
        this.directionOfEdges = directionOfEdges;
        this.colorPicker = colorPicker;
        this.goodDirection = goodDir;
        this.badDirection = badDir;
    }

    // Phương thức sao chép
    public Item DeepCopy(Item item)
    {
        return new Item(
            item.itemId,
            item.itemName,
            item.imageName,
            item.kindsOfItem,
            item.width.ToString(),
            item.height.ToString(),
            item.length.ToString(),
            item.distance.ToString(),
            item.edgeLengthList,
            item.directionOfEdges,
            item.colorPicker,
            new List<string>(item.goodDirection),
            new List<string>(item.badDirection)
        );
    }

    public bool CompareKindOfItem(string kindOfItem)
    {
        foreach (string kind in this.kindsOfItem)
        {
            if(kind.Equals(kindOfItem))
            {
                return true;
            }    
        }
        return false;
    }
}
