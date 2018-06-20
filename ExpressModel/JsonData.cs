using System;

namespace ExpressModel
{
    /// <summary>
    /// JSON返回类
    /// </summary>
    public class JsonData
    {
        /// <summary>
        /// 操作状态：true:成功,false:失败
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }
}
