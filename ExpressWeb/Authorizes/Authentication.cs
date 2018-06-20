using System;
using System.Xml;
using System.Web;
using System.Web.Security;
using System.Collections;

using ExpressModel;

namespace ExpressWeb.Authorizes
{
    public class Authentication
    {
        /// <summary>
        /// 设置登录用户验证票据，但不将用户重定向到原始请求页面或默认页面，调用此方法后请代码跳转到指定页面
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userData"></param>
        /// <param name="createPersistentCookie"></param>
        /// <param name="cookiePath"></param>
        public static void RedirectFromLoginPage(string userName, string userData, bool createPersistentCookie, string cookiePath = null)
        {
            SetAuthCookie(userName, userData, createPersistentCookie, cookiePath);
        }

        /// <summary>
        /// 设置身份验证Cookie
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userData">用户信息，actid#actname#actgroupid</param>
        /// <param name="createPersistentCookie">是否创建持久身份验证Cookie</param>
        /// <param name="cookiePath">Cookie域名称</param>
        public static void SetAuthCookie(string userName, string userData, bool createPersistentCookie, string cookiePath = null)
        {
            FormsAuthenticationTicket ticket = CreateAuthenticationTicket(userName, userData, createPersistentCookie, cookiePath);
            string encrypetedTicket = FormsAuthentication.Encrypt(ticket);
            if (!FormsAuthentication.CookiesSupported)
            {
                //浏览器不支持Cookie时
                FormsAuthentication.SetAuthCookie(encrypetedTicket, createPersistentCookie);
            }
            else
            {
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypetedTicket);
                if (ticket.IsPersistent)
                {
                    authCookie.Expires = ticket.Expiration;
                }
                HttpContext.Current.Response.Cookies.Add(authCookie);
            }
        }

        /// <summary>
        /// 创建身份验证凭据
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userData">用户信息，json格式</param>
        /// <param name="isPersistent">是否创建持久身份验证Cookie</param>
        /// <param name="cookiePath">Cookie域名称</param>
        private static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, string userData, bool isPersistent, string cookiePath = null)
        {
            cookiePath = cookiePath == null ? FormsAuthentication.FormsCookiePath : cookiePath;
            int expirationMinutes = GetCookieTimeoutValue();
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddMinutes(expirationMinutes), isPersistent, userData, cookiePath);
            return ticket;
        }

        /// <summary>
        /// 获取Web.config中超时配置信息，默认30分钟
        /// </summary>
        private static int GetCookieTimeoutValue()
        {
            int timeout = 30;
            XmlDocument webConfig = new XmlDocument();
            webConfig.Load(HttpContext.Current.Server.MapPath(@"~\Web.config"));
            XmlNode node = webConfig.SelectSingleNode("/configuration/system.web/authentication/forms");
            if (node != null && node.Attributes["timeout"] != null)
            {
                timeout = int.Parse(node.Attributes["timeout"].Value);
            }
            return timeout;
        }

        /// <summary>
        /// 设置用户登陆成功凭据（Cookie存储）
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userPower">权限</param>
        public static void SetCookie(string userName, int userPower)
        {
            string userData = userName + "#" + userPower;
            if (true)
            {
                //数据放入ticket
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddHours(2), false, userData);
                //数据加密
                string enyTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enyTicket);
                cookie.HttpOnly = true;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// 判断用户是否登陆
        /// </summary>
        /// <returns>True,Fales</returns>
        public static bool IsLogin()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// 注销登陆
        /// </summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// 用户登录验证通过后判断用户是否重复登录
        /// </summary>
        /// <param name="userName"></param>
        public static void SingleUserCheck(string userName)
        {
            HttpContext httpContext = HttpContext.Current;
            Hashtable userOnline = (Hashtable)httpContext.Application["Online"];
            if (userOnline != null)
            {
                int i = 0;
                while (i < userOnline.Count)
                {
                    IDictionaryEnumerator idE = userOnline.GetEnumerator();
                    string strKey = string.Empty;
                    while (idE.MoveNext())
                    {
                        if (idE.Value != null && idE.Value.ToString().Equals(userName))  //如果当前用户已经登录，
                        {
                            strKey = idE.Key.ToString();
                            userOnline[strKey] = "XXXXXX";   //将当前用户已经在全局变量中的值设置为XX 
                            break;
                        }
                    }
                    i++;
                }
            }
            else
            {
                userOnline = new Hashtable();
            }
            userOnline[httpContext.Session.SessionID] = userName;  //初始化当前用户的  sessionid
            httpContext.Application.Lock();
            httpContext.Application["Online"] = userOnline;
            httpContext.Application.UnLock();
        }

        /// <summary>
        /// 获取凭据中的用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        public static int GetActId()
        {
            if (IsLogin())
            {
                string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;
                string[] userData = strUserData.Split('#');
                if (userData.Length != 0)
                {
                    return Convert.ToInt32(userData[0]);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取凭据中的用户名
        /// </summary>
        /// <returns>用户名</returns>
        public static string GetActName()
        {
            if (IsLogin())
            {
                string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;
                string[] userData = strUserData.Split('#');
                if (userData.Length != 0)
                {
                    return userData[1].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 在Session封装当前登录的用户
        /// 用户在某系统登录时加载
        /// </summary>
        public static ModEmployee WebAccount
        {
            get
            {
                if (HttpContext.Current.Session["CurrentWebAccount"] != null)
                {
                    return HttpContext.Current.Session["CurrentWebAccount"] as ModEmployee;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["CurrentWebAccount"] = value;
            }
        }
    }
}