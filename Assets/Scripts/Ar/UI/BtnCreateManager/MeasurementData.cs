using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MeasurementData
{
    public List<GameObject> basePoints = new List<GameObject>();
    public List<GameObject> heightPoints = new List<GameObject>();
    public List<TextMeshPro> distanceTexts = new List<TextMeshPro>();
    public List<LineRenderer> lines = new List<LineRenderer>();
    public TextMeshPro baseAreaText;
    public TextMeshPro topAreaText;
    public List<TextMeshPro> sideAreaTexts = new List<TextMeshPro>();
}
