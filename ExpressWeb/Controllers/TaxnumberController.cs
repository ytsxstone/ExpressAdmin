using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 商品税号管理控制器类
    /// </summary>
    public class TaxNumberController : BaseController
    {
        public static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + Authentication.WebAccount?.EmployeeAccount + "/";
        int globalMsgRowSize = 1;

        DalTaxNumber dalTaxNumber = new DalTaxNumber();

        // GET: Taxnumber
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询商品税号信息
        /// </summary>
        public ContentResult GetGoodTaxNumberData(FormCollection fc)
        {
            try
            {
                //设置排序参数
                string sortColumn = fc["sort"] ?? "pid";
                string sortType = fc["order"] ?? "asc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                var number = fc["number"];
                var price = fc["price"];
                var rate = fc["rate"];

                int total = 0;
                var dt = dalTaxNumber.GetGoodTaxNumberData(number, price, rate, pageSize, pageIndex, sortColumn, sortType, ref total);

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
        /// 创建商品税号信息
        /// </summary>
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var number = fc["number"];
                var price = fc["price"];
                var rate = fc["rate"];

                if (!dalTaxNumber.GetTaxNumberIsExists(0, number))
                {
                    json.Status = false;
                    json.Msg = "商品税号已存在！";
                }
                else
                {
                    var num = dalTaxNumber.Create(number, price, rate);
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
        /// 修改商品税号信息
        /// </summary>
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var number = fc["number"];
                var price = fc["price"];
                var rate = fc["rate"];
                var pid = Convert.ToInt32( fc["pid"]);

                if (!dalTaxNumber.GetTaxNumberIsExists(pid, number))
                {
                    json.Status = false;
                    json.Msg = "商品税号已存在！";
                }
                else
                {
                    var num = dalTaxNumber.Update(pid, number, price, rate);
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
        /// 删除商品税号信息
        /// </summary>
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData();

            try
            {
                var ids = fc["ids"].Trim();

                var num = dalTaxNumber.Delete(ids);
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
        public JsonResult Import(HttpPostedFileBase p_taxnumber_file)
        {
            var json = new JsonData();
            try
            {
                #region 上传文件

                if ((p_taxnumber_file.ContentLength / 1024 / 1024) > 10)
                {
                    json.Status = false;
                    json.Msg = "文件大小不能超过" + (10).ToString() + "M！";
                    return Json(json);
                }

                var fileName = p_taxnumber_file.FileName;//文件名
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
                p_taxnumber_file.SaveAs(Path.Combine(filePath, fileName));

                #endregion

                #region 读取excel的内容, 验证格式是否符合规范

                //将excel文件的数据加载到datatable中
                DataTable dt_import = ExcelHelper.ExportToDataTable(Path.Combine(filePath, fileName));

                //DataTable数据完整性验证
                List<string> fieldList = new List<string>() { "商品税号", "完税价格", "税率" };
                string fieldMsg = "";
                if (!ComHelper.DataTableFieldValid(dt_import, fieldList, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断商品税号、完税价格、税率是否有为空的记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("商品税号")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("完税价格")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("税率"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在商品税号、完税价格或税率为空的记录，导入失败！" });
                }

                //判断是否存在商品税号重复的情况
                if (!ComHelper.IsRepeatByColumnName(dt_import, "商品税号"))
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在商品税号重复的记录，导入失败！" });
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
                List<ModTaxnNumber> listEmportData = new List<ModTaxnNumber>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listEmportData.Add(new ModTaxnNumber()
                    {
                        PTaxNumber = row["商品税号"].ToString().Trim(),
                        PTaxPrice = Convert.ToDecimal(row["完税价格"]),
                        PTaxRate = Convert.ToDecimal(row["税率"])
                    });
                }

                //批量导入
                dalTaxNumber.BulkEmport(listEmportData);

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
                json.Msg = ex.Message;
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
            string validMsg = "商品税号：" + row["商品税号"].ToString().Trim();

            //不为空时, 正则匹配输入是否为合法的数字 -- 只能输入数字，并且最多可包含两位小数
            if (!string.IsNullOrWhiteSpace(row["完税价格"].ToString()) && !ComHelper.IsNumberic(row["完税价格"].ToString().Trim()))
            {
                flag = false;
                validMsg += "  完税价格只能输入数字，并且最多可包含两位小数";
            }
            //不为空时, 正则匹配输入是否为合法的数字 -- 只能输入0~100以内的数字，并且最多可包含两位小数
            if (!string.IsNullOrWhiteSpace(row["税率"].ToString()) && !ComHelper.IsRateNumberic(row["税率"].ToString().Trim()))
            {
                flag = false;
                validMsg += "  税率只能输入0~100以内的数字，并且最多可包含两位小数";
            }

            //验证通过, 清空字符串
            if (flag)
                validMsg = "";

            return validMsg;
        }

        #region 导出

        public FileStreamResult ExportExcel(string number, string price, string rate, string sort, string order)
        {
            try
            {
                //设置排序参数
                string sortColumn = sort ?? "pid";
                string sortType = order ?? "asc";

                //获取导出数据
                DataTable dt_export = dalTaxNumber.GetGoodTaxNumberExportData(number, price, rate, sortColumn, sortType);

                //重命名列名
                DataTableColumnRename(dt_export);

                #region 配置导出参数

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "商品税号", "完税价格", "税率" };

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("商品税号", 13.85);
                dictWidthColumns.Add("完税价格", 13.85);
                dictWidthColumns.Add("税率", 10.85);

                #endregion

                List<string> empty = new List<string>();
                //获取保存文件名称                     
                string fileName = "税号数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

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
            dt_export.Columns["ptaxnumber"].ColumnName = "商品税号";
            dt_export.Columns["ptaxprice"].ColumnName = "完税价格";
            dt_export.Columns["ptaxrate"].ColumnName = "税率";
        }

        #endregion
    }
}