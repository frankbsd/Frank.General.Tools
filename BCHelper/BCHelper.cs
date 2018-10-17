using System;

namespace Frank.General.Tools
{
    /*
     * 在程序设计中有时会遇到的全角和半角的字符，
     * 比如利用关键字查询某些信息，输入相同字符的全角和半角如果不进行处理就会造成获得的结果不相同。
     * 因此，在需要转换全角半角的地方下面两个函数会对你有所帮助。
     * 
     *  转换依据：
     *  全角空格为12288，半角空格为32，
     *  其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248。
     */
    public class BCHelper
    {
        /// <summary>
        /// 内容转全角
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static String ToSBC(String inputString)
        {
            Char[] characterArray = inputString.ToCharArray();

            for (int i = 0; i < characterArray.Length; i++)
            {
                if (characterArray[i] == 32)
                {
                    characterArray[i] = (Char)12288;
                    continue;
                }
                if (characterArray[i] < 127)
                {
                    characterArray[i] = (Char)(characterArray[i] + 65248);
                }
            }

            return new String(characterArray);
        }


        /// <summary>
        /// 内容转半角
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static String ToDBC(String inputString)
        {
            Char[] characterArray = inputString.ToCharArray();
            for (int i = 0; i < characterArray.Length; i++)
            {
                if (characterArray[i] == 12288)
                {
                    characterArray[i] = (Char)32;
                    continue;
                }
                if (characterArray[i] > 65280 && characterArray[i] < 65375)
                {
                    characterArray[i] = (Char)(characterArray[i] - 65248);
                }
            }

            return new String(characterArray);
        }
    }
}
