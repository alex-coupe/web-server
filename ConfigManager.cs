using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebServer
{
    public static class ConfigManager
    {
        public static Configuration Configuration { get; set; }
        public static void Init()
        {
            if (File.Exists("./config.json"))
            {
                try
                {
                    // Read the JSON file
                    string jsonContent = File.ReadAllText("./config.json");
                    // Deserialize the JSON content into the Configuration object
                    Configuration = JsonSerializer.Deserialize<Configuration>(jsonContent);

                    Console.WriteLine("JSON file loaded and deserialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Error: JSON file not found.");
            }
        }
    }
}
