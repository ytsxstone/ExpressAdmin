using System;
using System.Collections.Generic;

namespace ExpressModel
{
    /// <summary>
    /// 员工Model
    /// </summary>
    public partial class ModEmployee
    {
        /// <summary>
        /// 表编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        public string EmployeeAccount { get; set; }

        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName { get; set; }
        
        /// <summary>
        /// 员工密码
        /// </summary>
        public string EmployeePwd { get; set; }

        /// <summary>
        /// 员工备注
        /// </summary>
        public string EmployeeDesc { get; set; }
    }
}
