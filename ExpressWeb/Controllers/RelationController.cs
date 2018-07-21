using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using System.Collections.Generic;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 物品对应关系管理控制器类
    /// </summary>
    public class RelationController : BaseController
    {
        public static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + Authentication.WebAccount?.EmployeeAccount + "/";
        int globalMsgRowSize = 1;

        DalRelation dalRelation = new DalRelation();

        // GET: Relation
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取商品对应关系数据
        /// </summary>
        public ContentResult GetGoodRelationData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "rid";
                string sortType = fc["order"] ?? "asc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                ModGoodRelation model = new ModGoodRelation();
                model.OriginalName = fc["originalname"];
                model.TaxNumber = fc["taxnumber"];
                model.NewName1 = fc["newname1"];

                int total = 0;
                var dt = dalRelation.GetGoodRelationData(model, pageSize, pageIndex, sortColumn, sortType, ref total);

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
        /// 创建商品关系
        /// </summary>
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();
            try
            {
                ModGoodRelation model = new ModGoodRelation();
                model.OriginalName = fc["originalname"].Trim();

                if (!dalRelation.GetOriginalNameIsExists(0, model.OriginalName))
                {
                    json.Status = false;
                    json.Msg = "原物品名称已存在！";
                }
                else
                {
                    model.TaxNumber = fc["taxnumber"];
                    model.NewName1 = fc["newname1"];
                    model.NewName2 = fc["newname2"];
                    model.NewName3 = fc["newname3"];
                    model.NewName4 = fc["newname4"];
                    model.NewName5 = fc["newname5"];
                    model.NewName6 = fc["newname6"];
                    model.NewName7 = fc["newname7"];
                    model.NewName8 = fc["newname8"];
                    model.NewName9 = fc["newname9"];
                    model.NewName10 = fc["newname10"];
                    model.NewName11 = fc["newname11"];
                    model.NewName12 = fc["newname12"];
                    model.NewName13 = fc["newname13"];
                    model.NewName14 = fc["newname14"];
                    model.NewName15 = fc["newname15"];
                    model.NewName16 = fc["newname16"];
                    model.NewName17 = fc["newname17"];
                    model.NewName18 = fc["newname18"];
                    model.NewName19 = fc["newname19"];
                    model.NewName20 = fc["newname20"];
                    model.Created = Authentication.WebAccount.EmployeeName;

                    var num = dalRelation.Create(model);
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
        /// 修改商品关系
        /// </summary>
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                ModGoodRelation model = new ModGoodRelation();
                model.OriginalName = fc["originalname"].Trim();
                model.Rid = Convert.ToInt32(fc["rid"]);

                if (!dalRelation.GetOriginalNameIsExists(model.Rid, model.OriginalName))
                {
                    json.Status = false;
                    json.Msg = "原物品名称已存在！";
                }
                else
                {
                    model.TaxNumber = fc["taxnumber"];
                    model.NewName1 = fc["newname1"];
                    model.NewName2 = fc["newname2"];
                    model.NewName3 = fc["newname3"];
                    model.NewName4 = fc["newname4"];
                    model.NewName5 = fc["newname5"];
                    model.NewName6 = fc["newname6"];
                    model.NewName7 = fc["newname7"];
                    model.NewName8 = fc["newname8"];
                    model.NewName9 = fc["newname9"];
                    model.NewName10 = fc["newname10"];
                    model.NewName11 = fc["newname11"];
                    model.NewName12 = fc["newname12"];
                    model.NewName13 = fc["newname13"];
                    model.NewName14 = fc["newname14"];
                    model.NewName15 = fc["newname15"];
                    model.NewName16 = fc["newname16"];
                    model.NewName17 = fc["newname17"];
                    model.NewName18 = fc["newname18"];
                    model.NewName19 = fc["newname19"];
                    model.NewName20 = fc["newname20"];
                    model.Updated = Authentication.WebAccount.EmployeeName;

                    var num = dalRelation.Update(model);
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
        /// 删除商品关系
        /// </summary>
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var ids = fc["ids"].Trim();

                var num = dalRelation.Delete(ids);
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
        public JsonResult Import(HttpPostedFileBase p_relation_file)
        {
            var json = new JsonData();
            try
            {
                #region 上传文件

                if ((p_relation_file.ContentLength / 1024 / 1024) > 10)
                {
                    json.Status = false;
                    json.Msg = "文件大小不能超过" + (10).ToString() + "M！";
                    return Json(json);
                }

                var fileName = p_relation_file.FileName;//文件名
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
                p_relation_file.SaveAs(Path.Combine(filePath, fileName));

                #endregion

                #region 验证格式是否符合规范

                //将excel文件的数据加载到datatable中
                DataTable dt_import = ExcelHelper.ExportToDataTable(Path.Combine(filePath, fileName));

                //DataTable数据完整性验证
                List<string> fieldList = new List<string>() { "原物品名称", "商品税号", "新物品名称1", "新物品名称2", "新物品名称3", "新物品名称4", "新物品名称5", "新物品名称6",
                        "新物品名称7", "新物品名称8", "新物品名称9", "新物品名称10", "新物品名称11", "新物品名称12", "新物品名称13", "新物品名称14", "新物品名称15", "新物品名称16",
                        "新物品名称17", "新物品名称18", "新物品名称19", "新物品名称20" };
                string fieldMsg = "";
                if (!ComHelper.DataTableFieldValid(dt_import, fieldList, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断原物品名称、商品税号是否有为空的记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("原物品名称")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("商品税号"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在原物品名称或商品税号为空的记录，导入失败！" });
                }

                //判断是否存在原物品名称重复的情况
                if (!ComHelper.IsRepeatByColumnName(dt_import, "原物品名称"))
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在原物品名称重复的记录，导入失败！" });
                }

                #endregion

                #region 入库

                //将dataTable转换为list集合
                List<ModGoodRelation> listEmportData = new List<ModGoodRelation>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listEmportData.Add(new ModGoodRelation()
                    {
                        OriginalName = row["原物品名称"].ToString().Trim(),
                        TaxNumber = row["商品税号"].ToString().Trim(),
                        NewName1 = row["新物品名称1"].ToString().Trim(),
                        NewName2 = row["新物品名称2"].ToString().Trim(),
                        NewName3 = row["新物品名称3"].ToString().Trim(),
                        NewName4 = row["新物品名称4"].ToString().Trim(),
                        NewName5 = row["新物品名称5"].ToString().Trim(),
                        NewName6 = row["新物品名称6"].ToString().Trim(),
                        NewName7 = row["新物品名称7"].ToString().Trim(),
                        NewName8 = row["新物品名称8"].ToString().Trim(),
                        NewName9 = row["新物品名称9"].ToString().Trim(),
                        NewName10 = row["新物品名称10"].ToString().Trim(),
                        NewName11 = row["新物品名称11"].ToString().Trim(),
                        NewName12 = row["新物品名称12"].ToString().Trim(),
                        NewName13 = row["新物品名称13"].ToString().Trim(),
                        NewName14 = row["新物品名称14"].ToString().Trim(),
                        NewName15 = row["新物品名称15"].ToString().Trim(),
                        NewName16 = row["新物品名称16"].ToString().Trim(),
                        NewName17 = row["新物品名称17"].ToString().Trim(),
                        NewName18 = row["新物品名称18"].ToString().Trim(),
                        NewName19 = row["新物品名称19"].ToString().Trim(),
                        NewName20 = row["新物品名称20"].ToString().Trim(),
                    });
                }

                //批量导入
                dalRelation.BulkEmport(listEmportData, Authentication.WebAccount.EmployeeName);

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

        #region 导出

        public FileStreamResult ExportExcel(string originalname, string taxnumber, string newname1, string sort, string order)
        {
            try
            {
                //设置排序参数
                string sortColumn = sort ?? "rid";
                string sortType = order ?? "asc";

                //获取导出数据
                DataTable dt_export = dalRelation.GetGoodRelationExportData(originalname, taxnumber, newname1, sortColumn, sortType);

                //重命名列名
                DataTableColumnRename(dt_export);

                #region 配置导出参数

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "原物品名称", "商品税号", "新物品名称1", "新物品名称2", "新物品名称3", "新物品名称4", "新物品名称5",
                        "新物品名称6", "新物品名称7", "新物品名称8", "新物品名称9", "新物品名称10", "新物品名称11", "新物品名称12", "新物品名称13", 
                        "新物品名称14", "新物品名称15", "新物品名称16", "新物品名称17", "新物品名称18", "新物品名称19", "新物品名称20"};

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("原物品名称", 15.15);
                dictWidthColumns.Add("商品税号", 15.15);
                dictWidthColumns.Add("新物品名称1", 15.15);
                dictWidthColumns.Add("新物品名称2", 15.15);
                dictWidthColumns.Add("新物品名称3", 15.15);
                dictWidthColumns.Add("新物品名称4", 15.15);
                dictWidthColumns.Add("新物品名称5", 15.15);
                dictWidthColumns.Add("新物品名称6", 15.15);
                dictWidthColumns.Add("新物品名称7", 15.15);
                dictWidthColumns.Add("新物品名称8", 15.15);
                dictWidthColumns.Add("新物品名称9", 15.15);
                dictWidthColumns.Add("新物品名称10", 15.15);
                dictWidthColumns.Add("新物品名称11", 15.15);
                dictWidthColumns.Add("新物品名称12", 15.15);
                dictWidthColumns.Add("新物品名称13", 15.15);
                dictWidthColumns.Add("新物品名称14", 15.15);
                dictWidthColumns.Add("新物品名称15", 15.15);
                dictWidthColumns.Add("新物品名称16", 15.15);
                dictWidthColumns.Add("新物品名称17", 15.15);
                dictWidthColumns.Add("新物品名称18", 15.15);
                dictWidthColumns.Add("新物品名称19", 15.15);
                dictWidthColumns.Add("新物品名称20", 15.15);
                #endregion

                List<string> empty = new List<string>();
                //获取保存文件名称                     
                string fileName = "物品对应关系数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

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
            dt_export.Columns["originalname"].ColumnName = "原物品名称";
            dt_export.Columns["taxnumber"].ColumnName = "商品税号";
            dt_export.Columns["newname1"].ColumnName = "新物品名称1";
            dt_export.Columns["newname2"].ColumnName = "新物品名称2";
            dt_export.Columns["newname3"].ColumnName = "新物品名称3";
            dt_export.Columns["newname4"].ColumnName = "新物品名称4";
            dt_export.Columns["newname5"].ColumnName = "新物品名称5";
            dt_export.Columns["newname6"].ColumnName = "新物品名称6";
            dt_export.Columns["newname7"].ColumnName = "新物品名称7";
            dt_export.Columns["newname8"].ColumnName = "新物品名称8";
            dt_export.Columns["newname9"].ColumnName = "新物品名称9";
            dt_export.Columns["newname10"].ColumnName = "新物品名称10";
            dt_export.Columns["newname11"].ColumnName = "新物品名称11";
            dt_export.Columns["newname12"].ColumnName = "新物品名称12";
            dt_export.Columns["newname13"].ColumnName = "新物品名称13";
            dt_export.Columns["newname14"].ColumnName = "新物品名称14";
            dt_export.Columns["newname15"].ColumnName = "新物品名称15";
            dt_export.Columns["newname16"].ColumnName = "新物品名称16";
            dt_export.Columns["newname17"].ColumnName = "新物品名称17";
            dt_export.Columns["newname18"].ColumnName = "新物品名称18";
            dt_export.Columns["newname19"].ColumnName = "新物品名称19";
            dt_export.Columns["newname20"].ColumnName = "新物品名称20";
        }

        #endregion
    }
}