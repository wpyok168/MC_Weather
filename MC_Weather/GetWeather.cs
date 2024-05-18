using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;
using System.Linq;

namespace MC_Weather
{
    public class GetWeather
    {
        public string Getweather(string city)
        {
            string outstr = string.Empty;
            string jsonstr = GetWeatherprivateM(city);
            bool b = jsonstr.Contains("invalid-citykey");
            if (jsonstr.Contains("invilad-citykey") || jsonstr.Contains("invalid-citykey"))
            {
                outstr = "未找到对应城市"; //无效城市
            }
            else
            {
                outstr = JsonParst(jsonstr);
               
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
            //string url = $"https://zhwnlapi.etouch.cn/Ecalender/weather_mini?city={city}";//中华万年历服务器已关停
            string url = $"https://v2-zhwnlapi.etouch.cn/Ecalender/api/v2/weather?city={city}";//中华万年历新服务器
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
            //Dictionary<string, object> weeklyDto = (Dictionary<string, object>)weater["weeklyDto"];
            Dictionary<string, object> meta = (Dictionary<string, object>)weater["meta"];
            string city = meta["city"].ToString();
            ArrayList arrayList = (ArrayList)weater["forecast"];//forecast 27天   weekly 7天
            //Dictionary<string, object> weater2 = (Dictionary<string, object>)arrayList[0];
            StringBuilder sb = new StringBuilder();
            sb.Append($"城市：{city}\r\n");  //中华万年历
            sb.Append("----------------\r\n");
            //string mouth = DateTime.Now.ToString("MM");

            for (int i = 1; i < arrayList.Count; i++)
            {
                Dictionary<string, object> weater2 = (Dictionary<string, object>)arrayList[i];

                string wdh = string.Empty; string wdl = string.Empty; //温度
                string daywthr = string.Empty; string nightwthr = string.Empty;  //天气
                string sunrise = string.Empty; string sunset = string.Empty;  //日出 日落
                string wd = string.Empty; string wp = string.Empty;  //风向 风力
                string wdp= string.Empty;

                foreach (KeyValuePair<string, object> item in weater2)
                {
                    if (item.Key.Equals("date"))
                    {
                        sb.Append($"日期：{item.Value}\r\n");
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
                        sb.Append("温度："+ wdl + "-" + wdh + "℃\r\n");
                        wdh = string.Empty;
                        wdl = string.Empty;
                    }
                    if (item.Key.Equals("day"))
                    {
                         Dictionary<string, object> wthr = item.Value as Dictionary<string, object>;
                         daywthr = wthr["wthr"].ToString();
                    }
                    if (item.Key.Equals("night"))
                    {
                        Dictionary<string, object> wthr = item.Value as Dictionary<string, object>;
                        nightwthr = wthr["wthr"].ToString();
                    }
                    if (!string.IsNullOrEmpty(daywthr) && !string.IsNullOrEmpty(nightwthr))
                    {
                        sb.Append("天气：" + daywthr + "转" + nightwthr + "\r\n");
                        daywthr = string.Empty;
                        nightwthr = string.Empty;
                        sb.Append(wdp);
                    }
                    if (item.Key.Equals("wd"))
                    {
                        wd = $"风向：{item.Value}\r\n";
                        //sb.Append($"风向：{item.Value}\r\n");
                    }
                    if (item.Key.Equals("wp"))
                    {
                        //fengli=<![CDATA[4级]]>
                        //sb.Append($"风力：{item.Value.ToString().Replace("<![CDATA[", "").Replace("]]>", "")}\r\n");
                        wp = $"风力：{item.Value.ToString().Replace("<![CDATA[", "").Replace("]]>", "")}\r\n";
                    }
                    if (!string.IsNullOrEmpty(wd) && !string.IsNullOrEmpty(wp))
                    {
                        wdp = wd + wp;
                        //sb.Append(wd+wp);
                        wd = string.Empty;
                        wp = string.Empty;
                    }

                    if (item.Key.Equals("sunrise"))
                    {
                        sunrise = $"{item.Value}";
                    }
                    if (item.Key.Equals("sunset"))
                    {
                        sunset = $"{item.Value}";
                    }
                    if (!string.IsNullOrEmpty(sunrise) && !string.IsNullOrEmpty(sunset))
                    {
                        sb.Append($"日出：{sunrise}\r\n日落：{sunset}\r\n");
                        sunrise = string.Empty;
                        sunset = string.Empty;
                    }
                    //if (item.Key.Equals("type"))
                    //{
                    //    sb.Append($"天气：{item.Value}\r\n");
                    //}
                }

                if (i < arrayList.Count-1)
                {
                    sb.Append("----------------\r\n");
                }
            }
            return sb.ToString();
        }

        public  List<TQ> GetWeather2(string cityname)
        {
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            long tt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var t = web.DownloadDataTaskAsync($"http://toy1.weather.com.cn/search?cityname={cityname}&callback=success_jsonpCallback&_={tt}");
            string city = Encoding.UTF8.GetString(t.Result);
            string pattern = @"\[\{(.)*?(?=\))";
            string cityjson = Regex.Match(city, pattern).ToString();
            JArray citylist = JArray.Parse(cityjson);
            //foreach (var item in citylist)
            //{
            //    string citys = item.SelectToken("$.ref").ToString();
            //    string[] strs = citys.Split('~');
            //}
            string citys = citylist[0].SelectToken("$.ref").ToString();
            string[] strs = citys.Split('~');

            string url = $"http://www.weather.com.cn/weather/{strs[0]}.shtml";
            var t1 = web.DownloadDataTaskAsync(url);
            //var t1 = web.DownloadDataTaskAsync("http://www.weather.com.cn/weather/101230203.shtml");
            string html = Encoding.UTF8.GetString(t1.Result);
            web.Dispose();
            //var web2 = new HtmlWeb();
            //var doc = web2.Load("http://www.weather.com.cn/weather/101230203.shtml");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html); //  The document contains <i><3级</i> tags
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id=\"7d\"]/ul"); // 有问题：丢失风力 ＜3级  Missing <i><3级</i> tags after SelectNodes
                                                                                           //CSS选择器需要再nuget: HtmlAgilityPack.CssSelectors
            HtmlNode hnode = doc.DocumentNode.QuerySelector(".t.clearfix");  // 有问题：丢失风力 ＜3级
            HtmlNode hnode1 = doc.DocumentNode.QuerySelector(".c7d");
           
            List<TQ> list = new List<TQ>();

            foreach (HtmlNode node in nodes.Elements())
            {
                if (node.Name == "li")
                {
                    TQ tQ = new TQ();
                    foreach (HtmlNode item in node.ChildNodes)
                    {
                        if (item.Name == "h1")
                        {
                            tQ.Date = item.InnerText;
                        }
                        if (item.Name == "p" && item.OuterHtml.Contains("wea"))
                        {
                            tQ.Temperature = item.InnerText;
                        }
                        if (item.QuerySelector(".tem") != null)
                        {
                            tQ.Weather = item.InnerText;
                        }
                        if (item.QuerySelector("p.win") != null)
                        {
                            if (item.QuerySelector("p.win").QuerySelector("em") != null)
                            {
                                tQ.NNW = item.QuerySelector("p.win").QuerySelector("em").QuerySelectorAll("span")[0].GetAttributeValue("title", "") + "转" + item.QuerySelector("p.win").QuerySelector("em").QuerySelectorAll("span")[1].GetAttributeValue("title", "");
                            }

                        }
                        if (item.QuerySelector(".win") != null)
                        {
                            List<HtmlNode> list1 = (item.ChildNodes.Where(m => m.Name == "i")).ToList<HtmlNode>();
                            tQ.Wind = (item.ChildNodes.Where(m => m.Name == "i")).ToList<HtmlNode>()[0].InnerText;
                        }
                    }
                    list.Add(tQ);
                }
            }
            return list;
        }
        /// <summary>
        /// 中国天气网爬虫
        /// </summary>
        /// <param name="cityname"></param>
        /// <returns></returns>
        public string GetWeather1(string cityname)
        {
            return ListToSB(GetWeather2(cityname), cityname);

        }
        private string ListToSB(List<TQ> list, string city)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"城市：{city}\r\n--------------");
            string mm = DateTime.Now.ToString("MMM");
            foreach (TQ item in list)
            {
                sb.AppendLine($"日期：{mm}{item.Date.Replace("\n", "")}");
                sb.Append($"温度：{item.Weather.Replace("\n", "")}\r\n");
                sb.Append($"天气：{item.Temperature}\r\n");
                sb.Append($"风向：{item.NNW}\r\n");
                sb.Append($"风力：{item.Wind}\r\n");
                sb.Append("--------------\r\n");
            }
            return sb.ToString();
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
        }
    }
}
