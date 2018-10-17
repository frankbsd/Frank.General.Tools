using System;
using System.Globalization;
using System.Text;

namespace Frank.General.Tools
{
    public class LunarCalendar
    {
        #region 字段
        public String LunarCalendarNow = String.Empty;
        public String GregorianCalendarNow = String.Empty;
        private static ChineseLunisolarCalendar calendar = new ChineseLunisolarCalendar();
        private static String ChineseNumber = "〇一二三四五六七八九";
        private const String CelestialStem = "甲乙丙丁戊己庚辛壬癸";
        private const String TerrestrialBranch = "子丑寅卯辰巳午未申酉戌亥";
        public static readonly String[] ChineseDayName = new String[]
        {
            "初一","初二","初三","初四","初五","初六","初七","初八","初九","初十",
         "十一","十二","十三","十四","十五","十六","十七","十八","十九","二十",
         "廿一","廿二","廿三","廿四","廿五","廿六","廿七","廿八","廿九","三十"
        };
        public static readonly String[] ChineseMonthName = new String[]
        {
            "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"
        };
        #endregion

        #region 方法
        /// <summary>
        /// 获取一个公历日期的农历年份
        /// </summary>
        /// <param name="time">一个公历日期</param>
        /// <returns>农历年份</returns>
        public String GetYear(DateTime time)
        {
            StringBuilder sb = new StringBuilder();
            Int32 year = calendar.GetYear(time);
            Int32 d=0;
            do
            {
                d = year % 10;
                sb.Insert(0, ChineseNumber[d]);
                year = year / 10;
            } while (year > 0);

            return sb.ToString();
        }


        /// <summary>
        /// 获取一个公历日期的农历月份
        /// </summary>
        /// <param name="time">一个公历日期</param>
        /// <returns>农历月份</returns>
        public String GetMonth(DateTime time)
        {
            Int32 month = calendar.GetMonth(time);
            Int32 year = calendar.GetYear(time);
            Int32 leap = 0;

            //正月不可能为闰月
            for (int i = 3; i <= month; i++)
            {
                if (calendar.IsLeapMonth(year, i))
                {
                    leap = i;
                    break;      //一年中最多有一个闰月
                }
            }
            if (leap > 0) month--;
            return (leap == month + 1 ? "闰" : "") + ChineseMonthName[month - 1];
        }


        /// <summary>
        /// 获取一个公历日期的农历日
        /// </summary>
        /// <param name="time">一个公历日期</param>
        /// <returns>农历日</returns>
        public String GetDay(DateTime time)
        {
            return ChineseDayName[calendar.GetDayOfMonth(time) - 1];
        }


        /// <summary>
        /// 获取一个公历日期对应的完整的农历日期
        /// </summary>
        /// <param name="time">公历日期</param>
        /// <returns>完整的农历日期</returns>
        public String GetChineseDate(DateTime time)
        {
            String strY = GetYear(time);
            String strM = GetMonth(time);
            String strD = GetDay(time);
            String strSB = GetStemBranch(time);
            String strDate= strY + "(" + strSB + ")年" + strM + "月" + strD;

            return strDate;
        }

        /// <summary>
        /// 获取一个公历日期的农历干支纪年
        /// </summary>
        /// <param name="time">一个公历日期</param>
        /// <returns>农历干支纪年</returns>
        public String GetStemBranch(DateTime time)
        {
            Int32 sexagenaryYear = calendar.GetSexagenaryYear(time);
            String stemBranch = CelestialStem.Substring(sexagenaryYear % 10 - 1, 1) + TerrestrialBranch.Substring(sexagenaryYear % 12 - 1, 1);

            return stemBranch;
        }
        #endregion

    }
}
