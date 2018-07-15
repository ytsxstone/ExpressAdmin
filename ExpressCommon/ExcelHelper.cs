using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aspose.Cells;

namespace ExpressCommon
{
    /// <summary>
    /// Excel文件类型
    /// </summary>
    public enum EnumExcelFileType
    {
        Excel2003 = 0,
        Excel2007 = 1
    }

    /// <summary>
    /// excel帮助类
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// excel文件加载到datatable
        /// </summary>
        /// <param name="filePath">Excel文件物理路径（带文件名）</param>
        /// <returns></returns>
        public static DataTable ExportToDataTable(string filePath)
        {
            DataTable dt = new DataTable();
            if (File.Exists(filePath))//获取文件的后缀名
            {
                Workbook workBook = new Workbook(filePath);//打开文件
                //workBook.Open(filePath);//打开文件
                if (workBook.Worksheets.Count == 0)
                {
                    return dt;
                }
                DataTable targetdt = new DataTable();
                Worksheet sheet = workBook.Worksheets[0];
                int rowscount = sheet.Cells.MaxDataRow;
                int coloumnscount = sheet.Cells.MaxDataColumn;
                if (rowscount == 0)
                {
                    return dt;
                }
                //dt = sheet.Cells.ExportDataTable(1, 0, rowscount, coloumnscount + 1);
                dt = sheet.Cells.ExportDataTableAsString(1, 0, rowscount, coloumnscount + 1); //导入字段类型全部默认为字符串
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    object clomnname = sheet.Cells[0, i].Value;
                    dt.Columns[i].ColumnName = clomnname != null ? clomnname.ToString() : ("ColumnName" + i);
                }
                return dt;
            }
            return dt;
        }

        /// <summary>
        /// 数据导出到Excel（支持多表导出多个Sheet页）
        /// </summary>
        /// <param name="dataset">数据集</param>
        /// <param name="filePath">保存文件夹路径</param>
        /// <param name="fileName">保存文件名称</param>
        /// <param name="saveFileType">保存文件类型（.xls;.xlsx）</param>
        /// <returns>文件物理路径</returns>
        public static string SaveDataSetToExcel(DataSet dataset, string filePath, string fileName, EnumExcelFileType saveFileType)
        {
            string savePath = Path.Combine(filePath, fileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                Workbook workBook = new Workbook();
                workBook.Worksheets.Clear();
                foreach (DataTable dt in dataset.Tables)
                {
                    workBook.Worksheets.Add(dt.TableName);
                    Worksheet ws = workBook.Worksheets[dt.TableName];
                    ws.FreezePanes(1, 1, 1, 0); //冻结第一行

                    int rowscount = dt.Rows.Count;
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        ws.Cells[0, col].PutValue(dt.Columns[col].Caption);
                    }
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        for (int row = 0; row < rowscount; row++)
                        {
                            ws.Cells[row + 1, col].PutValue(dt.Rows[row].ItemArray[col].ToString());
                        }
                    }
                    if (dt.Columns.Count > 0)
                    {
                        ws.AutoFitColumns();
                    }
                    if (dt.Rows.Count > 0)
                    {
                        ws.AutoFitRows();
                    }
                }

                if (saveFileType == EnumExcelFileType.Excel2003)
                {
                    workBook.Save(savePath, SaveFormat.Excel97To2003);
                }
                else
                {
                    workBook.Save(savePath, SaveFormat.Xlsx);
                }
            }
            catch (Exception e)
            {
                LogScopeHelper.Error(e.Message, e);
                return string.Empty;
            }
            return savePath;
        }

        /// <summary>
        /// 数据导出到Excel
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="filePath">保存文件夹路径</param>
        /// <param name="fileName">保存文件名称</param>
        /// <param name="saveFileType">保存文件类型（.xls;.xlsx）</param>
        /// <returns>文件物理路径</returns>
        public static string SaveDataTableToExcel(DataTable dt, string filePath, string fileName, EnumExcelFileType saveFileType)
        {
            string savePath = Path.Combine(filePath, fileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                Workbook workBook = new Workbook();
                workBook.Worksheets.Clear();
                workBook.Worksheets.Add(dt.TableName);
                Worksheet ws = workBook.Worksheets[dt.TableName];
                ws.FreezePanes(1, 1, 1, 0); //冻结第一行

                int rowscount = dt.Rows.Count;
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    ws.Cells[0, col].PutValue(dt.Columns[col].Caption);
                }
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    for (int row = 0; row < rowscount; row++)
                    {
                        ws.Cells[row + 1, col].PutValue(dt.Rows[row].ItemArray[col].ToString());
                    }
                }
                if (dt.Columns.Count > 0)
                {
                    ws.AutoFitColumns();
                }
                if (dt.Rows.Count > 0)
                {
                    ws.AutoFitRows();
                }

                if (saveFileType == EnumExcelFileType.Excel2003)
                {
                    workBook.Save(savePath, SaveFormat.Excel97To2003);
                }
                else
                {
                    workBook.Save(savePath, SaveFormat.Xlsx);
                }
            }
            catch (Exception e)
            {
                LogScopeHelper.Error(e.Message, e);
                return string.Empty;
            }
            return savePath;
        }

        /// <summary>
        /// 数据导出到Excel, 自定义excel样式
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="numberColumns">需要设置单元格为数值列的字段集合</param>
        /// <param name="redStyleColumns">需要设置红色背景的字段集合</param>
        /// <param name="centerStyleColumns">需要设置标题行文本居中的字段集合</param>
        /// <param name="contentLeftStyleColumns">需要设置内容靠左显示的字段集合</param>
        /// <param name="dictWidthColumns">导出excel列宽字典</param>
        /// <param name="filePath">保存文件夹路径</param>
        /// <param name="fileName">保存文件名称</param>
        /// <param name="saveFileType">保存文件类型（.xls;.xlsx）</param>
        /// <returns>文件物理路径</returns>
        public static string SaveDataTableToExcel(DataTable dt, List<string> numberColumns, List<string> redStyleColumns, List<string> centerStyleColumns, List<string> contentLeftStyleColumns, Dictionary<string, double> dictWidthColumns, string filePath, string fileName, EnumExcelFileType saveFileType)
        {
            string savePath = Path.Combine(filePath, fileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                Workbook workBook = new Workbook();
                workBook.Worksheets.Clear();
                workBook.Worksheets.Add(dt.TableName);
                Worksheet ws = workBook.Worksheets[dt.TableName];
                ws.FreezePanes(1, 1, 1, 0); //冻结第一行

                //设置标题样式
                Aspose.Cells.Style headerStyle = workBook.CreateStyle();
                headerStyle.HorizontalAlignment = TextAlignmentType.Right; //文本水平显示方式
                headerStyle.Font.Color = System.Drawing.Color.White; //字体颜
                headerStyle.Font.Name = "宋体"; //文字字体
                headerStyle.Font.Size = 10; //文字大小
                headerStyle.Font.IsBold = true; //粗体
                headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(91, 155, 213); //背景色
                headerStyle.Pattern = Aspose.Cells.BackgroundType.Solid; //单元格的线：实线 
                //设置内容列样式
                Aspose.Cells.Style contentStyle = workBook.CreateStyle();
                contentStyle.HorizontalAlignment = TextAlignmentType.Center; //文本水平显示方式
                //设置验证失败的内容样式1
                Aspose.Cells.Style errorStyle1 = workBook.CreateStyle();
                errorStyle1.HorizontalAlignment = TextAlignmentType.Center;
                errorStyle1.Font.Color = System.Drawing.Color.Red;
                //设置验证失败的内容样式2
                Aspose.Cells.Style errorStyle2 = workBook.CreateStyle();
                errorStyle2.HorizontalAlignment = TextAlignmentType.Center;
                errorStyle2.ForegroundColor = System.Drawing.Color.Red;
                errorStyle2.Pattern = Aspose.Cells.BackgroundType.Solid;

                int rowscount = dt.Rows.Count;
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    ws.Cells[0, col].PutValue(dt.Columns[col].Caption);

                    //设置标题行文本水平显示方式
                    if (centerStyleColumns.Contains(dt.Columns[col].Caption))
                        headerStyle.HorizontalAlignment = TextAlignmentType.Center;
                    else
                        headerStyle.HorizontalAlignment = TextAlignmentType.Right;
                    //设置标题行背景色
                    if (redStyleColumns.Contains(dt.Columns[col].Caption))
                        headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(192, 0, 0);
                    else
                        headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(91, 155, 213);

                    //设置标题行样式
                    ws.Cells[0, col].SetStyle(headerStyle);
                    //设置列宽
                    ws.Cells.SetColumnWidth(col, dictWidthColumns[dt.Columns[col].Caption]);
                }
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    //设置内容行文本水平显示方式
                    if (contentLeftStyleColumns.Contains(dt.Columns[col].Caption))
                        contentStyle.HorizontalAlignment = TextAlignmentType.Left;
                    else
                        contentStyle.HorizontalAlignment = TextAlignmentType.Center;

                    bool styleFlag = true;
                    for (int row = 0; row < rowscount; row++)
                    {
                        styleFlag = true;
                        ws.Cells[row + 1, col].PutValue(dt.Rows[row].ItemArray[col].ToString());

                        //设置内容行样式
                        ws.Cells[row + 1, col].SetStyle(contentStyle);

                        //物品名称为空
                        if (dt.Columns[col].Caption == "货物明细信息|物品名称①" && string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //结算重量是否超过10kg
                        if (dt.Columns[col].Caption == "结算重量" && !string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) && Convert.ToDecimal(dt.Rows[row].ItemArray[col]) > 10m)
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //保价是否超过10000
                        if (dt.Columns[col].Caption == "保价" && !string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) && Convert.ToDecimal(dt.Rows[row].ItemArray[col]) > 10000m)
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //物品名称不为空, 税关号为空
                        if (dt.Columns[col].Caption == "货物明细信息|税关号①" && string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) && !string.IsNullOrEmpty(dt.Rows[row].ItemArray[col - 1].ToString()))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        if (dt.Columns[col].Caption == "货物明细信息|税关号②" && string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) && !string.IsNullOrEmpty(dt.Rows[row].ItemArray[col - 1].ToString()))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        if (dt.Columns[col].Caption == "货物明细信息|税关号③" && string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) && !string.IsNullOrEmpty(dt.Rows[row].ItemArray[col - 1].ToString()))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //收件地邮编为空或不是有效的邮编
                        if (dt.Columns[col].Caption == "收件人信息|收件地邮编" && (string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) || !Regex.IsMatch(dt.Rows[row].ItemArray[col].ToString(), @"^\d{6}$")))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //收件人电话为空或不是有效的11位手机号码
                        if (dt.Columns[col].Caption == "收件人信息|收件人电话" && (string.IsNullOrEmpty(dt.Rows[row].ItemArray[col].ToString()) || !Regex.IsMatch(dt.Rows[row].ItemArray[col].ToString(), @"^1[1-9]\d{9}$")))
                        {
                            styleFlag = false;
                            ws.Cells[row + 1, col].SetStyle(errorStyle2);
                        }
                        //税金超出允许范围

                        //设置运单编号单元格样式
                        if (!styleFlag)
                        {
                            ws.Cells[row + 1, 1].SetStyle(errorStyle1);
                        }

                        //设置单元格数值列属性
                        if (numberColumns.Contains(dt.Columns[col].Caption))
                        {
                            //继承原有样式
                            Aspose.Cells.Style numberStyle = ws.Cells[row + 1, col].GetStyle();
                            numberStyle.Number = 4;
                            ws.Cells[row + 1, col].SetStyle(numberStyle);
                        }
                    }
                }
                //if (dt.Columns.Count > 0)
                //{
                //    ws.AutoFitColumns(); //自动适应宽度
                //}
                if (dt.Rows.Count > 0)
                {
                    ws.AutoFitRows(); //自动适应高度
                }

                if (saveFileType == EnumExcelFileType.Excel2003)
                {
                    workBook.Save(savePath, SaveFormat.Excel97To2003);
                }
                else
                {
                    workBook.Save(savePath, SaveFormat.Xlsx);
                }
            }
            catch (Exception e)
            {
                LogScopeHelper.Error(e.Message, e);
                return string.Empty;
            }
            return savePath;
        }

        /// <summary>
        /// 数据导出到Excel, 自定义excel样式
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="zeroColumns">需要清空零值的字段集合</param>
        /// <param name="redStyleColumns">需要设置红色背景的字段集合</param>
        /// <param name="centerStyleColumns">需要设置标题行文本居中的字段集合</param>
        /// <param name="contentLeftStyleColumns">需要设置内容靠左显示的字段集合</param>
        /// <param name="dictWidthColumns">导出excel列宽字典</param>
        /// <param name="filePath">保存文件夹路径</param>
        /// <param name="fileName">保存文件名称</param>
        /// <param name="saveFileType">保存文件类型（.xls;.xlsx）</param>
        /// <returns>文件物理路径</returns>
        public static string SaveDataTableToExcel2(DataTable dt, List<string> zeroColumns, List<string> redStyleColumns, List<string> centerStyleColumns, List<string> contentLeftStyleColumns, Dictionary<string, double> dictWidthColumns, string filePath, string fileName, EnumExcelFileType saveFileType)
        {
            string savePath = Path.Combine(filePath, fileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                Workbook workBook = new Workbook();
                workBook.Worksheets.Clear();
                workBook.Worksheets.Add(dt.TableName);
                Worksheet ws = workBook.Worksheets[dt.TableName];
                ws.FreezePanes(1, 1, 1, 0); //冻结第一行

                //设置标题样式
                Aspose.Cells.Style headerStyle = workBook.CreateStyle();
                headerStyle.HorizontalAlignment = TextAlignmentType.Right; //文本水平显示方式
                headerStyle.Font.Color = System.Drawing.Color.White; //字体颜
                headerStyle.Font.Name = "宋体"; //文字字体
                headerStyle.Font.Size = 10; //文字大小
                headerStyle.Font.IsBold = true; //粗体
                headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(91, 155, 213); //背景色
                headerStyle.Pattern = Aspose.Cells.BackgroundType.Solid; //单元格的线：实线 

                //设置内容列样式
                Aspose.Cells.Style contentStyle = workBook.CreateStyle();
                contentStyle.HorizontalAlignment = TextAlignmentType.Center; //文本水平显示方式
                //设置验证失败的内容样式1
                Aspose.Cells.Style errorStyle1 = workBook.CreateStyle();
                errorStyle1.Font.Color = System.Drawing.Color.Red;
                //设置验证失败的内容样式2
                Aspose.Cells.Style errorStyle2 = workBook.CreateStyle();
                headerStyle.ForegroundColor = System.Drawing.Color.Red;
                headerStyle.Pattern = Aspose.Cells.BackgroundType.Solid;

                int rowscount = dt.Rows.Count;
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    ws.Cells[0, col].PutValue(dt.Columns[col].Caption);

                    //设置标题行文本水平显示方式
                    if (centerStyleColumns.Contains(dt.Columns[col].Caption))
                        headerStyle.HorizontalAlignment = TextAlignmentType.Center;
                    else
                        headerStyle.HorizontalAlignment = TextAlignmentType.Right;
                    //设置标题行背景色
                    if (redStyleColumns.Contains(dt.Columns[col].Caption))
                        headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(192, 0, 0);
                    else
                        headerStyle.ForegroundColor = System.Drawing.Color.FromArgb(91, 155, 213);

                    //设置标题行样式
                    ws.Cells[0, col].SetStyle(headerStyle);
                    //设置列宽
                    ws.Cells.SetColumnWidth(col, dictWidthColumns[dt.Columns[col].Caption]);
                }
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    //设置内容行文本水平显示方式
                    if (contentLeftStyleColumns.Contains(dt.Columns[col].Caption))
                        contentStyle.HorizontalAlignment = TextAlignmentType.Left;
                    else
                        contentStyle.HorizontalAlignment = TextAlignmentType.Center;

                    if (zeroColumns.Contains(dt.Columns[col].Caption))
                    {
                        for (int row = 0; row < rowscount; row++)
                        {
                            if (dt.Rows[row].ItemArray[col].ToString() == "0" || dt.Rows[row].ItemArray[col].ToString() == "0.0" || dt.Rows[row].ItemArray[col].ToString() == "0.00")
                            {
                                ws.Cells[row + 1, col].PutValue("");
                            }
                            else
                            {
                                ws.Cells[row + 1, col].PutValue(dt.Rows[row].ItemArray[col].ToString());
                            }

                            //设置内容行样式
                            ws.Cells[row + 1, col].SetStyle(contentStyle);
                        }
                    }
                    else
                    {
                        for (int row = 0; row < rowscount; row++)
                        {
                            ws.Cells[row + 1, col].PutValue(dt.Rows[row].ItemArray[col].ToString());

                            //设置内容行样式
                            ws.Cells[row + 1, col].SetStyle(contentStyle);
                        }
                    }
                }
                //if (dt.Columns.Count > 0)
                //{
                //    ws.AutoFitColumns(); //自动适应宽度
                //}
                if (dt.Rows.Count > 0)
                {
                    ws.AutoFitRows(); //自动适应高度
                }

                if (saveFileType == EnumExcelFileType.Excel2003)
                {
                    workBook.Save(savePath, SaveFormat.Excel97To2003);
                }
                else
                {
                    workBook.Save(savePath, SaveFormat.Xlsx);
                }
            }
            catch (Exception e)
            {
                LogScopeHelper.Error(e.Message, e);
                return string.Empty;
            }
            return savePath;
        }

        /// <summary>
        /// 合并行
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="node"></param>
        /// <param name="startindex"></param>
        /// <param name="endRowIndex"></param>
        private static void MergeRowCell(Cells cells, LinkedListNode<int> node, int startindex, int endRowIndex)
        {
            if (node == null)
            {
                return;
            }
            int col = node.Value;
            Cell prevecell = cells[startindex, col];
            Cell firstCell = cells[startindex, col];
            int rowspan = 1;
            for (int i = startindex; i < endRowIndex; i++)
            {
                Cell currentcell = cells[i, col];
                if (currentcell == prevecell)
                {
                    continue;
                }
                if (currentcell.Value == null || prevecell.Value == null)
                {
                    return;
                }
                if (currentcell.Value.Equals(prevecell.Value) && i <= endRowIndex)
                {
                    rowspan++;
                    continue;
                }
                else
                {
                    cells.Merge(firstCell.Row, firstCell.Column, rowspan, 1);

                    MergeRowCell(cells, node.Next, firstCell.Row, currentcell.Row - 1);
                    prevecell = currentcell;
                    firstCell = currentcell;
                    rowspan = 1;
                }
            }
            if (rowspan > 1)
            {
                MergeRowCell(cells, node.Next, firstCell.Row, endRowIndex);
                cells.Merge(firstCell.Row, firstCell.Column, rowspan, 1);
            }
        }

        /// <summary>
        /// 合并行
        /// </summary>
        /// <param name="cells">表格</param>
        /// <param name="startRowindex">开始合并下标</param>
        /// <param name="col">开始列</param>
        /// <param name="extend">扩展列</param>
        public static void MergeRow(Cells cells, int startRowindex, int col, params int[] extend)
        {
            LinkedList<int> list = new LinkedList<int>();
            list.AddLast(new LinkedListNode<int>(col));
            for (int i = 0; i < extend.Length; i++)
            {
                LinkedListNode<int> node = new LinkedListNode<int>(extend[i]);
                list.AddLast(node);
            }
            MergeRowCell(cells, list.First, startRowindex, cells.Rows.Count);
        }
    }
}
