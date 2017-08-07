using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AudioView.Common.Export
{
    public class ExcelExport
    {
        public IList<Reading> readings { get; set; }
        public Project project { get; set; }

        public ExcelExport(Project project, IList<Reading> readings)
        {
            this.project = project;
            this.readings = readings;
        }

        public XLWorkbook createExecel()
        {
            XLWorkbook workbook = null;
            try
            {
                workbook = new XLWorkbook();
                workbook.Style.Font.FontName = "Arial";
                workbook.Style.Font.FontSize = 11;

                createCoverWorksheet(workbook.Worksheets.Add("COVER"));
                createMinorIntervalWorkheet(workbook.Worksheets.Add("Minor Interval Data"));
                createMajorIntervalWorkheet(workbook.Worksheets.Add("Major Interval Data"));
                createAllDataWorkheet(workbook.Worksheets.Add("All Data"));
            }
            catch (Exception exp)
            {
                
            }
            return workbook;
        }

        private void createAllDataWorkheet(IXLWorksheet worksheet)
        {
            worksheet.Row(1).Style.Alignment.WrapText = true;
            worksheet.Row(1).Height = 15;
            worksheet.Row(1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(1).Style.Font.Bold = true;

            worksheet.Row(2).Style.Alignment.WrapText = true;
            worksheet.Row(2).Height = 31;
            worksheet.Row(2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Row(2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(2).Style.Font.Bold = true;

            worksheet.Column("A").Width = 13;
            worksheet.Column("B").Width = 13;
            worksheet.Column("C").Width = 13;

            worksheet.Cell(2, 1).Value = "DATE";
            addRightBottomBorder(worksheet.Cell(2, 1));
            worksheet.Cell(2, 2).Value = "TIME";
            addRightBottomBorder(worksheet.Cell(2, 2));
            worksheet.Cell(2, 3).Value = "DURATION";
            addRightBottomBorder(worksheet.Cell(2, 3));
            worksheet.Cell(2, 4).Value = "LAeq";
            addRightBottomBorder(worksheet.Cell(2, 4));
            worksheet.Cell(2, 5).Value = "LAMax";
            addRightBottomBorder(worksheet.Cell(2, 5));
            worksheet.Cell(2, 6).Value = "LAMin";
            addRightBottomBorder(worksheet.Cell(2, 6));
            worksheet.Cell(2, 7).Value = "LZMax";
            addRightBottomBorder(worksheet.Cell(2, 7));
            worksheet.Cell(2, 8).Value = "LZMin";
            addRightBottomBorder(worksheet.Cell(2, 8));

            int oneThirdStart = 9;
            int col = oneThirdStart;
            Type oneThird = typeof(ReadingData.OctaveBandOneThird);
            foreach (var propertyInfo in oneThird.GetProperties())
            {
                worksheet.Cell(2, col).Value = propertyInfo.Name.Replace("_", ".").Replace("Hz", "") + "Hz";
                addRightBottomBorder(worksheet.Cell(2, col));

                col++;
            }
            worksheet.Cell(1, oneThirdStart).Value = "1/3 Octave Band LZeq,t";
            worksheet.Range(worksheet.Cell(1, oneThirdStart), worksheet.Cell(1, col - 1)).Merge();


            int oneOneStart = col;
            Type oneOne = typeof(ReadingData.OctaveBandOneOne);
            foreach (var propertyInfo in oneOne.GetProperties())
            {
                worksheet.Cell(2, col).Value = propertyInfo.Name.Replace("_", ".").Replace("Hz", "") + "Hz";
                addRightBottomBorder(worksheet.Cell(2, col));
                col++;
            }
            worksheet.Cell(1, oneOneStart).Value = "1/1 Octave Band LZeq,t";
            worksheet.Range(worksheet.Cell(1, oneOneStart), worksheet.Cell(1, col - 1)).Merge();

            worksheet.Row(1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Style.Border.BottomBorderColor = XLColor.Black;


            int index = 3;
            foreach (var r in readings.OrderBy(x => x.Time))
            {
                worksheet.Cell(index, 1).Value = r.Time.ToString("dd/MM/yyyy");
                addRightBottomBorder(worksheet.Cell(index, 1));
                string vale = r.Time.ToString("HH:mm:ss");
                worksheet.Cell(index, 2).Value = r.Time.ToString("HH:mm:ss");
                addRightBottomBorder(worksheet.Cell(index, 2));
                worksheet.Cell(index, 3).Value = r.Major ? project.MajorInterval : project.MinorInterval;
                addRightBottomBorder(worksheet.Cell(index, 3));
                worksheet.Cell(index, 4).Value = oneDig(r.Data.LAeq);
                addRightBottomBorder(worksheet.Cell(index, 4));
                worksheet.Cell(index, 5).Value = oneDig(r.Data.LAMax);
                addRightBottomBorder(worksheet.Cell(index, 5));
                worksheet.Cell(index, 6).Value = oneDig(r.Data.LAMin);
                addRightBottomBorder(worksheet.Cell(index, 6));
                worksheet.Cell(index, 7).Value = oneDig(r.Data.LZMax);
                addRightBottomBorder(worksheet.Cell(index, 7));
                worksheet.Cell(index, 8).Value = oneDig(r.Data.LZMin);
                addRightBottomBorder(worksheet.Cell(index, 8));

                col = 9;
                foreach (var propertyInfo in oneThird.GetProperties())
                {
                    worksheet.Cell(index, col).Value = oneDig((Double)propertyInfo.GetValue(r.Data.LAeqOctaveBandOneThird));
                    addRightBottomBorder(worksheet.Cell(index, col));

                    col++;
                }
                foreach (var propertyInfo in oneOne.GetProperties())
                {
                    worksheet.Cell(index, col).Value = oneDig((Double)propertyInfo.GetValue(r.Data.LAeqOctaveBandOneOne));
                    addRightBottomBorder(worksheet.Cell(index, col));
                    col++;
                }

                worksheet.Row(index).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                index++;
            }
        }

        private void createMajorIntervalWorkheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 20;
            worksheet.Column("B").Width = 20;
            worksheet.Column("C").Width = 20;

            worksheet.Row(1).Height = 22.5;
            worksheet.Row(1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(1).Style.Font.Bold = true;

            worksheet.Cell("A1").Value = "DATE";
            addRightBottomBorder(worksheet.Cell("A1"));
            worksheet.Cell("B1").Value = "TIME";
            addRightBottomBorder(worksheet.Cell("B1"));
            worksheet.Cell("C1").Value = "dB LAeq,t";
            addBottomBorder(worksheet.Cell("C1"));

            int index = 1;
            foreach (var r in readings.Where(x => x.Major).OrderBy(x => x.Time))
            {
                index++;

                worksheet.Cell("A" + index).Value = r.Time.ToString("dd/MM/yy");
                addRightBottomBorder(worksheet.Cell("A" + index));
                worksheet.Cell("B" + index).Value = r.Time.ToString("HH:mm:ss");
                addRightBottomBorder(worksheet.Cell("B" + index));
                worksheet.Cell("C" + index).Value = Math.Round(r.Data.LAeq, 1).ToString();
                addRightBottomBorder(worksheet.Cell("C" + index));

                worksheet.Row(index).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
        }

        private string oneDig(Double d)
        {
            return Math.Round(d, 1).ToString();
        }

        private void createMinorIntervalWorkheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 20;
            worksheet.Column("B").Width = 20;
            worksheet.Column("C").Width = 20;

            worksheet.Row(1).Height = 22.5;
            worksheet.Row(1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(1).Style.Font.Bold = true;

            worksheet.Cell("A1").Value = "DATE";

            addRightBottomBorder(worksheet.Cell("A1"));
            worksheet.Cell("B1").Value = "TIME";

            addRightBottomBorder(worksheet.Cell("B1"));
            worksheet.Cell("C1").Value = "dB LAeq,t";
            addBottomBorder(worksheet.Cell("C1"));

            int index = 1;
            foreach (var r in readings.Where(x=>!x.Major).OrderBy(x=>x.Time))
            {
                index++;

                worksheet.Cell("A" + index).Value = r.Time.ToString("dd/MM/yy");
                addRightBottomBorder(worksheet.Cell("A" + index));
                worksheet.Cell("B" + index).Value = r.Time.ToString("HH:mm:ss");
                addRightBottomBorder(worksheet.Cell("B" + index));
                worksheet.Cell("C" + index).Value = Math.Round(r.Data.LAeq, 1).ToString();
                addBottomBorder(worksheet.Cell("C" + index));

                worksheet.Row(index).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
        }

        private void createCoverWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Style.Font.Bold = true;
            worksheet.Column("A").Style.Font.FontSize = 11;
            worksheet.Column("A").Style.Font.FontName = "Arial";


            worksheet.Column("A").Width = 35;
            worksheet.Column("B").Width = 26;
            worksheet.Row(1).Height = 116;
            worksheet.Cell("A1").Value = "AUDIOVIEW EXPORT";
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Range("A1","B1").Merge();

        
            worksheet.Cell("A2").Value = "PROJECT NAME";
            worksheet.Cell("B2").Value = project.Name;
            worksheet.Cell("A3").Value = "PROJECT NUMBER";
            worksheet.Cell("B3").Value = project.Number;
            worksheet.Cell("A4").Value = "DATE";
            worksheet.Cell("B4").Value = project.Created.ToString();
            worksheet.Cell("A5").Value = "MINOR INTERVAL PERIOD";
            worksheet.Cell("B5").Value = project.MinorInterval.ToString();
            worksheet.Cell("A6").Value = "MINOR INTERVAL LIMIT";
            worksheet.Cell("B6").Value = project.MinorDBLimit;
            worksheet.Cell("A7").Value = "MAJOR INTERVAL PERIOD";
            worksheet.Cell("B7").Value = project.MajorInterval.ToString();
            worksheet.Cell("A8").Value = "MAJOR INTERVAL LIMIT";
            worksheet.Cell("B8").Value = project.MajorDBLimit;

            worksheet.Style.Fill.BackgroundColor = XLColor.White;
            worksheet.Columns("A", "B").Style.Fill.BackgroundColor = XLColor.White;
            worksheet.Rows(1, 8).Style.Fill.BackgroundColor = XLColor.White;
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.White;
            worksheet.Column("B").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        }

        private void addRightBottomBorder(IXLCell cell, XLBorderStyleValues border = XLBorderStyleValues.Thick, XLColor color = null)
        {
            if (color == null)
            {
                color = XLColor.Black;
            }

            addBottomBorder(cell, border, color);
            addRightBorder(cell, border, color);
        }
        private void addRightBorder(IXLCell cell, XLBorderStyleValues border = XLBorderStyleValues.Thick, XLColor color = null)
        {
            if (color == null)
            {
                color = XLColor.Black;
            }

            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.RightBorderColor = XLColor.Black;
        }

        private void addBottomBorder(IXLCell cell, XLBorderStyleValues border = XLBorderStyleValues.Thick, XLColor color = null)
        {
            if (color == null)
            {
                color = XLColor.Black;
            }

            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = XLColor.Black;
        }

        public void writeFile(string path)
        {
            var excel = createExecel();
            excel.SaveAs(path);
        }

        public void writeStream(Stream memoryStream)
        {
            var excel = createExecel();
            excel.SaveAs(memoryStream);
        }
    }
}
