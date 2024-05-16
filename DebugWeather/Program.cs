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
            weather.Getweather("思明");
        }
    }
}
