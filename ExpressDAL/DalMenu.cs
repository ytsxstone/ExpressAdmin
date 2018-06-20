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
    /// 菜单Dal类
    /// </summary>
    public class DalMenu
    {
        /// <summary>
        /// 获取菜单数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataTable GetMenuData(string name = "")
        {
            var sql = string.Format(@"select id, parentid, menucode, menuname, menuurl, menusort, menuicon from menu sa where 1=1");
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += string.Format(" and sa.menuname like '%{0}%' ", name);
            }
            sql += " order by sa.menusort asc";

            return SQLHelper.ExecuteDataset(SQLHelper.defConnStr, CommandType.Text, sql, null).Tables[0];
        }

        /// <summary>
        /// 插入菜单
        /// </summary>
        public int Create(int parentId, string code, string name, string url, int sort, string icon)
        {
            var sql = $@"insert into menu(parentid, menucode, menuname, menuurl, menusort, menuicon) 
                values({parentId}, '{code}', '{name}', '{url}', {sort}, '{icon}')";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 修改菜单
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="sort"></param>
        /// <param name="icon"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Update(int id, int parentId, string code, string name, string url, int sort, string icon)
        {
            var sql = $@"update menu set parentid={parentId}, menucode='{code}', menuname='{name}', menuurl='{url}', 
                menusort={sort}, menuicon='{icon}' where id={id}";

            return SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns></returns>
        public int Delete(string ids)
        {
            var sql = $"delete from menu where id in ({ids})";
            var result = SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);

            sql = $"delete from authconfig where menuid in ({ids})";
            result += SQLHelper.ExecuteNonQuery(SQLHelper.defConnStr, CommandType.Text, sql, null);

            return result;
        }
    }
}