using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class NetworkUtils
{
    public static void writeVector3(BinaryWriter writer, Vector3 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    public static void writeVector2(BinaryWriter writer, Vector2 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
    }

    public static byte[] colorsToByteArray(Color[] colors)
    {
        byte[] colorBytes = new byte[colors.Length * 3];

        for(int i = 0; i < colors.Length; i++)
        {
            colorBytes[i*3] = (byte) (colors[i].r * 255);
            colorBytes[i*3+1] = (byte)(colors[i].g * 255);
            colorBytes[i*3+2] = (byte)(colors[i].b * 255);
        }

        return colorBytes;
    }

    public static Color[] byteArrayToColors(byte[] bytes)
    {
        Color[] colors = new Color[bytes.Length / 3];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(bytes[i*3] / 255f, bytes[i*3+1] / 255f, bytes[i*3+2] / 255f);
        }

        return colors;
    }

    public static void writeColors(BinaryWriter writer, Color[] colors)
    {
        int colorCount = Mathf.Min(255, colors.Length);
        writer.Write(colorCount);
        for (int i = 0; i < colorCount; i++)
        {
            Color color = colors[i];
            writer.Write((byte)(color.r * 255f));
            writer.Write((byte)(color.g * 255f));
            writer.Write((byte)(color.b * 255f));
        }
    }

    public static Color[] readColors(BinaryReader reader)
    {
        int colorCount = Mathf.Min(255, reader.ReadInt32());
        Color[] colors = new Color[colorCount];

        for (int i = 0; i < colorCount; i++)
        {
            colors[i] = new Color(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f);
        }

        return colors;
    }

    public static Vector3 readVector3(BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector2 readVector2(BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector2 fromStringToVector2(string coordinates)
    {
        string[] datas = coordinates.Split('#');
        return new Vector2(float.Parse(datas[0]), float.Parse(datas[1]));
    }

    public static string toStringFromVector2(Vector2 position)
    {
        return position.x.ToString("0.00") + "#" + position.y.ToString("0.00");
    }

}
