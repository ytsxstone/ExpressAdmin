using System;
using System.IO;
using System.Text;
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
    /// 地区管理控制器类
    /// </summary>
    public class CityAreaController : BaseController
    {
        public static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + Authentication.WebAccount?.EmployeeAccount + "/";
        int globalMsgRowSize = 1;

        DalCityArea dalCityArea = new DalCityArea();

        // GET: CityArea
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取地区信息
        /// </summary>
        public ContentResult GetCityAreaData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "areaid";
                string sortType = fc["order"] ?? "asc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                var name = fc["name"];

                var total = 0;
                var dt = dalCityArea.GetCityAreaData(name, pageSize, pageIndex, sortColumn, sortType, ref total);

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
                var province = fc["province"].Trim();
                var city = fc["city"].Trim();
                var postcode = fc["postcode"].Trim();

                if (!dalCityArea.GetCityAreaIsExists(0, province, city))
                {
                    json.Status = false;
                    json.Msg = "省份、城市记录已存在！";
                }
                else
                {
                    var num = dalCityArea.Create(province, city, postcode, Authentication.WebAccount.EmployeeName);
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
                var province = fc["province"].Trim();
                var city = fc["city"].Trim();
                var postcode = fc["postcode"].Trim();
                var areaid = Convert.ToInt32(fc["areaid"].Trim());

                if (!dalCityArea.GetCityAreaIsExists(areaid, province, city))
                {
                    json.Status = false;
                    json.Msg = "省份、城市记录已存在！";
                }
                else
                {
                    var num = dalCityArea.Update(areaid, province, city, postcode, Authentication.WebAccount.EmployeeName);
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

                var num = dalCityArea.Delete(ids);
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

                var dt_import = ExcelHelper.ExportToDataTable(Path.Combine(filePath, fileName));

                //DataTable数据完整性验证
                List<string> fieldList = new List<string>() { "省份/直辖市", "城市", "邮编" };
                string fieldMsg = "";
                if (!ComHelper.DataTableFieldValid(dt_import, fieldList, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断省份/直辖市、城市、邮编是否有为空的记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("省份/直辖市")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("城市")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("邮编"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在省份/直辖市、城市或邮编为空的记录，导入失败！" });
                }

                //判断是否存在省份/直辖市、城市重复的情况
                if (!IsRepeatByColumnName(dt_import))
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在省份/直辖市、城市重复的记录，导入失败！" });
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
                List<ModCityArea> listEmportData = new List<ModCityArea>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listEmportData.Add(new ModCityArea()
                    {
                        AreaProvince = row["省份/直辖市"].ToString().Trim(),
                        AreaCity = row["城市"].ToString().Trim(),
                        AreaPostcode = row["邮编"].ToString().Trim()
                    });
                }

                //批量导入
                dalCityArea.BulkEmport(listEmportData, Authentication.WebAccount.EmployeeName);

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

        public FileStreamResult ExportExcel(string name, string sort, string order)
        {
            try
            {
                //设置排序参数
                string sortColumn = sort ?? "areaid";
                string sortType = order ?? "asc";

                //获取导出数据
                DataTable dt_export = dalCityArea.GetCityAreaExportData(name, sortColumn, sortType);

                //重命名列名
                DataTableColumnRename(dt_export);

                #region 配置导出参数

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "省份/直辖市", "城市", "邮编" };

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("省份/直辖市", 15.85);
                dictWidthColumns.Add("城市", 15.85);
                dictWidthColumns.Add("邮编", 12.85);

                #endregion

                List<string> empty = new List<string>();
                //获取保存文件名称                     
                string fileName = "地区数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

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
            dt_export.Columns["areaprovince"].ColumnName = "省份/直辖市";
            dt_export.Columns["areacity"].ColumnName = "城市";
            dt_export.Columns["areapostcode"].ColumnName = "邮编";
        }

        #endregion

        /// <summary>
        /// 导入数据验证
        /// </summary>
        /// <param name="row">数据行</param>
        /// <returns></returns>
        private string EmportRowValidData(DataRow row)
        {
            bool flag = true;
            string valueNumberError = string.Empty;
            string validMsg = "省份/直辖市：" + row["省份/直辖市"].ToString().Trim() + "  城市：" + row["城市"].ToString().Trim();

            //不为空时, 正则匹配输入是否为合法的数字
            if (!string.IsNullOrWhiteSpace(row["邮编"].ToString()) && !ComHelper.IsNumber(row["邮编"].ToString().Trim()))
            {
                flag = false;
                validMsg += "  邮编只能为数字";
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
                                group p by new { f1 = p.Field<string>("省份/直辖市"), f2 = p.Field<string>("城市") } into g
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
    }
}