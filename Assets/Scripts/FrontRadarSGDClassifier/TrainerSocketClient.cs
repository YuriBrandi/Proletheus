using System;
using System.IO;
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
        // Building message: feature1,feature2,...,[optional_label]
        string msg = string.Join(",", features);
        if (label.HasValue)
            msg += $",{label.Value}";

        writer.WriteLine(msg);
        writer.Flush();

        string response = reader.ReadLine();
        return int.Parse(response); // 0-1 discrete classification
    }

    void OnApplicationQuit()
    {
        writer?.Close();
        reader?.Close();
        client?.Close();
    }
}