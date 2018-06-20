using System;
using System.Web;
using System.Web.Mvc;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 登录登出控制器
    /// </summary>
    public class AuthController : Controller
    {
        public DalAuth dalAuth = new DalAuth();

        // GET: Auth
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Login(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                string userName = fc["username"].Trim();
                string userPwd = CEncryptHelper.DesEncrypt(fc["userpwd"].Trim());

                //获取登录对象
                var loginEmployee = ComHelper.ReaderToModel<ModEmployee>(dalAuth.GetEmployeeData(userName, userPwd));

                if (loginEmployee != null) //登录成功
                {
                    //判断用户是否重复登录
                    Authentication.SingleUserCheck(userName);
                    //此语句是为了解决无任何权限的用户登录后不按退出按钮而再次跳转进入登录页面以登录其它有权限的用户后依然显示无权限
                    this.Session.RemoveAll();

                    //设置登录用户票据信息
                    Authentication.RedirectFromLoginPage(userName, loginEmployee.Id + "#" + userName, false);
                    //存储登录用户对象到session
                    Authentication.WebAccount = loginEmployee;

                    json.Status = true;
                    json.Msg = "登录成功！";
                }
                else
                {
                    json.Status = false;
                    json.Msg = "登录失败，用户名或密码错误！";
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "登录失败，Error：" + ex.Message;
            }
            
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Logout()
        {
            //清空session
            Authentication.Logout();
            Session.Abandon();

            JsonData json = new JsonData();
            json.Status = true;
            json.Msg = "/Auth/Login";

            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}