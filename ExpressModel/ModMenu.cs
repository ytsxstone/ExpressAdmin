using System;
using System.Collections.Generic;

namespace ExpressModel
{
    /// <summary>
    /// 菜单Model
    /// </summary>
    public class ModMenu
    {
        /// <summary>
        /// 表id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 父节点id
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public string MenuCode { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        public string MenuUrl { get; set; }

        /// <summary>
        /// 菜单排序
        /// </summary>
        public int MenuSort { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string MenuIcon { get; set; }
    }
}
