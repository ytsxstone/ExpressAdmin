using System;
using System.IO;
using System.Text;
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
    /// 禁运物品管理控制器类
    /// </summary>
    public class ProhibitedController : BaseController
    {
        public static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + Authentication.WebAccount?.EmployeeAccount + "/";
        int globalMsgRowSize = 1;

        DalProhibited dalProhibited = new DalProhibited();

        // GET: Prohibited
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询禁运信息
        /// </summary>
        public ContentResult GetProhibitedData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "pid";
                string sortType = fc["order"] ?? "asc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                var pname = fc["pname"];
                var type = Convert.ToInt32(fc["type"]);

                int total = 0;
                var dt = dalProhibited.GetProhibitedData(pname, type, pageSize, pageIndex, sortColumn, sortType, ref total);

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
        /// 创建禁运信息
        /// </summary>
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var pname = fc["pname"];
                var premark = fc["premark"];
                var type = Convert.ToInt32(fc["type"]);

                if (!dalProhibited.GetProhibitedNameIsExists(0, type, pname))
                {
                    json.Status = false;
                    json.Msg = "该类型名称重覆！";
                }
                else
                {
                    var num = dalProhibited.Create(pname, premark, type, Authentication.WebAccount.EmployeeAccount);
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
        /// 修改禁运信息
        /// </summary>
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var pname = fc["pname"];
                var premark = fc["premark"];
                var type = Convert.ToInt32(fc["type"]);
                var pid = Convert.ToInt32(fc["pid"]);

                if (!dalProhibited.GetProhibitedNameIsExists(pid, type, pname))
                {
                    json.Status = false;
                    json.Msg = "该类型名称重覆！";
                }
                else
                {
                    var num = dalProhibited.Update(pid, pname, premark, type, Authentication.WebAccount.EmployeeAccount);
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
        /// 删除禁运信息
        /// </summary>
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var ids = fc["ids"].Trim();

                var num = dalProhibited.Delete(ids);
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

        /// <summary>
        /// 导入
        /// </summary>
        [HttpPost]
        public JsonResult Import(HttpPostedFileBase p_file)
        {
            var json = new JsonData();
            try
            {
                #region 上传文件

                if ((p_file.ContentLength / 1024 / 1024) > 10)
                {
                    json.Status = false;
                    json.Msg = "文件大小不能超过" + (10).ToString() + "M！";
                    return Json(json);
                }

                var fileName = p_file.FileName;//文件名
                var fType = ComHelper.GetFileType(fileName);
                var fileTypes = new string[] { "xls", "xlsx" };   //可上传文件格式

                if (!fileTypes.Contains(fType.ToLower()))
                {
                    json.Status = false;
                    json.Msg = "只能导入excel格式！";
                    return Json(json);
                }

                var a = fileName.LastIndexOf('\\');
                fileName = fileName.Substring(a + 1);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                p_file.SaveAs(Path.Combine(filePath, fileName));

                #endregion

                #region 读取excel的内容, 验证格式是否符合规范

                //将excel文件的数据加载到datatable中
                DataTable dt_import =ExcelHelper.ExportToDataTable(Path.Combine(filePath, fileName));

                //DataTable数据完整性验证
                List<string> fieldList = new List<string>() { "名称", "备注", "类型" };
                string fieldMsg = "";
                if (!ComHelper.DataTableFieldValid(dt_import, fieldList, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断禁运物品名称是否有为空的记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("名称")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("类型"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在名称或类型为空的记录，导入失败！" });
                }

                //判断是否存在名称、类型重复的情况
                if (!IsRepeatByColumnName(dt_import))
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在名称、类型重复的记录，导入失败！" });
                }

                //数据有效性验证
                StringBuilder validMsg = new StringBuilder();
                string validStr = string.Empty;
                bool validFlag = true;
                int rowCount = 0;
                //循环验证
                foreach (DataRow row in dt_import.Rows)
                {
                    //导入行数据验证
                    validStr = EmportRowValidData(row);
                    if (!string.IsNullOrEmpty(validStr))
                    {
                        rowCount++;
                        validMsg.Append(validStr);

                        if (rowCount % globalMsgRowSize == 0)
                        {
                            validMsg.Append("<br/>");
                            validMsg.Append("<br/>");
                        }
                        else
                        {
                            validMsg.Append("；");
                        }

                        //验证不通过
                        validFlag = false;
                        continue;
                    }
                }
                //数据验证不通过
                if (!validFlag)
                {
                    return Json(new JsonData() { Status = false, Msg = validMsg.ToString() });
                }

                #endregion

                #region 入库

                //将dataTable转换为list集合
                List<ModProhibited> listEmportData = new List<ModProhibited>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listEmportData.Add(new ModProhibited()
                    {
                        Type = Convert.ToInt32(row["类型"]),
                        PName = row["名称"].ToString().Trim(),
                        PRemark = row["备注"].ToString().Trim()
                    });
                }

                //批量导入
                dalProhibited.BulkEmport(listEmportData, Authentication.WebAccount.EmployeeAccount);

                json.Status = true;
                json.Msg = "成功导入" + listEmportData.Count.ToString() + "条数据！";

                #endregion

                #region 删除临时文件

                try
                {
                    FileInfo fi = new FileInfo(Path.Combine(filePath, fileName));
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }

                    //删除目录
                    if (!Directory.Exists(filePath))
                    {
                        Directory.Delete(filePath);
                    }
                }
                catch
                { }

                #endregion
            }
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "导入失败！" + ex.Message;
            }
            return Json(json);
        }

        /// <summary>
        /// 导入数据验证
        /// </summary>
        /// <param name="row">数据行</param>
        /// <returns></returns>
        private string EmportRowValidData(DataRow row)
        {
            bool flag = true;
            string valueNumberError = string.Empty;
            string validMsg = "名称：" + row["名称"].ToString().Trim() + "，类型：" + row["类型"].ToString().Trim();

            //不为空时, 类型只能输入数字
            if (!string.IsNullOrWhiteSpace(row["类型"].ToString()) && !ComHelper.CustomeRegex(row["类型"].ToString().Trim(), "^[1,2,3]$"))
            {
                flag = false;
                validMsg += "  类型只能输入数字[1,2,3]";
            }

            //验证通过, 清空字符串
            if (flag)
                validMsg = "";

            return validMsg;
        }

        /// <summary>
        /// 判断指定列是否重复
        /// </summary>
        /// <param name="dt_import"></param>
        /// <returns></returns>
        private bool IsRepeatByColumnName(DataTable dt_import)
        {
            bool result = true;

            //获取分组记录大于1 的记录
            int repeatCount = ((from p in dt_import.AsEnumerable()
                                group p by new { f1 = p.Field<string>("名称"), f2 = p.Field<string>("类型") } into g
                                select new
                                {
                                    RepeatKey = g.Key,
                                    RepeatCount = g.Count()
                                }).Where(p => p.RepeatCount > 1)).Count();

            //大于0 表示存在重复记录
            if (repeatCount > 0)
            {
                result = false;
            }

            return result;
        }

        #region 导出

        public FileStreamResult ExportExcel(string name, int type, string sort, string order)
        {
            try
            {
                //设置排序参数
                string sortColumn = sort ?? "pid";
                string sortType = order ?? "asc";

                //获取导出数据
                DataTable dt_export = dalProhibited.GetProhibitedExportData(name, type, sortColumn, sortType);

                //重命名列名
                DataTableColumnRename(dt_export);

                #region 配置导出参数

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "名称", "类型", "备注" };

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("名称", 13.85);
                dictWidthColumns.Add("类型", 13.85);
                dictWidthColumns.Add("备注", 25.85);

                #endregion

                List<string> empty = new List<string>();
                //获取保存文件名称                     
                string fileName = "禁运物品数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

                //导出到excel
                string saveStatus = ExcelHelper.SaveDataTableToExcel2(dt_export, empty, empty, empty, contentLeftStyleColumns, dictWidthColumns, filePath, fileName, EnumExcelFileType.Excel2003);

                var fileStream = new MemoryStream();

                return File(new FileStream(saveStatus, FileMode.Open), "application/ms-excel", Server.HtmlEncode(fileName));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 重命名datacolumn列名
        /// </summary>
        /// <param name="dt_export"></param>
        private void DataTableColumnRename(DataTable dt_export)
        {
            dt_export.Columns["pname"].ColumnName = "名称";
            dt_export.Columns["type"].ColumnName = "类型";
            dt_export.Columns["premark"].ColumnName = "备注";
        }

        #endregion
    }
}