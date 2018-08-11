using ExpressCommon;
using ExpressModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 导入导出api
    /// </summary>
    public class ApiController : Controller
    {
        public string ArrayName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["nameArray"]) ? "" : ConfigurationManager.AppSettings["nameArray"];
        int minTaxValue = 45, maxTaxValue = 50;

        //必填项
        readonly string[] requireArray_New = new string[] { "总重", "收件人姓名", "收件人地址 1",
                "收件人城市", "货物名称 1", "件数 1"};

        //价格、重量验证, 只能输入数字, 并且最多包含两位小数
        readonly string[] decimalArray_New = new string[] { "总重", "单价 1",  "单价 2",
                 "单价 3", "申报价值"};

        //件数, 只能输入正整数                                                                                                                                                                                                                                                                   
        readonly string[] integerArray_New = new string[] { "件数 1", "件数 2", "件数 3" };

        //物品名称验证, 是否为违禁物品
        readonly string[] prohibitedArray_New = new string[] { "货物名称 1", "货物名称 2", "货物名称 3" };

        /// <summary>
        /// 用户名检测  通过则true,不通过则false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckUserName(string name)
        {
            var arr = ArrayName.Split(',');
            foreach (var item in arr)
            {
                if (name.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 导入方法
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InputExpress(FormCollection fc)
        {
            try
            {
                var loginAccount = "管理员";

                var username = fc["name"];
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Json(new JsonData() { Status = false, Msg = "用户名称不能为空" });
                }
                if (!CheckUserName(username))
                {
                    return Json(new JsonData() { Status = false, Msg = "用户名称验证不通过" });
                }
                var data = fc["data"];
                DataTable dt_import = Newtonsoft.Json.JsonConvert.DeserializeObject(data, typeof(DataTable)) as DataTable;

                #region 设置默认区间

                string defaultTaxInterval = ConfigurationManager.AppSettings["DefaultTaxInterval"] ?? "45,50";

                if (defaultTaxInterval.Split(',').Length != 2 || Convert.ToInt32(defaultTaxInterval.Split(',')[0]) > Convert.ToInt32(defaultTaxInterval.Split(',')[1]))
                {
                    return Json(new JsonData() { Status = false, Msg = "价格区间配置错误，参考格式'45,50'" });
                }

                minTaxValue = Convert.ToInt32(defaultTaxInterval.Split(',')[0]);
                maxTaxValue = Convert.ToInt32(defaultTaxInterval.Split(',')[1]);

                #endregion

                #region 验证格式是否符合规范

                //DataTable数据完整性验证
                string fieldMsg = "";
                if (!DataTableFieldValid_New(dt_import, out fieldMsg))
                {
                    return Json(new JsonData() { Status = false, Msg = fieldMsg });
                }

                //判断是否存在入仓号或则运单编号为空记录
                DataRow[] emptyRows = (from p in dt_import.AsEnumerable()
                                       where string.IsNullOrWhiteSpace(p.Field<string>("参考编号"))
                                       select p).ToArray();
                if (emptyRows.Count() > 0)
                {
                    return Json(new JsonData() { Status = false, Msg = "推送记录中存在参考编号为空的记录，导入失败！" });
                }

                //判断是否存在运单编号重复的情况
                if (!IsRepeatByColumnName(dt_import, "参考编号"))
                {
                    return Json(new JsonData() { Status = false, Msg = "推送记录中存在参考编号重复的记录，导入失败！" });
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
                    validStr = EmportRowValidData_New(row, dt_prohibited);
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
                string importBatchName = "import_interface_" + username + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                //生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
                string emptyTaxMsg = GenerateWaybillData_New(dt_import, dt_taxnumber, dt_relation, dt_area);

                //将dataTable转换为list集合
                List<ModWayBill> listWaybill = new List<ModWayBill>();
                //将excel导入结果存入list列表
                foreach (DataRow row in dt_import.Rows)
                {
                    //添加到list集合
                    listWaybill.Add(new ModWayBill()
                    {
                        WarehousingNo = row["入仓号"].ToString().Trim(),
                        WaybillNumber = row["参考编号"].ToString().Trim(),
                        SettlementWeight = string.IsNullOrWhiteSpace(row["总重"].ToString()) ? 0 : Convert.ToDecimal(row["总重"]),
                        SingleChannel = row["e特快单号"].ToString().Trim(),
                        Recipient = row["收件人姓名"].ToString().Trim(),
                        RecPhone = row["收件人电话"].ToString().Trim(),
                        RecAddress = row["收件人地址 1"].ToString().Trim(),
                        RecCity = row["收件人城市"].ToString().Trim(),
                        RecProvince = row["收件人省份"].ToString().Trim(),
                        RecPostcode = row["邮编"].ToString().Trim(),
                        GoodsName1 = row["货物名称 1"].ToString().Trim(),
                        CustomsNo1 = row["税则号 1"].ToString().Trim(),
                        Price1 = string.IsNullOrWhiteSpace(row["单价 1"].ToString()) ? 0 : Convert.ToDecimal(row["单价 1"]),
                        PieceNum1 = string.IsNullOrWhiteSpace(row["件数 1"].ToString()) ? 0 : Convert.ToInt32(row["件数 1"]),
                        PieceWeight1 = string.IsNullOrWhiteSpace(row["单个重量 1"].ToString()) ? 0 : Convert.ToDecimal(row["单个重量 1"]),
                        GoodsName2 = row["货物名称 2"].ToString().Trim(),
                        CustomsNo2 = row["税则号 2"].ToString().Trim(),
                        Price2 = string.IsNullOrWhiteSpace(row["单价 2"].ToString()) ? 0 : Convert.ToDecimal(row["单价 2"]),
                        PieceNum2 = string.IsNullOrWhiteSpace(row["件数 2"].ToString()) ? 0 : Convert.ToInt32(row["件数 2"]),
                        PieceWeight2 = string.IsNullOrWhiteSpace(row["单个重量 2"].ToString()) ? 0 : Convert.ToDecimal(row["单个重量 2"]),
                        GoodsName3 = row["货物名称 3"].ToString().Trim(),
                        CustomsNo3 = row["税则号 3"].ToString().Trim(),
                        Price3 = string.IsNullOrWhiteSpace(row["单价 3"].ToString()) ? 0 : Convert.ToDecimal(row["单价 3"]),
                        PieceNum3 = string.IsNullOrWhiteSpace(row["件数 3"].ToString()) ? 0 : Convert.ToInt32(row["件数 3"]),
                        PieceWeight3 = string.IsNullOrWhiteSpace(row["单个重量 3"].ToString()) ? 0 : Convert.ToDecimal(row["单个重量 3"]),
                        DeclaredValue = string.IsNullOrWhiteSpace(row["申报价值"].ToString()) ? 0 : Convert.ToDecimal(row["申报价值"]),
                        DeclaredCurrency = row["申报货币"].ToString().Trim(),
                        IsPayDuty = string.IsNullOrWhiteSpace(row["代付税金"].ToString()) ? 0 : (row["代付税金"].ToString().ToLower().Equals("yes") ? 1 : 0),
                        TypingType = row["提单日期"].ToString().Trim(),
                        Insured = 0,
                        Destination = "",
                        DestinationPoint = "",
                        Sender = "",
                        SendPhone = "",
                        SendAddress = "",
                        Freight = 0,
                        CustomerQuotation = 0,
                        Tax = 0m,
                        PhoneCount = 1,
                        ImportBatch = importBatchName,
                        ExportBatch = "",
                        Created = loginAccount
                    });
                }

                //根据批次号计算当前导入运单的phonecount值
                CalculateWaybillPhoneCount(listWaybill);

                //插入到数据库 -- 启用事务
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
                var json = new JsonData();
                json.Status = true;

                //税关号未添加提示
                if (!string.IsNullOrEmpty(emptyTaxMsg))
                {
                    json.Msg = $"{emptyTaxMsg}<br/>成功推送" + listWaybill.Count.ToString() + "条数据！";
                }
                else
                {
                    json.Msg = "成功推送" + listWaybill.Count.ToString() + "条数据！";
                }

                #endregion

                return Json(json);
            }
            catch (Exception ex)
            {
                return Json(new JsonData() { Status = false, Msg = ex.Message });
            }
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

        #region 新模板验证
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
        /// 导入datatable字段验证
        /// </summary>
        /// <param name="dt_import"></param>
        private bool DataTableFieldValid_New(DataTable dt_import, out string fieldMsg)
        {
            List<string> fieldList = new List<string>() {
               "序号", "参考编号", "总重", "收件人姓名", "收件人电话", "收件人地址 1",
               "收件人城市", "收件人省份", "邮编", "货物名称 1", "税则号 1",
               "单价 1", "件数 1", "单个重量 1", "货物名称 2", "税则号 2",
               "单价 2", "件数 2", "单个重量 2", "货物名称 3", "税则号 3",
               "单价 3", "件数 3", "单个重量 3", "申报价值", "申报货币", "代付税金", "提单日期"
            };

            //判断字段是否都存在于推送的datatable中
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
                fieldMsg = "推送数据不完整，以下数据不存在：<br/>" + fieldStr.TrimEnd(',') + "<br/><br/>推送失败！";
            }

            return result;
        }

        /// <summary>
        /// 导入数据验证 如必填、数字有效性，是否违禁物品
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="dt_prohibited">违禁物品表</param>
        /// <returns></returns>
        private string EmportRowValidData_New(DataRow row, DataTable dt_prohibited)
        {
            bool flag = true;
            string valueEmpty = string.Empty,
                   valueDecError = string.Empty,
                   valueIntError = string.Empty,
                   valueProError = string.Empty;
            string validMsg = "参考编号：" + row["参考编号"].ToString().Trim();

            //必填项验证
            foreach (string col in requireArray_New)
            {
                if (string.IsNullOrWhiteSpace(row[col].ToString()))
                {
                    valueEmpty += col + "，";
                }
            }
            //若 貨物名稱 2 有内容, 则 件數 2 必填
            if ((!string.IsNullOrWhiteSpace(row["货物名称 2"].ToString())
                && string.IsNullOrWhiteSpace(row["件数 2"].ToString()))
                || (!string.IsNullOrWhiteSpace(row["货物名称 2"].ToString())
                && !string.IsNullOrWhiteSpace(row["件数 2"].ToString())
                && Regex.IsMatch(row["件数 2"].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$")
                && Convert.ToInt32(row["件数 2"].ToString().Trim()) == 0))
            {
                valueEmpty += "件数 2，";
            }
            //若 貨物名稱 3 有内容, 则 单件件数③ 必填
            if ((!string.IsNullOrWhiteSpace(row["货物名称 3"].ToString())
                && string.IsNullOrWhiteSpace(row["件数 3"].ToString()))
                || (!string.IsNullOrWhiteSpace(row["货物名称 3"].ToString())
                && !string.IsNullOrWhiteSpace(row["件数 3"].ToString())
                && Regex.IsMatch(row["件数 3"].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$")
                && Convert.ToInt32(row["件数 3"].ToString().Trim()) == 0))
            {
                valueEmpty += "件数 3，";
            }
            //若 物品名称③ 有内容, 则 物品名称② 必须要有内容
            if (!string.IsNullOrWhiteSpace(row["货物名称 3"].ToString()) && string.IsNullOrWhiteSpace(row["货物名称 2"].ToString()))
            {
                valueEmpty += "货物名称 2，";
            }
            valueEmpty = valueEmpty.TrimEnd('，');

            //价格、重量验证
            foreach (string col in decimalArray_New)
            {
                //不为空时, 正则匹配输入是否为合法的数字
                if (!string.IsNullOrWhiteSpace(row[col].ToString()) && !Regex.IsMatch(row[col].ToString().Trim(), @"^((0|[1-9]\d{0,20}(\.\d{1,2})?)|(0\.\d{1,2}))$"))
                {
                    valueDecError += col + "，";
                }
            }
            valueDecError = valueDecError.TrimEnd('，');

            //件数验证
            foreach (string col in integerArray_New)
            {
                //不为空时, 正则匹配输入是否为合法的数字
                if (!string.IsNullOrWhiteSpace(row[col].ToString()) && !Regex.IsMatch(row[col].ToString().Trim(), @"^(0|\+?[1-9][0-9]*)$"))
                {
                    valueIntError += col + "，";
                }
            }
            valueIntError = valueIntError.TrimEnd('，');

            //禁寄物品验证
            foreach (string col in prohibitedArray_New)
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
                validMsg += "<br/>&nbsp;&nbsp;字段：" + valueDecError + "格式错误，价格、重量只能是数字，并且最多包含两位小数";
            }
            if (!string.IsNullOrEmpty(valueIntError))
            {
                flag = false;
                validMsg += "<br/>&nbsp;&nbsp;字段：" + valueIntError + "格式错误，件數、代付税金只能输入整数";
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
        /// 生成运单数据  如重命名物品名称、关联地区信息、商品税则号、完税价格
        /// </summary>
        /// <param name="dt_import">导入数据</param>
        /// <param name="dt_taxnumber">商品税则号对应关系表</param>
        /// <param name="dt_relation">原物品对应关系</param>
        /// <param name="dt_area">地区信息</param>
        /// <returns></returns>
        private string GenerateWaybillData_New(DataTable dt_import, DataTable dt_taxnumber, DataTable dt_relation, DataTable dt_area)
        {
            //需要关联税则号的列
            string[] relevanceArray = new string[] { "货物名称 1", "货物名称 2", "货物名称 3" };
            string[] taxArray = new string[] { "税则号 1", "税则号 2", "税则号 3" };
            string[] priceArray = new string[] { "单价 1", "单价 2", "单价 3" };
            string[] pieceNumArray = new string[] { "件数 1", "件数 2", "件数 3" };
            string[] pieceWeightArray = new string[] { "单个重量 1", "单个重量 2", "单个重量 3" };

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
                                      where p.Field<string>("areacity").Contains(row["收件人城市"].ToString().Trim())
                                      select p).ToArray();
                if (areaRows.Count() > 0)
                {
                    row["收件人城市"] = areaRows[0]["areacity"].ToString();
                    row["收件人省份"] = areaRows[0]["areaprovince"].ToString();
                    row["邮编"] = areaRows[0]["areapostcode"].ToString();
                }

                //重命名物品名称、关联税关号、匹配完税价格
                flag = true;
                validMsg = "参考编号：" + row["参考编号"].ToString() + "<br/>&nbsp;&nbsp;";

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
                    validMsg += "，税则号未添加";

                    emptySb.Append(validMsg);
                    emptySb.Append("<br/>");
                }
            }

            return emptySb.ToString();
        }

        #endregion

        /// <summary>
        /// 导出方法
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExportExpress(FormCollection fc)
        {
            try
            {
                var username = fc["name"];
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Json(new { Success = false, Msg = "用户名称不能为空" });
                }
                if (!CheckUserName(username))
                {
                    return Json(new { Success = false, Msg = "用户名称验证不通过" });
                }
                #region 查询条件处理

                string searchWhere = "";
                searchWhere += " and exportbatch = ''";
                searchWhere += $" and importbatch like 'import_interface_" + username + "_%'";

                #endregion

                //获取导出数据
                DataTable dt_export = GetExportData(searchWhere);
                if (dt_export != null && dt_export.Rows.Count > 0)
                {
                    //记录导出数据记录的主键ID
                    string oidStr = string.Empty;
                    foreach (DataRow row in dt_export.Rows)
                    {
                        oidStr += row["oid"].ToString() + ",";
                    }
                    oidStr = oidStr.TrimEnd(',');

                    //删除oid, tax, exportbatch列, 不需要导出
                    //dt_export.Columns.Remove("warehousingno");
                    dt_export.Columns.Remove("tax");
                    dt_export.Columns.Remove("importbatch");
                    dt_export.Columns.Remove("exportbatch");
                    dt_export.Columns.Remove("Insured");
                    dt_export.Columns.Remove("Destination");
                    dt_export.Columns.Remove("DestinationPoint");
                    dt_export.Columns.Remove("SendPhone");
                    dt_export.Columns.Remove("SendAddress");
                    dt_export.Columns.Remove("Freight");
                    dt_export.Columns.Remove("CustomerQuotation");
                    dt_export.Columns.Remove("PhoneCount");
                    dt_export.Columns.Remove("sender");

                    //需要清空零值的字段集合
                    List<string> zeroColumns = new List<string>() { "price1", "piecenum1", "pieceweight1", "price2", "piecenum2", "pieceweight2",
                    "price3", "piecenum3", "pieceweight3" };

                    //处理double类型值的显示格式
                    dt_export = ComHelper.ConvertDataTableToString(dt_export, zeroColumns);

                    //重命名列名
                    DataTableColumnRename_New(dt_export);

                    var array = new string[] { "序号", "参考编号", "e特快单号", "收件人姓名", "收件人电话", "收件人地址 1", "收件人城市", "收件人省份", "邮编", "货物名称 1", "税则号 1", "单价 1", "件数 1", "单个重量 1", "货物名称 2", "税则号 2", "单价 2", "件数 2", "单个重量 2", "货物名称 3", "税则号 3", "单价 3", "件数 3", "单个重量 3", "申报货币", "申报价值", "总重", "运单日期", "代付税金", "入仓号" };
                    var newRow = dt_export.NewRow();
                    for (int i = 0; i < array.Length; i++)
                    {
                        newRow[i] = array[i];
                    }
                    dt_export.Rows.InsertAt(newRow, 0);

                    //更新运单表的数据的导出批次
                    string exportBatchName = "export_interface_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    string strSql = string.Format(@"update waybill set exportbatch = '{0}' where oid in ({1})", exportBatchName, oidStr);
                    SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, strSql, null);

                    JsonSerializerSettings setting = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    var ret = JsonConvert.SerializeObject(dt_export, setting);
                    
                    return Json(new { Success = true, Msg = "导出成功", data = ret });
                }
                else
                {
                    return Json(new { Success = true, Msg = "无数据", data = "" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Msg = ex.Message, data = "" });
            }
        }

        /// <summary>
        /// 获取导出数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private DataTable GetExportData(string searchtext)
        {
            //查询SQL
            string strSql = string.Format(@"select oid,waybillnumber,singlechannel,recipient,recphone,recaddress,reccity,
                        recprovince,recpostcode,goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,
                        goodsname3,customsno3,price3,piecenum3,pieceweight3,declaredcurrency,declaredvalue,settlementweight,typingtype,ispayduty,insured,destination,destinationpoint,
                        sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch,sender,warehousingno from waybill 
                        where 1=1 {0} order by created_time desc", searchtext);

            //获取数据集
            DataSet ds = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            return ds.Tables[0];
        }

        /// <summary>
        /// 导出新列
        /// </summary>
        /// <param name="dt_export"></param>
        private void DataTableColumnRename_New(DataTable dt_export)
        {
            dt_export.Columns["oid"].ColumnName = "S.No";
            dt_export.Columns["warehousingno"].ColumnName = "batch_no";
            dt_export.Columns["waybillnumber"].ColumnName = "customer_hawb";
            dt_export.Columns["settlementweight"].ColumnName = "weight";
            dt_export.Columns["singlechannel"].ColumnName = "e Express no#";
            dt_export.Columns["recipient"].ColumnName = "receiver_name";
            dt_export.Columns["recphone"].ColumnName = "receiver_phone";
            dt_export.Columns["recaddress"].ColumnName = "receiver_address1";
            dt_export.Columns["reccity"].ColumnName = "receiver_city";
            dt_export.Columns["recprovince"].ColumnName = "receiver_province";
            dt_export.Columns["recpostcode"].ColumnName = "receiver_Zip";
            dt_export.Columns["goodsname1"].ColumnName = "s_content1";
            dt_export.Columns["customsno1"].ColumnName = "Tax_code1";
            dt_export.Columns["price1"].ColumnName = "s_price1";
            dt_export.Columns["piecenum1"].ColumnName = "s_pieces1";
            dt_export.Columns["pieceweight1"].ColumnName = "s_weight1";
            dt_export.Columns["goodsname2"].ColumnName = "s_content2";
            dt_export.Columns["customsno2"].ColumnName = "Tax_code2";
            dt_export.Columns["price2"].ColumnName = "s_price2";
            dt_export.Columns["piecenum2"].ColumnName = "s_pieces2";
            dt_export.Columns["pieceweight2"].ColumnName = "s_weight2";
            dt_export.Columns["goodsname3"].ColumnName = "s_content3";
            dt_export.Columns["customsno3"].ColumnName = "Tax_code3";
            dt_export.Columns["price3"].ColumnName = "s_price3";
            dt_export.Columns["piecenum3"].ColumnName = "s_pieces3";
            dt_export.Columns["pieceweight3"].ColumnName = "s_weight3";
            dt_export.Columns["declaredvalue"].ColumnName = "declare_value";
            dt_export.Columns["declaredcurrency"].ColumnName = "declare_currency";
            dt_export.Columns["ispayduty"].ColumnName = "duty_paid";
            dt_export.Columns["typingtype"].ColumnName = "shipment_date";
            //dt_export.Columns["sender"].ColumnName = "member_name";
        }
    }
}