using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles the year input from the user, calculates their zodiac sign based on the input year,
/// </summary>
public class YearProcessing : MonoBehaviour
{
    [Header("Input canvas")]
    public GameObject inputCanvasGO;
    public TMP_InputField yearInputField;
    public Button confirmBTN;

    [Header("Notification")]
    public GameObject notificationCanvasGO;
    public GameObject succesCanvas;
    public GameObject failedCanvas;
    public TextMeshProUGUI succesZodiacText;
    public TextMeshProUGUI failedNotifiText;
    public Button nextBTN;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    //Zodiac
    private static readonly string[] ZodiacAnimals = new string[]
    {
        "Tý (Chuột)",    // Rat
        "Sửu (Trâu)",    // Ox
        "Dần (Hổ)",      // Tiger
        "Mão (Mèo)",     // Rabbit
        "Thìn (Rồng)",   // Dragon
        "Tỵ (Rắn)",      // Snake
        "Ngọ (Ngựa)",    // Horse
        "Mùi (Dê)",      // Goat
        "Thân (Khỉ)",    // Monkey
        "Dậu (Gà)",      // Rooster
        "Tuất (Chó)",    // Dog
        "Hợi (Lợn)"      // Pig
    };

    public void ConfirmYearOnclick()
    {
        //Caculate year old user
        string yearText = yearInputField.text;
        if (yearText.Trim() == "")
        {
            return;
        }

        notificationCanvasGO.SetActive(true);
        inputCanvasGO.SetActive(false);

        int year = 0;
        int o;
        bool checkYear = int.TryParse(yearText, out o);
        if (checkYear)
        {
            year = int.Parse(yearText);
        }

        if (year <= 1900 || year > DateTime.Now.Year)
        {
            //Khong hop le

            //Canvas
            failedCanvas.SetActive(true);
            succesCanvas.SetActive(false);

            //BTN
            nextBTN.interactable = false;
        }
        else
        {
            //Hop le

            //Canvas
            failedCanvas.SetActive(false);
            succesCanvas.SetActive(true);

            //BTN
            nextBTN.interactable = true;

            //Text
            succesZodiacText.text = "Con giáp của bạn: " + GetZodiac(year);
            gameManager.userZodiac = GetZodiac(year);
            gameManager.userZodiacIndex = (year - 4) % 12;
        }
    }

    public static string GetZodiac(int year)
    {
        int index = (year - 4) % 12;
        return ZodiacAnimals[index];
    }
}
