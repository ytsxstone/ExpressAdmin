using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 菜单控制器
    /// </summary>
    public class MenuController : BaseController
    {
        DalMenu dalMenu = new DalMenu();

        // GET: Index
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetMenuData(FormCollection fc)
        {
            try
            {
                var name = fc["name"];

                var dt = dalMenu.GetMenuData(name);
                var strJson = "";
                if (dt != null && dt.Rows.Count > 0)
                {
                    strJson = "{\"total\":\"" + dt.Rows.Count + "\",";

                    var tabJson = JsonHelper.TableToJson(dt);

                    strJson += "\"rows\":" + tabJson + "}";

                    return Content(strJson);
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
        /// 插入菜单
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var parentid = Convert.ToInt32(fc["parentid"].Trim());
                var code = fc["code"].Trim();
                var name = fc["name"].Trim();
                var path = fc["path"].Trim();
                var sort = Convert.ToInt32(fc["sort"].Trim());
                var icon = fc["icon"].Trim();

                if (dalMenu.Create(parentid, code, name, path, sort, icon) > 0)
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
            catch(Exception ex)
            {
                json.Status = false;
                json.Msg = "新增失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改菜单
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var id = Convert.ToInt32(fc["id"].Trim());
                var parentid = Convert.ToInt32(fc["parentid"].Trim());
                var code = fc["code"].Trim();
                var name = fc["name"].Trim();
                var path = fc["path"].Trim();
                var sort = Convert.ToInt32(fc["sort"].Trim());
                var icon = fc["icon"].Trim();

                if (dalMenu.Update(id, parentid, code, name, path, sort, icon) > 0)
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
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "修改失败！Error：" + ex.Message;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var ids = fc["ids"].Trim();

                if (dalMenu.Delete(ids) > 0)
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