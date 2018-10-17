using System;
using System.Collections.Generic;

namespace Frank.General.Tools
{
    public static class LogHelper
    {

        private static Dictionary<String, Log> LogDictionary = new Dictionary<String, Log>();

        /// <summary>
        /// 载入日志
        /// </summary>
        /// <param name="name">日志名称</param>
        /// <returns></returns>
        public static Log LoadLog(String ownerId, String name)
        {
            String keyString = GenerateKeyString(ownerId, name, DateTime.Now);
            //找到当天的日志 直接返回当天的日志
            if (LogDictionary.ContainsKey(keyString))
            {
                //返回当天日志
                return LogDictionary[keyString];
            }


            //没有找到当天的日志->>查找前一天的日志,存在日志过天的情况
            keyString = GenerateKeyString(ownerId, name, DateTime.Now.AddDays(-1));
            if (LogDictionary.ContainsKey(keyString))
            {
                //找到前一天的日志->>关闭日志并从字典中移除
                LogDictionary[keyString].Close();
                LogDictionary.Remove(keyString);
            }
            //没有找到前一天的日志：证明没有日志存在创建日志返回
            return CreateLog(ownerId, name);

        }

        private static Log CreateLog(String ownerId, String name)
        {
            //创建当天日志
            Log newlog = new Log(ownerId, name);
            newlog.Open();
            newlog.Append(message: String.Format("日志 【{0}】 初始化成功", name));

            String keyString = GenerateKeyString(ownerId, name, DateTime.Now);
            LogDictionary.Add(keyString, newlog);
            newlog.Append(message: String.Format("当前共有【{0}】个日志", LogDictionary.Count));

            return LogDictionary[keyString];
        }

        private static String GenerateKeyString(String ownerId, String name, DateTime date)
        {
            return String.Format("{0}_{1}_{2}", ownerId, name, date.ToString("yyyyMMdd"));
        }
    }
}
