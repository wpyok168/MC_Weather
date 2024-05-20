using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MC_Weather;

namespace DebugWeather
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GetWeather weather = new GetWeather();
            //string str1 = weather.Getweather("思明");
            string str2 = weather.GetWeather1("官桥");//广西科技馆  官桥
            //GetWeatherZNAPI getWeatherZNAPI = new GetWeatherZNAPI();
            //string str3 = getWeatherZNAPI.GetCityWeatherAPI("广西科技馆");
        }
    }
}
