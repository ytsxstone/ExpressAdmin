using System;
using System.Collections.Generic;

namespace ExpressModel
{
    /// <summary>
    /// 运单信息
    /// </summary>
    public class ModWayBill
    {
        #region Model
        private int _oid;
        private string _warehousingno;
        private string _waybillnumber;
        private decimal _settlementweight;
        private string _singlechannel;
        private string _recipient;
        private string _recphone;
        private string _recaddress;
        private string _reccity;
        private string _recprovince;
        private string _recpostcode;
        private string _goodsname1;
        private string _customsno1;
        private decimal _price1;
        private int _piecenum1;
        private decimal _pieceweight1;
        private string _goodsname2;
        private string _customsno2;
        private decimal _price2;
        private int _piecenum2;
        private decimal _pieceweight2;
        private string _goodsname3;
        private string _customsno3;
        private decimal _price3;
        private int _piecenum3;
        private decimal _pieceweight3;
        private decimal _declaredvalue;
        private string _declaredcurrency;
        private int _ispayduty;
        private decimal _insured;
        private string _typingtype;
        private string _destination;
        private string _destinationpoint;
        private string _sender;
        private string _sendphone;
        private string _sendaddress;
        private decimal _freight;
        private decimal _customerquotation;
        private decimal _tax;
        private int _phonecount;
        private string _importbatch;
        private string _exportbatch;
        private string _created;
        private string _updated;

        /// <summary>
        /// 主键ID
        /// </summary>
        public int Oid
        {
            set { _oid = value; }
            get { return _oid; }
        }
        /// <summary>
        /// 入库号
        /// </summary>
        public string WarehousingNo
        {
            set { _warehousingno = value; }
            get { return _warehousingno; }
        }
        /// <summary>
        /// 运单编号
        /// </summary>
        public string WaybillNumber
        {
            set { _waybillnumber = value; }
            get { return _waybillnumber; }
        }
        /// <summary>
        /// 结算重量
        /// </summary>
        public decimal SettlementWeight
        {
            set { _settlementweight = value; }
            get { return _settlementweight; }
        }
        /// <summary>
        /// 转单渠道
        /// </summary>
        public string SingleChannel
        {
            set { _singlechannel = value; }
            get { return _singlechannel; }
        }
        /// <summary>
        /// 收件人信息|收件人
        /// </summary>
        public string Recipient
        {
            set { _recipient = value; }
            get { return _recipient; }
        }
        /// <summary>
        /// 收件人信息|收件人电话
        /// </summary>
        public string RecPhone
        {
            set { _recphone = value; }
            get { return _recphone; }
        }
        /// <summary>
        /// 收件人信息|收件地址
        /// </summary>
        public string RecAddress
        {
            set { _recaddress = value; }
            get { return _recaddress; }
        }
        /// <summary>
        /// 收件人信息|收件城市
        /// </summary>
        public string RecCity
        {
            set { _reccity = value; }
            get { return _reccity; }
        }
        /// <summary>
        /// 收件人信息|收件省份
        /// </summary>
        public string RecProvince
        {
            set { _recprovince = value; }
            get { return _recprovince; }
        }
        /// <summary>
        /// 收件人信息|收件地邮编
        /// </summary>
        public string RecPostcode
        {
            set { _recpostcode = value; }
            get { return _recpostcode; }
        }
        /// <summary>
        /// 货物明细信息|物品名称①
        /// </summary>
        public string GoodsName1
        {
            set { _goodsname1 = value; }
            get { return _goodsname1; }
        }
        /// <summary>
        /// 货物明细信息|税关号①
        /// </summary>
        public string CustomsNo1
        {
            set { _customsno1 = value; }
            get { return _customsno1; }
        }
        /// <summary>
        /// 货物明细信息|单价①
        /// </summary>
        public decimal Price1
        {
            set { _price1 = value; }
            get { return _price1; }
        }
        /// <summary>
        /// 货物明细信息|单件件数①
        /// </summary>
        public int PieceNum1
        {
            set { _piecenum1 = value; }
            get { return _piecenum1; }
        }
        /// <summary>
        /// 货物明细信息|单件重量①
        /// </summary>
        public decimal PieceWeight1
        {
            set { _pieceweight1 = value; }
            get { return _pieceweight1; }
        }
        /// <summary>
        /// 货物明细信息|物品名称②
        /// </summary>
        public string GoodsName2
        {
            set { _goodsname2 = value; }
            get { return _goodsname2; }
        }
        /// <summary>
        /// 货物明细信息|税关号②
        /// </summary>
        public string CustomsNo2
        {
            set { _customsno2 = value; }
            get { return _customsno2; }
        }
        /// <summary>
        /// 货物明细信息|单价②
        /// </summary>
        public decimal Price2
        {
            set { _price2 = value; }
            get { return _price2; }
        }
        /// <summary>
        /// 货物明细信息|单件件数②
        /// </summary>
        public int PieceNum2
        {
            set { _piecenum2 = value; }
            get { return _piecenum2; }
        }
        /// <summary>
        /// 货物明细信息|单件重量②
        /// </summary>
        public decimal PieceWeight2
        {
            set { _pieceweight2 = value; }
            get { return _pieceweight2; }
        }
        /// <summary>
        /// 货物明细信息|物品名称③
        /// </summary>
        public string GoodsName3
        {
            set { _goodsname3 = value; }
            get { return _goodsname3; }
        }
        /// <summary>
        /// 货物明细信息|税关号③
        /// </summary>
        public string CustomsNo3
        {
            set { _customsno3 = value; }
            get { return _customsno3; }
        }
        /// <summary>
        /// 货物明细信息|单价③
        /// </summary>
        public decimal Price3
        {
            set { _price3 = value; }
            get { return _price3; }
        }
        /// <summary>
        /// 货物明细信息|单件件数③
        /// </summary>
        public int PieceNum3
        {
            set { _piecenum3 = value; }
            get { return _piecenum3; }
        }
        /// <summary>
        /// 货物明细信息|单件重量③
        /// </summary>
        public decimal PieceWeight3
        {
            set { _pieceweight3 = value; }
            get { return _pieceweight3; }
        }
        /// <summary>
        /// 申报价值
        /// </summary>
        public decimal DeclaredValue
        {
            set { _declaredvalue = value; }
            get { return _declaredvalue; }
        }
        /// <summary>
        /// 申报货币
        /// </summary>
        public string DeclaredCurrency
        {
            set { _declaredcurrency = value; }
            get { return _declaredcurrency; }
        }
        /// <summary>
        /// 是否代缴关税
        /// </summary>
        public int IsPayDuty
        {
            set { _ispayduty = value; }
            get { return _ispayduty; }
        }
        /// <summary>
        /// 保价
        /// </summary>
        public decimal Insured
        {
            set { _insured = value; }
            get { return _insured; }
        }
        /// <summary>
        /// 打单类型
        /// </summary>
        public string TypingType
        {
            set { _typingtype = value; }
            get { return _typingtype; }
        }
        /// <summary>
        /// 目的地
        /// </summary>
        public string Destination
        {
            set { _destination = value; }
            get { return _destination; }
        }
        /// <summary>
        /// 目的网点
        /// </summary>
        public string DestinationPoint
        {
            set { _destinationpoint = value; }
            get { return _destinationpoint; }
        }
        /// <summary>
        /// 寄件人信息|寄件人
        /// </summary>
        public string Sender
        {
            set { _sender = value; }
            get { return _sender; }
        }
        /// <summary>
        /// 寄件人信息|寄件电话
        /// </summary>
        public string SendPhone
        {
            set { _sendphone = value; }
            get { return _sendphone; }
        }
        /// <summary>
        /// 寄件人信息|寄件地址
        /// </summary>
        public string SendAddress
        {
            set { _sendaddress = value; }
            get { return _sendaddress; }
        }
        /// <summary>
        /// 运费
        /// </summary>
        public decimal Freight
        {
            set { _freight = value; }
            get { return _freight; }
        }
        /// <summary>
        /// 客户报价
        /// </summary>
        public decimal CustomerQuotation
        {
            set { _customerquotation = value; }
            get { return _customerquotation; }
        }
        /// <summary>
        /// 税金
        /// </summary>
        public decimal Tax
        {
            set { _tax = value; }
            get { return _tax; }
        }
        /// <summary>
        /// 手机号码出现频率
        /// </summary>
        public int PhoneCount
        {
            set { _phonecount = value; }
            get { return _phonecount; }
        }
        /// <summary>
        /// 导入批次
        /// </summary>
        public string ImportBatch
        {
            set { _importbatch = value; }
            get { return _importbatch; }
        }
        /// <summary>
        /// 导出批次
        /// </summary>
        public string ExportBatch
        {
            set { _exportbatch = value; }
            get { return _exportbatch; }
        }
        /// <summary>
        /// 创建者
        /// </summary>
        public string Created
        {
            set { _created = value; }
            get { return _created; }
        }
        /// <summary>
        /// 修改者
        /// </summary>
        public string Updated
        {
            set { _updated = value; }
            get { return _updated; }
        }
        #endregion Model

