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
    /// 员工Dal类
    /// </summary>
    public class DalEmployee
    {
        /// <summary>
        /// 获取员工信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public DataTable GetEmployee(string name, string sortColumn, string sortType)
        {
            var sql = string.Format("select id, employeeaccount, employeename, employeepwd, employeedesc from employee where 1=1");
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += string.Format(" and employeename='{0}'", name);
            }
            sql += string.Format(@" order by {0} {1}", sortColumn, sortType);

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 判断员工编号是否已存在
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool GetEmployeeAccountIsExists(int id, string code)
        {
            string sql = "";

            if (id > 0)
                sql = $"select id, employeeaccount, employeename from employee where employeeaccount = '{code}' and id <> {id}";
            else
                sql = $"select id, employeeaccount, employeename from employee where employeeaccount = '{code}'";

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
        /// 新增员工
        /// </summary>
        /// <param name="account"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public int Create(string account, string name, string pwd, string desc)
        {
            var sql = $@"insert into employee(employeeaccount, employeename, employeepwd, employeedesc) 
                values('{account}','{name}', '{pwd}', '{desc}')";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 修改员工
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public int Update(int id, string account, string name, string desc)
        {
            var sql = $@"update employee set employeeaccount='{account}', employeename='{name}',
                employeedesc='{desc}' where id={id}";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 删除员工信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(string ids)
        {
            var sql = $"delete from employee where id in ({ids})";
            var result = SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);

            sql = $"delete from authconfig where employeeid in ({ids})";
            result += SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);

            return result;
        }

        /// <summary>
        /// 检查原密码是否正确
        /// </summary>
        public bool CheckOldPwd(string pwd, int id)
        {
            var sql = string.Format("select * from employee where id={0} and employeepwd='{1}'", id, pwd);
            DataTable dt = SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];

            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 重置账号登录密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int UpdatePassword(int id, string password)
        {
            var sql = string.Format("update employee set employeepwd='{0}' where id={1}", password, id);

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 获取员工权限菜单配置
        /// </summary>
        /// <param name="eId"></param>
        /// <returns></returns>
        public DataTable GetEmployeeAuthConfig(string eId)
        {
            var sql = string.Format("select * from authconfig where employeeid={0}", eId);

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 修改员工权限菜单配置
        /// </summary>
        /// <param name="eId"></param>
        /// <param name="aId"></param>
        /// <returns></returns>
        public int UpdateEmployeeAuthConfig(string eId, List<string> aId)
        {
            var sql = string.Format("delete from authconfig where employeeid={0}", eId);
            var result = SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);

            foreach (var item in aId)
            {
                sql = string.Format(@"insert into authconfig(employeeid, menuid) values({0},{1})", eId, item);
                result += SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
            }

            return result;
        }

        /// <summary>
        /// 根据获取员工信息
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        public DataTable GetEmployeeById(string eid)
        {
            var sql = string.Format("select * from employee where id={0}", eid);

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }
    }
}