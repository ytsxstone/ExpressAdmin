using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using ExpressModel;
using ExpressCommon;

namespace ExpressDAL
{
    /// <summary>
    /// 运单信息
    /// </summary>
    public class DalWayBill
    {
        /// <summary>
        /// 获取运单信息数据
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <param name="importBatch"></param>
        /// <param name="exportBatch"></param>
        /// <param name="searchText"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public DataTable GetWayBillData(string loginAccount, string importBatch, string exportBatch, string searchText, int pageSize, int pageIndex, string sortColumn, string sortType, ref int total)
        {
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

            string sqlCount = $@"select count(1) from waybill where 1=1 {searchWhere}";

            string sql = $@"select oid,waybillnumber,warehousingno,settlementweight,singlechannel,recipient,recphone,recaddress,reccity,
                                recprovince,recpostcode,goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,
                                goodsname3,customsno3,price3,piecenum3,pieceweight3,declaredvalue,declaredcurrency,ispayduty,insured,typingtype,destination,destinationpoint,
                                sender,sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch from waybill 
                                where 1=1 {searchWhere} order by {sortColumn} {sortType} offset {pageSize}*{pageIndex - 1} rows fetch next {pageSize} rows only";

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 新增运单信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Create(ModWayBill model)
        {
            var sql = @"insert into waybill(warehousingno,waybillnumber,settlementweight,singlechannel,recipient,recphone,recaddress,reccity,recprovince,recpostcode,
                        goodsname1,customsno1,price1,piecenum1,pieceweight1,goodsname2,customsno2,price2,piecenum2,pieceweight2,goodsname3,customsno3,price3,piecenum3,pieceweight3,
                        declaredvalue,declaredcurrency,ispayduty,insured,typingtype,destination,destinationpoint,sender,sendphone,sendaddress,freight,customerquotation,tax,phonecount,importbatch,exportbatch, created, created_time) 
                    values(@warehousingno,@waybillnumber,@settlementweight,@singlechannel,@recipient,@recphone,@recaddress,@reccity,@recprovince,@recpostcode,
                            @goodsname1,@customsno1,@price1,@piecenum1,@pieceweight1,@goodsname2,@customsno2,@price2,@piecenum2,@pieceweight2,@goodsname3,@customsno3,@price3,@piecenum3,@pieceweight3,
                            @declaredvalue,@declaredcurrency,@ispayduty,@insured,@typingtype,@destination,@destinationpoint,@sender,@sendphone,@sendaddress,@freight,@customerquotation,@tax,@phonecount,@importbatch,@exportbatch, @created, getdate())";

            //参数数组
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

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 修改运单信息
        /// </summary>
        public int Update(ModWayBill model)
        {
            //更新到数据库
            StringBuilder sbSql = new StringBuilder();

            sbSql.Append("update waybill set ");
            sbSql.Append("warehousingno=@warehousingno,");
            sbSql.Append("waybillnumber=@waybillnumber,");
            sbSql.Append("settlementweight=@settlementweight,");
            sbSql.Append("singlechannel=@singlechannel,");
            sbSql.Append("recipient=@recipient,");
            sbSql.Append("recphone=@recphone,");
            sbSql.Append("recaddress=@recaddress,");
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
            sbSql.Append("declaredcurrency=@declaredcurrency,");
            sbSql.Append("ispayduty=@ispayduty,");
            sbSql.Append("insured=@insured,");
            sbSql.Append("typingtype=@typingtype,");
            sbSql.Append("destination=@destination,");
            sbSql.Append("destinationpoint=@destinationpoint,");
            sbSql.Append("sender=@sender,");
            sbSql.Append("sendphone=@sendphone,");
            sbSql.Append("sendaddress=@sendaddress,");
            sbSql.Append("freight=@freight,");
            sbSql.Append("customerquotation=@customerquotation,");
            sbSql.Append("tax=@tax,");
            sbSql.Append("phonecount=@phonecount,");
            sbSql.Append("importbatch=@importbatch");
            sbSql.Append("exportbatch=@exportbatch");
            sbSql.Append("updated=@updated,");
            sbSql.Append("updated_time=getdate()");
            sbSql.Append(" where oid=@oid");

            //参数数组
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
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@oid", model.Oid)
            };

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sbSql.ToString(), _params);
        }

        /// <summary>
        /// 删除运单信息
        /// </summary>
        public int Delete(string ids)
        {
            var sql = $"delete from waybill where oid in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 查询所有导入批次号
        /// </summary>
        /// <returns></returns>
        public DataTable GetImportBatch(string loginAccount)
        {
            string searchWhere = "";
            //帐号过滤
            if (!string.IsNullOrWhiteSpace(loginAccount))
            {
                searchWhere = $" and created = '{loginAccount}'";
            }

            var sql = $"select DISTINCT importbatch from waybill where importbatch <> '' {searchWhere}";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 查询所有导出批次号
        /// </summary>
        /// <returns></returns>
        public DataTable GetExportBatch(string loginAccount)
        {
            string searchWhere = "";
            //帐号过滤
            if (!string.IsNullOrWhiteSpace(loginAccount))
            {
                searchWhere = $" and created = '{loginAccount}'";
            }

            var sql = $"select DISTINCT exportbatch from waybill where exportbatch <> '' {searchWhere}";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }
    }
}
