using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace MC_Weather
{
    public class GetWeather
    {
        public string Getweather(string city)
        {
            string outstr = string.Empty;
            string jsonstr = GetWeatherprivateM(city);
            if (!jsonstr.Contains("invilad-citykey"))
            {
                outstr = JsonParst(jsonstr);
            }
            else
            {
                outstr = "无效城市";
            }
            return outstr;
        }
        private string GetWeatherprivateM(string city)
        {
            //http://toy1.weather.com.cn/search?cityname=思明&callback=success_jsonpCallback&_=1664936165826
            //http://www.weather.com.cn/weather/101230203.shtml
            /*
             * http://www.weather.com.cn/data/sk/101010100.html
               http://www.weather.com.cn/data/cityinfo/101010100.html
             */
            //string url = $"http://wthrcdn.etouch.cn/weather_mini?city={city}";//中华万年历服务器已关停
            // https://zhwnlapi.etouch.cn/Ecalender/weather_mini?city=%E5%8E%A6%E9%97%A8&callback=flightHandler&callback=flightHandler&_=1664939521904 
            string url = $"https://zhwnlapi.etouch.cn/Ecalender/weather_mini?city={city}";//中华万年历新服务器
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
            hwr.Method = "get";
            hwr.ContentType = "application/x-www-form-urlencoded";
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            hwr.KeepAlive = true;
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4292.2 Safari/537.36";
            hwr.Headers.Add("Accept-Encoding", "gzip, deflate");
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,ru;q=0.7,de;q=0.6");
            HttpWebResponse hws = null;
            string outhtml = string.Empty;
            using (hws = (HttpWebResponse)hwr.GetResponse())
            {
                if (hws.ContentEncoding.ToLower().Equals("gzip"))
                {
                    outhtml = new StreamReader(new GZipStream(hws.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                }
                else if (hws.ContentEncoding.ToLower().Equals("deflate"))
                {
                    outhtml = new StreamReader(new DeflateStream(hws.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                }
                else
                {
                    outhtml = new StreamReader(hws.GetResponseStream()).ReadToEnd();
                }
            }
            return outhtml;
        }
        private string JsonParst(string jsonstr)
        {
            Dictionary<string, object> weater = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonstr);
            Dictionary<string, object> weater1 = (Dictionary<string, object>)weater["data"];
            string city = (string)weater1["city"];
            ArrayList arrayList = (ArrayList)weater1["forecast"];
            //Dictionary<string, object> weater2 = (Dictionary<string, object>)arrayList[0];
            StringBuilder sb = new StringBuilder();
            sb.Append($"城市：{city}\r\n");
            sb.Append("----------------\r\n");
            string mouth = DateTime.Now.ToString("MM");
            for (int i = 0; i < arrayList.Count; i++)
            {
                Dictionary<string, object> weater2 = (Dictionary<string, object>)arrayList[i];
                string wdh = string.Empty;
                string wdl = string.Empty;
                foreach (KeyValuePair<string, object> item in weater2)
                {
                    if (item.Key.Equals("date"))
                    {
                        sb.Append($"日期：{mouth}月{item.Value}\r\n");
                    }
                    if (item.Key.Equals("high"))
                    {
                        wdh = $"{item.Value}";
                    }
                    if (item.Key.Equals("low"))
                    {
                        //wdl = $"温度：{item.Value}\r\n";
                        wdl = $"{item.Value}";
                    }
                    if (!string.IsNullOrEmpty(wdl) && !string.IsNullOrEmpty(wdh))
                    {
                        sb.Append(wdl + "-" + wdh + "\r\n");
                        wdh = string.Empty;
                        wdl = string.Empty;
                    }
                    if (item.Key.Equals("fengli"))
                    {
                        //fengli=<![CDATA[4级]]>
                        sb.Append($"风力：{item.Value.ToString().Replace("<![CDATA[", "").Replace("]]>", "")}\r\n");
                    }
                    if (item.Key.Equals("fengxiang"))
                    {
                        sb.Append($"风向：{item.Value}\r\n");
                    }
                    if (item.Key.Equals("type"))
                    {
                        sb.Append($"天气：{item.Value}\r\n");
                    }
                }
                if (i < arrayList.Count-1)
                {
                    sb.Append("----------------\r\n");
                }
            }
            return sb.ToString();
        }
    }
}
