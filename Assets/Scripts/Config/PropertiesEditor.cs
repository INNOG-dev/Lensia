using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PropertiesEditor
{

    private string path;

    private Dictionary<string, string> fileContents;

    public PropertiesEditor(string path)
    {
        this.path = path;
        fileContents = new Dictionary<string, string>();

        foreach (string line in File.ReadLines(path))
        {
            if (!line.StartsWith("#"))
            {
                string[] splitedData = line.Split(':');
                if (splitedData.Length < 2)
                {
                    throw new System.Exception("Erreur dans le syntaxe du fichier");
                }
                fileContents.Add(splitedData[0], splitedData[1].Substring(1));
            }
            else
            {
                fileContents.Add(line, "");
            }
        }
    }

    public static PropertiesEditor loadProperties(string path)
    {
        if (!File.Exists(path)) return null;
        return new PropertiesEditor(path);
    }

    public void clearData()
    {
        fileContents.Clear();
    }

    public string getString(string key)
    {
        if (fileContents.ContainsKey(key))
            return fileContents[key];
        return null;
    }

    public float getFloat(string key)
    {
        float value;
        if (!float.TryParse(fileContents[key], out value)) throw new System.Exception("value is not a float");
        return value;
    }

    public int getInt(string key)
    {
        int value;
        if (!int.TryParse(fileContents[key], out value)) throw new System.Exception("value is not a integer");
        return value;
    }

    public bool getBool(string key)
    {
        bool value;
        if (!bool.TryParse(fileContents[key], out value)) throw new System.Exception("value is not a boolean");
        return value;
    }

    public double getDouble(string key)
    {
        double value;
        if (!double.TryParse(fileContents[key], out value)) throw new System.Exception("value is not a double");
        return value;
    }

    public void writeValue(string key, object obj)
    {
        fileContents.Add(key, obj.ToString());
    }

    public void writeCommentary(string commentary)
    {
        fileContents.Add("#" + commentary,"");
    }

    public bool propertiesIsEmpty()
    {
        return fileContents.Count == 0;
    }

    public void saveProperties()
    {
        string[] contents = new string[fileContents.Count];
        int i = 0;
        foreach (KeyValuePair<string, string> entry in fileContents)
        {
            if (entry.Key.StartsWith("#"))
            {
                contents[i] = entry.Key;
            }
            else
            {
                contents[i] = entry.Key + ": " + entry.Value;
            }
            i++;
        }
        File.WriteAllLines(path, contents);
    }

}
