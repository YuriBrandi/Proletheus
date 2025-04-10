using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class TrainerSocketClient : MonoBehaviour
{
    private TcpClient client;
    private StreamWriter writer;
    private StreamReader reader;

    void Start()
    {
        client = new TcpClient("127.0.0.1", 5005);
        NetworkStream stream = client.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream);
    }
    
    public int RadarClassifyObject(float[] features, int? label = null)
    {
        // Usa punto come separatore decimale
        string msg = string.Join("|", features.Select(f => f.ToString(CultureInfo.InvariantCulture)));

        if (label.HasValue)
            msg += $"|{label.Value}";

        writer.WriteLine(msg);
        writer.Flush();

        string response = reader.ReadLine();
        return int.Parse(response);
    }

    void OnApplicationQuit()
    {
        writer?.Close();
        reader?.Close();
        client?.Close();
    }
}