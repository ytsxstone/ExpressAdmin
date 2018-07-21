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
    /// 禁运物品管理Dal类
    /// </summary>
    public class DalProhibited
    {
        /// <summary>
        /// 获取禁运信息数据
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="type"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public DataTable GetProhibitedData(string pname, int type, int pageSize, int pageIndex, string sortColumn, string sortType, ref int total)
        {
            var sqlCount = string.Format(@"select count(1) from prohibitedgood where 1=1");
            var sql = string.Format(@"select pid, pname, premark, type, created, created_time, 
                    updated, updated_time from prohibitedgood where 1=1");

            if (!string.IsNullOrWhiteSpace(pname))
            {
                sql += string.Format(@" and pname='{0}' ", pname);
                sqlCount += string.Format(@" and pname='{0}' ", pname);
            }
            if (type > 0)
            {
                sql += string.Format(@" and type='{0}'", type);
                sqlCount += string.Format(@" and type='{0}'", type);
            }

            sql += string.Format(@" order by {2} {3} offset {0}*{1} rows fetch next {0} rows only", pageSize, pageIndex - 1, sortColumn, sortType);

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取导出禁运信息数据
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="type"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public DataTable GetProhibitedExportData(string pname, int type, string sortColumn, string sortType)
        {
            var sql = string.Format(@"select pname, premark, (case type when 1 then '禁运物品' when 2 then '收件人地址' when 3 then '收件人名称' when 4 then '收件人电话' else '' end) as type from prohibitedgood where 1=1");

            if (!string.IsNullOrWhiteSpace(pname))
            {
                sql += string.Format(@" and pname='{0}' ", pname);
            }
            if (type > 0)
            {
                sql += string.Format(@" and type='{0}'", type);
            }

            sql += string.Format(@" order by {0} {1}", sortColumn, sortType);

            //返回结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入禁运信息
        /// </summary>
        public int Create(string pname, string premark, int type, string created)
        {
            var sql = $@"insert into prohibitedgood(pname,premark,type,created,created_time) values('{pname}','{premark}',{type},'{created}',getdate())";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 修改禁运信息
        /// </summary>
        public int Update(int pid, string pname, string premark, int type, string updated)
        {
            var sql = $@"update prohibitedgood set pname='{pname}',premark='{premark}',type={type},updated='{updated}',updated_time=getdate() where pid={pid}";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 删除禁运信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int Delete(string ids)
        {
            var sql = $"delete from prohibitedgood where pid in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 判断名称是否已存在
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pType"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        public bool GetProhibitedNameIsExists(int pId, int pType, string pName)
        {
            string sql = "";

            if (pId > 0)
                sql = $"select pid,type,pname from prohibitedgood where type={pType} and pname = '{pName}' and rid <> {pId}";
            else
                sql = $"select pid,type,pname from prohibitedgood where type={pType} and pname = '{pName}'";

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
        /// <param name="listData">导入数据集</param>
        /// <param name="employeeAccount">导入员工帐号</param>
        public void BulkEmport(List<ModProhibited> listData, string employeeAccount)
        {
            //插入到数据库 -- 启用SQLite事务
            using (SqlConnection conn = new SqlConnection(SQLHelper.defConnStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    //循环插入 or 更新
                    foreach (ModProhibited model in listData)
                    {
                        //判断记录是否已存在, 存在则更新, 否则插入
                        int pID = EmportRowIsRepeat(model, transaction);

                        if (pID > 0)
                        {
                            model.Pid = pID;
                            model.Updated = employeeAccount;

                            //更新到数据库
                            EmportRowUpdate(model, transaction);
                        }
                        else
                        {
                            model.Created = employeeAccount;

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
        /// 判断数据库中禁运物品名称是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModProhibited model)
        {
            //不为空时, 禁运物品名称 数据库是否已存在
            string strSql = string.Format("select top 1 pid from prohibitedgood where type = {0} and pname = '{1}'", 
                model.Type, model.PName.Trim());

            object result = SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            //返回对应记录ID
            if (null == result)
                return 0;
            else
                return Convert.ToInt32(result);
        }

        /// <summary>
        /// 判断数据库中禁运物品名称是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModProhibited model, SqlTransaction transaction)
        {
            //不为空时, 禁运物品名称 数据库是否已存在
            string strSql = string.Format("select top 1 pid from prohibitedgood where type = {0} and pname = '{1}'",
                model.Type, model.PName.Trim());

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
        private void EmportRowInsert(ModProhibited model)
        {
            //insert语句
            string sql = @"insert into prohibitedgood(type, pname, premark, created, created_time) 
                            values(@type, @pname, @premark, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@type", model.Type),
                new SqlParameter("@pname", model.PName),
                new SqlParameter("@premark", model.PRemark),
                new SqlParameter("@created", model.Created)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据插入到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowInsert(ModProhibited model, SqlTransaction transaction)
        {
            //insert语句
            string sql = @"insert into prohibitedgood(type, pname, premark, created, created_time) 
                            values(@type, @pname, @premark, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@type", model.Type),
                new SqlParameter("@pname", model.PName),
                new SqlParameter("@premark", model.PRemark),
                new SqlParameter("@created", model.Created)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowUpdate(ModProhibited model)
        {
            //update语句
            string sql = @"update prohibitedgood set type = @type, pname = @pname, premark = @premark,
                    updated = @updated, updated_time = getdate() where pid = @pid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@type", model.Type),
                new SqlParameter("@pname", model.PName),
                new SqlParameter("@premark", model.PRemark),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@pid", model.Pid)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowUpdate(ModProhibited model, SqlTransaction transaction)
        {
            //update语句
            string sql = @"update prohibitedgood set type = @type, pname = @pname, premark = @premark,
                    updated = @updated, updated_time = getdate() where pid = @pid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@type", model.Type),
                new SqlParameter("@pname", model.PName),
                new SqlParameter("@premark", model.PRemark),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@pid", model.Pid)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }
    }
}
