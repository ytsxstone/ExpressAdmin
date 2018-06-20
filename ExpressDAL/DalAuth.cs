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
    /// 登录Dal类
    /// </summary>
    public class DalAuth
    {
        /// <summary>
        /// 获取登录员工信息
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IDataReader GetEmployeeData(string account, string password)
        {
            var sql = $@"select id, employeeaccount, employeename, employeepwd from employee 
                    where employeeaccount = '{account}' and employeepwd = '{password}'";

            return SQLHelper.ExecuteReader(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 获取员工权限菜单
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public DataTable GetEmployeeMenuAuth(int employeeId)
        {
            var sql = $@"select b.* from authconfig a inner join menu b on a.menuid = b.id 
                where a.employeeid = {employeeId} order by b.menusort";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }
    }
}