        /// <summary>
        /// 数据完整性验证
        /// </summary>
        /// <returns></returns>
        public bool IsCheckModel()
        {
            //入仓号
            if (string.IsNullOrWhiteSpace(this._warehousingno))
            {
                return false;
            }
            //运单编号
            if (string.IsNullOrWhiteSpace(this._waybillnumber))
            {
                return false;
            }
            //转单渠道
            if (string.IsNullOrWhiteSpace(this._singlechannel))
            {
                return false;
            }
            //收件人姓名
            if (string.IsNullOrWhiteSpace(this._recipient))
            {
                return false;
            }
            //收件人地址
            if (string.IsNullOrWhiteSpace(this._recaddress))
            {
                return false;
            }
            //收件地城市
            if (string.IsNullOrWhiteSpace(this._reccity))
            {
                return false;
            }
            //货物明细信息|物品名称①
            if (string.IsNullOrWhiteSpace(this._goodsname1))
            {
                return false;
            }
            //货物明细信息|单件件数①
            if (this._piecenum1 == 0)
            {
                return false;
            }
            //货物明细信息|物品名称② 有内容时，货物明细信息|单件件数②必填
            if (!string.IsNullOrWhiteSpace(this._goodsname2) && this._piecenum2 == 0)
            {
                return false;
            }
            //货物明细信息|物品名称③ 有内容时，货物明细信息|单件件数③必填
            if (!string.IsNullOrWhiteSpace(this._goodsname3) && this._piecenum3 == 0)
            {
                return false;
            }

            return true;
        }
    }
}
