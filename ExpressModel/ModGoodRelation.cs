using System;
using System.Collections.Generic;

namespace ExpressModel
{
    /// <summary>
    /// 物品关系表
    /// </summary>
    public class ModGoodRelation
    {
        // 主键ID
        public int Rid { get; set; }

        // 原物品名称
        public string OriginalName { get; set; }

        // 商品税号
        public string TaxNumber { get; set; }

        // 新物品名称1
        public string NewName1 { get; set; }

        public string NewName2 { get; set; }

        public string NewName3 { get; set; }

        public string NewName4 { get; set; }

        public string NewName5 { get; set; }

        public string NewName6 { get; set; }

        public string NewName7 { get; set; }

        public string NewName8 { get; set; }

        public string NewName9 { get; set; }

        public string NewName10 { get; set; }

        public string NewName11 { get; set; }

        public string NewName12 { get; set; }

        public string NewName13 { get; set; }

        public string NewName14 { get; set; }

        public string NewName15 { get; set; }

        public string NewName16 { get; set; }

        public string NewName17 { get; set; }

        public string NewName18 { get; set; }

        public string NewName19 { get; set; }

        public string NewName20 { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Created { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string Updated { get; set; }

        /// <summary>
        /// 数据完整性验证
        /// </summary>
        /// <returns></returns>
        public bool IsCheckModel()
        {
            if (string.IsNullOrWhiteSpace(this.OriginalName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.TaxNumber))
            {
                return false;
            }

            return true;
        }
    }
}
