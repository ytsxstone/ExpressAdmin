using System;

namespace ExpressModel
{
    /// <summary>
    /// 地区信息类
    /// </summary>
    public class ModCityArea
    {
        // 主键ID
        public int AreaID { get; set; }

        // 省份/直辖市
        public string AreaProvince { get; set; }

        // 城市
        public string AreaCity { get; set; }

        //邮编
        public string AreaPostcode { get; set; }

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
            if (string.IsNullOrWhiteSpace(this.AreaProvince))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.AreaCity))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.AreaPostcode))
            {
                return false;
            }

            return true;
        }
    }
}
