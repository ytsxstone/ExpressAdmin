using System;
using log4net;

namespace ExpressCommon
{
    /// <summary>
    /// log4.net日志记录帮助类，此类实现了IDisposable接口
    /// </summary>
    public class LogScopeHelper : IDisposable
    {
        /// <summary>
        /// 一般消息日志器名称(静态成员)
        /// </summary>
        public readonly static string InfoLogger = "InfoLogger";

        /// <summary>
        /// 运行日志器名称（实例成员）
        /// </summary>
        public readonly string RunnLogger = "RunLogger";

        /// <summary>
        /// 错误日志器名称（静态成员）
        /// </summary>
        public readonly static string ErrorLogger = "ErrorLogger";

        /// <summary>
        /// 日志消息（string类型）
        /// </summary>
        private string stringLogMessage = string.Empty;

        /// <summary>
        /// 日志Guid标识
        /// </summary>
        private Guid scopeId = Guid.NewGuid();

        /// <summary>
        /// 根据日志消息和参数名称实例化LogScope类，datas参数会与stringLogMessage合并
        /// </summary>
        /// <param name="stringLogMessage">日志消息</param>
        /// <param name="datas">参数</param>
        public LogScopeHelper(string stringLogMessage, params object[] datas)
        {
            ILog logger = LogManager.GetLogger(RunnLogger);
            logger.Info(string.Format("Enter {0} - {1}. params: ", stringLogMessage, scopeId.ToString()) + string.Join(" ", datas));
            this.stringLogMessage = stringLogMessage;
        }

        /// <summary>
        /// IDisposable.Dispose
        /// </summary>
        public void Dispose()
        {
            ILog logger = LogManager.GetLogger(RunnLogger);
            logger.Info(string.Format("Exit {0} - {1}.", this.stringLogMessage, this.scopeId.ToString()));
        }

        /// <summary>
        /// 记录错误，这会获取ErrorLogger日志器并记录异常
        /// </summary>
        /// <param name="stringLogMessage"></param>
        /// <param name="ex"></param>
        public static void Error(string stringLogMessage, Exception ex)
        {
            LogManager.GetLogger(ErrorLogger).Error(stringLogMessage, ex);
        }

        /// <summary>
        /// 记录一般消息，这会获取InfoLogger日志器并记录异常
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            LogManager.GetLogger(InfoLogger).Info(message);
        }
    }
}
