using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CEHost
{
    class Program
    {
        const string path = @"D:\00_Temp\TestNMH.txt";

        static void Main(string[] args)
        {
            string startDateTime = DateTime.Now.ToString();
            var fileExists = File.Exists(path);
            var fInfo = fileExists ? Environment.NewLine + $"New start of writing at {startDateTime}" :
                $"New file for writing from NMH created at [{startDateTime}]!";
            using (StreamWriter sw = new StreamWriter(path, fileExists))
            {
                sw.WriteLine(fInfo);
            }


            JObject data;
            while ((data = Read()) != null)
            {
                var processed = ProcessMessage(data);
                //Write(processed);
                if (processed == "exit")
                {
                    return;
                }
            }
        }
        public static string ProcessMessage(JObject data)
        {
            var message = data["msg"].Value<string>();

            //code to write to the file received message
            var curDataTime = DateTime.Now;
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine($"Writing to file the message from Chrome at [{curDataTime}]: " + message);
            }

            switch (message)
            {
                case "test":
                    return "testing!";
                case "exit":
                    return "exit";
                default:
                    return "echo: " + message;
            }
        }

        public static JObject Read()
        {
            var stdin = Console.OpenStandardInput();
            var length = 0;

            var lengthBytes = new byte[4];
            stdin.Read(lengthBytes, 0, 4);
            length = BitConverter.ToInt32(lengthBytes, 0);

            var buffer = new char[length];
            using (var reader = new StreamReader(stdin))
            {
                while (reader.Peek() >= 0)
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
            }

            return (JObject)JsonConvert.DeserializeObject<JObject>(new string(buffer));
        }

        public static void Write(JToken data)
        {
            var json = new JObject();
            //json["data"] = data;
            json["msg"] = data;

            var bytes = System.Text.Encoding.UTF8.GetBytes(json.ToString(Formatting.None));

            var stdout = Console.OpenStandardOutput();
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }
    }
}
