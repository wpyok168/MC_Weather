using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MC_Weather
{
    /// <summary>
    /// 中国天气网 移动网络API 15天 天气
    /// </summary>
    public class GetWeatherZNAPI
    {
        public string GetCityWeatherAPI(string cityname)
        {
            //GetCityID("安溪");
            //https://d1.weather.com.cn/wap_40d/101230502.html
            Dictionary<string, string> cityid = GetCityID(cityname);
            if (cityid.Count == 0)
            {
                return "未找到对应城市";
            }
            // Dictionary<string, string> citypv = GetCityID(cityid["pv"]);
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            web.Headers.Add("Referer", "https://m.weather.com.cn/");
            //web.Headers.Add("Cookie", $"f_city={cityid["pv"]}|{citypv["ac"]}|");
            var t = web.DownloadDataTaskAsync(new Uri($"https://d1.weather.com.cn/wap_40d/{cityid["ac"]}.html"));
            t.Wait();
            string html = Encoding.UTF8.GetString(t.Result);
            //具体可参考  http://www.weatherdt.com/help.html  说明
            string pattern = @"\[\{(.)*?(?=;)";
            MatchCollection cityjson = Regex.Matches(html, pattern);
            JArray citylist = JArray.Parse(cityjson[0].Value.ToString());
            // citylist.FirstOrDefault(x=>x.)
            List<TQ> list = new List<TQ>();
            foreach (JToken item in citylist.Children())
            {
                Dictionary<string, string> temp = item.ToObject<Dictionary<string, string>>();
                if (string.IsNullOrEmpty(temp["005"]) || string.IsNullOrEmpty(temp["005"]) || string.IsNullOrEmpty(temp["008"]))
                {
                    continue;
                }
                TQ tQ = new TQ();
                tQ.Date = temp["009"];
                tQ.Lunar = temp["010"];
                tQ.Weather = temp["004"] + "/" + temp["003"];
                tQ.Temperature = GetWeatherN(int.Parse(temp["001"])) + " 转 " + GetWeatherN(int.Parse(temp["002"]));
                tQ.NNW = GetWindD(int.Parse(temp["007"])) + " 转 " + GetWindD(int.Parse(temp["008"]));
                tQ.Wind = GetWindF(int.Parse(temp["005"])) + " 转 " + GetWindF(int.Parse(temp["006"]));
                tQ.Sunrise = temp["014"];
                tQ.Sunset = temp["015"];
                temp = new Dictionary<string, string>();
                list.Add(tQ);
            }

            return ListToSB1(list, cityname);
        }
        /// <summary>
        /// 获取城市天气编号
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public  Dictionary<string, string> GetCityID(string city)
        {
            System.Collections.Generic.Dictionary<string, string> cityid = new System.Collections.Generic.Dictionary<string, string>(); ;

            JToken obj = JObject.Parse(GetCityJsonData()).SelectToken("$.citySearchSource");
            //JArray jArray = obj.ToObject<JArray>();
            foreach (JToken item in obj)
            {
                //string a = item.SelectToken("$.n").ToString();
                if (item.SelectToken("$.n").ToString().Contains(city))
                {
                    cityid = item.ToDictionary(keyK => keyK.ToString(), valueV => valueV.ToString());
                    cityid = item.ToObject<Dictionary<string, string>>();
                    //cityid = item.SelectToken("$.ac").ToString();
                    //cityid = item;
                    break;
                }
            }

            return cityid;
        }
        private string GetCityJsonData()
        {
            string retstr = string.Empty;
            //string name = Assembly.GetExecutingAssembly().GetName().Name;
            //string namespc = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            Assembly asm = Assembly.GetExecutingAssembly();
            //var cdm = asm.GetManifestResourceNames();   
            foreach (string c in asm.GetManifestResourceNames())
            {
                if (c.Contains("CityDataMobile.json"))
                {

                    //var stream1 = asm.GetManifestResourceStream(MethodBase.GetCurrentMethod().DeclaringType.Namespace+ ".CityDataMobile.json");
                    var stream2 = asm.GetManifestResourceStream(c);
                    using (StreamReader sr = new StreamReader(stream2))
                    {
                        retstr = sr.ReadToEnd();
                    }
                }
            }
            //JObject.Parse
            return retstr;
        }

        private string ListToSB1(List<TQ> list, string city)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"城市：{city}\r\n--------------");
            //string mm = DateTime.Now.ToString("MMM");
            foreach (TQ item in list)
            {
                sb.AppendLine($"日期：{item.Date.Replace("\n", "")}");
                sb.AppendLine($"农历：{item.Lunar.Replace("\n", "")}");
                sb.Append($"温度：{item.Weather.Replace("\n", "")}℃\r\n");
                sb.Append($"天气：{item.Temperature}\r\n");
                sb.Append($"风向：{item.NNW}\r\n");
                sb.Append($"风力：{item.Wind}\r\n");
                sb.Append($"日出：{item.Sunrise}\r\n");
                sb.Append($"日落：{item.Sunset}\r\n");
                sb.Append("--------------\r\n");
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获取风向编号
        /// </summary>
        /// <param name="windnum"></param>
        /// <returns></returns>
        private string GetWindD(int windnum)
        {
            string wind = string.Empty;
            switch (windnum)
            {
                case 0:
                    wind = "无持续风向";
                    break;
                case 1:
                    wind = "东北风";
                    break;
                case 2:
                    wind = "东风";
                    break;
                case 3:
                    wind = "东南风";
                    break;
                case 4:
                    wind = "南风";
                    break;
                case 5:
                    wind = "西南风";
                    break;
                case 6:
                    wind = "西风";
                    break;
                case 7:
                    wind = "西北风";
                    break;
                case 8:
                    wind = "北风";
                    break;
                case 9:
                    wind = "旋转风";
                    break;
                default:
                    break;
            }
            return wind;
        }
        /// <summary>
        /// 获取风力编号
        /// </summary>
        /// <param name="windnum"></param>
        /// <returns></returns>
        private string GetWindF(int windnum)
        {
            string wind = string.Empty;
            switch (windnum)
            {
                case 0:
                    wind = "＜3级";//微风
                    break;
                case 1:
                    wind = "3-4级";
                    break;
                case 2:
                    wind = "4-5级";
                    break;
                case 3:
                    wind = "5-6级";
                    break;
                case 4:
                    wind = "6-7级";
                    break;
                case 5:
                    wind = "7-8级";
                    break;
                case 6:
                    wind = "8-9级";
                    break;
                case 7:
                    wind = "9-10级";
                    break;
                case 8:
                    wind = "10-11级";
                    break;
                case 9:
                    wind = "11-12级";
                    break;
                default:
                    break;
            }
            return wind;
        }

        /// <summary>
        /// 获取天气编号
        /// </summary>
        /// <param name="wenaternum"></param>
        /// <returns></returns>
        private string GetWeatherN(int wenaternum)
        {
            string weather = string.Empty;
            switch (wenaternum)
            {
                case 0:
                    weather = "晴";
                    break;
                case 1:
                    weather = "多云";
                    break;
                case 2:
                    weather = "阴";
                    break;
                case 3:
                    weather = "阵雨";
                    break;
                case 4:
                    weather = "雷阵雨";
                    break;
                case 5:
                    weather = "雷阵雨伴有冰雹";
                    break;
                case 6:
                    weather = "雨夹雪";
                    break;
                case 7:
                    weather = "小雨";
                    break;
                case 8:
                    weather = "中雨";
                    break;
                case 9:
                    weather = "大雨";
                    break;
                case 10:
                    weather = "暴雨";
                    break;
                case 11:
                    weather = "大暴雨";
                    break;
                case 12:
                    weather = "特大暴雨";
                    break;
                case 13:
                    weather = "阵雪";
                    break;
                case 14:
                    weather = "小雪";
                    break;
                case 15:
                    weather = "中雪";
                    break;
                case 16:
                    weather = "大雪";
                    break;
                case 17:
                    weather = "暴雪";
                    break;
                case 18:
                    weather = "雾";
                    break;
                case 19:
                    weather = "大雨冻雨";
                    break;
                case 20:
                    weather = "沙尘暴";
                    break;
                case 21:
                    weather = "小到中雨";
                    break;
                case 22:
                    weather = "中到大雨";
                    break;
                case 23:
                    weather = "大到暴雨";
                    break;
                case 24:
                    weather = "暴雨到大暴雨";
                    break;
                case 25:
                    weather = "大暴雨到特大暴雨";
                    break;
                case 26:
                    weather = "小到中雪";
                    break;
                case 27:
                    weather = "中到大雪";
                    break;
                case 28:
                    weather = "大到暴雪";
                    break;
                case 29:
                    weather = "浮尘";
                    break;
                case 30:
                    weather = "扬沙";
                    break;
                case 31:
                    weather = "强沙尘暴";
                    break;
                case 32:
                    weather = "浓雾";
                    break;
                case 99:
                    weather = "无";
                    break;
                case 49:
                    weather = "强浓雾";
                    break;
                case 54:
                    weather = "中度霾";
                    break;
                case 55:
                    weather = "重度霾";
                    break;
                case 56:
                    weather = "严重霾";
                    break;
                case 57:
                    weather = "大雾";
                    break;
                case 58:
                    weather = "特强浓雾";
                    break;
                case 301:
                    weather = "雨";
                    break;
                case 302:
                    weather = "雪";
                    break;
                default:
                    break;
            }
            return weather;
        }
        public class TQ
        {
            /// <summary>
            /// 日期
            /// </summary>
            public string Date { get; set; } //日期
            /// <summary>
            /// 温度
            /// </summary>
            public string Weather { get; set; } //温度
            /// <summary>
            /// 天气
            /// </summary>
            public string Temperature { get; set; } //天气
            /// <summary>
            /// 风力
            /// </summary>
            public string Wind { get; set; } //风力
            /// <summary>
            /// 风向
            /// </summary>
            public string NNW { get; set; } //方向
            //string sunrise = string.Empty; string sunset = string.Empty;
            /// <summary>
            /// 日出
            /// </summary>
            public string Sunrise { get; set; }
            /// <summary>
            /// 日落
            /// </summary>
            public string Sunset { get; set; }
            /// <summary>
            /// 农历
            /// </summary>
            public string Lunar { get; set; }
        }
    }
}
