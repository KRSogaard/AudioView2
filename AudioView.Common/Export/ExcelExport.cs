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
            worksheet.Column("A").Width = 30;
            worksheet.Column("B").Width = 30;
            worksheet.Column("C").Width = 30;

            worksheet.Cell(1, 1).Value = "DATE";
            addRightBottomBorder(worksheet.Cell(1, 1));
            worksheet.Cell(1, 2).Value = "TIME";
            addRightBottomBorder(worksheet.Cell(1, 2));
            worksheet.Cell(1, 3).Value = "LAeq";
            addRightBottomBorder(worksheet.Cell(1, 3));
            worksheet.Cell(1, 4).Value = "LAMax";
            addRightBottomBorder(worksheet.Cell(1, 4));
            worksheet.Cell(1, 5).Value = "LAMin";
            addRightBottomBorder(worksheet.Cell(1, 5));
            worksheet.Cell(1, 6).Value = "LZMax";
            addRightBottomBorder(worksheet.Cell(1, 6));
            worksheet.Cell(1, 7).Value = "LZMin";
            addRightBottomBorder(worksheet.Cell(1, 7));

            int col = 8;
            Type oneThird = typeof(ReadingData.OctaveBandOneThird);
            foreach (var propertyInfo in oneThird.GetProperties())
            {
                worksheet.Cell(1, col).Value = "1/3 " + propertyInfo.Name.Replace("_", ".").Replace("Hz", "") + " Hz";
                addRightBottomBorder(worksheet.Cell(1, col));

                col++;
            }
            Type oneOne = typeof(ReadingData.OctaveBandOneOne);
            foreach (var propertyInfo in oneOne.GetProperties())
            {
                worksheet.Cell(1, col).Value = "1/1 " + propertyInfo.Name.Replace("_", ".").Replace("Hz", "") + " Hz";
                addRightBottomBorder(worksheet.Cell(1, col));
                col++;
            }

            int index = 2;
            foreach (var r in readings.OrderBy(x => x.Time))
            {
                worksheet.Cell(index, 1).Value = r.Time.ToString("dd/MM/yyyy");
                addRightBottomBorder(worksheet.Cell(index, 1));
                worksheet.Cell(index, 2).Value = r.Time.ToString("HH:mm");
                addRightBottomBorder(worksheet.Cell(index, 2));
                worksheet.Cell(index, 3).Value = oneDig(r.Data.LAeq);
                addRightBottomBorder(worksheet.Cell(index, 3));
                worksheet.Cell(index, 4).Value = oneDig(r.Data.LAMax);
                addRightBottomBorder(worksheet.Cell(index, 4));
                worksheet.Cell(index, 5).Value = oneDig(r.Data.LAMin);
                addRightBottomBorder(worksheet.Cell(index, 5));
                worksheet.Cell(index, 6).Value = oneDig(r.Data.LZMax);
                addRightBottomBorder(worksheet.Cell(index, 6));
                worksheet.Cell(index, 7).Value = oneDig(r.Data.LZMin);
                addRightBottomBorder(worksheet.Cell(index, 7));

                col = 8;
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

                index++;
            }
        }

        private void createMajorIntervalWorkheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 30;
            worksheet.Column("B").Width = 30;
            worksheet.Column("C").Width = 30;

            worksheet.Cell("A1").Value = "DATE";
            addRightBottomBorder(worksheet.Cell("A1"));
            worksheet.Cell("B1").Value = "TIME";
            addRightBottomBorder(worksheet.Cell("B1"));
            worksheet.Cell("C1").Value = "dB LAeq, xx min";
            addBottomBorder(worksheet.Cell("C1"));

            int index = 1;
            foreach (var r in readings.Where(x => x.Major).OrderBy(x => x.Time))
            {
                index++;

                worksheet.Cell("A" + index).Value = r.Time.ToString("dd/MM/yy");
                addRightBottomBorder(worksheet.Cell("A" + index));
                worksheet.Cell("B" + index).Value = r.Time.ToString("HH:mm");
                addRightBottomBorder(worksheet.Cell("B" + index));
                worksheet.Cell("C" + index).Value = Math.Round(r.Data.LAeq, 1).ToString();
                addRightBottomBorder(worksheet.Cell("C" + index));
            }
        }

        private string oneDig(Double d)
        {
            return Math.Round(d, 1).ToString();
        }

        private void createMinorIntervalWorkheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 30;
            worksheet.Column("B").Width = 30;
            worksheet.Column("C").Width = 30;

            worksheet.Cell("A1").Value = "DATE";
            addRightBottomBorder(worksheet.Cell("A1"));
            worksheet.Cell("B1").Value = "TIME";
            addRightBottomBorder(worksheet.Cell("B1"));
            worksheet.Cell("C1").Value = "dB LAeq, xx min";
            addBottomBorder(worksheet.Cell("C1"));

            int index = 1;
            foreach (var r in readings.Where(x=>!x.Major).OrderBy(x=>x.Time))
            {
                index++;

                worksheet.Cell("A" + index).Value = r.Time.ToString("dd/MM/yy");
                addRightBottomBorder(worksheet.Cell("A" + index));
                worksheet.Cell("B" + index).Value = r.Time.ToString("HH:mm");
                addRightBottomBorder(worksheet.Cell("B" + index));
                worksheet.Cell("C" + index).Value = Math.Round(r.Data.LAeq, 1).ToString();
                addBottomBorder(worksheet.Cell("C" + index));
            }
        }

        private void createCoverWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 21;
            worksheet.Column("B").Width = 26;
            worksheet.Cell("A1").Value = "AUDIOVIEW EXPORT";
            worksheet.Range("A1","B1").Merge();
            addBottomBorder(worksheet.Cell("A1"));
            addBottomBorder(worksheet.Cell("B1"));

            worksheet.Cell("A2").Value = "PROJECT NAME";
            addRightBottomBorder(worksheet.Cell("A2"));

            worksheet.Cell("B2").Value = project.Name;
            addBottomBorder(worksheet.Cell("B2"));
            worksheet.Cell("A3").Value = "PROJECT NUMBER";
            addRightBottomBorder(worksheet.Cell("A3"));
            worksheet.Cell("B3").Value = project.Number;
            addBottomBorder(worksheet.Cell("B3"));
            worksheet.Cell("A4").Value = "DATE";
            addRightBottomBorder(worksheet.Cell("A4"));
            worksheet.Cell("B4").Value = project.Created.ToString();
            addBottomBorder(worksheet.Cell("B4"));
            worksheet.Cell("A5").Value = "MINOR INTERVAL PERIOD";
            addRightBottomBorder(worksheet.Cell("A5"));
            worksheet.Cell("B5").Value = project.MinorInterval.ToString();
            addBottomBorder(worksheet.Cell("B5"));
            worksheet.Cell("A6").Value = "MINOR INTERVAL LIMIT";
            addRightBottomBorder(worksheet.Cell("A6"));
            worksheet.Cell("B6").Value = project.MinorDBLimit;
            addBottomBorder(worksheet.Cell("B6"));
            worksheet.Cell("A7").Value = "MAJOR INTERVAL PERIOD";
            addRightBottomBorder(worksheet.Cell("A7"));
            worksheet.Cell("B7").Value = project.MajorInterval.ToString();
            addBottomBorder(worksheet.Cell("B7"));
            worksheet.Cell("A8").Value = "MAJOR INTERVAL LIMIT";
            addRightBottomBorder(worksheet.Cell("A8"));
            worksheet.Cell("B8").Value = project.MajorDBLimit;
            addBottomBorder(worksheet.Cell("B8"));
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

            cell.Style.Border.RightBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.RightBorderColor = XLColor.Black;
        }
        private void addBottomBorder(IXLCell cell, XLBorderStyleValues border = XLBorderStyleValues.Thick, XLColor color = null)
        {
            if (color == null)
            {
                color = XLColor.Black;
            }

            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
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
