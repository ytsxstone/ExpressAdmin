using System;

namespace ExpressModel
{
    /// <summary>
    /// 禁运物品信息类
    /// </summary>
    public class ModProhibited
    {
        // 主键ID
        public int Pid { get; set; }

        //类型 1-物品名称 2-收件人地址 3-收件人名称 4-收件人电话
        public int Type { get; set; }

        // 禁运物品名称
        public string PName { get; set; }

        // 禁运物品备注
        public string PRemark { get; set; }

        //创建者
        public string Created { get; set; }

        //修改者
        public string Updated { get; set; }

        /// <summary>
        /// 数据完整性验证
        /// </summary>
        /// <returns></returns>
        public bool IsCheckModel()
        {
            if (string.IsNullOrWhiteSpace(this.PName))
            {
                return false;
            }

            return true;
        }
    }
}
