using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Frank.General.Tools
{


    public class Log
    {

        #region 字段

        //设定读取内容长度，单位MB,默认4MB
        private const Int32 MessageLength = 4 * 1024 * 1024;

        /// <summary>
        /// 日志文件保存路径，初始化时赋值
        /// </summary>
        private String LogFilePath;


        /// <summary>
        /// 错误日志保存路径，初始化时赋值
        /// </summary>
        private String ErrorLogPath;


        /// <summary>
        /// 列队集合
        /// 注意：1、集合大小参数需根据业务量
        ///             调大以提高性能，默认值可能造成数据写入不完整
        ///             调小以提高资源利用率
        ///       2、经测试，默认值也能稳定保证一次性写入百万次，
        ///          只是第一次运行时，易报队列索引错
        /// </summary>
        private Queue<Content> ListQueue = new Queue<Content>();


        /// <summary>
        /// 队列线程
        /// </summary>
        Thread thread = null;


        /// <summary>
        /// System.Threading.ReaderWriterLockSlim 类
        /// 用于管理资源访问的锁定状态，可实现多线程读取或进行独占式写入访问。
        /// 利用这个类，我们就可以避免在同一时间段内多线程同时写入一个文件而导致的并发写入问题。
        /// 
        /// 读写锁是以 ReaderWriterLockSlim 对象作为锁管理资源的，
        /// 不同的 ReaderWriterLockSlim 对象中锁定同一个文件也会被视为不同的锁进行管理，
        /// 这种差异可能会再次导致文件的并发写入问题，
        /// 所以 ReaderWriterLockSlim 应尽量定义为只读的静态对象
        /// 
        /// ReaderWriterLockSlim 有几个关键的方法：
        /// 调用 EnterWriteLock 方法 进入写入状态，在调用线程进入锁定状态之前一直处于阻塞状态，因此可能永远都不返回。
        /// 调用 TryEnterWriteLock 方法 进入写入状态，可指定阻塞的间隔时间，如果调用线程在此间隔期间并未进入写入模式，将返回false。
        /// 调用 ExitWriteLock 方法 退出写入状态，应使用 finally 块执行 ExitWriteLock 方法，从而确保调用方退出写入模式。
        /// </summary>
        private static readonly ReaderWriterLockSlim logWriteLock = new ReaderWriterLockSlim();

        #endregion


        #region 构造函数


        /// <summary>
        /// 初始化日志(指定 功能模块名【文件夹】 和 日志文件名 ）
        /// </summary>
        /// <param name="modelName">功能模块名【文件夹】</param>
        /// <param name="fileName">日志文件名，可为空（默认为当前日期“yyyyMMdd”）</param>
        public Log(String modelName, String fileName = null)
        {
            LogFilePath = GenerateLogPath(modelName, fileName);
            CheckDirectoryAndTxt(LogFilePath);
            ErrorLogPath = GenerateErrorLogPath();
        }

        #endregion


        #region 公共方法

        /// <summary>
        /// 把日志加入待写入的队列
        /// </summary>
        /// <param name="level">日志等级，可为空（默认：消息）</param>
        /// <param name="behavior">程序行为：可为空（默认：行为记录）</param>
        /// <param name="message">日志信息正文：可为空（默认：未写日志正文）</param>
        public void Append(String level = null, String behavior = null, String message = null)
        {
            Content content = new Content
            {
                Level = String.IsNullOrEmpty(level) ? "消息" : level,
                Behavior = String.IsNullOrEmpty(behavior) ? "行为记录" : behavior,
                Message = String.IsNullOrEmpty(message) ? "未写日志正文" : message
            };
            try
            {
                ListQueue.Enqueue(content);
            }
            catch (Exception ex)
            {
                WriteLogError(String.Format("{0}\tAppend()方法错误\r\n写入:[{1}]\r\n{2}\r\n\r\n",
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                            content,
                                            ex.Message));
            }
        }


        /// <summary>
        /// 开启日志队列
        /// </summary>
        public void Open()
        {
            try
            {
                thread = new Thread(ThreadStart) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                WriteLogError(String.Format("{0}\tOpen()方法错误\r\n{1}\r\n\r\n",
                                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                           ex.Message));
            }
        }


        /// <summary>
        /// 关闭日志队列
        /// </summary>
        public void Close()
        {
            try
            {
                ListQueue.Clear();
                if (thread != null && thread.IsAlive == true)
                {
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                WriteLogError(String.Format("{0}\tClose()方法错误\r\n{1}\r\n\r\n",
                                             DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                             ex.Message));
            }
        }
        #endregion


        #region 私有方法

        /// <summary>
        /// 生成日志路径
        /// </summary>
        /// <param name="modelName">模块名</param>
        /// <param name="fileName">文件名，可为空（默认“yyyyMMdd”）</param>
        /// <returns></returns>
        private String GenerateLogPath(String modelName, String fileName = null)
        {
            //记录当前时间
            DateTime time = DateTime.Now;

            return String.Format(@"{0}\Log\{1}\{2}\{3}\{4}\{5}.txt",
                                      AppDomain.CurrentDomain.BaseDirectory,    //获取当前程序运行的目录的绝对路径
                                      time.ToString("yyyy"),                    //按年保存
                                      time.ToString("yyyyMM"),                  //按月保存
                                      time.ToString("yyyyMMdd"),                //按日保存
                                      modelName,                                //按模块保存
                                      fileName ?? time.ToString("yyyyMMdd"));   //默认为当前日期("yyyyMMdd")
        }


        /// <summary>
        /// 确认给定路径的文件夹存在
        /// </summary>
        /// <param name="tempfilePath"></param>
        /// <returns></returns>
        private void CheckDirectoryAndTxt(String tempfilePath)
        {
            //检查给定路径的文件是否存在
            if (!File.Exists(tempfilePath))
            {
                //文件夹不存在，创建文件夹
                String folderPath = Path.GetDirectoryName(tempfilePath);
                Directory.CreateDirectory(folderPath);
            }
        }


        /// <summary>
        /// 生成错误日志路径
        /// </summary>
        /// <returns></returns>
        private String GenerateErrorLogPath()
        {
            //记录当前时间
            DateTime time = DateTime.Now;

            return String.Format(@"{0}\Log\{1}\{2}\{3}\错误.txt",
                             AppDomain.CurrentDomain.BaseDirectory,         //获取当前程序运行的目录的绝对路径
                             time.ToString("yyyy"),                         //按年保存
                             time.ToString("yyyyMM"),                       //按月保存
                             time.ToString("yyyyMMdd"));                    //按日保存
        }

        /// <summary>
        /// 以线程的方式执行写日志
        /// </summary>
        private void ThreadStart()
        {
            while (true)
            {
                if (ListQueue.Count > 0)
                {
                    WriteLog();              //写日志
                }
                else
                {
                    Thread.Sleep(1000);         //没有任务
                }
            }
        }


        /// <summary>
        /// 写入队列里的日志
        /// </summary>
        private void WriteLog()
        {
            //组装日志内容
            String message = ReadLogTo4MbOrEndOfQueue();

            #region 写日志

            try
            {
                #region LogWriteLock.EnterWriteLock()说明
                /*
                 * 设置读写锁为写入模式独占资源，其他写入请求需要等待本次写入结束之后才能继续写入
                 * 注意：1、长时间持有读线程锁或写线程锁会使其他线程发生饥饿 (starve)。
                 *          为了得到最好的性能，需要考虑重新构造应用程序以将写访问的持续时间减少到最小。
                 *       2、从性能方面考虑，请求进入写入模式应该紧跟文件操作之前
                 *       3、因进入与退出写入模式应在同一个try finally语句块内，
                 *          所以在请求进入写入模式之前不能触发异常，
                 *          否则释放次数大于请求次数将会触发异常
                 */
                #endregion
                logWriteLock.EnterWriteLock();

                File.AppendAllText(LogFilePath, message);
            }
            catch (Exception ex)
            {
                WriteLogError(String.Format("{0}\r\nWriteLog方法写入[{1}]错误\r\n{2}\r\n\r\n",
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                        message,
                                        ex.Message));
            }
            finally
            {
                #region LogWriteLock.ExitWriteLock()说明
                /*
                 * 退出写入模式，释放资源占用
                 * 注意：1、一次请求对应一次释放
                 *       2、若释放次数大于请求次数将会触发异常
                 *          [写入锁定未经保持即被释放]
                 *       3、若请求处理完成后未释放将会触发异常
                 *          [此模式下不允许以递归方式获取写入锁定]
                 */
                #endregion
                logWriteLock.ExitWriteLock();
            }

            #endregion

        }


        /// <summary>
        /// 日志管理器本身发生错误
        /// </summary>
        /// <param name="msg">错误时间、堆栈等信息</param>
        private void WriteLogError(String msg)
        {
            File.AppendAllText(ErrorLogPath, msg);
        }


        /// <summary>
        /// 从Log队列里读取日志，直到日志内容大于 x MB或队列里的日志读完了
        /// </summary>
        /// <returns></returns>
        private String ReadLogTo4MbOrEndOfQueue()
        {

            StringBuilder message = new StringBuilder(MessageLength);
            bool isEndOfQueue = false;
            do
            {
                Content content = ListQueue.Dequeue();
                if (content == null)
                {
                    isEndOfQueue = true;
                }
                else
                {
                    message.Append(String.Format("{0}\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), content));
                }
            } while (message.Length < MessageLength && ListQueue.Count > 0 && !isEndOfQueue);

            return message.ToString();
        }

        #endregion
    }
}
