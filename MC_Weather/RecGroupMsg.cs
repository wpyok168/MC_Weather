using MC_SDK;
using MC_SDK.Enum;
using MC_SDK.Interface;
using MC_SDK.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_Weather
{
    public class RecGroupMsg : IGroupMsg
    {
        public int GroupMsgComply(GroupMsg e)
        {
            if (e.FromQQ == 1000032 || Common.MC_API.GetRobotQQ() == e.FromQQ.ToString())//不处理匿名信息和自己
            {
                return (int)EventProcessEnum.消息处理_忽略;
            }
            if (e.Msg.Contains("天气") && e.Msg.Length > "天气".Length)
            {
                /**
                //中华万年历天气接口
                GetWeather weather = new GetWeather();
                string temp = weather.Getweather(e.Msg.Replace("天气", ""));
                Common.MC_API.SendGroupMsg_(e.FromGroup, temp);
                if (temp.Contains("无效城市") || temp.Contains("未找到对应城市"))
                {
                    return (int)EventProcessEnum.消息处理_忽略;
                }
                //中国天气网 爬虫
                Common.MC_API.SendGroupMsg_(e.FromGroup, weather.GetWeather1(e.Msg.Replace("天气", "")));
                 **/

                //中国天气网 移动网络API
                GetWeatherZNAPI getWeatherZNAPI = new GetWeatherZNAPI();
                //string str3 = getWeatherZNAPI.GetCityWeatherAPI(e.Msg.Replace("天气", ""));
                Common.MC_API.SendGroupMsg_(e.FromGroup, getWeatherZNAPI.GetCityWeatherAPI(e.Msg.Replace("天气", "")));
            }
            return (int)EventProcessEnum.消息处理_忽略;
        }
    }
}
