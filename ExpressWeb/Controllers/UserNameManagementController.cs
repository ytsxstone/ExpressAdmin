using ExpressCommon;
using ExpressDAL;
using ExpressModel;
using ExpressWeb.Authorizes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// api接口允许访问接口管理
    /// </summary>
    public class UserNameManagementController : BaseController
    {
        DalUserNameMge dal = new DalUserNameMge();

        // GET: UserNameManagement
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取地区信息
        /// </summary>
        public ContentResult GetUserNameManageData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "created_time";
                string sortType = fc["order"] ?? "desc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                var name = fc["name"];

                var total = 0;
                var dt = dal.GetUserNameManageData(name, pageSize, pageIndex, ref total);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var tabJson = JsonHelper.TableToJson(dt);

                    return Content("{\"total\":\"" + total + "\",\"rows\":" + tabJson + ",\"sortColumn\":\"" + sortColumn + "\",\"sortType\":\"" + sortType + "\"}");
                }
                else
                {
                    return Content("{\"total\":\"0\",\"rows\":[],\"sortColumn\":\"" + sortColumn + "\",\"sortType\":\"" + sortType + "\"}");
                }
            }
            catch (Exception ex)
            {
                return Content("{\"total\":\"0\",\"rows\":[],\"msg\":\"" + ex.Message + "\"}");
            }
        }

        /// <summary>
        /// 新增地区信息
        /// </summary>
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var username = fc["username"].Trim();
                if (!dal.GetUserNameManagementIsExists(0, username))
                {
                    json.Status = false;
                    json.Msg = "用户名已存在！";
                }
                else
                {
                    var num = dal.Create(username, Authentication.WebAccount.EmployeeName);
                    if (num > 0)
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
        /// 修改地区信息
        /// </summary>
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var username = fc["username"].Trim();
                var id = Convert.ToInt32(fc["id"].Trim());

                if (!dal.GetUserNameManagementIsExists(id, username))
                {
                    json.Status = false;
                    json.Msg = "用户名已存在！";
                }
                else
                {
                    var num = dal.Update(id, username, Authentication.WebAccount.EmployeeName);
                    if (num > 0)
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
        /// 删除地区信息
        /// </summary>
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var ids = fc["ids"].Trim();

                var num = dal.Delete(ids);
                if (num > 0)
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
    }
}