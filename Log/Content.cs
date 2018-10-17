using System;

namespace Frank.General.Tools
{
    class Content
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public String Level { get; set; }


        /// <summary>
        /// 日志行为
        /// </summary>
        public String Behavior { get; set; }


        /// <summary>
        /// 日志正文
        /// </summary>
        public String Message { get; set; }

        public override String ToString()
        {
            return String.Format("{0}\t{1}\t{2}", Level, Behavior, Message);
        }
    }
}
