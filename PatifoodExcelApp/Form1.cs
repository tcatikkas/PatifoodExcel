using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Serialization;
using Excel = Microsoft.Office.Interop.Excel;
using System.Configuration;

namespace PatifoodExcelApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            logTxt.Text = "StartDate: " + dateTimePicker1.Value + " EndDate: " + dateTimePicker2.Value;

            logTxt.AppendText("\r\ngetOrders");
            var param = new Dictionary<string, string>();

            param.Add("act", "getOrders");
            param.Add("start_date", String.Format("{0:yyyy-MM-dd}", dateTimePicker1.Value));
            param.Add("end_date", String.Format("{0:yyyy-MM-dd}", dateTimePicker2.Value));

            var pageContent = HttpPostRequest(param);


            XmlSerializer serializer = new XmlSerializer(typeof(Orders.items));
            StringReader rdr = new StringReader(pageContent);
            Orders.items orders = (Orders.items)serializer.Deserialize(rdr);

            var errMessage = new List<string>();

            #region Categorys

            logTxt.AppendText("\r\ngetCategory");

            param = new Dictionary<string, string>();

            param.Add("act", "getCategory");
            param.Add("parent_id", "999999802");

            pageContent = HttpPostRequest(param);

            serializer = new XmlSerializer(typeof(Categorys.items));
            rdr = new StringReader(pageContent);
            Categorys.items categorys = (Categorys.items)serializer.Deserialize(rdr);

            #endregion

            if (string.IsNullOrEmpty(FileNameLbl.Text))
            {
                MessageBox.Show("Excel dosyasını seçiniz.");
                return;
            }

            logTxt.AppendText("\r\nOpen Excel");

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(FileNameLbl.Text);
            xlApp.Visible = false;

            NameValueCollection cnfgPages =
                (NameValueCollection)ConfigurationManager.GetSection("Sayfalar");

            //try
            //{
            Excel._Worksheet SheetKopek = xlWorkbook.Sheets[1];
            Excel._Worksheet SheetKedi = xlWorkbook.Sheets[2];
            Excel._Worksheet SheetYavruKopek = xlWorkbook.Sheets[3];
            Excel._Worksheet SheetKonserve = xlWorkbook.Sheets[4];
            Excel._Worksheet SheetSnoopy = xlWorkbook.Sheets[5];

            if (SheetKopek.AutoFilter != null) SheetKopek.AutoFilterMode = false;
            if (SheetKedi.AutoFilter != null) SheetKedi.AutoFilterMode = false;
            if (SheetYavruKopek.AutoFilter != null) SheetYavruKopek.AutoFilterMode = false;
            if (SheetKonserve.AutoFilter != null) SheetKonserve.AutoFilterMode = false;
            if (SheetSnoopy.AutoFilter != null) SheetSnoopy.AutoFilterMode = false;


            progressBar1.Maximum = orders.item.Count();

            bool hataVar = false;

            var say = 0;
            foreach (var orderItem in orders.item)
            {
                try
                {
                    logTxt.AppendText("\r\n");
                    logTxt.AppendText("OrderId:" + orderItem.order_id + " Name" + orderItem.firstname + " " + orderItem.lastname);
                    say++;
                    progressBar1.Value = say;
                    lblStatus.Text = say + "/" + progressBar1.Maximum;

                    if (orderItem.order_id == 82061)
                        MessageBox.Show("buldum");

                    //if (say == 94)
                    //    MessageBox.Show("");

                    if (orderItem.order_status_id != 1 && orderItem.order_status_id != 12)
                        continue;

                    #region OrderProducts
                    param = new Dictionary<string, string>();

                    logTxt.AppendText("\r\n");
                    logTxt.AppendText("getOrderProducts");

                    param.Add("act", "getOrderProducts");
                    param.Add("order_id", orderItem.order_id.ToString());

                    pageContent = HttpPostRequest(param);

                    serializer = new XmlSerializer(typeof(OrderProducts.items));
                    rdr = new StringReader(pageContent);
                    OrderProducts.items orderProducts = (OrderProducts.items)serializer.Deserialize(rdr);

                    // var orderProduct = orderProducts.item.First();

                    #endregion

                    foreach (var orderProduct in orderProducts.item)
                    {
                        try
                        {
                            logTxt.AppendText("\r\n");
                            logTxt.AppendText("OrderProductId:" + orderProduct.order_product_id + " Name" + orderProduct.name);

                            #region Product

                            logTxt.AppendText("\r\n");
                            logTxt.AppendText("getOrderProducts");

                            param = new Dictionary<string, string>();

                            param.Add("act", "getProduct");
                            param.Add("product_id", orderProduct.product_id.ToString());

                            pageContent = HttpPostRequest(param);

                            serializer = new XmlSerializer(typeof(Product.items));
                            rdr = new StringReader(pageContent);
                            Product.items product = (Product.items)serializer.Deserialize(rdr);

                            #endregion

                            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr-TR");

                            var productName = orderProduct.option_name.ToString(new System.Globalization.CultureInfo("tr-TR"));

                            if (categorys.item.Any(c => c.category_id == product.item.category_id))
                            {
                                foreach (var key in cnfgPages.AllKeys)
                                {
                                    if (productName.ToLower().Contains(key.ToLower()))
                                    {
                                        var pageNum = int.Parse(cnfgPages[key]);
                                        var sheet = xlWorkbook.Sheets[pageNum];
                                        if (sheet.AutoFilter != null) sheet.AutoFilterMode = false;

                                        int birim = 0;

                                        if (productName.ToLower().Contains("konserve"))
                                            birim = getAdet(productName);
                                        else
                                            birim = getKg(productName);

                                        AddRow(sheet, orderItem, orderProduct, birim);

                                        break;
                                    }
                                }


                                //if (productName.ToLower().Contains("konserve"))
                                //{
                                //    var ad = getAdet(orderProduct.option_name);

                                //    AddRow(SheetKonserve, orderItem, orderProduct, ad);
                                //}
                                //else if (productName.Contains("Yavru Köpek"))
                                //{
                                //    var kg = getKg(orderProduct.option_name);

                                //    AddRow(SheetYavruKopek, orderItem, orderProduct, kg);
                                //}
                                //else if (productName.Contains("Köpek"))
                                //{
                                //    var kg = getKg(orderProduct.option_name);

                                //    AddRow(SheetKopek, orderItem, orderProduct, kg);

                                //}
                                //else if (productName.Contains("Kedi"))
                                //{
                                //    var kg = getKg(orderProduct.option_name);

                                //    AddRow(SheetKedi, orderItem, orderProduct, kg);
                                //}
                                //else if (productName.Contains("Snoopy"))
                                //{
                                //    var kg = getKg(orderProduct.option_name);

                                //    AddRow(SheetSnoopy, orderItem, orderProduct, kg);

                                //}
                            }
                            else
                            {

                                if (string.IsNullOrEmpty(productName))
                                    productName = orderProduct.name.ToString(new System.Globalization.CultureInfo("tr-TR"));

                                var pageNum = int.Parse(cnfgPages["Diğer"]);

                                var sheet = xlWorkbook.Sheets[pageNum];

                                if (sheet.AutoFilter != null) sheet.AutoFilterMode = false;

                                int birim = 0;

                                if (productName.ToLower().Contains("adet"))
                                    birim = getAdet(productName);
                                else
                                    birim = getKg(productName);

                                AddRow(sheet, orderItem, orderProduct, birim);

                            }
                        }
                        catch (Exception ex)
                        {
                            hataVar = true;
                            logTxt.AppendText("\r\n");
                            logTxt.AppendText(ex.Message + ex.StackTrace);
                        }
                    }

                }
                catch(Exception ex)
                {
                    hataVar = true;
                    logTxt.AppendText("\r\n");
                    logTxt.AppendText(ex.Message + ex.StackTrace);
                }
            }
            //}
            //catch(Exception ex)
            //{
            //    xlWorkbook.Close();
            //    xlApp.Quit();

            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
            //    throw ex;
            //}

            if (hataVar)
                MessageBox.Show("Hata var.");

            xlWorkbook.Save();
            xlWorkbook.Close();
            xlApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
            MessageBox.Show("İşlem Tamamlandı.");
            //var json = Newtonsoft.Json.JsonConvert.DeserializeObject();


        }

        private void AddRow(Excel._Worksheet sheet, Orders.itemsItem orderItem, OrderProducts.itemsItem orderProduct, int birim)
        {
            var searchRange = sheet.get_Range("L1", "L" + sheet.Rows.Count);
            Excel.Range findRng = searchRange.Find(orderProduct.order_product_id,
                Missing.Value,
                Excel.XlFindLookIn.xlFormulas,
                Excel.XlLookAt.xlPart,
                Excel.XlSearchOrder.xlByRows,
                Excel.XlSearchDirection.xlNext,
                false,
                false,
                Missing.Value);

            if (findRng != null)
            {
                logTxt.AppendText("\r\n");
                logTxt.AppendText("Zaten kayıtlı.");
                return;
            }

            Excel.Range line = (Excel.Range)sheet.Rows[2];
            line.Insert();

            Excel.Range last = (Excel.Range)sheet.Rows[2];
            Excel.Range range = sheet.get_Range("A1", last);

            Excel.Range R1 = sheet.get_Range("A3", "M3");
            R1.Copy(Type.Missing);

            Excel.Range xlRange = sheet.UsedRange;

            var lastRow = last.Row;

            Excel.Range R2 = sheet.get_Range("A" + lastRow.ToString(), "M" + lastRow.ToString());
            R2.PasteSpecial(Excel.XlPasteType.xlPasteFormats,
                Excel.XlPasteSpecialOperation.xlPasteSpecialOperationMultiply, false, false);

            sheet.Cells[last.Row, 1] = orderItem.date_added;
            sheet.Cells[last.Row, 2] = orderItem.order_id;
            sheet.Cells[last.Row, 3] = orderItem.firstname;
            sheet.Cells[last.Row, 4] = orderItem.lastname;
            sheet.Cells[last.Row, 5] = orderProduct.option_name;
            sheet.Cells[last.Row, 6] = orderProduct.name;
            sheet.Cells[last.Row, 7] = orderProduct.quantity;
            sheet.Cells[last.Row, 12] = orderProduct.order_product_id;
            sheet.Cells[last.Row, 13] = orderItem.order_status_id;
            sheet.Cells[last.Row, 14] = orderProduct.option_code;

            sheet.Cells[last.Row, 8] = birim;
            sheet.Cells[last.Row, 9] = birim * orderProduct.quantity;
            sheet.Cells[last.Row, 18] = orderProduct.product_id.ToString();
        }

        private int getKg(string productName)
        {
            try
            {
                var kgIndex = productName.ToLower().IndexOf("kg") - 1;
                var spaceIndex = productName.Substring(0, kgIndex).LastIndexOf(' ') + 1;

                var kg = productName.Substring(spaceIndex, kgIndex - spaceIndex);
                int result = 0;
                int.TryParse(kg, out result);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private int getAdet(string productName)
        {
            try
            {
                var adIndex = productName.ToLower().IndexOf("adet") - 1;
                var spaceIndex = productName.Substring(0, adIndex).LastIndexOf(' ') + 1;

                var ad = productName.Substring(spaceIndex, adIndex - spaceIndex);

                ad = ad.Replace('(', ' ').Trim();

                int result = 0;
                int.TryParse(ad, out result);
                return result;
            }
            catch (Exception ex)
            {
                return 0;

            }
        }

        private string HttpPostRequest(Dictionary<string, string> postParameters)
        {
            string postData = "";

            postData += HttpUtility.UrlEncode("password") + "="
                     + HttpUtility.UrlEncode("mikro2255.?") + "&";
            postData += HttpUtility.UrlEncode("username") + "="
                     + HttpUtility.UrlEncode("mikro") + "&";

            foreach (string key in postParameters.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postParameters[key]) + "&";
            }

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://www.patifood.com/index.php?page=module%2Fwebservice");
            myHttpWebRequest.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = data.Length;

            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();

            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.UTF8);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();

            return pageContent;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel Dosyası |*.xlsx";
            var result = file.ShowDialog();

            if (result == DialogResult.OK)
            {
                FileNameLbl.Text = file.FileName;
            }
        }
    }
}
