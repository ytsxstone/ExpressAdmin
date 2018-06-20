using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExpressCommon
{
    /// <summary>
    /// 公共帮助类
    /// </summary>
    public static class ComHelper
    {
        /// <summary>
        /// 判断同一个类的两个对象是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <returns></returns>
        public static bool CompareType<T>(T oldModel, T newModel, List<string> PropertyNameList = null)
        {
            Type oldType = oldModel.GetType();
            Type newType = newModel.GetType();

            PropertyInfo[] oldProperties = oldType.GetProperties();
            PropertyInfo[] newProperties = newType.GetProperties();

            bool isMatch = true;
            for (int i = 0; i < oldProperties.Length; i++)
            {
                //判断属性值是否需要验证相等
                if (PropertyNameList != null && !PropertyNameList.Contains(oldProperties[i].Name))
                {
                    continue;
                }

                //比较的数据类型
                if (!oldProperties[i].PropertyType.Equals(typeof(Int32)) && !oldProperties[i].PropertyType.Equals(typeof(String)))
                {
                    continue;
                }

                //判断属性值是否相等
                if (oldProperties[i].GetValue(oldModel, null).ToString() != newProperties[i].GetValue(newModel, null).ToString())
                {
                    isMatch = false;
                    break;
                }
            }

            return isMatch;
        }

        /// <summary>
        /// 发送web请求，获取返回数据
        /// </summary>
        /// <param name="reqUrl"></param>
        /// <param name="reqParam"></param>
        /// <returns></returns>
        private static string GetWebRequest(string reqUrl, string reqParam)
        {
            string result = string.Empty;
            string postData = string.Empty;
            //创建HttpWebRequest对象
            HttpWebRequest webRequest = WebRequest.Create(reqUrl) as HttpWebRequest;
            //代理信息
            webRequest.Proxy = null;
            //是否建立持久连接
            webRequest.KeepAlive = false;
            //不允许重定向
            webRequest.AllowAutoRedirect = false;
            //超时时间 30秒
            webRequest.Timeout = 30000;
            //请求参数
            postData = "{\"param\":\"" + reqParam + "\"}";
            //传输编码,采用UTF8编码
            byte[] sendData = System.Text.Encoding.UTF8.GetBytes(postData);
            //JSON格式传输
            webRequest.ContentType = "application/json";
            //传输长度
            webRequest.ContentLength = sendData.Length;
            //POST访问
            webRequest.Method = "POST";
            //写入请求参数
            using (Stream ioStream = webRequest.GetRequestStream())
            {
                ioStream.Write(sendData, 0, sendData.Length);
                ioStream.Close();
            }

            //获取返回值
            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(webResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                //将文件流转化为字符串
                result = reader.ReadToEnd();
                reader.Close();
                webResponse.Close();
            }

            return result;
        }

        /// <summary>
        /// 修改对象某属性的值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="Value"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SetModelValue(string FieldName, string Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName).PropertyType);
                Ts.GetProperty(FieldName).SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DataReader转泛型
        /// </summary>
        /// <typeparam name="T">传入的实体类</typeparam>
        /// <param name="objReader">DataReader对象</param>
        /// <returns></returns>
        public static IList<T> ReaderToList<T>(this IDataReader objReader)
        {
            using (objReader)
            {
                List<T> list = new List<T>();

                //获取传入的数据类型
                Type modelType = typeof(T);

                //遍历DataReader对象
                while (objReader.Read())
                {
                    //使用与指定参数匹配最高的构造函数，来创建指定类型的实例
                    T model = Activator.CreateInstance<T>();
                    for (int i = 0; i < objReader.FieldCount; i++)
                    {
                        //判断字段值是否为空或不存在的值
                        if (!IsNullOrDBNull(objReader[i]))
                        {
                            //匹配字段名
                            PropertyInfo pi = modelType.GetProperty(objReader.GetName(i), BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (pi != null)
                            {
                                //绑定实体对象中同名的字段  
                                pi.SetValue(model, CheckType(objReader[i], pi.PropertyType), null);
                            }
                        }
                    }
                    list.Add(model);
                }
                return list;
            }
        }

        /// <summary>
        /// 对可空类型进行判断转换(*要不然会报错)
        /// </summary>
        /// <param name="value">DataReader字段的值</param>
        /// <param name="conversionType">该字段的类型</param>
        /// <returns></returns>
        private static object CheckType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        /// 判断指定对象是否是有效值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsNullOrDBNull(object obj)
        {
            return (obj == null || (obj is DBNull)) ? true : false;
        }

        /// <summary>
        /// DataReader转模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objReader"></param>
        /// <returns></returns>
        public static T ReaderToModel<T>(this IDataReader objReader)
        {
            using (objReader)
            {
                if (objReader.Read())
                {
                    Type modelType = typeof(T);
                    int count = objReader.FieldCount;
                    T model = Activator.CreateInstance<T>();
                    for (int i = 0; i < count; i++)
                    {
                        if (!IsNullOrDBNull(objReader[i]))
                        {
                            PropertyInfo pi = modelType.GetProperty(objReader.GetName(i), BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (pi != null)
                            {
                                pi.SetValue(model, CheckType(objReader[i], pi.PropertyType), null);
                            }
                        }
                    }
                    return model;
                }
            }
            return default(T);
        }

        /// <summary>
        /// 获取文件后缀名
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileType(string filePath)
        {
            string fType = "";
            if (!string.IsNullOrEmpty(filePath))
            {
                fType = filePath.Substring(filePath.LastIndexOf('.') + 1);
            }

            return fType;
        }

        #region 字符串正则验证验证

        /// <summary>
        /// 文本框预处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PreTreating(string str)
        {
            string myString = str;

            if ("0.00" == myString || "0.0" == myString)
            {
                myString = "0";
            }
            else if (IsSDC(myString))
            {
                myString = ToDBC(myString);
            }

            return myString;
        }

        /// <summary>
        /// 判断字符串是否为全角字符
        /// </summary>
        /// <param name="strSDC"></param>
        /// <returns></returns>
        public static bool IsSDC(string strSDC)
        {
            bool flag = false;
            if (strSDC.Length != Encoding.Default.GetByteCount(strSDC))
            {
                flag = true;
                return flag;
            }
            else
            {
                return flag;
            }
        }

        /// <summary>
        /// 将全角转换为半角；replace SBC case to DBC case
        /// 全角空格为12288，半角空格为32；
        /// 其他字符半角（33-126）与全角（65281-65374）的对应关系是：均相差65248；
        /// </summary>
        /// <param name="strSBC"></param>
        /// <returns></returns>
        public static string ToDBC(string strSBC)
        {
            char[] ch = strSBC.ToCharArray();
            for (int i = 0; i < ch.Length; i++)
            {
                if (ch[i] == 12288)
                {
                    ch[i] = (char)32;
                    continue;
                }
                if (ch[i] > 65280 && ch[i] < 65375)
                {
                    ch[i] = (char)(ch[i] - 65248);
                }
            }
            return new string(ch);
        }

        /// <summary>
        /// 验证是否为数字
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool IsNumber(string strValue)
        {
            //正则验证 ^\d*$
            if (!Regex.IsMatch(strValue, @"^[0-9]*$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证是否为数字，并且最多只能包含两位小数
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool IsNumberic(string strValue)
        {
            //正则验证 ^((0|[1-9]\d{0,20}(\.\d{0,1}\d)?)|(0\.\d{0,1}\d))$
            if (!Regex.IsMatch(strValue, @"^((0|[1-9]\d{0,20}(\.\d{1,2})?)|(0\.\d{1,2}))$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证是否为正整数
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool IsPositiveInteger(string strValue)
        {
            //正则验证 
            if (!Regex.IsMatch(strValue, @"^(0|\+?[1-9][0-9]*)$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证是否为0~100以内的数字，并且最多只能包含两位小数
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool IsRateNumberic(string strValue)
        {
            //正则验证 
            if (!Regex.IsMatch(strValue, @"^((\d|[1-9]\d)(\.\d{1,2})?|100|100.0|100.00)$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool CustomeRegex(string strValue, string pattern)
        {
            //正则验证 
            if (!Regex.IsMatch(strValue, pattern))
            {
                return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 通用类型转换 Convert.ChangeType
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type)
        {
            if (value == null && type.IsGenericType)
                return Activator.CreateInstance(type);
            if (value == null)
                return null;
            if (type == value.GetType())
                return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid))
                return new Guid(value as string);
            if (value is string && type == typeof(Version))
                return new Version(value as string);
            if (!(value is IConvertible))
                return value;

            return Convert.ChangeType(value, type);
        }

        /// <summary> 
        /// 利用反射将DataTable转换为List<T>对象
        /// </summary> 
        /// <param name="dt">DataTable 对象</param> 
        /// <returns>List<T>集合</returns> 
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            #region 利用反射将DataTable转换为List<T>对象 

            //// 定义集合 
            //List<T> ts = new List<T>();
            ////定义一个临时变量 
            //string tempName = string.Empty;
            ////遍历DataTable中所有的数据行 
            //foreach (DataRow dr in dt.Rows)
            //{
            //    T t = new T();
            //    // 获得此模型的公共属性 
            //    PropertyInfo[] propertys = t.GetType().GetProperties();
            //    //遍历该对象的所有属性 
            //    foreach (PropertyInfo pi in propertys)
            //    {
            //        //将属性名称赋值给临时变量 
            //        tempName = pi.Name.ToLower();
            //        //检查DataTable是否包含此列（列名==对象的属性名）  
            //        if (dt.Columns.Contains(tempName))
            //        {
            //            //取值 
            //            object value = dr[tempName];
            //            //如果非空，则赋给对象的属性 
            //            if (value != DBNull.Value)
            //            {
            //                pi.SetValue(t, value, null);
            //            }
            //        }
            //    }
            //    //对象添加到泛型集合中 
            //    ts.Add(t);
            //}
            //return ts;

            #endregion

            // 定义集合 
            List<T> ts = new List<T>();
            //定义一个临时变量 
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行 
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性 
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性 
                foreach (PropertyInfo pi in propertys)
                {
                    //将属性名称赋值给临时变量 
                    tempName = pi.Name.ToLower();

                    //检查DataTable是否包含此列（列名==对象的属性名）  
                    if (dt.Columns.Contains(tempName) && dr[tempName] != DBNull.Value)
                    {
                        //类型强转，将table字段类型转为集合字段类型  
                        var obj = ChangeType(dr[tempName], pi.PropertyType);
                        pi.SetValue(t, obj, null);
                    }
                }
                //对象添加到泛型集合中 
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// 导入datatable字段验证
        /// </summary>
        /// <param name="dt_import">导入excel</param>
        /// <param name="fieldList">excel文件中应包含的字段集合</param>
        /// <param name="fieldMsg">错误信息</param>
        /// <returns></returns>
        public static bool DataTableFieldValid(DataTable dt_import, List<string> fieldList, out string fieldMsg)
        {
            //判断字段是否都存在与导入的datatable中
            bool result = true;
            fieldMsg = "";
            string fieldStr = string.Empty;
            int i = 0;
            foreach (string field in fieldList)
            {
                if (!dt_import.Columns.Contains(field))
                {
                    //字段不存在
                    result = false;
                    if (i != 0 && i % 3 == 0)
                    {
                        fieldStr += "<br/>";
                    }
                    fieldStr += field + "，";
                    i++;
                }
            }

            if (!result)
            {
                fieldMsg = "导入的Excel文件不完整，以下列不存在：<br/>" + fieldStr.TrimEnd(',') + "<br/><br/>导入失败！";
            }

            return result;
        }

        /// <summary>
        /// 判断指定列数据是否重复
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool IsRepeatByColumnName(DataTable dt_import, string columnName)
        {
            bool result = true;

            //获取分组记录大于1 的记录
            int repeatCount = ((from p in dt_import.AsEnumerable()
                                group p by p.Field<string>(columnName) into g
                                select new
                                {
                                    RepeatKey = g.Key,
                                    RepeatCount = g.Count()
                                }).Where(p => p.RepeatCount > 1)).Count();

            //大于0 表示存在重复记录
            if (repeatCount > 0)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 将datatable字段转为string类型
        /// </summary>
        /// <param name="dt_data"></param>
        /// <returns></returns>
        public static DataTable ConvertDataTableToString(DataTable dt_data)
        {
            //clone表结构, 将dt_result表所有字段类型修改为String
            DataTable dt_result = dt_data.Clone();
            foreach (DataColumn col in dt_result.Columns)
            {
                col.DataType = typeof(String);
            }

            //将dt_data表中的数据添加到dt_result表
            foreach (DataRow row in dt_data.Rows)
            {
                DataRow newDtRow = dt_result.NewRow();
                foreach (DataColumn col in dt_data.Columns)
                {
                    if (col.DataType == typeof(decimal)) //设置decimal类型保留两位小数
                    {
                        newDtRow[col.ColumnName] = decimal.Parse(row[col.ColumnName].ToString()).ToString("f2");
                    }
                    else if (col.DataType == typeof(double)) //设置double类型保留两位小数
                    {
                        newDtRow[col.ColumnName] = double.Parse(row[col.ColumnName].ToString()).ToString("f2");
                    }
                    else //保留原样
                    {
                        newDtRow[col.ColumnName] = row[col.ColumnName];
                    }
                }
                dt_result.Rows.Add(newDtRow);
            }

            return dt_result;
        }

        /// <summary>
        /// 将datatable字段转为string类型, 并清空指定字段的零值
        /// </summary>
        /// <param name="dt_data"></param>
        /// <returns></returns>
        public static DataTable ConvertDataTableToString(DataTable dt_data, List<string> zeroColumns)
        {
            //clone表结构, 将dt_result表所有字段类型修改为String
            DataTable dt_result = dt_data.Clone();
            foreach (DataColumn col in dt_result.Columns)
            {
                col.DataType = typeof(String);
            }

            //将dt_data表中的数据添加到dt_result表
            foreach (DataRow row in dt_data.Rows)
            {
                DataRow newDtRow = dt_result.NewRow();
                foreach (DataColumn col in dt_data.Columns)
                {
                    if (col.DataType == typeof(decimal)) //设置decimal类型保留两位小数
                    {
                        newDtRow[col.ColumnName] = decimal.Parse(row[col.ColumnName].ToString()).ToString("f2");
                    }
                    else if (col.DataType == typeof(double)) //设置double类型保留两位小数
                    {
                        newDtRow[col.ColumnName] = double.Parse(row[col.ColumnName].ToString()).ToString("f2");
                    }
                    else //保留原样
                    {
                        newDtRow[col.ColumnName] = row[col.ColumnName];
                    }

                    //清空零值
                    if (zeroColumns.Contains(col.ColumnName))
                    {
                        if (newDtRow[col.ColumnName].ToString() == "0" || newDtRow[col.ColumnName].ToString() == "0.0" || newDtRow[col.ColumnName].ToString() == "0.00")
                        {
                            newDtRow[col.ColumnName] = "";
                        }
                    }
                }
                dt_result.Rows.Add(newDtRow);
            }

            return dt_result;
        }
    }
}
