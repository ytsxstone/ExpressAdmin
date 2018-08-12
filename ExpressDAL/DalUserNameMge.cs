using ExpressCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressDAL
{
    /// <summary>
    /// 用户名管理
    /// </summary>
    public class DalUserNameMge
    {
        /// <summary>
        /// 获取用户名数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public DataTable GetUserNameManageData(string name, int pageSize, int pageIndex, ref int total)
        {
            var sqlCount = string.Format(@"select count(1) from user_name_management where 1=1");
            var sql = string.Format(@"select * from user_name_management where 1=1");

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += string.Format(@" and username like '%{0}%' ", name);
                sqlCount += string.Format(@" and username like '%{0}%'", name);
            }

            sql += string.Format(@" order by created_time desc offset {0}*{1} rows fetch next {0} rows only", pageSize, pageIndex - 1);

            //查询总数
            int.TryParse(SQLHelper.ExecuteScalar(SQLHelper.defConnStr, CommandType.Text, sqlCount, null).ToString(), out total);

            //返回分页结果集
            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入地区信息
        /// </summary>
        public int Create(string username,string created)
        {
            //插入到数据库
            string sql = $@"insert into user_name_management(username,created, created_time) 
                                values(@username,@created, getdate())";
            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@username", username),
                new SqlParameter("@created", created)
            };

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, _params);
        }

        /// <summary>
        /// 更新地区信息
        /// </summary>
        public int Update(int id, string username, string modifier)
        {
            //更新到数据库
            string sql = $@"update user_name_management set username = @username,
                                updated = @updated, updated_time = getdate() 
                            where id = @id";
            //参数数组
            SqlParameter[] _params = new SqlParameter[] {
                new SqlParameter("@username", username),
                new SqlParameter("@updated", modifier),
                new SqlParameter("@id", id),
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
            var sql = $@"delete from user_name_management where id in ({ids})";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 判断省份+城市组合是否已存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool GetUserNameManagementIsExists(int id, string username)
        {
            string sql = "";

            if (id > 0)
                sql = $"select * from user_name_management where username='{username}' and id <> {id}";
            else
                sql = $"select * from user_name_management where username='{username}'";

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
    }
}
