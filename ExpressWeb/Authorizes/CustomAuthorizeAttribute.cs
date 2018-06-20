using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections;

using ExpressCommon;
using Newtonsoft.Json;

namespace ExpressWeb.Authorizes
{
    /// <summary>
    ///  表示一个特性，该特性用于限制调用方对操作方法的访问。
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private string controllerName;
        private string actionName;

        /// <summary>
        /// 实例化CustomAuthorizeAttribute的一个实例
        /// </summary>
        public CustomAuthorizeAttribute()
        {

        }

        /// <summary>
        /// 重写AuthorizeCore方法，根据数据库中的配置来判断用户是否有权限访问
        /// 及根据是否单一用户登录来做判断(配置文件中配置)
        /// 此方法会在OnAuthorization方法调用后调用
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorized = false;
            try
            {
                if (this.controllerName.ToUpper() == "Auth".ToUpper())
                {
                    authorized = true;
                }
                else
                {
                    if (httpContext.Request.IsAuthenticated)
                    {
                        //从session中获取登录对象
                        if (null == Authentication.WebAccount && null == httpContext.Request.UrlReferrer)
                        {
                            return false;
                        }
                        else if(null == Authentication.WebAccount && null != httpContext.Request.UrlReferrer)
                        {
                            return false;
                        }

                        //将多个同时登录的用户T下线
                        Hashtable userOnline = (Hashtable)(httpContext.Application["Online"]);
                        if (userOnline != null)
                        {
                            IDictionaryEnumerator idE = userOnline.GetEnumerator();
                            string strkey = string.Empty;
                            if (userOnline.Count > 0)
                            {
                                while (idE.MoveNext())
                                {
                                    //登录时判断保存的session是否与当前页面的session相同
                                    if (userOnline.Contains(httpContext.Session.SessionID))
                                    {
                                        if (idE.Key != null && idE.Key.ToString().Equals(httpContext.Session.SessionID))
                                        {
                                            //判断当前session保存的值是否为被注销值
                                            if (idE.Value != null && "XXXXXX".Equals(idE.Value.ToString()))
                                            {
                                                FormsAuthentication.SignOut();
                                                //验证被注销则清空session                                    
                                                userOnline.Remove(httpContext.Session.SessionID);
                                                httpContext.Application.Lock();
                                                httpContext.Application["Online"] = userOnline;
                                                httpContext.Response.Clear();
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //设置权限
                        authorized = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogScopeHelper.Error(ex.Message, ex);
            }
            return authorized;
        }

        /// <summary>
        /// 重写OnAuthorization方法，获取ControllerName
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                this.controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                this.actionName = filterContext.ActionDescriptor.ActionName;
                base.OnAuthorization(filterContext);
            }
            catch (Exception ex)
            {
                LogScopeHelper.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 在此处统一作处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())  // ajax请求
            {
                filterContext.HttpContext.Response.StatusCode = 405;
            }
            else
            {
                // 重定向到登录
                filterContext.Result = new RedirectResult("/Auth/Login");
            }
        }
    }
}