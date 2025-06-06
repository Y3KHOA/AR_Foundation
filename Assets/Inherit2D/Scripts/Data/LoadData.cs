using System.Collections.Generic;
using System.Xml.Linq;
using System;
using UnityEngine;

/// <summary>
/// Lớp này chịu trách nhiệm tải dữ liệu từ các tệp XML nằm trong thư mục Tài nguyên.
/// </summary>
public class LoadData : MonoBehaviour
{
    public static LoadData instance;

    private string dataItemsFilePath;
    private string dataCategoriesFilePath;
    private string dataMaterialsFilePath;

    private void Awake()
    {
        instance = this;

        dataItemsFilePath = "Data/DataItems";
        dataCategoriesFilePath = "Data/DataCategories";
        dataMaterialsFilePath = "Data/DataMaterialGround";
    }

    public List<Item> LoadItems()
    {
        TextAsset xmlData = Resources.Load<TextAsset>(dataItemsFilePath);
        if (xmlData == null)
        {
            Debug.LogError($"File not found at path: {dataItemsFilePath}");
            return null;
        }

        try
        {
            XDocument doc = XDocument.Parse(xmlData.text); // Sử dụng .text để lấy nội dung từ TextAsset

            List<Item> itemsList = new List<Item>();

            var items = doc.Descendants("item");
            foreach (var item in items)
            {
                string id = item.Element("id")?.Value;
                string name = item.Element("name")?.Value;
                string image = item.Element("image")?.Value;
                var kinds = item.Element("kindsOfItem");
                string width = item.Element("width")?.Value;
                string height = item.Element("height")?.Value;
                string length = item.Element("length")?.Value;
                string distance = item.Element("distance")?.Value;
                var edgeLengths = item.Element("edgeLengths");
                var corners = item.Element("corners");

                //Kinds of item
                List<string> kindsOfItem = new List<string>();
                if (kinds != null)
                {
                    foreach (var kind in kinds.Elements())
                    {
                        kindsOfItem.Add(kind.Value);
                    }
                }

                //Edge lenghts
                List<float> edgeLengthList = new List<float>();
                if (edgeLengths != null)
                {
                    foreach (var edgeLength in edgeLengths.Elements())
                    {
                        edgeLengthList.Add(float.Parse(edgeLength.Value));
                    }
                }

                //Direction edge
                List<Vector3> directionEdgeList = new List<Vector3>();
                var directionOfEdges = item.Element("directionOfEdges");
                if (directionOfEdges != null)
                {
                    foreach (var direction in directionOfEdges.Elements())
                    {
                        string[] values = direction.Value.Split(',');
                        if (values.Length == 3)
                        {
                            float x, y, z;
                            if (float.TryParse(values[0], out x) &&
                                float.TryParse(values[1], out y) &&
                                float.TryParse(values[2], out z))
                            {
                                directionEdgeList.Add(new Vector3(x, y, z));
                            }
                        }
                    }
                }

                // Load directions
                List<string> goodDirections = new List<string>();
                List<string> badDirections = new List<string>();

                var directions = item.Element("direction");
                if (directions != null)
                {
                    var goodDir = directions.Element("good");
                    var badDir = directions.Element("bad");

                    if (goodDir != null)
                    {
                        foreach (var direction in goodDir.Elements())
                        {
                            goodDirections.Add(direction.Value);
                        }
                    }

                    if (badDir != null)
                    {
                        foreach (var direction in badDir.Elements())
                        {
                            badDirections.Add(direction.Value);
                        }
                    }
                }

                Item itemTemp = new Item(id, name, image, kindsOfItem, width, height, length, distance, edgeLengthList, directionEdgeList, new ColorPicker(), goodDirections, badDirections);
                itemsList.Add(itemTemp);
            }
            return itemsList;
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while loading the XML file: {e.Message}");
            return null;
        }
    }

    public List<Category> LoadCategories()
    {
        TextAsset xmlData = Resources.Load<TextAsset>(dataCategoriesFilePath);
        if (xmlData == null)
        {
            Debug.LogError($"File not found at path: {dataCategoriesFilePath}");
            return null;
        }
        try
        {
            XDocument doc = XDocument.Parse(xmlData.text); // Sử dụng .text để lấy nội dung từ TextAsset

            List<Category> categoriesList = new List<Category>();

            var categories = doc.Descendants("category");
            foreach (var category in categories)
            {
                Category cate = new Category();
                cate.categoryName = category.Element("name")?.Value;
                cate.numberOfItem = int.Parse(category.Element("numberOfItem")?.Value);
                categoriesList.Add(cate);
            }
            return categoriesList;
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while loading the XML file: {e.Message}");
            return null;
        }
    }

    public List<MaterialGround> LoadMaterialsGround()
    {
        TextAsset xmlData = Resources.Load<TextAsset>(dataMaterialsFilePath);
        if (xmlData == null)
        {
            Debug.LogError($"File not found at path: {dataMaterialsFilePath}");
            return null;
        }
        try
        {
            XDocument doc = XDocument.Parse(xmlData.text); // Sử dụng .text để lấy nội dung từ TextAsset

            List<MaterialGround> materialsList = new List<MaterialGround>();

            var materials = doc.Descendants("material");
            foreach (var material in materials)
            {
                MaterialGround m = new MaterialGround();
                m.nameMaterial = material.Element("name")?.Value;
                m.image = material.Element("image")?.Value;
                materialsList.Add(m);

            }
            return materialsList;
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while loading the XML file: {e.Message}");
            return null;
        }
    }
}
