using System;
using System.Linq;
using System.Data;
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
    /// 首页控制器
    /// </summary>
    public class HomeController : BaseController
    {
        public DalAuth dalAuth = new DalAuth();
        public string treeImage = string.Empty;

        // GET: Index
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 左侧菜单
        /// </summary>
        /// <returns></returns>
        public JsonResult LeftMenu()
        {
            List<EasyTreeData> treeList = new List<EasyTreeData>();

            //获取登录用户权限菜单
            var dt = dalAuth.GetEmployeeMenuAuth(Authentication.WebAccount.Id);

            if (dt != null && dt.Rows.Count > 0)
            {
                var IRow = new List<DataRow>();
                foreach (DataRow item in dt.Rows)
                {
                    IRow.Add(item);
                }

                var list = DataRowConvertToEntity<ModMenu>(IRow);
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        List<EasyTreeData> cc = new List<EasyTreeData>();
                        EasyTreeData treeData = new EasyTreeData(item.Id.ToString(), "<a href='Javascript:void(0);' " +
                        (!string.IsNullOrWhiteSpace(item.MenuUrl)
                            ? "onclick=\"OpenTabs(\'" + item.MenuName + "\', \'" + item.MenuUrl +
                              "?AuthorityId=" + item.Id + "\', \'" +
                              item.MenuIcon + "\')\""
                            : "") + " >" + item.MenuName + "</a>",
                            !string.IsNullOrWhiteSpace(item.MenuIcon) ? item.MenuIcon : treeImage, "open");
                        treeData.children = cc;
                        treeList.Add(treeData);
                    }
                }
            }

            return Json(treeList, JsonRequestBehavior.AllowGet);
        }
    }
}