using Frank.General.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Log log = new Log("测试", "test");

            log.Open();

            #region 日志测试
            Stopwatch sw = new Stopwatch();
            int Total = 1000000;
            sw.Start();
            for (int i = 0; i < Total; i++)
            {
                log.Append("INFO", "性能测试", i.ToString());
                //Thread.Sleep(1);
            }

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            log.Append("统计", "结果", string.Format("写{0}次耗时:\t{1}分{2}秒{3}毫秒", Total, ts.Minutes, ts.Seconds, ts.Milliseconds));
            Console.WriteLine("统计" + "结果" + string.Format("写{0}次耗时:\t{1}分{2}秒{3}毫秒", Total, ts.Minutes, ts.Seconds, ts.Milliseconds));
            #endregion


            //Int32 j = 0;
            //CodeTimer.Time("测试Log性能", 1000000, () =>
            //{
            //    log.Append("INFO", "性能测试", (j++).ToString());
            //});

            #region 农历
            //LunarCalendar lc = new LunarCalendar();
            //String chineseTimeNow = lc.GetChineseDate(DateTime.Parse("1986-07-31"));
            //Console.WriteLine(chineseTimeNow);
            #endregion
            Console.ReadLine();
        }
    }
}
