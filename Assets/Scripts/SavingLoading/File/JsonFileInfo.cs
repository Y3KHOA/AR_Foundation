using System;
using UnityEngine;

public class JsonFileInfo: IComparable<JsonFileInfo>
{
    public string fileName;
    public string displayName;
    public string timestamp;
    public int CompareTo(JsonFileInfo other)
    {
        if (other == null) return 1;

        DateTime thisTime = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", null);
        DateTime otherTime = DateTime.ParseExact(other.timestamp, "yyyy-MM-dd HH:mm:ss", null);

        return otherTime.CompareTo(thisTime);
    }
}

