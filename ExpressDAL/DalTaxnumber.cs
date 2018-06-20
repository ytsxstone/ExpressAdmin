using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using ExpressModel;
using ExpressCommon;

namespace ExpressDAL
{
    /// <summary>
    /// 商品税号管理
    /// </summary>
    public class DalTaxNumber
    {
        /// <summary>
        /// 获取商品税号信息数据
        /// </summary>
        public DataTable GetGoodTaxNumberData(string ptaxnumber, string price, string rate, int pageSize, int pageIndex, string sortColumn, string sortType, ref int total)
        {
            var sqlCount = string.Format(@"select count(1) from goodtaxnumber where 1=1");
            var sql = string.Format(@"select pid, ptaxnumber, ptaxprice, ptaxrate from goodtaxnumber where 1=1");

            if (!string.IsNullOrWhiteSpace(ptaxnumber))
            {
                sql += string.Format(@" and ptaxnumber='{0}'", ptaxnumber);
                sqlCount += string.Format(@" and ptaxnumber='{0}'", ptaxnumber);
            }
            if (!string.IsNullOrWhiteSpace(price))
            {
                sql += string.Format(@" and ptaxprice='{0}'", price);
                sqlCount += string.Format(@" and ptaxprice='{0}'", price);
            }
            if (!string.IsNullOrWhiteSpace(rate))
            {
                sql += string.Format(@" and ptaxrate='{0}'", rate);
                sqlCount += string.Format(@" and ptaxrate='{0}'", rate);
            }

            sql += string.Format(@" order by {2} {3} offset {0}*{1} rows fetch next {0} rows only", pageSize, pageIndex - 1, sortColumn, sortType);

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取导出商品税号信息数据
        /// </summary>
        public DataTable GetGoodTaxNumberExportData(string ptaxnumber, string price, string rate, string sortColumn, string sortType)
        {
            var sql = string.Format(@"select ptaxnumber, ptaxprice, ptaxrate from goodtaxnumber where 1=1");

            if (!string.IsNullOrWhiteSpace(ptaxnumber))
            {
                sql += string.Format(@" and ptaxnumber='{0}'", ptaxnumber);
            }
            if (!string.IsNullOrWhiteSpace(price))
            {
                sql += string.Format(@" and ptaxprice='{0}'", price);
            }
            if (!string.IsNullOrWhiteSpace(rate))
            {
                sql += string.Format(@" and ptaxrate='{0}'", rate);
            }

            sql += string.Format(@" order by {0} {1}", sortColumn, sortType);

            //返回结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入商品税号信息
        /// </summary>
        public int Create(string number, string price, string rate)
        {
            var sql = $@"insert into goodtaxnumber(ptaxnumber,ptaxprice,ptaxrate) values('{number}','{price}','{rate}')";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 修改商品税号信息
        /// </summary>
        public int Update(int pid, string number, string price, string rate)
        {
            var sql = $@"update goodtaxnumber set ptaxnumber='{number}',ptaxprice='{price}',ptaxrate='{rate}' where pid={pid}";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 删除商品税号信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int Delete(string ids)
        {
            var sql = $"delete from goodtaxnumber where pid in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 判断税号是否已存在
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="taxNumber"></param>
        /// <returns></returns>
        public bool GetTaxNumberIsExists(int pId, string taxNumber)
        {
            string sql = "";

            if (pId > 0)
                sql = $"select pid,ptaxnumber from goodtaxnumber where ptaxnumber = '{taxNumber}' and rid <> {pId}";
            else
                sql = $"select pid,ptaxnumber from goodtaxnumber where ptaxnumber = '{taxNumber}'";

            DataTable dt = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];

            if (dt != null && dt.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 批量导入
        /// </summary>
        /// <param name="listData"></param>
        public void BulkEmport(List<ModTaxnNumber> listData)
        {
            //插入到数据库 -- 启用SQLite事务
            using (SqlConnection conn = new SqlConnection(SQLHelper.defConnStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    //循环插入 or 更新
                    foreach (ModTaxnNumber model in listData)
                    {
                        //判断记录是否已存在, 存在则更新, 否则插入
                        int pID = EmportRowIsRepeat(model, transaction);

                        if (pID > 0)
                        {
                            model.Pid = pID;

                            //更新到数据库
                            EmportRowUpdate(model, transaction);
                        }
                        else
                        {
                            //插入到数据库
                            EmportRowInsert(model, transaction);
                        }
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
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 判断数据库中商品税号是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModTaxnNumber model)
        {
            //不为空时, 商品税号 数据库是否已存在
            string strSql = string.Format("select top 1 pid from goodtaxnumber where ptaxnumber = '{0}'", model.PTaxNumber.Trim());

            object result = SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            //返回对应记录ID
            if (null == result)
                return 0;
            else
                return Convert.ToInt32(result);
        }

        /// <summary>
        /// 判断数据库中商品税号是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModTaxnNumber model, SqlTransaction transaction)
        {
            //不为空时, 商品税号 数据库是否已存在
            string strSql = string.Format("select top 1 pid from goodtaxnumber where ptaxnumber = '{0}'", model.PTaxNumber.Trim());

            object result = SQLHelper.ExecuteScalar(transaction, CommandType.Text, strSql, null);

            //返回对应记录ID
            if (null == result)
                return 0;
            else
                return Convert.ToInt32(result);
        }

        /// <summary>
        /// 导入行数据插入到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowInsert(ModTaxnNumber model)
        {
            //insert语句
            string sql = @"insert into goodtaxnumber(ptaxnumber, ptaxprice, ptaxrate) values(@ptaxnumber, @ptaxprice, @ptaxrate)";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@ptaxnumber", model.PTaxNumber),
                new SqlParameter("@ptaxprice", model.PTaxPrice),
                new SqlParameter("@ptaxrate", model.PTaxRate)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据插入到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowInsert(ModTaxnNumber model, SqlTransaction transaction)
        {
            //insert语句
            string sql = @"insert into goodtaxnumber(ptaxnumber, ptaxprice, ptaxrate) values(@ptaxnumber, @ptaxprice, @ptaxrate)";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@ptaxnumber", model.PTaxNumber),
                new SqlParameter("@ptaxprice", model.PTaxPrice),
                new SqlParameter("@ptaxrate", model.PTaxRate)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowUpdate(ModTaxnNumber model)
        {
            //update语句
            string sql = @"update goodtaxnumber set ptaxnumber = @ptaxnumber, ptaxprice = @ptaxprice, ptaxrate = @ptaxrate 
                                    where pid = @pid";
            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@ptaxnumber", model.PTaxNumber),
                new SqlParameter("@ptaxprice", model.PTaxPrice),
                new SqlParameter("@ptaxrate", model.PTaxRate),
                new SqlParameter("@pid", model.Pid)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowUpdate(ModTaxnNumber model, SqlTransaction transaction)
        {
            //update语句
            string sql = @"update goodtaxnumber set ptaxnumber = @ptaxnumber, ptaxprice = @ptaxprice, ptaxrate = @ptaxrate 
                                    where pid = @pid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@ptaxnumber", model.PTaxNumber),
                new SqlParameter("@ptaxprice", model.PTaxPrice),
                new SqlParameter("@ptaxrate", model.PTaxRate),
                new SqlParameter("@pid", model.Pid)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }
    }
}
