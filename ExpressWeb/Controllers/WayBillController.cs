using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;
using System.Data.SqlClient;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 运单管理控制器类
    /// </summary>
    public class WayBillController : BaseController
    {
        protected static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + Authentication.WebAccount?.EmployeeAccount + "/";
        protected static readonly string QueryAllWayBill = ConfigurationManager.AppSettings["QueryAllWayBill"] ?? "";
        int minTaxValue = 45, maxTaxValue = 50;
        int globalMsgRowSize = 1;

        //必填项
        readonly string[] requireArray = new string[] { "结算重量", "转单渠道", "收件人信息|收件人", "收件人信息|收件地址",
                "收件人信息|收件城市", "货物明细信息|物品名称①", "货物明细信息|单件件数①"};

        //价格、重量验证, 只能输入数字, 并且最多包含两位小数
        readonly string[] decimalArray = new string[] { "结算重量", "货物明细信息|单价①", "货物明细信息|单件重量①", "货物明细信息|单价②",
                "货物明细信息|单件重量②", "货物明细信息|单价③", "货物明细信息|单件重量③", "申报价值", "保价", "运费", "客户报价"};

        //件数、是否代缴关税验证, 只能输入正整数
        readonly string[] integerArray = new string[] { "货物明细信息|单件件数①", "货物明细信息|单件件数②", "货物明细信息|单件件数③", "是否代缴关税" };

        //物品名称验证, 是否为违禁物品
        readonly string[] prohibitedArray = new string[] { "货物明细信息|物品名称①", "货物明细信息|物品名称②", "货物明细信息|物品名称③" };

        DalWayBill dalWayBll = new DalWayBill();

        // GET: WayBill
        public ActionResult Index()
        {
            //登录帐号
            var loginAccount = Authentication.WebAccount.EmployeeAccount;
            //可以查看所有
            if (!string.IsNullOrWhiteSpace(QueryAllWayBill) && QueryAllWayBill.Split(',').Contains(loginAccount))
            {
                loginAccount = "";
            }

            //导入
            var dt_import = dalWayBll.GetImportBatch(loginAccount);
            //导出
            var dt_export = dalWayBll.GetExportBatch(loginAccount);

            ViewBag.dtImportBatch = dt_import;
            ViewBag.dtExportBatch = dt_export;

            return View();
        }

        /// <summary>
        /// 获取运单数据
        /// </summary>
        public ContentResult GetWayBillData(FormCollection fc)
        {
            try
            {
                //登录帐号
                var loginAccount = Authentication.WebAccount.EmployeeAccount;
                //可以查看所有
                if (!string.IsNullOrWhiteSpace(QueryAllWayBill) && QueryAllWayBill.Split(',').Contains(loginAccount))
                {
                    loginAccount = "";
                }

                //设置排序参数
                string sortColumn = fc["sort"] ?? "warehousingno";
                string sortType = fc["order"] ?? "asc";
                //设置分页参数
                var pageSize = int.Parse(fc["rows"] ?? "10");
                var pageIndex = int.Parse(fc["page"] ?? "1");

                string importBatch = fc["importBatch"].Trim();
                string exportBatch = fc["exportBatch"].Trim();
                string searchText = fc["searchText"];

                int total = 0;
                var dt = dalWayBll.GetWayBillData(loginAccount, importBatch, exportBatch, searchText, pageSize, pageIndex, sortColumn, sortType, ref total);

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
        /// 创建运单信息
        /// </summary>
        public JsonResult Create(FormCollection fc)
        {
            var json = new JsonData();
            try
            {
                ModWayBill model = new ModWayBill();
                model.WaybillNumber = fc["waybillnumber"];

                //判断添加的运单编号是否已存在
                string strSql = strSql = string.Format("select count(oid) from waybill where waybillnumber = '{0}'", model.WaybillNumber);
                int result = Convert.ToInt32(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null));
                if (result > 0)
                {
                    json.Status = false;
                    json.Msg = "运单编号已存在，请重新输入！";
                    return Json(json);
                }

                model.WarehousingNo = fc["warehousingno"];
                model.SettlementWeight = decimal.Parse(fc["settlementweight"]);
                model.SingleChannel = fc["singlechannel"];
                model.Recipient = fc["recipient"];
                model.RecPhone = fc["recphone"];
                model.RecAddress = fc["recaddress"];
                model.RecCity = fc["reccity"];
                model.RecProvince = fc["recprovince"];
                model.RecPostcode = fc["recpostcode"];
                model.GoodsName1 = fc["goodsname1"];
                model.CustomsNo1 = fc["customsno1"];
                model.Price1 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price1"]) ? "0" : fc["price1"]);
                model.PieceNum1 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum1"]) ? "0" : fc["piecenum1"]);
                model.PieceWeight1 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight1"]) ? "0" : fc["pieceweight1"]);
                model.GoodsName2 = fc["goodsname2"];
                model.CustomsNo2 = fc["customsno2"];
                model.Price2 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price2"]) ? "0" : fc["price2"]);
                model.PieceNum2 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum2"]) ? "0" : fc["piecenum2"]);
                model.PieceWeight2 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight2"]) ? "0" : fc["pieceweight2"]);
                model.GoodsName3 = fc["goodsname3"];
                model.CustomsNo3 = fc["customsno3"];
                model.Price3 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price3"]) ? "0" : fc["price3"]);
                model.PieceNum3 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum3"]) ? "0" : fc["piecenum3"]);
                model.PieceWeight3 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight3"]) ? "0" : fc["pieceweight3"]);
                model.DeclaredValue = decimal.Parse(string.IsNullOrWhiteSpace(fc["declaredvalue"]) ? "0" : fc["declaredvalue"]);
                model.DeclaredCurrency = fc["declaredcurrency"];
                model.IsPayDuty = int.Parse(string.IsNullOrWhiteSpace(fc["ispayduty"]) ? "0" : fc["ispayduty"]);
                model.Insured = decimal.Parse(string.IsNullOrWhiteSpace(fc["insured"]) ? "0" : fc["insured"]);
                model.TypingType = fc["typingtype"];
                model.Destination = fc["destination"];
                model.DestinationPoint = fc["destinationpoint"];
                model.Sender = fc["sender"];
                model.SendPhone = fc["sendphone"];
                model.SendAddress = fc["sendaddress"];
                model.Freight = decimal.Parse(string.IsNullOrWhiteSpace(fc["freight"]) ? "0" : fc["freight"]);
                model.CustomerQuotation = decimal.Parse(string.IsNullOrWhiteSpace(fc["customerquotation"]) ? "0" : fc["customerquotation"]);
                model.Tax = decimal.Parse(string.IsNullOrWhiteSpace(fc["tax"]) ? "0" : fc["tax"]);
                //model.PhoneCount = int.Parse(fc["phonecount"]);
                model.ImportBatch = "";
                model.ExportBatch = "";
                model.Created = Authentication.WebAccount.EmployeeAccount;

                var num = dalWayBll.Create(model);
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
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "系统异常" + ex.Message;
            }
            return Json(json);
        }

        /// <summary>
        /// 修改运单信息
        /// </summary>
        public JsonResult Update(FormCollection fc)
        {
            var json = new JsonData();
            try
            {
                var oid = fc["oid"];
                ModWayBill model = new ModWayBill();
                model.Oid = int.Parse(oid);
                model.WaybillNumber = fc["waybillnumber"];

                //判断修改的运单编号是否已存在
                string strSql = strSql = string.Format("select count(oid) from waybill where waybillnumber = '{0}' and oid <> {1}",
                                          model.WaybillNumber, model.Oid);
                int result = Convert.ToInt32(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null));
                if (result > 0)
                {
                    json.Status = false;
                    json.Msg = "运单编号已存在，请重新输入！";
                    return Json(json);
                }

                model.WarehousingNo = fc["warehousingno"];
                model.SettlementWeight = decimal.Parse(fc["settlementweight"]);
                model.SingleChannel = fc["singlechannel"];
                model.Recipient = fc["recipient"];
                model.RecPhone = fc["recphone"];
                model.RecAddress = fc["recaddress"];
                model.RecCity = fc["reccity"];
                model.RecProvince = fc["recprovince"];
                model.RecPostcode = fc["recpostcode"];
                model.GoodsName1 = fc["goodsname1"];
                model.CustomsNo1 = fc["customsno1"];
                model.Price1 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price1"]) ? "0" : fc["price1"]);
                model.PieceNum1 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum1"]) ? "0" : fc["piecenum1"]);
                model.PieceWeight1 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight1"]) ? "0" : fc["pieceweight1"]);
                model.GoodsName2 = fc["goodsname2"];
                model.CustomsNo2 = fc["customsno2"];
                model.Price2 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price2"]) ? "0" : fc["price2"]);
                model.PieceNum2 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum2"]) ? "0" : fc["piecenum2"]);
                model.PieceWeight2 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight2"]) ? "0" : fc["pieceweight2"]);
                model.GoodsName3 = fc["goodsname3"];
                model.CustomsNo3 = fc["customsno3"];
                model.Price3 = decimal.Parse(string.IsNullOrWhiteSpace(fc["price3"]) ? "0" : fc["price3"]);
                model.PieceNum3 = int.Parse(string.IsNullOrWhiteSpace(fc["piecenum3"]) ? "0" : fc["piecenum3"]);
                model.PieceWeight3 = decimal.Parse(string.IsNullOrWhiteSpace(fc["pieceweight3"]) ? "0" : fc["pieceweight3"]);
                model.DeclaredValue = decimal.Parse(string.IsNullOrWhiteSpace(fc["declaredvalue"]) ? "0" : fc["declaredvalue"]);
                model.DeclaredCurrency = fc["declaredcurrency"];
                model.IsPayDuty = int.Parse(string.IsNullOrWhiteSpace(fc["ispayduty"]) ? "0" : fc["ispayduty"]);
                model.Insured = decimal.Parse(string.IsNullOrWhiteSpace(fc["insured"]) ? "0" : fc["insured"]);
                model.TypingType = fc["typingtype"];
                model.Destination = fc["destination"];
                model.DestinationPoint = fc["destinationpoint"];
                model.Sender = fc["sender"];
                model.SendPhone = fc["sendphone"];
                model.SendAddress = fc["sendaddress"];
                model.Freight = decimal.Parse(string.IsNullOrWhiteSpace(fc["freight"]) ? "0" : fc["freight"]);
                model.CustomerQuotation = decimal.Parse(string.IsNullOrWhiteSpace(fc["customerquotation"]) ? "0" : fc["customerquotation"]);
                model.Tax = decimal.Parse(string.IsNullOrWhiteSpace(fc["tax"]) ? "0" : fc["tax"]);
                //model.PhoneCount = int.Parse(fc["phonecount"]);
                model.ImportBatch = "";
                model.ExportBatch = "";
                var num = dalWayBll.Update(model);
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
            catch (Exception ex)
            {
                json.Status = false;
                json.Msg = "系统异常" + ex.Message;
            }
            return Json(json);
        }

        /// <summary>
        /// 删除运单信息
        /// </summary>
        public JsonResult Delete(FormCollection fc)
        {
            var json = new JsonData() ;

            try
            {
                var ids = fc["ids"].Trim();

                var num = dalWayBll.Delete(ids);
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

            return Json(json);
        }

        /// <summary>
        /// 导入
        /// </summary>
        [HttpPost]
        public JsonResult Import(HttpPostedFileBase wb_file)
        {
            var json = new JsonData();
            try
            {
                //登录帐号
                var loginAccount = Authentication.WebAccount.EmployeeAccount;

                #region 设置默认区间

                string defaultTaxInterval = ConfigurationManager.AppSettings["DefaultTaxInterval"] ?? "45,50";

                if (defaultTaxInterval.Split(',').Length != 2 || Convert.ToInt32(defaultTaxInterval.Split(',')[0]) > Convert.ToInt32(defaultTaxInterval.Split(',')[1]))
                {
                    return Json(new JsonData() { Status = false, Msg = "价格区间配置错误，参考格式'45,50'" });
                }

                minTaxValue = Convert.ToInt32(defaultTaxInterval.Split(',')[0]);
                maxTaxValue = Convert.ToInt32(defaultTaxInterval.Split(',')[1]);

                #endregion

                #region 上传文件

                if ((wb_file.ContentLength / 1024 / 1024) > 10)
                {
                    json.Status = false;
                    json.Msg = "文件大小不能超过" + (10).ToString() + "M！";
                    return Json(json);
                }

                var fileName = wb_file.FileName;//文件名
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
                wb_file.SaveAs(Path.Combine(filePath, fileName));

                #endregion

                #region 读取excel的内容，验证格式是否符合规范

                //将excel文件的数据加载到datatable中
                DataTable dt_import = ExcelHelper.ExportToDataTable(Path.Combine(filePath, fileName));

                //DataTable数据完整性验证
                string fieldMsg = "";
                if (!DataTableFieldValid(dt_import, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断是否存在入仓号或则运单编号为空记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("入仓号")) ||
                                             string.IsNullOrWhiteSpace(p.Field<string>("运单编号"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在入仓号或运单编号为空的记录，导入失败！" });
                }

                //判断是否存在运单编号重复的情况
                if (!IsRepeatByColumnName(dt_import, "运单编号"))
                {
                    return Json(new JsonData() { Status = false, Msg = "导入excel文件中存在运单编号重复的记录，导入失败！" });
                }

                //加载基础数据
                DataTable dt_area = this.GetAreaCityData();
                DataTable dt_relation = this.GetGoodRelationData();
                DataTable dt_prohibited = this.GetProhibitedGoodData();
                DataTable dt_taxnumber = this.GetGoodTaxNumberData();

                //数据有效性验证
                StringBuilder validMsg = new StringBuilder();
                string validStr = string.Empty;
                bool validFlag = true;
                //循环验证
                foreach (DataRow row in dt_import.Rows)
                {
                    //导入行数据验证
                    validStr = EmportRowValidData(row, dt_prohibited);
                    if (!string.IsNullOrEmpty(validStr))
                    {
                        validMsg.Append(validStr);
                        validMsg.Append("<br/>");
                        validMsg.Append("<br/>");

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
                //导入批次
                string importBatchName = "import_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                //生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
                string emptyTaxMsg = GenerateWaybillData(dt_import, dt_taxnumber, dt_relation, dt_area);

                //将dataTable转换为list集合
                List<ModWayBill> listWaybill = new List<ModWayBill>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listWaybill.Add(new ModWayBill()
                    {
                        WarehousingNo = row["入仓号"].ToString().Trim(),
                        WaybillNumber = row["运单编号"].ToString().Trim(),
                        SettlementWeight = string.IsNullOrWhiteSpace(row["结算重量"].ToString()) ? 0 : Convert.ToDecimal(row["结算重量"]),
                        SingleChannel = row["转单渠道"].ToString().Trim(),
                        Recipient = row["收件人信息|收件人"].ToString().Trim(),
                        RecPhone = row["收件人信息|收件人电话"].ToString().Trim(),
                        RecAddress = row["收件人信息|收件地址"].ToString().Trim(),
                        RecCity = row["收件人信息|收件城市"].ToString().Trim(),
                        RecProvince = row["收件人信息|收件省份"].ToString().Trim(),
                        RecPostcode = row["收件人信息|收件地邮编"].ToString().Trim(),
                        GoodsName1 = row["货物明细信息|物品名称①"].ToString().Trim(),
                        CustomsNo1 = row["货物明细信息|税关号①"].ToString().Trim(),
                        Price1 = string.IsNullOrWhiteSpace(row["货物明细信息|单价①"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单价①"]),
                        PieceNum1 = string.IsNullOrWhiteSpace(row["货物明细信息|单件件数①"].ToString()) ? 0 : Convert.ToInt32(row["货物明细信息|单件件数①"]),
                        PieceWeight1 = string.IsNullOrWhiteSpace(row["货物明细信息|单件重量①"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单件重量①"]),
                        GoodsName2 = row["货物明细信息|物品名称②"].ToString().Trim(),
                        CustomsNo2 = row["货物明细信息|税关号②"].ToString().Trim(),
                        Price2 = string.IsNullOrWhiteSpace(row["货物明细信息|单价②"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单价②"]),
                        PieceNum2 = string.IsNullOrWhiteSpace(row["货物明细信息|单件件数②"].ToString()) ? 0 : Convert.ToInt32(row["货物明细信息|单件件数②"]),
                        PieceWeight2 = string.IsNullOrWhiteSpace(row["货物明细信息|单件重量②"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单件重量②"]),
                        GoodsName3 = row["货物明细信息|物品名称③"].ToString().Trim(),
                        CustomsNo3 = row["货物明细信息|税关号③"].ToString().Trim(),
                        Price3 = string.IsNullOrWhiteSpace(row["货物明细信息|单价③"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单价③"]),
                        PieceNum3 = string.IsNullOrWhiteSpace(row["货物明细信息|单件件数③"].ToString()) ? 0 : Convert.ToInt32(row["货物明细信息|单件件数③"]),
                        PieceWeight3 = string.IsNullOrWhiteSpace(row["货物明细信息|单件重量③"].ToString()) ? 0 : Convert.ToDecimal(row["货物明细信息|单件重量③"]),
                        DeclaredValue = string.IsNullOrWhiteSpace(row["申报价值"].ToString()) ? 0 : Convert.ToDecimal(row["申报价值"]),
                        DeclaredCurrency = row["申报货币"].ToString().Trim(),
                        IsPayDuty = string.IsNullOrWhiteSpace(row["是否代缴关税"].ToString()) ? 0 : Convert.ToInt32(row["是否代缴关税"]),
                        Insured = string.IsNullOrWhiteSpace(row["保价"].ToString()) ? 0 : Convert.ToDecimal(row["保价"]),
                        TypingType = row["打单类型"].ToString().Trim(),
                        Destination = row["目的地"].ToString().Trim(),
                        DestinationPoint = row["目的网点"].ToString().Trim(),
                        Sender = row["寄件人信息|寄件人"].ToString().Trim(),
                        SendPhone = row["寄件人信息|寄件电话"].ToString().Trim(),
                        SendAddress = row["寄件人信息|寄件地址"].ToString().Trim(),
                        Freight = string.IsNullOrWhiteSpace(row["运费"].ToString()) ? 0 : Convert.ToDecimal(row["运费"]),
                        CustomerQuotation = string.IsNullOrWhiteSpace(row["客户报价"].ToString()) ? 0 : Convert.ToDecimal(row["客户报价"]),
                        Tax = 0m,
                        PhoneCount = 1,
                        ImportBatch = importBatchName,
                        ExportBatch = "",
                        Created = loginAccount
                    });
                }

                //根据批次号计算当前导入运单的phonecount值
                CalculateWaybillPhoneCount(listWaybill);

                //插入到数据库 -- 启用SQLite事务
                using (SqlConnection conn = new SqlConnection(SQLHelper.defConnStr))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        //循环插入
                        foreach (ModWayBill model in listWaybill)
                        {
                            //插入数据库
                            EmportRowInsert(model, transaction);
                        }

                        //提交事务
                        transaction.Commit();
                    }
                    catch (SqlException sqliteEx)
                    {
                        //事务回滚
                        transaction.Rollback();
                        throw new Exception(sqliteEx.Message);
                    }
                }

                json.Status = true;

                //税关号未添加提示
                if (!string.IsNullOrEmpty(emptyTaxMsg))
                {
                    json.Msg = $"{emptyTaxMsg}<br/>成功导入" + listWaybill.Count.ToString() + "条数据！";
                }
                else
                {
                    json.Msg = "成功导入" + listWaybill.Count.ToString() + "条数据！";
                }

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

        #region 生成

        /// <summary>
        /// 生成
        /// </summary>
        /// <returns></returns>
        public JsonResult Generate(FormCollection fc)
        {
            var json = new JsonData() { Status = false, Msg = "" };

            try
            {
                string importBatch = fc["importBatch"].Trim();
                string exportBatch = fc["exportBatch"].Trim(); 
                string searchText = fc["searchText"].Trim();

                //登录帐号
                var loginAccount = Authentication.WebAccount.EmployeeAccount;
                //可以查看所有
                if (!string.IsNullOrWhiteSpace(QueryAllWayBill) && QueryAllWayBill.Split(',').Contains(loginAccount))
                {
                    loginAccount = "";
                }

                #region 查询条件处理

                string searchWhere = "";

                //帐号过滤
                if (!string.IsNullOrWhiteSpace(loginAccount))
                {
                    searchWhere += $" and created = '{loginAccount}'";
                }

                //导入批次条件处理
                if (!string.IsNullOrWhiteSpace(importBatch) && importBatch != "全部")
                {
                    if (importBatch == "人工录入")
                    {
                        searchWhere += " and importbatch = ''";
                    }
                    else
                    {
                        searchWhere += $" and importbatch = '{importBatch}'";
                    }
                }

                //导出批次条件处理
                if (!string.IsNullOrWhiteSpace(exportBatch) && exportBatch != "全部")
                {
                    if (exportBatch == "未导出")
                    {
                        searchWhere += " and exportbatch = ''";
                    }
                    else
                    {
                        searchWhere += $" and exportbatch = '{exportBatch}'";
                    }
                }

                //入仓号、运单编号条件处理
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchWhere += $"and (warehousingno like '%{searchText}%' or waybillnumber like '%{searchText}%')";
                }

                #endregion

                //加载基础数据
                DataTable dt_area = this.GetAreaCityData();
                DataTable dt_relation = this.GetGoodRelationData();
                DataTable dt_prohibited = this.GetProhibitedGoodData();
                DataTable dt_taxnumber = this.GetGoodTaxNumberData();
                //获取需要生成的数据
                DataTable dt_generate = this.GetGenerateData(searchWhere);

                //生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
                string emptyTaxMsg = GenerateWaybillData2(dt_generate, dt_taxnumber, dt_relation, dt_area);

                //待生成总记录数
                int total = dt_generate.Rows.Count;

                //将DataTable数据转换为List数组
                List<ModWayBill> listWaybill = ComHelper.DataTableToList<ModWayBill>(dt_generate);

                StringBuilder validMsg = new StringBuilder();
                string validStr = string.Empty;
                bool validFlag = true;
                int count = 0;

                //循环计算
                var updated = Authentication.WebAccount.EmployeeAccount;

                foreach (ModWayBill model in listWaybill)
                {
                    count++;

                    //计算运单物品单价、税金、申报价值、物品数量
                    validStr = CalculateWaybillTaxes(model, dt_taxnumber);
                    if (!string.IsNullOrEmpty(validStr))
                    {
                        validMsg.Append(validStr);
                        validMsg.Append("<br/>");
                        validMsg.Append("<br/>");

                        //计算失败
                        validFlag = false;
                        continue;
                    }

                    ////更新运单物品单价、税金、申报价值信息
                    //UpdateRowTaxPrice(model);

                    model.Updated = updated;
                    UpdateRowTaxPrice2(model);
                }

                //提示计算失败运单信息
                if (!validFlag)
                {
                    json.Msg = "生成失败！Error：" + validMsg.ToString();
                }
                else
                {
                    json.Status = true;
                    json.Msg = "生成成功！";
                }
            }
            catch (Exception ex)
            {
                json.Msg = "生成失败！Error：" + ex.Message;
            }

            return Json(json);
        }

        /// <summary>
        /// 获取生成数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private DataTable GetGenerateData(string searchtext)
        {
            //查询SQL
            string strSql = string.Format(@"select oid,warehousingno,waybillnumber,settlementweight,singlechannel,recipient,recphone,recaddress,reccity,
                                recprovince,recpostcode,goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,
                                goodsname3,customsno3,price3,piecenum3,pieceweight3,declaredvalue,declaredcurrency,ispayduty,insured,typingtype,destination,destinationpoint,
                                sender,sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch from waybill 
                                where 1=1 {0} order by oid", searchtext);

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        #endregion

        #region 导出

        public FileStreamResult ExportExcel(string importBatch, string exportBatch, string searchText, string sort, string order)
        {
            try
            {
                //设置排序参数
                string sortColumn = sort ?? "warehousingno";
                string sortType = order ?? "asc";

                //登录帐号
                var loginAccount = Authentication.WebAccount.EmployeeAccount;
                //可以查看所有
                if (!string.IsNullOrWhiteSpace(QueryAllWayBill) && QueryAllWayBill.Split(',').Contains(loginAccount))
                {
                    loginAccount = "";
                }

                #region 查询条件处理

                string searchWhere = "";

                //帐号过滤
                if (!string.IsNullOrWhiteSpace(loginAccount))
                {
                    searchWhere += $" and created = '{loginAccount}'";
                }

                //导入批次条件处理
                if (!string.IsNullOrWhiteSpace(importBatch) && importBatch != "全部")
                {
                    if (importBatch == "人工录入")
                    {
                        searchWhere += " and importbatch = ''";
                    }
                    else
                    {
                        searchWhere += $" and importbatch = '{importBatch}'";
                    }
                }

                //导出批次条件处理
                if (!string.IsNullOrWhiteSpace(exportBatch) && exportBatch != "全部")
                {
                    if (exportBatch == "未导出")
                    {
                        searchWhere += " and exportbatch = ''";
                    }
                    else
                    {
                        searchWhere += $" and exportbatch = '{exportBatch}'";
                    }
                }

                //入仓号、运单编号条件处理
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchWhere += $"and (warehousingno like '%{searchText}%' or waybillnumber like '%{searchText}%')";
                }

                #endregion

                //获取导出数据
                DataTable dt_export = GetExportData(searchWhere, sortColumn, sortType);

                //记录导出数据记录的主键ID
                string oidStr = string.Empty;
                foreach (DataRow row in dt_export.Rows)
                {
                    oidStr += row["oid"].ToString() + ",";
                }
                oidStr = oidStr.TrimEnd(',');

                //删除oid, tax, exportbatch列, 不需要导出
                dt_export.Columns.Remove("oid");
                dt_export.Columns.Remove("tax");
                dt_export.Columns.Remove("importbatch");
                dt_export.Columns.Remove("exportbatch");

                //需要清空零值的字段集合
                List<string> zeroColumns = new List<string>() { "price1", "piecenum1", "pieceweight1", "price2", "piecenum2", "pieceweight2",
                    "price3", "piecenum3", "pieceweight3", "freight", "customerquotation" };

                //处理double类型值的显示格式
                dt_export = ComHelper.ConvertDataTableToString(dt_export, zeroColumns);

                //重命名列名
                DataTableColumnRename(dt_export);

                #region 配置导出参数

                ////需要清空零值的字段集合
                //List<string> zeroColumns = new List<string>() { "货物明细信息|单价①" , "货物明细信息|单件件数①" ,
                //    "货物明细信息|单件重量①", "货物明细信息|单价②", "货物明细信息|单件件数②", "货物明细信息|单件重量②",
                //    "货物明细信息|单价③", "货物明细信息|单件件数③", "货物明细信息|单件重量③", "运费", "客户报价" };

                //需要设置单元格为数值列的字段集合
                List<string> numberColumns = new List<string>() { "结算重量", "货物明细信息|单价①" , "货物明细信息|单件件数①" ,
                        "货物明细信息|单件重量①", "货物明细信息|单价②", "货物明细信息|单件件数②", "货物明细信息|单件重量②",
                        "货物明细信息|单价③", "货物明细信息|单件件数③", "货物明细信息|单件重量③", "申报价值", "保价", "运费", "客户报价" };

                //需要设置红色背景的字段集合
                List<string> redStyleColumns = new List<string>() { "收件人信息|收件省份", "收件人信息|收件地邮编", "货物明细信息|税关号①",
                        "货物明细信息|单件重量①", "货物明细信息|税关号②", "货物明细信息|单件重量②", "货物明细信息|税关号③",
                        "货物明细信息|单件重量③", "寄件人信息|寄件人", "寄件人信息|寄件电话", "寄件人信息|寄件地址", "运费", "客户报价" };

                //需要设置标题行文本居中的字段集合
                List<string> centerStyleColumns = new List<string>() { "入仓号", "运单编号", "结算重量", "转单渠道", "收件人信息|收件地址",
                        "申报价值", "申报货币", "保价", "打单类型", "目的地", "目的网点", "运费", "客户报价", "time" };

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "收件人信息|收件人", "收件人信息|收件地址", "货物明细信息|物品名称①",
                        "货物明细信息|物品名称②", "货物明细信息|物品名称③", "寄件人信息|寄件地址" };

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("入仓号", 11.85);
                dictWidthColumns.Add("运单编号", 15.35);
                dictWidthColumns.Add("结算重量", 7.65);
                dictWidthColumns.Add("转单渠道", 7.65);
                dictWidthColumns.Add("收件人信息|收件人", 9.00);
                dictWidthColumns.Add("收件人信息|收件人电话", 13.10);
                dictWidthColumns.Add("收件人信息|收件地址", 31.75);
                dictWidthColumns.Add("收件人信息|收件城市", 8.15);
                dictWidthColumns.Add("收件人信息|收件省份", 8.15);
                dictWidthColumns.Add("收件人信息|收件地邮编", 10.10);
                dictWidthColumns.Add("货物明细信息|物品名称①", 10.00);
                dictWidthColumns.Add("货物明细信息|税关号①", 8.10);
                dictWidthColumns.Add("货物明细信息|单价①", 6.00);
                dictWidthColumns.Add("货物明细信息|单件件数①", 3.90);
                dictWidthColumns.Add("货物明细信息|单件重量①", 1.88);
                dictWidthColumns.Add("货物明细信息|物品名称②", 10.00);
                dictWidthColumns.Add("货物明细信息|税关号②", 8.10);
                dictWidthColumns.Add("货物明细信息|单价②", 6.00);
                dictWidthColumns.Add("货物明细信息|单件件数②", 3.90);
                dictWidthColumns.Add("货物明细信息|单件重量②", 1.88);
                dictWidthColumns.Add("货物明细信息|物品名称③", 10.00);
                dictWidthColumns.Add("货物明细信息|税关号③", 8.10);
                dictWidthColumns.Add("货物明细信息|单价③", 6.00);
                dictWidthColumns.Add("货物明细信息|单件件数③", 3.90);
                dictWidthColumns.Add("货物明细信息|单件重量③", 1.88);
                dictWidthColumns.Add("申报价值", 7.75);
                dictWidthColumns.Add("申报货币", 7.75);
                dictWidthColumns.Add("是否代缴关税", 3.75);
                dictWidthColumns.Add("保价", 6.25);
                dictWidthColumns.Add("打单类型", 7.85);
                dictWidthColumns.Add("目的地", 12.50);
                dictWidthColumns.Add("目的网点", 12.50);
                dictWidthColumns.Add("寄件人信息|寄件人", 6.10);
                dictWidthColumns.Add("寄件人信息|寄件电话", 7.85);
                dictWidthColumns.Add("寄件人信息|寄件地址", 7.85);
                dictWidthColumns.Add("运费", 4.75);
                dictWidthColumns.Add("客户报价", 7.95);
                dictWidthColumns.Add("time", 4.50);

                #endregion

                //获取保存文件名称                     
                string fileName = "运单数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

                //导出到excel
                string saveStatus = ExcelHelper.SaveDataTableToExcel(dt_export, numberColumns, redStyleColumns, centerStyleColumns, contentLeftStyleColumns, dictWidthColumns, filePath, fileName, EnumExcelFileType.Excel2003);

                //更新运单表的数据的导出批次
                string exportBatchName = "export_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string strSql = string.Format(@"update waybill set exportbatch = '{0}' where oid in ({1})", exportBatchName, oidStr);
                SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, strSql, null);

                var fileStream = new MemoryStream();

                return File(new FileStream(saveStatus, FileMode.Open), "application/ms-excel", Server.HtmlEncode(fileName));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取导出数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private DataTable GetExportData(string searchtext, string sortColumn, string sortType)
        {
            //查询SQL
            string strSql = string.Format(@"select oid,warehousingno,waybillnumber,settlementweight,singlechannel,recipient,recphone,recaddress,reccity,
                        recprovince,recpostcode,goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,
                        goodsname3,customsno3,price3,piecenum3,pieceweight3,declaredvalue,declaredcurrency,ispayduty,insured,typingtype,destination,destinationpoint,
                        sender,sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch from waybill 
                        where 1=1 {0} order by {1} {2}", searchtext, sortColumn, sortType);
            
            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        #endregion

        #region 导出税关号为空的物品数据

        public FileStreamResult ExportFailExcel()
        {
            try
            {
                //登录帐号
                var loginAccount = Authentication.WebAccount.EmployeeAccount;
                //可以查看所有
                if (!string.IsNullOrWhiteSpace(QueryAllWayBill) && QueryAllWayBill.Split(',').Contains(loginAccount))
                {
                    loginAccount = "";
                }

                #region 查询条件处理

                string searchWhere = "";

                //帐号过滤
                if (!string.IsNullOrWhiteSpace(loginAccount))
                {
                    searchWhere += $" and created = '{loginAccount}'";
                }

                #endregion

                //获取导出数据
                DataTable dt_export = GetExportFailData(searchWhere);

                //重命名列名
                dt_export.Columns["goodsname"].ColumnName = "原物品名称";
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

                #region 配置导出参数

                //需要设置内容靠左显示的字段集合
                List<string> contentLeftStyleColumns = new List<string>() { "原物品名称", "商品税号", "新物品名称1", "新物品名称2", "新物品名称3", "新物品名称4", "新物品名称5",
                        "新物品名称6", "新物品名称7", "新物品名称8", "新物品名称9", "新物品名称10", "新物品名称11", "新物品名称12", "新物品名称13",
                        "新物品名称14", "新物品名称15", "新物品名称16", "新物品名称17", "新物品名称18", "新物品名称19", "新物品名称20"};

                //列宽字典
                Dictionary<string, double> dictWidthColumns = new Dictionary<string, double>();
                dictWidthColumns.Add("原物品名称", 15.85);
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

                List<string> empty = new List<string>();

                #endregion

                //获取保存文件名称                     
                string fileName = "无税关号的物品数据_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";

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
        /// 获取税关号为空的物品数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private DataTable GetExportFailData(string searchWhere)
        {
            //查询SQL
            string strSql = $@"select goodsname, '' taxnumber, '' newname1, '' newname2, '' newname3, '' newname4, '' newname5, '' newname6, '' newname7,
                    '' newname8, '' newname9, '' newname10, '' newname11, '' newname12, '' newname13, '' newname14, '' newname15, '' newname16, '' newname17,
                    '' newname18, '' newname19, '' newname20 from (
                    select goodsname1 as goodsname from waybill where goodsname1 <> '' and customsno1 = '' {searchWhere}
                    union all 
                    select goodsname2 as goodsname from waybill where goodsname2 <> '' and customsno2 = '' {searchWhere}
                    union all 
                    select goodsname3 as goodsname from waybill where goodsname3 <> '' and customsno3 = '' {searchWhere}) as a 
                group by goodsname 
                order by goodsname asc";

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        #endregion

        #region Excel导入数据处理

        /// <summary>
        /// 导入datatable字段验证
        /// </summary>
        /// <param name="dt_import"></param>
        private bool DataTableFieldValid(DataTable dt_import, out string fieldMsg)
        {
            //excel文件中应包含的字段集合
            List<string> fieldList = new List<string>() {
               "入仓号", "运单编号", "结算重量", "转单渠道", "收件人信息|收件人", "收件人信息|收件人电话", "收件人信息|收件地址",
               "收件人信息|收件城市", "收件人信息|收件省份", "收件人信息|收件地邮编", "货物明细信息|物品名称①", "货物明细信息|税关号①",
               "货物明细信息|单价①", "货物明细信息|单件件数①", "货物明细信息|单件重量①", "货物明细信息|物品名称②", "货物明细信息|税关号②",
               "货物明细信息|单价②", "货物明细信息|单件件数②", "货物明细信息|单件重量②", "货物明细信息|物品名称③", "货物明细信息|税关号③",
               "货物明细信息|单价③", "货物明细信息|单件件数③", "货物明细信息|单件重量③", "申报价值", "申报货币", "是否代缴关税", "保价", "打单类型",
               "目的地", "目的网点", "寄件人信息|寄件人", "寄件人信息|寄件电话", "寄件人信息|寄件地址", "运费", "客户报价"
            };

            //判断字段是否都存在与导入的datatable中
            bool result = true;
            fieldMsg = "";
            string fieldStr = string.Empty;
            int i = 0;
            foreach (string field in fieldList)
            {
                if (!dt_import.Columns.Contains(field))
                {
                    //字段不存在
                    result = false;
                    if (i != 0 && i % 3 == 0)
                    {
                        fieldStr += "<br/>";
                    }
                    fieldStr += field + "，";
                    i++;
                }
            }

            if (!result)
            {
                fieldMsg = "导入的Excel文件不完整，以下列不存在：<br/>" + fieldStr.TrimEnd(',') + "<br/><br/>导入失败！";
            }

            return result;
        }

        /// <summary>
        /// 重命名datacolumn列名
        /// </summary>
        /// <param name="dt_export"></param>
        private void DataTableColumnRename(DataTable dt_export)
        {
            //dt_export.Columns["oid"].ColumnName = "ID";
            dt_export.Columns["warehousingno"].ColumnName = "入仓号";
            dt_export.Columns["waybillnumber"].ColumnName = "运单编号";
            dt_export.Columns["settlementweight"].ColumnName = "结算重量";
            dt_export.Columns["singlechannel"].ColumnName = "转单渠道";
            dt_export.Columns["recipient"].ColumnName = "收件人信息|收件人";
            dt_export.Columns["recphone"].ColumnName = "收件人信息|收件人电话";
            dt_export.Columns["recaddress"].ColumnName = "收件人信息|收件地址";
            dt_export.Columns["reccity"].ColumnName = "收件人信息|收件城市";
            dt_export.Columns["recprovince"].ColumnName = "收件人信息|收件省份";
            dt_export.Columns["recpostcode"].ColumnName = "收件人信息|收件地邮编";
            dt_export.Columns["goodsname1"].ColumnName = "货物明细信息|物品名称①";
            dt_export.Columns["customsno1"].ColumnName = "货物明细信息|税关号①";
            dt_export.Columns["price1"].ColumnName = "货物明细信息|单价①";
            dt_export.Columns["piecenum1"].ColumnName = "货物明细信息|单件件数①";
            dt_export.Columns["pieceweight1"].ColumnName = "货物明细信息|单件重量①";
            dt_export.Columns["goodsname2"].ColumnName = "货物明细信息|物品名称②";
            dt_export.Columns["customsno2"].ColumnName = "货物明细信息|税关号②";
            dt_export.Columns["price2"].ColumnName = "货物明细信息|单价②";
            dt_export.Columns["piecenum2"].ColumnName = "货物明细信息|单件件数②";
            dt_export.Columns["pieceweight2"].ColumnName = "货物明细信息|单件重量②";
            dt_export.Columns["goodsname3"].ColumnName = "货物明细信息|物品名称③";
            dt_export.Columns["customsno3"].ColumnName = "货物明细信息|税关号③";
            dt_export.Columns["price3"].ColumnName = "货物明细信息|单价③";
            dt_export.Columns["piecenum3"].ColumnName = "货物明细信息|单件件数③";
            dt_export.Columns["pieceweight3"].ColumnName = "货物明细信息|单件重量③";
            dt_export.Columns["declaredvalue"].ColumnName = "申报价值";
            dt_export.Columns["declaredcurrency"].ColumnName = "申报货币";
            dt_export.Columns["ispayduty"].ColumnName = "是否代缴关税";
            dt_export.Columns["insured"].ColumnName = "保价";
            dt_export.Columns["typingtype"].ColumnName = "打单类型";
            dt_export.Columns["destination"].ColumnName = "目的地";
            dt_export.Columns["destinationpoint"].ColumnName = "目的网点";
            dt_export.Columns["sender"].ColumnName = "寄件人信息|寄件人";
            dt_export.Columns["sendphone"].ColumnName = "寄件人信息|寄件电话";
            dt_export.Columns["sendaddress"].ColumnName = "寄件人信息|寄件地址";
            dt_export.Columns["freight"].ColumnName = "运费";
            dt_export.Columns["customerquotation"].ColumnName = "客户报价";
            //dt_export.Columns["tax"].ColumnName = "税金";
            dt_export.Columns["phonecount"].ColumnName = "time";
            //dt_export.Columns["importbatch"].ColumnName = "导入批次";
            //dt_export.Columns["exportbatch"].ColumnName = "导出批次";
        }

        /// <summary>
        /// 导入数据验证 如必填、数字有效性，是否违禁物品
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="dt_prohibited">违禁物品表</param>
        /// <returns></returns>
        private string EmportRowValidData(DataRow row, DataTable dt_prohibited)
        {
            bool flag = true;
            string valueEmpty = string.Empty,
                   valueDecError = string.Empty,
                   valueIntError = string.Empty,
                   valueProError = string.Empty;
            string validMsg = "运单编号：" + row["运单编号"].ToString().Trim();

            //必填项验证
            foreach (string col in requireArray)
            {
                if (string.IsNullOrWhiteSpace(row[col].ToString()))
                {
                    valueEmpty += col + "，";
                }
            }
            //若 物品名称② 有内容, 则 单件件数② 必填
            if ((!string.IsNullOrWhiteSpace(row["货物明细信息|物品名称②"].ToString())
                && string.IsNullOrWhiteSpace(row["货物明细信息|单件件数②"].ToString()))
                || (!string.IsNullOrWhiteSpace(row["货物明细信息|物品名称②"].ToString())
                && !string.IsNullOrWhiteSpace(row["货物明细信息|单件件数②"].ToString())
                && Regex.IsMatch(row["货物明细信息|单件件数②"].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$")
                && Convert.ToInt32(row["货物明细信息|单件件数②"].ToString().Trim()) == 0))
            {
                valueEmpty += "货物明细信息|单件件数②，";
            }
            //若 物品名称③ 有内容, 则 单件件数③ 必填
            if ((!string.IsNullOrWhiteSpace(row["货物明细信息|物品名称③"].ToString())
                && string.IsNullOrWhiteSpace(row["货物明细信息|单件件数③"].ToString()))
                || (!string.IsNullOrWhiteSpace(row["货物明细信息|物品名称③"].ToString())
                && !string.IsNullOrWhiteSpace(row["货物明细信息|单件件数③"].ToString())
                && Regex.IsMatch(row["货物明细信息|单件件数③"].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$")
                && Convert.ToInt32(row["货物明细信息|单件件数③"].ToString().Trim()) == 0))
            {
                valueEmpty += "货物明细信息|单件件数③，";
            }
            //若 物品名称③ 有内容, 则 物品名称② 必须要有内容
            if (!string.IsNullOrWhiteSpace(row["货物明细信息|物品名称③"].ToString()) && string.IsNullOrWhiteSpace(row["货物明细信息|物品名称②"].ToString()))
            {
                valueEmpty += "货物明细信息|物品名称②，";
            }
            valueEmpty = valueEmpty.TrimEnd('，');

            //价格、重量验证
            foreach (string col in decimalArray)
            {
                //不为空时, 正则匹配输入是否为合法的数字
                if (!string.IsNullOrWhiteSpace(row[col].ToString()) && !Regex.IsMatch(row[col].ToString().Trim(), @"^((0|[1-9]\d{0,20}(\.\d{1,2})?)|(0\.\d{1,2}))$"))
                {
                    valueDecError += col + "，";
                }
            }
            valueDecError = valueDecError.TrimEnd('，');

            //件数验证
            foreach (string col in integerArray)
            {
                //不为空时, 正则匹配输入是否为合法的数字
                if (!string.IsNullOrWhiteSpace(row[col].ToString()) && !Regex.IsMatch(row[col].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$"))
                {
                    valueIntError += col + "，";
                }
            }
            valueIntError = valueIntError.TrimEnd('，');

            //禁寄物品验证
            foreach (string col in prohibitedArray)
            {
                //不为空时, 判断是否属于禁寄物品
                if (!string.IsNullOrWhiteSpace(row[col].ToString()))
                {
                    int rowCount = ((from p in dt_prohibited.AsEnumerable()
                                     where row[col].ToString().Trim().IndexOf(p.Field<string>("pname")) >= 0
                                     select p).ToArray()).Count();

                    if (rowCount > 0)
                    {
                        valueProError += row[col].ToString() + "，";
                    }
                }
            }
            valueProError = valueProError.TrimEnd('，');

            //验证不通过
            if (!string.IsNullOrEmpty(valueEmpty))
            {
                flag = false;
                validMsg += "<br/>&nbsp;&nbsp;字段：" + valueEmpty + "内容为空";
            }
            if (!string.IsNullOrEmpty(valueDecError))
            {
                flag = false;
                validMsg += "<br/>&nbsp;&nbsp;字段：" + valueDecError + "格式错误，价格、重量只能输入数字，并且最多包含两位小数";
            }
            if (!string.IsNullOrEmpty(valueIntError))
            {
                flag = false;
                validMsg += "<br/>&nbsp;&nbsp;字段：" + valueIntError + "格式错误，单件件数、是否代缴关税只能输入整数";
            }
            if (!string.IsNullOrEmpty(valueProError))
            {
                flag = false;
                validMsg += "<br/>&nbsp;&nbsp;物品：" + valueProError + "属于禁寄物品";
            }

            //验证通过, 清空字符串
            if (flag)
            {
                validMsg = "";
            }

            return validMsg;
        }

        /// <summary>
        /// 导入行数据插入到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowInsert(ModWayBill model, SqlTransaction transaction)
        {
            //insert语句
            string sql = @"insert into waybill(warehousingno,waybillnumber,settlementweight,singlechannel,recipient,recphone,recaddress,reccity,recprovince,recpostcode,
                            goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,goodsname3,customsno3,price3,piecenum3,pieceweight3,
                            declaredvalue,declaredcurrency,ispayduty,insured,typingtype,destination,destinationpoint,sender,sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch,created,created_time) 
                        values(@warehousingno,@waybillnumber,@settlementweight,@singlechannel,@recipient,@recphone,@recaddress,@reccity,@recprovince,@recpostcode,
                                @goodsname1,@customsno1,@price1,@piecenum1,@pieceweight1,@goodsname2,@customsno2,@price2,@piecenum2,@pieceweight2,@goodsname3,@customsno3,@price3,@piecenum3,@pieceweight3,
                                @declaredvalue,@declaredcurrency,@ispayduty,@insured,@typingtype,@destination,@destinationpoint,@sender,@sendphone,@sendaddress,@freight,@customerquotation,@tax,@phonecount,@importbatch,@exportbatch,@created,getdate())";

            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@warehousingno", model.WarehousingNo),
                new SqlParameter("@waybillnumber", model.WaybillNumber),
                new SqlParameter("@settlementweight", model.SettlementWeight),
                new SqlParameter("@singlechannel", model.SingleChannel),
                new SqlParameter("@recipient", model.Recipient),
                new SqlParameter("@recphone", model.RecPhone),
                new SqlParameter("@recaddress", model.RecAddress),
                new SqlParameter("@reccity", model.RecCity),
                new SqlParameter("@recprovince", model.RecProvince),
                new SqlParameter("@recpostcode", model.RecPostcode),
                new SqlParameter("@goodsname1", model.GoodsName1),
                new SqlParameter("@customsno1", model.CustomsNo1),
                new SqlParameter("@price1", model.Price1),
                new SqlParameter("@piecenum1", model.PieceNum1),
                new SqlParameter("@pieceweight1", model.PieceWeight1),
                new SqlParameter("@goodsname2", model.GoodsName2),
                new SqlParameter("@customsno2", model.CustomsNo2),
                new SqlParameter("@price2", model.Price2),
                new SqlParameter("@piecenum2", model.PieceNum2),
                new SqlParameter("@pieceweight2", model.PieceWeight2),
                new SqlParameter("@goodsname3", model.GoodsName3),
                new SqlParameter("@customsno3", model.CustomsNo3),
                new SqlParameter("@price3", model.Price3),
                new SqlParameter("@piecenum3", model.PieceNum3),
                new SqlParameter("@pieceweight3", model.PieceWeight3),
                new SqlParameter("@declaredvalue", model.DeclaredValue),
                new SqlParameter("@declaredcurrency", model.DeclaredCurrency),
                new SqlParameter("@ispayduty", model.IsPayDuty),
                new SqlParameter("@insured", model.Insured),
                new SqlParameter("@typingtype", model.TypingType),
                new SqlParameter("@destination", model.Destination),
                new SqlParameter("@destinationpoint", model.DestinationPoint),
                new SqlParameter("@sender", model.Sender),
                new SqlParameter("@sendphone", model.SendPhone),
                new SqlParameter("@sendaddress", model.SendAddress),
                new SqlParameter("@freight", model.Freight),
                new SqlParameter("@customerquotation", model.CustomerQuotation),
                new SqlParameter("@tax", model.Tax),
                new SqlParameter("@phonecount", model.PhoneCount),
                new SqlParameter("@importbatch", model.ImportBatch),
                new SqlParameter("@exportbatch", model.ExportBatch),
                new SqlParameter("@created", model.Created)
            };

            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 判断指定列数据是否重复
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool IsRepeatByColumnName(DataTable dt_import, string columnName)
        {
            bool result = true;

            //获取分组记录大于1 的记录
            int repeatCount = ((from p in dt_import.AsEnumerable()
                                group p by p.Field<string>(columnName) into g
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

        /// <summary>
        /// 生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
        /// </summary>
        /// <param name="dt_import">导入数据</param>
        /// <param name="dt_taxnumber">商品税则号对应关系表</param>
        /// <param name="dt_relation">原物品对应关系</param>
        /// <param name="dt_area">地区信息</param>
        /// <returns></returns>
        private string GenerateWaybillData(DataTable dt_import, DataTable dt_taxnumber, DataTable dt_relation, DataTable dt_area)
        {
            //需要关联税则号的列
            string[] relevanceArray = new string[] { "货物明细信息|物品名称①", "货物明细信息|物品名称②", "货物明细信息|物品名称③" };
            string[] taxArray = new string[] { "货物明细信息|税关号①", "货物明细信息|税关号②", "货物明细信息|税关号③" };
            string[] priceArray = new string[] { "货物明细信息|单价①", "货物明细信息|单价②", "货物明细信息|单价③" };
            string[] pieceNumArray = new string[] { "货物明细信息|单件件数①", "货物明细信息|单件件数②", "货物明细信息|单件件数③" };
            string[] pieceWeightArray = new string[] { "货物明细信息|单件重量①", "货物明细信息|单件重量②", "货物明细信息|单件重量③" };

            StringBuilder emptySb = new StringBuilder();
            string validMsg = string.Empty;
            bool flag = true;
            List<string> newNameList = null;
            DataRow[] taxRows = null;
            DataRow[] relRows = null;

            //遍历数据行
            foreach (DataRow row in dt_import.Rows)
            {
                //根据城市匹配省份、邮编信息
                DataRow[] areaRows = (from p in dt_area.AsEnumerable()
                                      where p.Field<string>("areacity").Contains(row["收件人信息|收件城市"].ToString().Trim())
                                      select p).ToArray();
                if (areaRows.Count() > 0)
                {
                    row["收件人信息|收件城市"] = areaRows[0]["areacity"].ToString();
                    row["收件人信息|收件省份"] = areaRows[0]["areaprovince"].ToString();
                    row["收件人信息|收件地邮编"] = areaRows[0]["areapostcode"].ToString();
                }

                //重命名物品名称、关联税关号、匹配完税价格
                flag = true;
                validMsg = "运单编号：" + row["运单编号"].ToString() + "<br/>&nbsp;&nbsp;";

                for (int i = 0; i < relevanceArray.Length; i++)
                {
                    //物品名称不为空时才去关联税关号、重命名物品名称
                    if (!string.IsNullOrWhiteSpace(row[relevanceArray[i]].ToString()))
                    {
                        //根据物品名称去物品对应关系表中匹配税关号与新物品名称列表
                        relRows = (from p in dt_relation.AsEnumerable()
                                   where p.Field<string>("originalname") == row[relevanceArray[i]].ToString().Trim()
                                   select p).ToArray();

                        if (relRows.Count() > 0)
                        {
                            //设置税关号
                            row[taxArray[i]] = relRows[0]["taxnumber"].ToString();

                            //重命名物品名称
                            newNameList = new List<string>();
                            for (int index = 3; index < relRows[0].ItemArray.Length; index++)
                            {
                                if (!string.IsNullOrWhiteSpace(relRows[0][index].ToString()))
                                {
                                    newNameList.Add(relRows[0][index].ToString().Trim());
                                }
                            }
                            if (newNameList.Count > 0)
                            {
                                //随机重命名
                                Random random = new Random(Math.Abs((int)BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0)));
                                int rindex = random.Next(0, newNameList.Count);
                                row[relevanceArray[i]] = newNameList[rindex];
                            }
                        }

                        //税关号不为空时, 关联商品完税价格
                        if (!string.IsNullOrWhiteSpace(row[taxArray[i]].ToString()))
                        {
                            //根据税关号去商品税号关系表匹配完税价格
                            taxRows = (from p in dt_taxnumber.AsEnumerable()
                                       where p.Field<string>("ptaxnumber") == row[taxArray[i]].ToString().Trim()
                                       select p).ToArray();

                            if (taxRows.Count() > 0)
                            {
                                //存在则更新完税价格
                                row[priceArray[i]] = Convert.ToDecimal(taxRows[0]["ptaxprice"]);
                            }
                            else
                            {
                                //没有匹配到税率, 清空物品单价
                                row[priceArray[i]] = 0m;
                            }
                        }
                        else
                        {
                            //税关号为空时, 清空物品单价
                            row[priceArray[i]] = 0m;
                        }
                    }
                    else
                    {
                        //物品名称为空时, 清空税关号、单价、件数、重量信息
                        row[taxArray[i]] = "";
                        row[priceArray[i]] = 0m;
                        row[pieceNumArray[i]] = 0;
                        row[pieceWeightArray[i]] = 0m;
                    }

                    //物品名称不为空, 税关号为空时, 记录税关号未添加
                    if (!string.IsNullOrWhiteSpace(row[relevanceArray[i]].ToString()) && string.IsNullOrWhiteSpace(row[taxArray[i]].ToString()))
                    {
                        flag = false;
                        validMsg += taxArray[i] + "，";
                    }
                }

                if (!flag)
                {
                    validMsg = validMsg.TrimEnd('，');
                    validMsg += "，税关号未添加";

                    emptySb.Append(validMsg);
                    emptySb.Append("<br/>");
                }
            }

            return emptySb.ToString();
        }

        /// <summary>
        /// 生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
        /// </summary>
        /// <param name="dt_import">导入数据</param>
        /// <param name="dt_taxnumber">商品税则号对应关系表</param>
        /// <param name="dt_relation">原物品对应关系</param>
        /// <param name="dt_area">地区信息</param>
        /// <returns></returns>
        private string GenerateWaybillData2(DataTable dt_import, DataTable dt_taxnumber, DataTable dt_relation, DataTable dt_area)
        {
            //需要关联税则号的列
            string[] relevanceArray = new string[] { "goodsname1", "goodsname2", "goodsname3" };
            string[] taxArray = new string[] { "customsno1", "customsno2", "customsno3" };
            string[] priceArray = new string[] { "price1", "price2", "price3" };
            string[] pieceNumArray = new string[] { "piecenum1", "piecenum2", "piecenum3" };
            string[] pieceWeightArray = new string[] { "pieceweight1", "pieceweight2", "pieceweight3" };

            StringBuilder emptySb = new StringBuilder();
            string validMsg = string.Empty;
            bool flag = true;
            List<string> newNameList = null;
            DataRow[] taxRows = null;
            DataRow[] relRows = null;

            //遍历数据行
            foreach (DataRow row in dt_import.Rows)
            {
                //根据城市匹配省份、邮编信息
                DataRow[] areaRows = (from p in dt_area.AsEnumerable()
                                      where p.Field<string>("areacity").Contains(row["reccity"].ToString().Trim())
                                      select p).ToArray();
                if (areaRows.Count() > 0)
                {
                    row["reccity"] = areaRows[0]["areacity"].ToString();
                    row["recprovince"] = areaRows[0]["areaprovince"].ToString();
                    row["recpostcode"] = areaRows[0]["areapostcode"].ToString();
                }

                //重命名物品名称、关联税关号、匹配完税价格
                flag = true;
                validMsg = "运单编号：" + row["waybillnumber"].ToString() + "<br/>&nbsp;&nbsp;";

                for (int i = 0; i < relevanceArray.Length; i++)
                {
                    //物品名称不为空时才去关联税关号、重命名物品名称
                    if (!string.IsNullOrWhiteSpace(row[relevanceArray[i]].ToString()))
                    {
                        //根据物品名称去物品对应关系表中匹配税关号与新物品名称列表
                        relRows = (from p in dt_relation.AsEnumerable()
                                   where p.Field<string>("originalname") == row[relevanceArray[i]].ToString().Trim()
                                   select p).ToArray();

                        if (relRows.Count() > 0)
                        {
                            //设置税关号
                            row[taxArray[i]] = relRows[0]["taxnumber"].ToString();

                            //重命名物品名称
                            newNameList = new List<string>();
                            for (int index = 3; index < relRows[0].ItemArray.Length; index++)
                            {
                                if (!string.IsNullOrWhiteSpace(relRows[0][index].ToString()))
                                {
                                    newNameList.Add(relRows[0][index].ToString().Trim());
                                }
                            }
                            if (newNameList.Count > 0)
                            {
                                //随机重命名
                                Random random = new Random(Math.Abs((int)BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0)));
                                int rindex = random.Next(0, newNameList.Count);
                                row[relevanceArray[i]] = newNameList[rindex];
                            }
                        }

                        //税关号不为空时, 关联商品完税价格
                        if (!string.IsNullOrWhiteSpace(row[taxArray[i]].ToString()))
                        {
                            //根据税关号去商品税号关系表匹配完税价格
                            taxRows = (from p in dt_taxnumber.AsEnumerable()
                                       where p.Field<string>("ptaxnumber") == row[taxArray[i]].ToString().Trim()
                                       select p).ToArray();

                            if (taxRows.Count() > 0)
                            {
                                //存在则更新完税价格
                                row[priceArray[i]] = Convert.ToDecimal(taxRows[0]["ptaxprice"]);
                            }
                            else
                            {
                                //没有匹配到税率, 清空物品单价
                                row[priceArray[i]] = 0m;
                            }
                        }
                        else
                        {
                            //税关号为空时, 清空物品单价
                            row[priceArray[i]] = 0m;
                        }
                    }
                    else
                    {
                        //物品名称为空时, 清空税关号、单价、件数、重量信息
                        row[taxArray[i]] = "";
                        row[priceArray[i]] = 0m;
                        row[pieceNumArray[i]] = 0;
                        row[pieceWeightArray[i]] = 0m;
                    }

                    //物品名称不为空, 税关号为空时, 记录税关号未添加
                    if (!string.IsNullOrWhiteSpace(row[relevanceArray[i]].ToString()) && string.IsNullOrWhiteSpace(row[taxArray[i]].ToString()))
                    {
                        flag = false;
                        validMsg += taxArray[i] + "，";
                    }
                }

                if (!flag)
                {
                    validMsg = validMsg.TrimEnd('，');
                    validMsg += "，税关号未添加";

                    emptySb.Append(validMsg);
                    emptySb.Append("<br/>");
                }
            }

            return emptySb.ToString();
        }

        /// <summary>
        /// 计算运单的物品单价、税金、申报价值、物品数量
        /// </summary>
        /// <param name="model">每个运单对象</param>
        /// <param name="dt_taxnumber">商品税号关系表</param>
        private string CalculateWaybillTaxes(ModWayBill model, DataTable dt_taxnumber)
        {
            decimal minPrice = 0m, maxPrice = 0m, calPrice = 0m;
            DataRow[] taxRows = null;
            decimal taxRate1 = 0m, taxRate2 = 0m, taxRate3 = 0m; //税率
            //使用指定种子的随机数生成器
            Random random = new Random(Math.Abs((int)BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0)));
            //计算信息
            bool flag = true;
            string validMsg = "运单编号：" + model.WaybillNumber;

            #region 设置各物品的商品税率

            //根据 货物明细信息|税关号① 匹配 税率①
            if (!string.IsNullOrWhiteSpace(model.GoodsName1) && !string.IsNullOrWhiteSpace(model.CustomsNo1))
            {
                taxRows = (from p in dt_taxnumber.AsEnumerable()
                           where p.Field<string>("ptaxnumber") == model.CustomsNo1
                           select p).ToArray();
                if (taxRows.Count() > 0)
                {
                    taxRate1 = Convert.ToDecimal(taxRows[0]["ptaxrate"]) / 100;
                }
            }
            //根据 货物明细信息|税关号② 匹配 税率②
            if (!string.IsNullOrWhiteSpace(model.GoodsName2) && !string.IsNullOrWhiteSpace(model.CustomsNo2))
            {
                taxRows = (from p in dt_taxnumber.AsEnumerable()
                           where p.Field<string>("ptaxnumber") == model.CustomsNo2
                           select p).ToArray();
                if (taxRows.Count() > 0)
                {
                    taxRate2 = Convert.ToDecimal(taxRows[0]["ptaxrate"]) / 100;
                }
            }
            //根据 货物明细信息|税关号③ 匹配 税率③
            if (!string.IsNullOrWhiteSpace(model.GoodsName3) && !string.IsNullOrWhiteSpace(model.CustomsNo3))
            {
                taxRows = (from p in dt_taxnumber.AsEnumerable()
                           where p.Field<string>("ptaxnumber") == model.CustomsNo3
                           select p).ToArray();
                if (taxRows.Count() > 0)
                {
                    taxRate3 = Convert.ToDecimal(taxRows[0]["ptaxrate"]) / 100;
                }
            }

            #endregion

            #region 计算运单物品数量

            if (string.IsNullOrEmpty(model.GoodsName2) && string.IsNullOrEmpty(model.GoodsName3))
            {
                //物品②、③不存在时, 物品数量①大于5的都改为5
                if (model.PieceNum1 > 5)
                    model.PieceNum1 = 5;
            }
            else if (string.IsNullOrEmpty(model.GoodsName3))
            {
                //物品③不存在时, 物品数量①、②大于3的都改为3
                if (model.PieceNum1 > 3)
                    model.PieceNum1 = 3;

                if (model.PieceNum2 > 3)
                    model.PieceNum2 = 3;
            }
            else
            {
                //物品①、②、③都存在时, 物品数量①、②、③大于2的都改为2
                if (model.PieceNum1 > 2)
                    model.PieceNum1 = 2;

                if (model.PieceNum2 > 2)
                    model.PieceNum2 = 2;

                if (model.PieceNum3 > 2)
                    model.PieceNum3 = 2;
            }

            #endregion

            #region 计算运单税金

            if (taxRate1 != 0m && taxRate2 != 0m && taxRate3 != 0m)
            {
                #region 三个物品都有对应税率

                if (model.Price1 != 0m && model.Price2 != 0m && model.Price3 != 0m) //都有对应完税价格
                {
                    //不需要计算单价
                }
                else if (model.Price1 != 0m && model.Price2 != 0m && model.Price3 == 0m) //model.Price3 没有完税价格
                {
                    //计算model.Price3的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price1 * model.PieceNum1 * taxRate1 - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price1 * model.PieceNum1 * taxRate1 - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }
                else if (model.Price1 != 0m && model.Price3 != 0m && model.Price2 == 0m) //model.Price2 没有完税价格
                {
                    //计算model.Price2的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price1 * model.PieceNum1 * taxRate1 - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price1 * model.PieceNum1 * taxRate1 - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;
                }
                else if (model.Price2 != 0m && model.Price3 != 0m && model.Price1 == 0m) //model.Price1 没有完税价格
                {
                    //计算model.Price1的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price2 * model.PieceNum2 * taxRate2 - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price2 * model.PieceNum2 * taxRate2 - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;
                }
                else if (model.Price1 != 0m && model.Price2 == 0m && model.Price3 == 0m) // model.Price2, model.Price3 没有完税价格
                {
                    //计算model.Price2、model.Price3的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        flag = false;
                        model.Price2 = 0m;
                        model.Price3 = 0m;

                        validMsg += "，单价②、单价③计算错误";
                    }
                    else
                    {
                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价②计算错误";
                        }
                        model.Price2 = calPrice;

                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价③计算错误";
                        }
                        model.Price3 = calPrice;
                    }
                }
                else if (model.Price2 != 0m && model.Price1 == 0m && model.Price3 == 0m) // model.Price1, model.Price3 没有完税价格
                {
                    //计算model.Price1、model.Price3的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum1 * taxRate1 + model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum1 * taxRate1 + model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        flag = false;
                        model.Price1 = 0m;
                        model.Price3 = 0m;

                        validMsg += "，单价①、单价③计算错误";
                    }
                    else
                    {
                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价①计算错误";
                        }
                        model.Price1 = calPrice;

                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价③计算错误";
                        }
                        model.Price3 = calPrice;
                    }
                }
                else if (model.Price3 != 0m && model.Price1 == 0m && model.Price2 == 0m) // model.Price1, model.Price2 没有完税价格
                {
                    //计算model.Price1、model.Price2的单价 
                    // minTaxValue-∑(完税价格m*件数m*税率m) <= ∑(单价n*件数n*税率n) < maxTaxValue-∑(完税价格m*件数m*税率m)
                    minPrice = Math.Truncate(((minTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        flag = false;
                        model.Price1 = 0m;
                        model.Price2 = 0m;

                        validMsg += "，单价①、单价②计算错误";
                    }
                    else
                    {
                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价①计算错误";
                        }
                        model.Price1 = calPrice;

                        calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;

                        if (calPrice < 0m)
                        {
                            flag = false;
                            calPrice = 0m;
                            validMsg += "，单价②计算错误";
                        }
                        model.Price2 = calPrice;
                    }
                }
                else // 三个物品都没有完税价格
                {
                    //计算model.Price1、model.Pric2、model.Pric3的单价 
                    // minTaxValue <= 单价1*件数1*税率1 + 单价2*件数2*税率2 + 单价3*件数3*税率3 < maxTaxValue
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;

                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;

                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round((model.Price1 * model.PieceNum1 * taxRate1
                                      + model.Price2 * model.PieceNum2 * taxRate2
                                      + model.Price3 * model.PieceNum3 * taxRate3), 2);

                #endregion
            }
            else if (taxRate1 != 0m && taxRate2 != 0m)
            {
                #region 物品①与物品②有对应税率

                if (model.Price1 != 0m && model.Price2 != 0m) //都有对应完税价格
                {
                    //不需要计算单价
                }
                else if (model.Price1 != 0m && model.Price2 == 0m) //model.Price2 没有完税价格
                {
                    //计算model.Price2的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;
                }
                else if (model.Price2 != 0m && model.Price1 == 0m) //model.Price1 没有完税价格
                {
                    //计算model.Price1的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum1 * taxRate1)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum1 * taxRate1)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;
                }
                else
                {
                    //计算model.Price1、model.Pric2的单价
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;

                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round((model.Price1 * model.PieceNum1 * taxRate1 + model.Price2 * model.PieceNum2 * taxRate2), 2);

                #endregion
            }
            else if (taxRate1 != 0m && taxRate3 != 0m)
            {
                #region 物品①与物品③有对应税率

                if (model.Price1 != 0m && model.Price3 != 0m) //都有对应完税价格
                {
                    //不需要计算单价
                }
                else if (model.Price1 != 0m && model.Price3 == 0m) //model.Price3 没有完税价格
                {
                    //计算model.Price3的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price1 * model.PieceNum1 * taxRate1) / (model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }
                else if (model.Price3 != 0m && model.Price1 == 0m) //model.Price1 没有完税价格
                {
                    //计算model.Price1的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum1 * taxRate1)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;
                }
                else
                {
                    //计算model.Price1、model.Pric3的单价
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum1 * taxRate1 + model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;

                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round((model.Price1 * model.PieceNum1 * taxRate1 + model.Price3 * model.PieceNum3 * taxRate3), 2);

                #endregion
            }
            else if (taxRate2 != 0m && taxRate3 != 0m)
            {
                #region 物品②与物品③有对应税率

                if (model.Price2 != 0m && model.Price3 != 0m) //都有对应完税价格
                {
                    //不需要计算单价
                }
                else if (model.Price2 != 0m && model.Price3 == 0m) //model.Price3 没有完税价格
                {
                    //计算model.Price3的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price2 * model.PieceNum2 * taxRate2) / (model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }
                else if (model.Price3 != 0m && model.Price2 == 0m) //model.Price2 没有完税价格
                {
                    //计算model.Price2的单价
                    minPrice = Math.Truncate(((minTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate(((maxTaxValue - model.Price3 * model.PieceNum3 * taxRate3) / (model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (minPrice < 0m || maxPrice < 0m)
                    {
                        calPrice = -0.1m;
                    }
                    else
                    {
                        if (calPrice < minPrice)
                            calPrice = calPrice + 0.1m;
                        if (calPrice > maxPrice)
                            calPrice = -0.1m;
                    }

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;
                }
                else
                {
                    //计算model.Price2、model.Pric3的单价
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum2 * taxRate2 + model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;

                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round((model.Price2 * model.PieceNum2 * taxRate2 + model.Price3 * model.PieceNum3 * taxRate3), 2);

                #endregion
            }
            else if (taxRate1 != 0m)
            {
                #region 物品①有对应税率

                //判断 货物明细信息|单价① 是否为 0, 税金范围: minTaxValue <= 税金 < maxTaxValue, 计算单价①
                if (model.Price1 == 0m)
                {
                    //计算最小价格,不为整数时 最小价格 + 0.1m
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum1 * taxRate1)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    //计算最大价格, 不包含边界值, 故最大价格 - 0.1m
                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum1 * taxRate1)) * 100) / 100 - 0.1m;

                    //计算物品单价
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价①计算错误";
                    }
                    model.Price1 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round(model.Price1 * model.PieceNum1 * taxRate1, 2);

                #endregion
            }
            else if (taxRate2 != 0m)
            {
                #region 物品②有对应税率

                //判断 货物明细信息|单价② 是否为 0, 税金范围: minTaxValue <= 税金 < maxTaxValue, 计算单价②
                if (model.Price2 == 0m)
                {
                    //计算最小价格,不为整数时 最小价格 + 0.1m
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum2 * taxRate2)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    //计算最大价格, 不包含边界值, 故最大价格 - 0.1m
                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum2 * taxRate2)) * 100) / 100 - 0.1m;

                    //计算物品单价
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价②计算错误";
                    }
                    model.Price2 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round(model.Price2 * model.PieceNum2 * taxRate2, 2);

                #endregion
            }
            else if (taxRate3 != 0m)
            {
                #region 物品③有对应税率

                //判断 货物明细信息|单价③ 是否为 0, 税金范围: minTaxValue <= 税金 < maxTaxValue, 计算单价③
                if (model.Price3 == 0m)
                {
                    //计算最小价格,不为整数时 最小价格 + 0.1m
                    minPrice = Math.Truncate((minTaxValue / (model.PieceNum3 * taxRate3)) * 100) / 100;
                    if ((int)minPrice != minPrice)
                        minPrice = minPrice + 0.1m;

                    //计算最大价格, 不包含边界值, 故最大价格 - 0.1m
                    maxPrice = Math.Truncate((maxTaxValue / (model.PieceNum3 * taxRate3)) * 100) / 100 - 0.1m;

                    //计算物品单价
                    calPrice = Math.Round(Convert.ToDecimal(Math.Truncate(random.NextDouble() * 100) / 100) * (maxPrice - minPrice) + minPrice, 1);
                    if (calPrice < minPrice)
                        calPrice = calPrice + 0.1m;
                    if (calPrice > maxPrice)
                        calPrice = -0.1m;

                    if (calPrice < 0m)
                    {
                        flag = false;
                        calPrice = 0m;
                        validMsg += "，单价③计算错误";
                    }
                    model.Price3 = calPrice;
                }

                //计算税金
                model.Tax = Math.Round(model.Price3 * model.PieceNum3 * taxRate3, 2);

                #endregion
            }
            else
            {
                //物品税率都为0时，不计算税金
                model.Tax = 0m;
            }

            #endregion

            //计算运单申报价值
            model.DeclaredValue = model.Price1 * model.PieceNum1 + model.Price2 * model.PieceNum2 + model.Price3 * model.PieceNum3;

            //返回计算信息
            if (flag)
                validMsg = "";

            return validMsg;
        }

        /// <summary>
        /// 更新运单物品单价、税金、申报价值、物品数量信息
        /// </summary>
        /// <param name="model"></param>
        private void UpdateRowTaxPrice(ModWayBill model)
        {
            //更新语句
            StringBuilder sbSql = new StringBuilder();

            sbSql.Append("update waybill set ");
            sbSql.Append("price1=@price1,");
            sbSql.Append("piecenum1=@piecenum1,");
            sbSql.Append("price2=@price2,");
            sbSql.Append("piecenum2=@piecenum2,");
            sbSql.Append("price3=@price3,");
            sbSql.Append("piecenum3=@piecenum3,");
            sbSql.Append("declaredvalue=@declaredvalue,");
            sbSql.Append("tax=@tax");
            sbSql.Append(" where oid=@oid");

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@price1", model.Price1),
                new SqlParameter("@piecenum1", model.PieceNum1),
                new SqlParameter("@price2", model.Price2),
                new SqlParameter("@piecenum2", model.PieceNum2),
                new SqlParameter("@price3", model.Price3),
                new SqlParameter("@piecenum3", model.PieceNum3),
                new SqlParameter("@declaredvalue", model.DeclaredValue),
                new SqlParameter("@tax", model.Tax),
                new SqlParameter("@oid", model.Oid)
            };

            //执行更新语句
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sbSql.ToString(), _params);
        }

        /// <summary>
        /// 更新运单物品单价、税金、申报价值、物品数量信息
        /// </summary>
        /// <param name="model"></param>
        private void UpdateRowTaxPrice2(ModWayBill model)
        {
            //更新到数据库
            StringBuilder sbSql = new StringBuilder();

            sbSql.Append("update waybill set ");
            sbSql.Append("reccity=@reccity,");
            sbSql.Append("recprovince=@recprovince,");
            sbSql.Append("recpostcode=@recpostcode,");
            sbSql.Append("goodsname1=@goodsname1,");
            sbSql.Append("customsno1=@customsno1,");
            sbSql.Append("price1=@price1,");
            sbSql.Append("piecenum1=@piecenum1,");
            sbSql.Append("pieceweight1=@pieceweight1,");
            sbSql.Append("goodsname2=@goodsname2,");
            sbSql.Append("customsno2=@customsno2,");
            sbSql.Append("price2=@price2,");
            sbSql.Append("piecenum2=@piecenum2,");
            sbSql.Append("pieceweight2=@pieceweight2,");
            sbSql.Append("goodsname3=@goodsname3,");
            sbSql.Append("customsno3=@customsno3,");
            sbSql.Append("price3=@price3,");
            sbSql.Append("piecenum3=@piecenum3,");
            sbSql.Append("pieceweight3=@pieceweight3,");
            sbSql.Append("declaredvalue=@declaredvalue,");
            sbSql.Append("tax=@tax,");
            sbSql.Append("updated=@updated,");
            sbSql.Append("updated_time=getdate()");
            sbSql.Append(" where oid=@oid");

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@reccity", model.RecCity),
                new SqlParameter("@recprovince", model.RecProvince),
                new SqlParameter("@recpostcode", model.RecPostcode),
                new SqlParameter("@goodsname1", model.GoodsName1),
                new SqlParameter("@customsno1", model.CustomsNo1),
                new SqlParameter("@price1", model.Price1),
                new SqlParameter("@piecenum1", model.PieceNum1),
                new SqlParameter("@pieceweight1", model.PieceWeight1),
                new SqlParameter("@goodsname2", model.GoodsName2),
                new SqlParameter("@customsno2", model.CustomsNo2),
                new SqlParameter("@price2", model.Price2),
                new SqlParameter("@piecenum2", model.PieceNum2),
                new SqlParameter("@pieceweight2", model.PieceWeight2),
                new SqlParameter("@goodsname3", model.GoodsName3),
                new SqlParameter("@customsno3", model.CustomsNo3),
                new SqlParameter("@price3", model.Price3),
                new SqlParameter("@piecenum3", model.PieceNum3),
                new SqlParameter("@pieceweight3", model.PieceWeight3),
                new SqlParameter("@declaredvalue", model.DeclaredValue),
                new SqlParameter("@tax", model.Tax),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@oid", model.Oid)
            };

            //执行更新语句
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sbSql.ToString(), _params);
        }

        /// <summary>
        /// 根据批次号计算当前导入运单的phonecount值, 收件人手机号码 第一次出现为1, 第二次出现为2, 依此类推...
        /// </summary>
        /// <param name="listWaybill"></param>
        private void CalculateWaybillPhoneCount(List<ModWayBill> listWaybill)
        {
            //获取导入excel文件的批次号
            var queryBatch = from p in listWaybill
                             group p by p.WarehousingNo into g
                             select new
                             {
                                 Name = g.Key
                             };
            //遍历入库批次
            foreach (var batch in queryBatch)
            {
                //查找指定批次RecPhone出现次数大于1的记录
                var queryPhone = from p in listWaybill
                                 where p.WarehousingNo == batch.Name
                                 group p by p.RecPhone into g
                                 where g.Count() > 1
                                 select new
                                 {
                                     Name = g.Key
                                 };

                //更新运单的PhoneCount信息
                List<ModWayBill> list = null;
                foreach (var phone in queryPhone)
                {
                    list = listWaybill.Where(p => p.RecPhone == phone.Name && p.WarehousingNo == batch.Name).ToList();

                    for (int i = 0; i < list.Count(); i++)
                    {
                        list[i].PhoneCount = i + 1;
                    }
                }
            }
        }

        #endregion

        #region 初始化基础数据

        /// <summary>
        /// 获取地区信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetAreaCityData()
        {
            string strSql = string.Format(@"select areaid, areaprovince, areacity, areapostcode from cityarea order by areaid");

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        /// <summary>
        /// 获取商品税号信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetGoodTaxNumberData()
        {
            string strSql = string.Format(@"select pid, ptaxnumber, ptaxprice, ptaxrate from goodtaxnumber order by ptaxnumber");

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        /// <summary>
        /// 获取违禁物品信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetProhibitedGoodData()
        {
            string strSql = string.Format(@"select pid, type, pname, premark from prohibitedgood order by pid");

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        /// <summary>
        /// 获取物品重命名对应关系信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetGoodRelationData()
        {
            string strSql = string.Format(@"select rid, originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, 
                            newname8, newname9, newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, 
                            newname18, newname19, newname20 from goodrelation order by rid");

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        #endregion
    }
}