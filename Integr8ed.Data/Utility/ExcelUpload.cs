using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Integr8ed.Data.DbModel.ClientAdmin;
//using Excel =  Microsoft.Office.Interop.Excel;

namespace Integr8ed.Data.Utility
{
    public class VisitorDtoExcel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Notes { get; set; }
        public bool IsView { get; set; }
        public bool IsEmailEdit { get; set; }
    }
    public class ExcelUpload
    {
        public void ReadExcelFile(string filepath)
        {
            try
            {
                //Lets open the existing excel file and read through its content . Open the excel using openxml sdk
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filepath + "\\AddVisitor.xlsx", false))
                {
                    //create the object for workbook part  
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
                    StringBuilder excelResult = new StringBuilder();

                    //using for each loop to get the sheet from the sheetcollection  
                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        excelResult.AppendLine("Excel Sheet Name : " + thesheet.Name);
                        excelResult.AppendLine("----------------------------------------------- ");
                        //statement to get the worksheet object by using the sheet id  
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();
                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                //statement to take the integer value  
                                string currentcellvalue = string.Empty;
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {

                                                //Visitors MT = new Visitors();
                                                //MT.Description = model.Description;
                                                //MT.Name = model.Name;
                                                //MT.SurName = model.SurName;
                                                //MT.Address = model.Address;
                                                //MT.PostCode = model.PostCode;
                                                //MT.Email = model.Email;
                                                //MT.Telephone = model.Telephone;
                                                //MT.Mobile = model.Mobile;
                                                //MT.Notes = model.Notes;
                                                //code to take the string value  
                                                excelResult.Append(item.Text.Text + " ");
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                currentcellvalue = item.InnerText;
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                currentcellvalue = item.InnerXml;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    excelResult.Append(Convert.ToInt16(thecurrentcell.InnerText) + " ");
                                }
                            }
                            excelResult.AppendLine();
                        }
                        excelResult.Append("");
                        Console.WriteLine(excelResult.ToString());
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        //public List<VisitorDtoExcel> getVisitor(string filepath)
        //{

        //    List<VisitorDtoExcel> VisitorList = new List<VisitorDtoExcel>();
        //    int rowsCount, columnsCount;
        //    Excel.Application oExcel = new Excel.Application();
        //    Excel.Workbook WB = oExcel.Workbooks.Open(filepath);
        //    string ExcelWorkbookname = WB.Name;
        //    int worksheetcount = WB.Worksheets.Count;
        //    Excel.Worksheet wks = (Excel.Worksheet)WB.Worksheets[1];

        //    Excel.Range xlRange;
        //    xlRange = wks.UsedRange;
        //    List<string> columns = new List<string>();
        //    try
        //    {



        //        string firstworksheetname = wks.Name;
        //        columnsCount = ExcelColumnCount(wks);
        //        rowsCount = ExcelRowCount(wks);


        //        for (int xlRow = 1  ; xlRow <= rowsCount; xlRow++)
        //        {
        //            string[] temparray;
        //            List<string> regionarray = new List<string>();
        //            List<string> amountArray = new List<string>();

        //            var VisitorExcel = new VisitorDtoExcel();

        //            VisitorExcel.Name = xlRange.Cells[xlRow, 1].ToString();
        //            VisitorExcel.SurName = xlRange.Cells[xlRow, 2].ToString();
        //            VisitorExcel.PostCode = xlRange.Cells[xlRow, 3].ToString();
        //            VisitorExcel.Email = xlRange.Cells[xlRow, 4].ToString();
        //            VisitorExcel.Telephone = xlRange.Cells[xlRow, 5].ToString();
        //            VisitorExcel.Mobile = xlRange.Cells[xlRow, 6].ToString();
        //             VisitorExcel.Notes = xlRange.Cells[xlRow, 7].ToString();
        //            VisitorExcel.Address = xlRange.Cells[xlRow, 8].ToString();


        //            VisitorList.Add(VisitorExcel);
        //        }
        //        WB.Close();
        //        oExcel.Quit();

        //        if (xlRange != null) Marshal.ReleaseComObject(xlRange);
        //        if (wks != null) Marshal.ReleaseComObject(wks);
        //        if (WB != null) Marshal.ReleaseComObject(WB);
        //        if (oExcel != null) Marshal.ReleaseComObject(oExcel);

        //    }
        //    catch (Exception ex)
        //    {
        //        WB.Close();
        //        oExcel.Quit();

        //        if (xlRange != null) Marshal.ReleaseComObject(xlRange);
        //        if (wks != null) Marshal.ReleaseComObject(wks);
        //        if (WB != null) Marshal.ReleaseComObject(WB);
        //        if (oExcel != null) Marshal.ReleaseComObject(oExcel);
        //        string error = ex.Message;
        //    }
        //    // Object[] arrays=new object[2];
        //    //arrays[0] = VisitorExcelsList;
        //    //arrays[1] = dataList;
        //    return VisitorList;
        //}



        //public int ExcelRowCount(Excel.Worksheet xlWorkSheet)
        //{
        //    int rowCount = xlWorkSheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                        System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                        Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlPrevious,
        //                        false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;
        //    return rowCount;
        //}

        //public int ExcelColumnCount(Excel.Worksheet xlWorkSheet)
        //{
        //    int columnCount = xlWorkSheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                        System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                        Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlPrevious,
        //                        false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;
        //    return columnCount;
        //}

    }

}