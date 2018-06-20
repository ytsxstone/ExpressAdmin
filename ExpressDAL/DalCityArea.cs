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
    /// 地区管理Dal类
    /// </summary>
    public class DalCityArea
    {
        /// <summary>
        /// 获取地区信息数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public DataTable GetCityAreaData(string name, int pageSize, int pageIndex, string sortColumn, string sortType, ref int total)
        {
            var sqlCount = string.Format(@"select count(1) from cityarea where 1=1");
            var sql = string.Format(@"select areaid, areaprovince, areacity, areapostcode, created, created_time, 
                    updated, updated_time from cityarea where 1=1");

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += string.Format(@" and areaprovince like '%{0}%' or areacity like '%{0}%' or areapostcode like '%{0}%'", name);
                sqlCount += string.Format(@" and areaprovince like '%{0}%' or areacity like '%{0}%' or areapostcode like '%{0}%'", name);
            }

            sql += string.Format(@" order by {2} {3} offset {0}*{1} rows fetch next {0} rows only", pageSize, pageIndex - 1, sortColumn, sortType);

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取导出地区信息数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public DataTable GetCityAreaExportData(string name, string sortColumn, string sortType)
        {
            var sql = string.Format(@"select areaprovince, areacity, areapostcode from cityarea where 1=1");

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += string.Format(@" and areaprovince like '%{0}%' or areacity like '%{0}%' or areapostcode like '%{0}%'", name);
            }

            sql += string.Format(@" order by {0} {1}", sortColumn, sortType);

            //返回结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入地区信息
        /// </summary>
        public int Create(string province, string city, string postcode, string creator)
        {
            //插入到数据库
            string sql = $@"insert into cityarea(areaprovince, areacity, areapostcode, created, created_time) 
                                values(@province, @city, @postcode, @creator, getdate())";
            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@province", province),
                new SqlParameter("@city", city),
                new SqlParameter("@postcode", postcode),
                new SqlParameter("@creator", creator)
            };

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 更新地区信息
        /// </summary>
        public int Update(int id, string province, string city, string postcode, string modifier)
        {
            //更新到数据库
            string sql = $@"update cityarea set areaprovince = @province, areacity = @city, areapostcode = @postcode,
                                updated = @updated, updated_time = getdate() 
                            where areaid = @areaid";
            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@province", province),
                new SqlParameter("@city", city),
                new SqlParameter("@postcode", postcode),
                new SqlParameter("@updated", modifier),
                new SqlParameter("@areaid", id),
            };

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 删除地区信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(string ids)
        {
            var sql = $@"delete from cityarea where areaid in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 判断省份+城市组合是否已存在
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public bool GetCityAreaIsExists(int areaId, string province, string city)
        {
            string sql = "";

            if (areaId > 0)
                sql = $"select areaid, areaprovince, areacity from cityarea where areaprovince='{province}' and areacity='{city}' and areaid <> {areaId}";
            else
                sql = $"select areaid, areaprovince, areacity from cityarea where areaprovince='{province}' and areacity='{city}'";

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
        /// 根据省份+城市获取记录
        /// </summary>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public DataTable GetCityArea(string province, string city)
        {
            string sql = $"select areaprovince, areacity from cityarea where areaprovince='{province}' and areacity='{city}'";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取省份数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetAreaProvinceData()
        {
            string sql = "select areaprovince from cityarea group by areaprovince order by areaprovince";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 获取城市数据
        /// </summary>
        /// <param name="areaProvince"></param>
        /// <returns></returns>
        public DataTable GetAreaCityData(string areaProvince)
        {
            string sql = string.Format("select areacity from cityarea where areaprovince = '{0}' group by areacity order by areacity", areaProvince);

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 批量导入
        /// </summary>
        /// <param name="listData">导入数据集</param>
        /// <param name="employeeAccount">导入员工帐号</param>
        public void BulkEmport(List<ModCityArea> listData, string employeeAccount)
        {
            //插入到数据库 -- 启用SQLite事务
            using (SqlConnection conn = new SqlConnection(SQLHelper.defConnStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    //循环插入 or 更新
                    foreach (ModCityArea model in listData)
                    {
                        //判断记录是否已存在, 存在则更新, 否则插入
                        int resID = EmportRowIsRepeat(model, transaction);

                        if (resID > 0)
                        {
                            model.AreaID = resID;
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
        /// 判断数据库中省份+城市是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModCityArea model)
        {
            //不为空时, 城市 数据库是否已存在
            string strSql = string.Format("select top 1 areaid from cityarea where areaprovince = '{0}' and areacity = '{1}' order by areaid", 
                model.AreaProvince.Trim(), model.AreaCity.Trim());

            object result = SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, strSql, null);

            //返回对应记录ID
            if (null == result)
                return 0;
            else
                return Convert.ToInt32(result);
        }

        /// <summary>
        /// 判断数据库中省份+城市是否已存在
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int EmportRowIsRepeat(ModCityArea model, SqlTransaction transaction)
        {
            //不为空时, 城市 数据库是否已存在
            string strSql = string.Format("select top 1 areaid from cityarea where areaprovince = '{0}' and areacity = '{1}' order by areaid",
                model.AreaProvince.Trim(), model.AreaCity.Trim());

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
        private void EmportRowInsert(ModCityArea model)
        {
            //insert语句
            string sql = @"insert into cityarea(areaprovince, areacity, areapostcode, created, created_time) 
                                values(@areaprovince, @areacity, @areapostcode, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@areaprovince", model.AreaProvince),
                new SqlParameter("@areacity", model.AreaCity),
                new SqlParameter("@areapostcode", model.AreaPostcode),
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
        private void EmportRowInsert(ModCityArea model, SqlTransaction transaction)
        {
            //insert语句
            string sql = @"insert into cityarea(areaprovince, areacity, areapostcode, created, created_time) 
                                values(@areaprovince, @areacity, @areapostcode, @created, getdate())";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@areaprovince", model.AreaProvince),
                new SqlParameter("@areacity", model.AreaCity),
                new SqlParameter("@areapostcode", model.AreaPostcode),
                new SqlParameter("@created", model.Created)
            };

            //执行插入操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        private void EmportRowUpdate(ModCityArea model)
        {
            //update语句
            string sql = @"update cityarea set areaprovince = @areaprovince, areacity = @areacity, areapostcode = @areapostcode,
                                    updated = @updated, updated_time = getdate() where areaid = @areaid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@areaprovince", model.AreaProvince),
                new SqlParameter("@areacity", model.AreaCity),
                new SqlParameter("@areapostcode", model.AreaPostcode),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@areaid", model.AreaID)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 导入行数据更新到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        private void EmportRowUpdate(ModCityArea model, SqlTransaction transaction)
        {
            //update语句
            string sql = @"update cityarea set areaprovince = @areaprovince, areacity = @areacity, areapostcode = @areapostcode,
                                    updated = @updated, updated_time = getdate() where areaid = @areaid";

            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@areaprovince", model.AreaProvince),
                new SqlParameter("@areacity", model.AreaCity),
                new SqlParameter("@areapostcode", model.AreaPostcode),
                new SqlParameter("@updated", model.Updated),
                new SqlParameter("@areaid", model.AreaID)
            };

            //执行更新操作
            SQLHelper.ExecuteNonQuery(transaction, CommandType.Text, sql, _params);
        }
    }
}
