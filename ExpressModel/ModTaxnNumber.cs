using System;

namespace ExpressModel
{
    /// <summary>
    /// 商品税号信息类
    /// </summary>
    public class ModTaxnNumber
    {
        // 主键ID
        public int Pid { get; set; }

        // 商品税号
        public string PTaxNumber { get; set; }

        // 完税价格
        public decimal PTaxPrice { get; set; }

        // 税率
        public decimal PTaxRate { get; set; }

        /// <summary>
        /// 数据完整性验证
        /// </summary>
        /// <returns></returns>
        public bool IsCheckModel()
        {
            if (string.IsNullOrWhiteSpace(this.PTaxNumber))
            {
                return false;
            }

            return true;
        }
    }
}
