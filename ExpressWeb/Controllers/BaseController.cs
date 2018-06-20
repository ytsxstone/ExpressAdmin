using System;
using System.Data;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;

using ExpressDAL;
using ExpressModel;
using ExpressCommon;
using ExpressWeb.Authorizes;

namespace ExpressWeb.Controllers
{
    /// <summary>
    /// 基类控制器
    /// </summary>
    [CustomAuthorize]
    public class BaseController : Controller
    {
        /// <summary>
        /// 实体数据转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public T DataRowConvertToEntity<T>(DataRow row) where T : new()
        {
            T entity = new T();

            foreach (var item in entity.GetType().GetProperties())
            {
                if (row.Table.Columns.Contains(item.Name))
                {
                    if (DBNull.Value != row[item.Name])
                    {
                        item.SetValue(entity, Convert.ChangeType(row[item.Name], item.PropertyType), null);
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// 实体数据转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IList<T> DataRowConvertToEntity<T>(IList<DataRow> rows)
        {
            IList<T> list = null;

            if (rows != null)
            {
                list = new List<T>();

                foreach (DataRow row in rows)
                {
                    T item = CreateEntityModel<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// 反射实体属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CreateEntityModel<T>(DataRow row)
        {
            T obj = default(T);
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();

                //忽略大小写
                BindingFlags flags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;

                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName, flags);
                    try
                    {
                        object value = row[column.ColumnName];
                        prop.SetValue(obj, value, null);
                    }
                    catch
                    {

                    }
                }
            }

            return obj;
        }
    }
}