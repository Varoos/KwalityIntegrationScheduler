using KwalityIntegrationScheduler.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KwalityIntegrationScheduler
{
    public partial class Form1 : Form
    {
        string baseUrl = ConfigurationManager.AppSettings["Server_API_IP"];
        static int compId = BL_Configdata.Focus8CompID;
        string sessionID = GetSessionId(compId);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            string intvl = ConfigurationManager.AppSettings["Interval"];
            timer1.Interval = Convert.ToInt32(intvl) * 1000;
            timer1.Enabled = true;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                StkTransferPIC();
                StkTransfer_KIF();
                StkTransfer_TB();

            }
            catch (Exception ex)
            {
                BL_Registry.SetLog(ex.ToString());
            }
        }

        private void StkTransferPIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getStkTransferPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id", 3 }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToInt32(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferPIC", docno);
                        }
                    }
                }

            }
        }

        private void StkTransfer_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getStkTransferKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsKIF.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id", 4 }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferKIF", docno);
                        }
                    }
                }

            }
        }

        private void StkTransfer_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getStkTransferTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsTB.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id",5}
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Issue - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferTB", docno);
                        }
                    }
                }

            }
        }


        private void StkTransferRet_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getStkTransferRetPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferRetPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id", 3 }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferRetPIC", docno);
                        }
                    }
                }

            }
        }

        private void StkTransferRet_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getStkTransferRetKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferRetKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsKIF.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id", 4 }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferRetKIF", docno);
                        }
                    }
                }

            }
        }

        private void StkTransferRet_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getStkTransferRetTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getStkTransferRetTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsTB.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string Warehouse = Pic["Warehouse"].ToString().Trim();
                    string Salesman = Pic["Salesman"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "Warehouse__Code", Warehouse},
                                         { "Warehouse From__Code", Warehouse },
                                         { "Warehouse To__Code", Salesman },
                                         { "Company Master__Id",5}
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Stock Transfer Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferRetTB", docno);
                        }
                    }
                }

            }
        }


        private void SalesInvoice_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getSalesInvoicePIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesInvoicePIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string SalesAccount = Pic["SalesAccount"].ToString().Trim();
                    string CustomerAC = Pic["CustomerAC"].ToString().Trim();
                    string DueDate = Pic["intDueDate"].ToString().Trim();
                    string WarehouseCode = Pic["Salesman"].ToString().Trim();
                    string RouteCode = Pic["Salesman"].ToString().Trim();
                    string Narration = Pic["Narration"].ToString().Trim();
                    string Grp = Pic["Grp"].ToString().Trim();
                    string LPONO = Pic["PONo"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         { "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iSalesAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Invoice - VAN";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Sales Invoice - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferRetPIC", docno);
                        }
                    }
                }

            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
        public static string getServiceLink()
        {
            XmlDocument xmlDoc = new XmlDocument();
            string strFileName = "";
            string sAppPath = BL_Configdata.Focus8Path;
            strFileName = sAppPath + "\\ERPXML\\ServerSettings.xml";

            xmlDoc.Load(strFileName);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/ServSetting/MasterServer/ServerName");
            string strValue;
            XmlNode node = nodeList[0];
            if (node != null)
                strValue = node.InnerText;
            else
                strValue = "";
            return strValue;
        }

        public static string GetSessionId(int CompId)
        {
            string sSessionId = "";
            try
            {
                string strServer = getServiceLink();
                int ccode = CompId;
                string User_Name = BL_Configdata.UserName;
                string Password = BL_Configdata.Password;


                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + strServer + "/focus8api/Login");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{" + "\"data\": [{" + "\"Username\":\"" + User_Name + "\"," + "\"password\":\"" + Password + "\"," + "\"CompanyId\":\"" + ccode + "\"}]}";
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader Updatereader = new StreamReader(httpResponse.GetResponseStream());
                string Udtcontent = Updatereader.ReadToEnd();

                JObject odtbj = JObject.Parse(Udtcontent);
                Temperatures Updtresult = JsonConvert.DeserializeObject<Temperatures>(Udtcontent);
                if (Updtresult.Result == 1)
                {
                    sSessionId = Updtresult.Data[0].FSessionId;
                }


                return sSessionId;
            }
            catch (Exception ex)
            {
                BL_Registry.SetLog(ex.ToString());
            }
            return sSessionId;
        }
        public partial class Datum
        {
            [JsonProperty("fSessionId")]
            public string FSessionId { get; set; }
        }
        public partial class Temperatures
        {
            [JsonProperty("data")]
            public Datum[] Data { get; set; }

            [JsonProperty("url")]
            public Uri Url { get; set; }

            [JsonProperty("result")]
            public long Result { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
        public class HashData
        {
            public string url { get; set; }
            public List<Hashtable> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }
    }
}
