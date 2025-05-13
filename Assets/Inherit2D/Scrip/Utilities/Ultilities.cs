using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ultilities
{
    //Để tính hướng phong thủy thì cần biết
    //-Vị trí chuột
    //-Vị trí trung tâm của board

    public static string CalculateDirection(float itemAngle)
    {
        // Xác định hướng dựa trên góc
        string directionStr = DetermineDirection(itemAngle);
        return directionStr;
    }

    private static string DetermineDirection(float angle)
    {
        if (angle >= 337.5f || angle < 22.5f) return "Bắc";
        else if (angle >= 22.5f && angle < 67.5f) return "Tây Bắc";
        else if (angle >= 67.5f && angle < 112.5f) return "Tây";
        else if (angle >= 112.5f && angle < 157.5f) return "Tây Nam";
        else if (angle >= 157.5f && angle < 202.5f) return "Nam";
        else if (angle >= 202.5f && angle < 247.5f) return "Đông Nam";
        else if (angle >= 247.5f && angle < 292.5f) return "Đông";
        else if (angle >= 292.5f && angle < 337.5f) return "Đông Bắc";
        return "Unknown";
    }
}
