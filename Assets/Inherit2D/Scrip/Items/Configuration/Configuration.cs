using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Configuration : MonoBehaviour
{
    public static Configuration instance;

    [Header("Item Created")]
    public ItemCreated itemCreated;

    [Header("Config Item")]
    public ItemConfigCanvas itemConfigCanvas;

    [Header("Config Ground")]
    public GroundConfigCanvas groundConfigCanvas;

    private void Awake()
    {
        instance = this;
    }
}
