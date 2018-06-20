using System;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Security.Cryptography;

namespace ExpressCommon
{
    /// <summary>
    /// 代码中用到的帮助类信息
    /// </summary>
    public class CodeHelper
    {
        /// <summary>
        /// 数字正则表达式
        /// </summary>
        private const string numPattern = @"^[-+]?[0-9]+(\.[0-9]+)?$";

        /// <summary>
        /// 日期表达式
        /// </summary>
        private const string datePattern = @"^(((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$";

        /// <summary>
        /// 获取GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGuid()
        {
            return "{" + Guid.NewGuid().ToString().ToUpper() + "}";
        }

        /// <summary>
        /// 生成GUID不带括号
        /// </summary>
        /// <returns></returns>
        public static string GetGuidNo()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        /// <summary>
        /// 截取指定长度字符串方法 
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="intLen"></param>
        /// <returns></returns>
        public static string CutString(string strInput, int intLen)
        {
            strInput = strInput.Trim();
            byte[] myByte = System.Text.Encoding.Default.GetBytes(strInput);

            if (myByte.Length > intLen)
            {
                //截取操作
                string resultStr = "";
                for (int i = 0; i < strInput.Length; i++)
                {
                    byte[] tempByte = System.Text.Encoding.Default.GetBytes(resultStr);
                    if (tempByte.Length < intLen)
                    {

                        resultStr += strInput.Substring(i, 1);
                    }
                    else
                    {
                        break;
                    }
                }
                return resultStr + "...";
            }
            else
            {
                return strInput;
            }
        }

        #region  人民币格式转换

        /// <summary>
        /// 转换人民币大小金额
        /// </summary>
        /// <param name="num">金额</param>
        /// <returns>返回大写形式</returns>
        public static string CmycurD(decimal? num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字
            string str3 = "";    //从原num值中取出的值
            string str4 = "";    //数字的字符串形式
            string str5 = "";  //人民币大写金额形式
            int i;    //循环变量
            int j;    //num的值乘以100的字符串长度
            string ch1 = "";    //数字的汉语读法
            string ch2 = "";    //数字位的汉字读法
            int nzero = 0;  //用来计算连续的零值是几个
            int temp;            //从原num值中取出的值

            num = Math.Round(Math.Abs(num.Value), 2, MidpointRounding.AwayFromZero);    //将num取绝对值并四舍五入取2位小数
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式
            j = str4.Length;      //找出最高位
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分

            //循环取出每一位需要转换的值
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值
                temp = Convert.ToInt32(str3);      //转换为数字
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整”
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            return str5;
        }

        /**/
        /// <summary>
        /// 一个重载，将字符串先转换成数字在调用CmycurD(decimal num)
        /// </summary>
        /// <param name="num">用户输入的金额，字符串形式未转成decimal</param>
        /// <returns></returns>
        public static string CmycurD(string numstr)
        {
            try
            {
                decimal num = Convert.ToDecimal(numstr);
                return CmycurD(num);
            }
            catch
            {
                return "非数字形式！";
            }
        }

        #endregion

        /// <summary>
        /// DataTable转为json串
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string ToJsonString(DataTable table)
        {
            return JsonConvert.SerializeObject(table, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }

        /// <summary>
        /// DataTable分页
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="pageIndex">页索引,注意：从1开始</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>分好页的DataTable数据</returns>              第1页        每页10条
        public static DataTable GetPagedTable(DataTable dt, int pageIndex, int pageSize)
        {
            if (pageIndex == 0)
            {
                return dt;
            }

            DataTable newdt = dt.Copy();
            newdt.Clear();

            int rowbegin = (pageIndex - 1) * pageSize;
            int rowend = pageIndex * pageSize;

            if (rowbegin >= dt.Rows.Count)
            {
                return newdt;
            }
            if (rowend > dt.Rows.Count)
            {
                rowend = dt.Rows.Count;
            }

            for (int i = rowbegin; i <= rowend - 1; i++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[i];

                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }

                newdt.Rows.Add(newdr);
            }

            return newdt;
        }

        /// <summary>
        /// 获取十位的时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GenerateTimeStamp(DateTime dt)
        {
            // Default implementation of UNIX time of the current UTC time  
            TimeSpan ts = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <param name="code">16 或 32</param>
        /// <returns></returns>
        public static string Md5(string str, int code)  //code 16 或 32  
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            string result = sBuilder.ToString().ToLower();

            if (code == 16) //16位MD5加密（取32位加密的9~25字符）  
            {
                return result.Substring(8, 16);
            }
            else if (code == 32) //32位加密  
            {
                return result;
            }
            else
            {
                return "00000000000000000000000000000000";
            }

            //if (code == 16) //16位MD5加密（取32位加密的9~25字符）  
            //{
            //    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower().Substring(8, 16);
            //}
            //if (code == 32) //32位加密  
            //{
            //    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower();
            //}
        }

        /// <summary>
        /// http请求方法
        /// </summary>
        /// <param name="url">url地址，带参数</param>
        /// <returns></returns>
        public static string DoGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>
        /// 匹配是否合法手机号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPhone(string input)
        {
            string pattern = "^1(3|4|5|6|7|8|9)\\d{9}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        /// <summary>
        /// WebApiPost请求
        /// </summary>
        /// <param name="requestUrl">请求的Url地址</param>
        /// <param name="para">请求的参数</param>
        /// <param name="hasJson">是否为json</param>
        /// <returns>返回json</returns>
        public static string DoPost(string requestUrl, object para, bool hasJson = false)
        {
            string postUrl = requestUrl;
            var httpClient = new System.Net.Http.HttpClient();

            try
            {
                var postPara = string.Empty;
                if (hasJson)
                {
                    postPara = para.ToString();
                }
                else
                {
                    postPara = JsonConvert.SerializeObject(para);
                }

                System.Net.Http.HttpContent httpContent = new System.Net.Http.StringContent(postPara);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string responseJson = httpClient.PostAsync(postUrl, httpContent).Result.Content.ReadAsStringAsync().Result;

                return responseJson;
            }
            catch
            {
                return "";
            }
            finally
            {
                //用完要记得释放
                httpClient.Dispose();
            }
        }

        /// <summary>
        /// 验证身份证号码
        /// </summary>
        /// <param name="id">身份证号码</param>
        /// <returns></returns>
        public static bool IsIdCard(string id)
        {
            long n = 0;
            if (long.TryParse(id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            if (address.IndexOf(id.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] ai = id.Remove(17).ToCharArray();
            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(wi[i]) * int.Parse(ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);

            if (arrVarifyCode[y] != id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }

            return true;//符合GB11643-1999标准
        }
    }
}
