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
    /// 关系管理
    /// </summary>
    public class DalRelation
    {
        /// <summary>
        /// 获取物品对应关系数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public DataTable GetGoodRelationData(ModGoodRelation model, int pageSize, int pageIndex, string sortColumn, string sortType, ref int total)
        {
            var sqlCount = $@"select count(1) from goodrelation where 1=1";
            var sql = $@"select rid, originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, newname8, newname9, 
                    newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, newname18, newname19, newname20, 
                    created, created_time, updated, updated_time from goodrelation where 1=1";

            if (!string.IsNullOrWhiteSpace(model.OriginalName))
            {
                sql += $@" and originalname='{model.OriginalName}' ";
                sqlCount += $@" and originalname='{model.OriginalName}' ";
            }
            if (!string.IsNullOrWhiteSpace(model.TaxNumber))
            {
                sql += $@" and taxnumber='{model.TaxNumber}' ";
                sqlCount += $@" and axnumber='{model.TaxNumber}' ";
            }
            if (!string.IsNullOrWhiteSpace(model.NewName1))
            {
                sql += $@" and newname1='{model.NewName1}' ";
                sqlCount += $@" and newname1='{model.NewName1}' ";
            }

            sql += string.Format(@" order by {2} {3} offset {0}*{1} rows fetch next {0} rows only", pageSize, pageIndex - 1, sortColumn, sortType);

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取导出物品对应关系数据
        /// </summary>
        /// <param name="originalname"></param>
        /// <param name="taxnumber"></param>
        /// <param name="newname1"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public DataTable GetGoodRelationExportData(string originalname, string taxnumber, string newname1, string sortColumn, string sortType)
        {
            var sql = $@"select originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, newname8, newname9, 
                    newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, newname18, newname19, 
                    newname20 from goodrelation where 1=1";

            if (!string.IsNullOrWhiteSpace(originalname))
            {
                sql += $@" and originalname='{originalname}' ";
            }
            if (!string.IsNullOrWhiteSpace(taxnumber))
            {
                sql += $@" and taxnumber='{taxnumber}' ";
            }
            if (!string.IsNullOrWhiteSpace(newname1))
            {
                sql += $@" and newname1='{newname1}' ";
            }

            sql += string.Format(@" order by {0} {1}", sortColumn, sortType);

            //返回结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入商品关系
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Create(ModGoodRelation model)
        {
            //insert语句
            string sql = @"insert into goodrelation(originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, newname8, newname9, 
                        newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, newname18, newname19, newname20, created, created_time) 
                    values(@originalname, @taxnumber, @newname1, @newname2, @newname3, @newname4, @newname5, @newname6, @newname7, @newname8, @newname9, @newname10,
                        @newname11, @newname12, @newname13, @newname14, @newname15, @newname16, @newname17, @newname18, @newname19, @newname20, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
                new SqlParameter("@created", model.Created)
            };

            //执行插入操作
            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 修改商品关系
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(ModGoodRelation model)
        {
            //update语句
            string sql = @"update goodrelation set originalname = @originalname, taxnumber = @taxnumber,
                            newname1 = @newname1,newname2 = @newname2,newname3 = @newname3,newname4 = @newname4,newname5 = @newname5,
                            newname6 = @newname6,newname7 = @newname7,newname8 = @newname8,newname9 = @newname9,newname10 = @newname10,
                            newname11 = @newname11,newname12 = @newname12,newname13 = @newname13,newname14 = @newname14,newname15 = @newname15,
                            newname16 = @newname16,newname17 = @newname17,newname18 = @newname18,newname19 = @newname19,newname20 = @newname20,
                            updated = @updated, updated_time = getdate() where rid = @rid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@rid", model.Rid)
            };

            //执行更新操作
            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 删除商品关系
        /// </summary>
        public int Delete(string ids)
        {
            var sql = $"delete from goodrelation where rid in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 判断原物品名称是否已存在
        /// </summary>
        /// <param name="rId"></param>
        /// <param name="originalName"></param>
        /// <returns></returns>
        public bool GetOriginalNameIsExists(int rId, string originalName)
        {
            string sql = "";

            if (rId > 0)
                sql = $"select rid,originalname,taxnumber from goodrelation where originalname = '{originalName}' and rid <> {rId}";
            else
                sql = $"select rid,originalname,taxnumber from goodrelation where originalname = '{originalName}'";

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
        public void BulkEmport(List<ModGoodRelation> listData, string employeeAccount)
        {
            //插入到数据库 -- 启用SQLite事务
            using (SqlConnection conn = new SqlConnection(SQLHelper.defConnStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    //循环插入 or 更新
                    foreach (ModGoodRelation model in listData)
                    {
                        //判断记录是否已存在, 存在则更新, 否则插入
                        int rID = EmportRowIsRepeat(model, transaction);

                        if (rID > 0)
                        {
                            model.Rid = rID;
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
        /// 判断数据库中原物品名称是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModGoodRelation model)
        {
            //不为空时, 原物品名称 数据库是否已存在
            string strSql = string.Format("select top 1 rid from goodrelation where originalname = '{0}'", model.OriginalName.Trim());

            object result = SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            //返回对应记录ID
            if (null == result)
                return 0;
            else
                return Convert.ToInt32(result);
        }

        /// <summary>
        /// 判断数据库中原物品名称是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModGoodRelation model, SqlTransaction transaction)
        {
            //不为空时, 原物品名称 数据库是否已存在
            string strSql = string.Format("select top 1 rid from goodrelation where originalname = '{0}'", model.OriginalName.Trim());

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
        private void EmportRowInsert(ModGoodRelation model)
        {
            //insert语句
            string sql = @"insert into goodrelation(originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, newname8, newname9, 
                        newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, newname18, newname19, newname20, created, created_time) 
                    values(@originalname, @taxnumber, @newname1, @newname2, @newname3, @newname4, @newname5, @newname6, @newname7, @newname8, @newname9, @newname10,
                        @newname11, @newname12, @newname13, @newname14, @newname15, @newname16, @newname17, @newname18, @newname19, @newname20, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
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
        private void EmportRowInsert(ModGoodRelation model, SqlTransaction transaction)
        {
            //insert语句
            string sql = @"insert into goodrelation(originalname, taxnumber, newname1, newname2, newname3, newname4, newname5, newname6, newname7, newname8, newname9, 
                        newname10, newname11, newname12, newname13, newname14, newname15, newname16, newname17, newname18, newname19, newname20, created, created_time) 
                    values(@originalname, @taxnumber, @newname1, @newname2, @newname3, @newname4, @newname5, @newname6, @newname7, @newname8, @newname9, @newname10,
                        @newname11, @newname12, @newname13, @newname14, @newname15, @newname16, @newname17, @newname18, @newname19, @newname20, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
                new SqlParameter("@created", model.Created)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowUpdate(ModGoodRelation model)
        {
            //update语句
            string sql = @"update goodrelation set originalname = @originalname, taxnumber = @taxnumber,
                            newname1 = @newname1,newname2 = @newname2,newname3 = @newname3,newname4 = @newname4,newname5 = @newname5,
                            newname6 = @newname6,newname7 = @newname7,newname8 = @newname8,newname9 = @newname9,newname10 = @newname10,
                            newname11 = @newname11,newname12 = @newname12,newname13 = @newname13,newname14 = @newname14,newname15 = @newname15,
                            newname16 = @newname16,newname17 = @newname17,newname18 = @newname18,newname19 = @newname19,newname20 = @newname20,
                            updated = @updated, updated_time = getdate() where rid = @rid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@rid", model.Rid)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowUpdate(ModGoodRelation model, SqlTransaction transaction)
        {
            //update语句
            string sql = @"update goodrelation set originalname = @originalname, taxnumber = @taxnumber,
                            newname1 = @newname1,newname2 = @newname2,newname3 = @newname3,newname4 = @newname4,newname5 = @newname5,
                            newname6 = @newname6,newname7 = @newname7,newname8 = @newname8,newname9 = @newname9,newname10 = @newname10,
                            newname11 = @newname11,newname12 = @newname12,newname13 = @newname13,newname14 = @newname14,newname15 = @newname15,
                            newname16 = @newname16,newname17 = @newname17,newname18 = @newname18,newname19 = @newname19,newname20 = @newname20,
                            updated = @updated, updated_time = getdate() where rid = @rid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@originalname", model.OriginalName),
                new SqlParameter("@taxnumber", model.TaxNumber),
                new SqlParameter("@newname1", model.NewName1),
                new SqlParameter("@newname2", model.NewName2),
                new SqlParameter("@newname3", model.NewName3),
                new SqlParameter("@newname4", model.NewName4),
                new SqlParameter("@newname5", model.NewName5),
                new SqlParameter("@newname6", model.NewName6),
                new SqlParameter("@newname7", model.NewName7),
                new SqlParameter("@newname8", model.NewName8),
                new SqlParameter("@newname9", model.NewName9),
                new SqlParameter("@newname10", model.NewName10),
                new SqlParameter("@newname11", model.NewName11),
                new SqlParameter("@newname12", model.NewName12),
                new SqlParameter("@newname13", model.NewName13),
                new SqlParameter("@newname14", model.NewName14),
                new SqlParameter("@newname15", model.NewName15),
                new SqlParameter("@newname16", model.NewName16),
                new SqlParameter("@newname17", model.NewName17),
                new SqlParameter("@newname18", model.NewName18),
                new SqlParameter("@newname19", model.NewName19),
                new SqlParameter("@newname20", model.NewName20),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@rid", model.Rid)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }
    }
}
