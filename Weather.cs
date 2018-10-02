using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

// FYI: The average for the first date may not be completely accurate because it could have just 1 or 2 values left in it, ex. the night time values at 18:00 and 21:00

namespace WeatherConsole
{
    public class Program
    {
        private static Dictionary<string, string> responses = new Dictionary<String, String>();
        public static string[] cities = new string[10] { "Marlboro", "San Diego", "Cheyenne", "Anchorage", "Austin", "Orlando", "Seattle", "Cleveland", "Portland", "Honolulu" }; 

        public static List<float> temperatureList = new List<float>();

        public static Dictionary<string, List<float>> CalculateDayTemp(string cityName)
        {
            var objectList = GetWeatherData(cityName);
            Dictionary<string, List<float>> dayTemp = new Dictionary<string, List<float>>();
            for (int i = 0; i < objectList.Length; i++)
            {
                string day = objectList[i].dt_txt.Substring(0, 10); // date in format of yyyy-mm-dd taken from the datetime text element
                float fTemp = objectList[i].main.temp; // temperature per 3hr interval
                // populate dictionary of string keys and a List of float values with the given day, associated with its respective 3hr interval of temperatures
                if (dayTemp.ContainsKey(day))
                {
                    temperatureList.Add(fTemp);
                    dayTemp[day].Add(fTemp);
                }
                else
                {
                    List<float> temperatureList = new List<float> { fTemp }; // create a new list (temperatureList.empty()) does not work because List is a reference type) for each new key(day)
                    dayTemp.Add(day, temperatureList); // adding the new key(the next day) and the temperatures
                }
            }
            return dayTemp;
        }

        public static Dictionary<string, string> CalculatePrecipitation(string cityName)
        {
            Dictionary<string, string> precipitationKV = new Dictionary<string, string>();
            var objectList = GetWeatherData(cityName);
            for (int i = 0; i < objectList.Length; i++)
            {
                string day = objectList[i].dt_txt.Substring(0, 10);

                if (objectList[i].rain != null && objectList[i].rain._3h > 0)
                {
                    if (!precipitationKV.ContainsKey(day))
                        precipitationKV.Add(day, "*");
                }
            }
            return precipitationKV;
        }

        public static void Main(string[] args)
        {
            for (int i = 0; i < cities.Length; i++)
            {
                responses.Add(cities[i], GetForecast(cities[i]));
            }

            Dictionary<string, float> averageTempPerDay = new Dictionary<string, float>();

            //Print the average temperature for the next 5 days
            foreach (var cityName in cities)
            {
                var dayTemp = CalculateDayTemp(cityName);
                var precipitationKV = CalculatePrecipitation(cityName);
                Console.WriteLine("______________________________________");
                Console.WriteLine(cityName);
                Console.WriteLine();
                Console.WriteLine("Date                     Avg Temp(F)");
                Console.WriteLine("--------------------------------------");
                float aver = 0.00f;

                foreach (string days in dayTemp.Keys)
                {
                    foreach (float n in dayTemp[days])
                    {
                        aver = dayTemp[days].Average();
                        averageTempPerDay[days] = aver;
                    }
                    Console.WriteLine("{0}{1}               {2} \n", days, precipitationKV.ContainsKey(days) ? precipitationKV[days] : string.Empty, (float)(Math.Round(averageTempPerDay[days], 2)));
                }
            }
        }

        public static string GetForecast(string cityName)
        {
            var url = String.Format("http://api.openweathermap.org/data/2.5/forecast?q={0}&units=imperial&appid=ea0ffd5931d25124ae888f196e796cfa", cityName);
            string jsonContent = "";
            // Opens and closes the connection when done
            using (WebClient client = new WebClient())
            {
                jsonContent = client.DownloadString(url);
            }
            return jsonContent;
        }

        private static List[] GetWeatherData(string location)
        {
            var response = responses[location];
            Rootobject data = new Rootobject();
            data = JsonConvert.DeserializeObject<Rootobject>(response);
            return data.list;
        }
    }
}

public class Rootobject
{
    public string cod { get; set; }
    public float message { get; set; }
    public int cnt { get; set; }
    public List[] list { get; set; }
    public City city { get; set; }
}

public class City
{
    public int id { get; set; }
    public string name { get; set; }
    public Coord coord { get; set; }
    public string country { get; set; }
    public int population { get; set; }
}

public class Coord
{
    public float lat { get; set; }
    public float lon { get; set; }
}

public class List
{
    public int dt { get; set; }
    public Main main { get; set; }
    public Weather[] weather { get; set; }
    public Clouds clouds { get; set; }
    public Wind wind { get; set; }
    public Rain rain { get; set; }
    public Sys sys { get; set; }
    public string dt_txt { get; set; }
}

public class Main
{
    public float temp { get; set; }
    public float temp_min { get; set; }
    public float temp_max { get; set; }
    public float pressure { get; set; }
    public float sea_level { get; set; }
    public float grnd_level { get; set; }
    public int humidity { get; set; }
    public float temp_kf { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class Wind
{
    public float speed { get; set; }
    public float deg { get; set; }
}

public class Rain
{
    [JsonProperty("3h")]
    public float _3h { get; set; }
}

public class Sys
{
    public string pod { get; set; }
}

public class Weather
{
    public int id { get; set; }
    public string main { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
}
