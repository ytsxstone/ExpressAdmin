using System;
using System.Web.Mvc;

using ExpressCommon;

namespace ExpressWeb.Authorizes
{
    /// <summary>
    ///  自定义的错误处理特性，该特性用于处理由操作方法引发的异常。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// 重写基类中的OnException，记录错误日志
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            Exception error = filterContext.Exception;
            LogScopeHelper.Error(error.Message, error);
        }
    }
}