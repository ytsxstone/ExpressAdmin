using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 员工操作控制器
    /// </summary>
    public class EmployeeController : BaseController
    {
        //默认密码
        public string DefaultPassword = ConfigurationManager.AppSettings["DefaultPassword"] ?? "123456";

        public DalEmployee dalEmployee = new DalEmployee();
        public DalMenu dalMenu = new DalMenu();

        #region 人员管理

        // GET: Index
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetEmployeeData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "id";
                string sortType = fc["order"] ?? "asc";

                var name = fc["name"];

                var dt = dalEmployee.GetEmployee(name, sortColumn, sortType);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var tabJson = JsonHelper.TableToJson(dt);

                    return Content("{\"total\":\"" + dt.Rows.Count + "\",\"rows\":" + tabJson + "}");
                }
                else
                {
                    return Content("{\"total\":\"0\",\"rows\":[]}");
                }
            }
            catch (Exception ex)
            {
                return Content("{\"total\":\"0\",\"rows\":[],\"msg\":\"" + ex.Message + "\"}");
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取参数
                var account = fc["emp_account"].Trim();
                var name = fc["emp_name"].Trim();
                var desc = fc["emp_desc"].Trim();

                if (!dalEmployee.GetEmployeeAccountIsExists(0, account))
                {
                    json.Status = false;
                    json.Msg = "员工账号不能重覆！";
                }
                else
                {
                    //默认密码
                    var pwd = DefaultPassword;
                    pwd = CEncryptHelper.DesEncrypt(pwd);

                    if (dalEmployee.Create(account, name, pwd, desc) > 0)
                    {
                        json.Status = true;
                        json.Msg = "新增成功！";
                    }
                    else
                    {
                        json.Status = false;
                        json.Msg = "新增失败！";
                    }
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "新增失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取参数
                var account = fc["emp_account"].Trim();
                var name = fc["emp_name"].Trim();
                var desc = fc["emp_desc"].Trim();
                var id = Convert.ToInt32(fc["id"].Trim());

                if (!dalEmployee.GetEmployeeAccountIsExists(id, account))
                {
                    json.Status = false;
                    json.Msg = "员工账号不能重覆！";
                }
                else
                {
                    if (dalEmployee.Update(id, account, name, desc) > 0)
                    {
                        json.Status = true;
                        json.Msg = "修改成功！";
                    }
                    else
                    {
                        json.Status = false;
                        json.Msg = "修改失败！";
                    }
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "修改失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取参数
                var ids = fc["ids"].Trim();

                if (dalEmployee.Delete(ids) > 0)
                {
                    json.Status = true;
                    json.Msg = "删除成功！";
                }
                else
                {
                    json.Status = false;
                    json.Msg = "删除失败！";
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "删除失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetPassword(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取参数
                var id = Convert.ToInt32(fc["id"].Trim());

                //默认密码
                var defaultPwd = CEncryptHelper.DesEncrypt(DefaultPassword);

                if (dalEmployee.UpdatePassword(id, defaultPwd) > 0)
                {
                    json.Status = true;
                    json.Msg = "重置成功！";
                }
                else
                {
                    json.Status = false;
                    json.Msg = "重置失败！";
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "重置失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 权限菜单

        /// <summary>
        /// 员工菜单管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AuthConfig()
        {
            return View();
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetEmployeeList()
        {
            var dt = dalEmployee.GetEmployee("", "id", "asc");

            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var data = JsonConvert.SerializeObject(dt, setting);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取员工菜单配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetAuthConfigByEmployeeId(string id)
        {
            var allMenu = dalMenu.GetMenuData();
            var empAuth = dalEmployee.GetEmployeeAuthConfig(id);

            //添加状态列
            if (!allMenu.Columns.Contains("checked"))
            {
                allMenu.Columns.Add("checked");
            }

            //构建tree
            foreach (DataRow row in allMenu.Rows)
            {
                var authRow = empAuth.Select(string.Format("menuid='{0}'", row["id"].ToString()));
                if (authRow.Length > 0)
                {
                    row["checked"] = true;
                }
                else
                {
                    row["checked"] = false;
                }
            }
            allMenu.AcceptChanges();

            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var data = JsonConvert.SerializeObject(allMenu, setting);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存员工菜单
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveAuthConfig(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取参数
                var eId = fc["eid"].Trim();
                var aId = fc["aid"].Trim();

                var list = new List<string>();
                var array = aId.Split(',');

                foreach (var item in array)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        list.Add(item);
                    }
                }

                if (dalEmployee.UpdateEmployeeAuthConfig(eId, list) > 0)
                {
                    json.Status = true;
                    json.Msg = "保存成功！";
                }
                else
                {
                    json.Status = false;
                    json.Msg = "保存失败！";
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "保存失败！Error：" + ex.Message;
            }
            
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 密码管理

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdatePassword()
        {
            return View();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdatePassword(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                //获取当前登录用户ID
                var employeeId = Authentication.WebAccount.Id;

                var pwd = CEncryptHelper.DesEncrypt(fc["pwd"].Trim());
                var newPwd = CEncryptHelper.DesEncrypt(fc["pwdNew"].Trim());

                if (dalEmployee.CheckOldPwd(pwd, employeeId))
                {
                    if (dalEmployee.UpdatePassword(employeeId, newPwd) > 0)
                    {
                        json.Status = true;
                        json.Msg = "修改成功！";
                    }
                    else
                    {
                        json.Status = false;
                        json.Msg = "修改失败！";
                    }
                }
                else
                {
                    json.Status = false;
                    json.Msg = "修改失败，原密码错误！";
                }
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "修改失败！Error：" + ex.Message;
            }

            return Json(json);
        }

        #endregion
    }
}