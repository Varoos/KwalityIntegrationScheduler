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
                StkTransferRet_PIC();
                StkTransferRet_KIF();
                StkTransferRet_TB();
                SalesInvoice_PIC();
                SalesInvoice_KIF();
                SalesInvoice_TB();
                SalesReturn_PIC();
                SalesReturn_KIF();
                SalesReturn_TB();
                DamageStock_PIC();
                DamageStock_KIF();
                DamageStock_TB();
                Receipts_PIC();
                Receipts_KIF();
                Receipts_TB();
                PDCReceipts_PIC();
                PDCReceipts_KIF();
                PDCReceipts_TB();
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
                    BL_Registry.SetLog("docno " + docno);
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
                    BL_Registry.SetLog("StkTransferPIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferPIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferPIC body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferPIC Content "+ sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    BL_Registry.SetLog("StkTransferPIC url " + Url1);
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
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransferPIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransferPIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    BL_Registry.SetLog("StkTransferKIF Header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferKIF rows count "+ rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferKIF BOdy Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferKIF Content "+ sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    BL_Registry.SetLog("StkTransferKIF url  " + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("StkTransferKIF posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("StkTransferKIF posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog(" StkTransferKIF Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("StkTransferKIF Stock Transfer Issue - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransfer_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransfer_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    BL_Registry.SetLog("StkTransferTB Header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferTB rows count "+rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferTB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferTB Content "+sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Issue - VAN";
                    BL_Registry.SetLog("StkTransferTB url "+Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("StkTransferTB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("StkTransferTB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("StkTransferTB Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Issue - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("StkTransferTB Stock Transfer Issue - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransfer_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransfer_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    BL_Registry.SetLog("StkTransferRetPIC Header Data Ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferRetPIC rows count "+rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferRetPIC Body Data Ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferRetPIC Content "+ sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    BL_Registry.SetLog("StkTransferRetPIC url " + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("StkTransferRetPIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("StkTransferRetPIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("StkTransferRetPIC Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Stock Transfer Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("StkTransferRetPIC Stock Transfer Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setStkTransferRetPIC", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransferRet_PIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransferRet_PIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    BL_Registry.SetLog("StkTransferRetKIF Header Data Ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferRetKIF rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferRetKIF Body Data Ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferRetKIF Content "+sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    BL_Registry.SetLog("StkTransferRetKIF url "+Url1);
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
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransferRet_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransferRet_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    BL_Registry.SetLog("StkTransferRetTB Header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("StkTransferRetTB rows count "+rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("StkTransferRetTB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("StkTransferRetTB Content "+ sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Stock Transfer Return - VAN";
                    BL_Registry.SetLog("StkTransferRetTB url "+Url1);
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
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" StkTransferRet_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" StkTransferRet_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
                    string Jurisdiction = Pic["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = Pic["PlaceOfSupply"].ToString().Trim();
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
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesInvoice_PIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesInvoice_PIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesInvoice_PIC Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesInvoice_PIC Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Invoice - VAN";
                    BL_Registry.SetLog("SalesInvoice_PIC post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesInvoice_PIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" SalesInvoice_PIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("SalesInvoice_PIC Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " SalesInvoice_PIC Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" SalesInvoice_PIC Sales Invoice - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesInvoicePIC", docno);
                            if(d == 1)
                            {
                                BL_Registry.SetLog(" SalesInvoice_PIC FOCUS_WINIT DB updation successed with DocNo = "+docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" SalesInvoice_PIC FOCUS_WINIT DB Updation failed with DocNo="+docno);
                            }
                        }
                    }
                }

            }
        }

        private void SalesInvoice_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getSalesInvoiceKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesInvoiceKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow KIF in dsKIF.Tables[0].Rows)
                {
                    docno = KIF["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = KIF["intDate"].ToString().Trim();
                    string SalesAccount = KIF["SalesAccount"].ToString().Trim();
                    string CustomerAC = KIF["CustomerAC"].ToString().Trim();
                    string DueDate = KIF["intDueDate"].ToString().Trim();
                    string WarehouseCode = KIF["Salesman"].ToString().Trim();
                    string RouteCode = KIF["Salesman"].ToString().Trim();
                    string Narration = KIF["Narration"].ToString().Trim();
                    string Grp = KIF["Grp"].ToString().Trim();
                    string LPONO = KIF["PONo"].ToString().Trim();
                    string Jurisdiction = KIF["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = KIF["PlaceOfSupply"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", KIF["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         { "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesInvoice_KIF header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesInvoice_KIF item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesInvoice_KIF Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesInvoice_KIF Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Invoice - VAN";
                    BL_Registry.SetLog("SalesInvoice_KIF post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesInvoice_KIF posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("SalesInvoice_KIF posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("SalesInvoice_KIF Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("SalesInvoice_KIF Sales Invoice - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesInvoiceKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" SalesInvoice_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" SalesInvoice_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void SalesInvoice_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getSalesInvoiceTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesInvoiceTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow TB in dsTB.Tables[0].Rows)
                {
                    docno = TB["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = TB["intDate"].ToString().Trim();
                    string SalesAccount = TB["SalesAccount"].ToString().Trim();
                    string CustomerAC = TB["CustomerAC"].ToString().Trim();
                    string DueDate = TB["intDueDate"].ToString().Trim();
                    string WarehouseCode = TB["Salesman"].ToString().Trim();
                    string RouteCode = TB["Salesman"].ToString().Trim();
                    string Narration = TB["Narration"].ToString().Trim();
                    string Grp = TB["Grp"].ToString().Trim();
                    string LPONO = TB["PONo"].ToString().Trim();
                    string Jurisdiction = TB["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = TB["PlaceOfSupply"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", TB["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         { "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesInvoice_TB header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesInvoice_TB item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesInvoice_TB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesInvoice_TB Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Invoice - VAN";
                    BL_Registry.SetLog("SalesInvoice_TB post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesInvoice_TB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("SalesInvoice_TB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("SalesInvoice_TB Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " SalesInvoice_TB Sales Invoice - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("SalesInvoice_TB Sales Invoice - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesInvoiceTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog("SalesInvoice_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog("SalesInvoice_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void SalesReturn_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getSalesReturnPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesReturnPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string SalesAccount = Pic["SalesAccount"].ToString().Trim();
                    string CustomerAC = Pic["CustomerAC"].ToString().Trim();
                    //string DueDate = Pic["intDueDate"].ToString().Trim();
                    string WarehouseCode = Pic["Salesman"].ToString().Trim();
                    string RouteCode = Pic["Salesman"].ToString().Trim();
                    string Narration = Pic["Narration"].ToString().Trim();
                    string Grp = Pic["Grp"].ToString().Trim();
                    string LPONO = Pic["PONo"].ToString().Trim();
                    string Jurisdiction = Pic["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = Pic["PlaceOfSupply"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesReturn_PIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesReturn_PIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesReturn_PIC Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesReturn_PIC Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Return - VAN";
                    BL_Registry.SetLog("SalesReturn_PIC post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesReturn_PIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" SalesReturn_PIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("SalesReturn_PIC Sales Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " SalesReturn_PIC Sales Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" SalesReturn_PIC Sales Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesReturnPIC", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" SalesReturn_PIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" SalesReturn_PIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void SalesReturn_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getSalesReturnKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesReturnKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow KIF in dsKIF.Tables[0].Rows)
                {
                    docno = KIF["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = KIF["intDate"].ToString().Trim();
                    string SalesAccount = KIF["SalesAccount"].ToString().Trim();
                    string CustomerAC = KIF["CustomerAC"].ToString().Trim();
                    //string DueDate = KIF["intDueDate"].ToString().Trim();
                    string WarehouseCode = KIF["Salesman"].ToString().Trim();
                    string RouteCode = KIF["Salesman"].ToString().Trim();
                    string Narration = KIF["Narration"].ToString().Trim();
                    string Grp = KIF["Grp"].ToString().Trim();
                    string LPONO = KIF["PONo"].ToString().Trim();
                    string Jurisdiction = KIF["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = KIF["PlaceOfSupply"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", KIF["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesReturn_KIF header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesReturn_KIF item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesReturn_KIF Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesReturn_KIF Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Return - VAN";
                    BL_Registry.SetLog("SalesReturn_KIF post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesReturn_KIF posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" SalesReturn_KIF posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog(" SalesReturn_KIF Sales Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Sales Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" SalesReturn_KIF Sales Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesReturnKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" SalesReturn_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" SalesReturn_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void SalesReturn_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getSalesReturnTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getSalesReturnTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow TB in dsTB.Tables[0].Rows)
                {
                    docno = TB["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = TB["intDate"].ToString().Trim();
                    string SalesAccount = TB["SalesAccount"].ToString().Trim();
                    string CustomerAC = TB["CustomerAC"].ToString().Trim();
                    //string DueDate = TB["intDueDate"].ToString().Trim();
                    string WarehouseCode = TB["Salesman"].ToString().Trim();
                    string RouteCode = TB["Salesman"].ToString().Trim();
                    string Narration = TB["Narration"].ToString().Trim();
                    string Grp = TB["Grp"].ToString().Trim();
                    string LPONO = TB["PONo"].ToString().Trim();
                    string Jurisdiction = TB["Jurisdiction"].ToString().Trim();
                    string PlaceOfSupply = TB["PlaceOfSupply"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", TB["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "Place of supply__Id", PlaceOfSupply},
                                         { "Jurisdiction__Id", Jurisdiction }
                                     };
                    BL_Registry.SetLog("SalesReturn_TB header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("SalesReturn_TB item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("SalesReturn_TB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("SalesReturn_TB Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Sales Return - VAN";
                    BL_Registry.SetLog("SalesReturn_TB post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("SalesReturn_TB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("SalesReturn_TB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("SalesReturn_TB Sales Return - VAN Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " SalesReturn_TB Sales Return - VAN Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("SalesReturn_TB Sales Return - VAN Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setSalesReturnTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog("SalesReturn_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog("SalesReturn_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void DamageStock_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getDamageStockPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getDamageStockPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string SalesAccount = Pic["SalesAccount"].ToString().Trim();
                    string CustomerAC = Pic["CustomerAC"].ToString().Trim();
                    //string DueDate = Pic["intDueDate"].ToString().Trim();
                    string WarehouseCode = Pic["Salesman"].ToString().Trim();
                    string RouteCode = Pic["Salesman"].ToString().Trim();
                    string Narration = Pic["Narration"].ToString().Trim();
                    string Grp = Pic["Grp"].ToString().Trim();
                    string LPONO = "";// Pic["PONo"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "ReturnType", "1" }

                                     };
                    BL_Registry.SetLog("DamageStock_PIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("DamageStock_PIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("DamageStock_PIC Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("DamageStock_PIC Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Damage Stock";
                    BL_Registry.SetLog("DamageStock_PIC post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("DamageStock_PIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" DamageStock_PIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("DamageStock_PIC Damage Stock Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " DamageStock_PIC Damage Stock Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" DamageStock_PIC Damage Stock Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setDamageStockPIC", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" DamageStock_PIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" DamageStock_PIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void DamageStock_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getDamageStockKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getDamageStockKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow KIF in dsKIF.Tables[0].Rows)
                {
                    docno = KIF["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = KIF["intDate"].ToString().Trim();
                    string SalesAccount = KIF["SalesAccount"].ToString().Trim();
                    string CustomerAC = KIF["CustomerAC"].ToString().Trim();
                    //string DueDate = KIF["intDueDate"].ToString().Trim();
                    string WarehouseCode = KIF["Salesman"].ToString().Trim();
                    string RouteCode = KIF["Salesman"].ToString().Trim();
                    string Narration = KIF["Narration"].ToString().Trim();
                    string Grp = KIF["Grp"].ToString().Trim();
                    string LPONO = "";//KIF["PONo"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", KIF["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "ReturnType", "1" }
                                     };
                    BL_Registry.SetLog("DamageStock_KIF header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("DamageStock_KIF item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("DamageStock_KIF Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("DamageStock_KIF Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Damage Stock";
                    BL_Registry.SetLog("DamageStock_KIF post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("DamageStock_KIF posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("DamageStock_KIF posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("DamageStock_KIF Damage Stock Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Damage Stock Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("DamageStock_KIF Damage Stock Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setDamageStockKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" DamageStock_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" DamageStock_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void DamageStock_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getDamageStockTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getDamageStockTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow TB in dsTB.Tables[0].Rows)
                {
                    docno = TB["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = TB["intDate"].ToString().Trim();
                    string SalesAccount = TB["SalesAccount"].ToString().Trim();
                    string CustomerAC = TB["CustomerAC"].ToString().Trim();
                    //string DueDate = TB["intDueDate"].ToString().Trim();
                    string WarehouseCode = TB["Salesman"].ToString().Trim();
                    string RouteCode = TB["Salesman"].ToString().Trim();
                    string Narration = TB["Narration"].ToString().Trim();
                    string Grp = TB["Grp"].ToString().Trim();
                    string LPONO = "";//TB["PONo"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", TB["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "SalesAC__Name", SalesAccount},
                                         { "CustomerAC__Code", CustomerAC },
                                         //{ "DueDate", Convert.ToInt32(DueDate) },
                                         { "Company Master__Id", 3 },
                                         { "Warehouse__Code", WarehouseCode },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "LPO_No", LPONO },
                                         { "ReturnType", "1" }
                                     };
                    BL_Registry.SetLog("DamageStock_TB header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("DamageStock_TB item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Item__Code", item["ItemCode"].ToString().Trim()},
                                         { "Description", item["Description1"].ToString().Trim()},
                                         { "Unit__Id", 24 },
                                         { "StockAC__Id", item["iStocksAccount"].ToString().Trim() },
                                         { "Quantity", Convert.ToDecimal(item["Qty"].ToString().Trim()) },
                                         { "Rate", Convert.ToDecimal(item["Rate"].ToString().Trim()) },
                                         { "Input Discount Amt", Convert.ToDecimal(item["Discount2"].ToString().Trim()) },
                                         { "TaxCode__Code", "SR" },
                                         { "VAT", Convert.ToDecimal(item["VAT2"].ToString().Trim()) }
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("DamageStock_TB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("DamageStock_TB Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Damage Stock";
                    BL_Registry.SetLog("DamageStock_TB post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("DamageStock_TB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("DamageStock_TB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("DamageStock_TB Damage Stock Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " DamageStock_TB Damage Stock Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("DamageStock_TB Damage Stock Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setDamageStockTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog("DamageStock_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog("DamageStock_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void Receipts_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getReceiptsPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getReceiptsPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string CashBankAC__Name = Pic["CashBankAC"].ToString().Trim();
                    string RouteCode = Pic["Salesman"].ToString().Trim();
                    string Narration = Pic["Narration"].ToString().Trim();
                    string Grp = Pic["Grp"].ToString().Trim();
                    string sChequeNo = Pic["ChequeNo"].ToString().Trim();
                    string CustomerAc = Pic["CustomerAC"].ToString().Trim();
                    string currencyId = Pic["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    BL_Registry.SetLog("Receipts_PIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("Receipts_PIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("Receipts_PIC Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("Receipts_PIC Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Receipts";
                    BL_Registry.SetLog("Receipts_PIC post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("Receipts_PIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" Receipts_PIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Receipts_PIC Receipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " Receipts_PIC Receipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" Receipts_PIC Receipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setReceiptsPIC", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" Receipts_PIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" Receipts_PIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void Receipts_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getReceiptsKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getReceiptsKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow KIF in dsKIF.Tables[0].Rows)
                {
                    docno = KIF["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = KIF["intDate"].ToString().Trim();
                    string CashBankAC__Name = KIF["CashBankAC"].ToString().Trim();
                    string RouteCode = KIF["Salesman"].ToString().Trim();
                    string Narration = KIF["Narration"].ToString().Trim();
                    string Grp = KIF["Grp"].ToString().Trim();
                    string sChequeNo = KIF["ChequeNo"].ToString().Trim();
                    string CustomerAc = KIF["CustomerAC"].ToString().Trim();
                    string currencyId = KIF["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", KIF["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Receipts";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Receipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "Receipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Receipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setReceiptsKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" Receipts_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" Receipts_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void Receipts_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getReceiptsTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getReceiptsTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow TB in dsTB.Tables[0].Rows)
                {
                    docno = TB["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = TB["intDate"].ToString().Trim();
                    string CashBankAC__Name = TB["CashBankAC"].ToString().Trim();
                    string RouteCode = TB["Salesman"].ToString().Trim();
                    string Narration = TB["Narration"].ToString().Trim();
                    string Grp = TB["Grp"].ToString().Trim();
                    string sChequeNo = TB["ChequeNo"].ToString().Trim();
                    string CustomerAc = TB["CustomerAC"].ToString().Trim();
                    string currencyId = TB["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", TB["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    BL_Registry.SetLog("Receipts_TB header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("Receipts_TB item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("Receipts_TB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("Receipts_TB Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Receipts";
                    BL_Registry.SetLog("Receipts_TB post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("Receipts_TB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("Receipts_TB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("Receipts_TB Receipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " Receipts_TB Receipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("Receipts_TB Receipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setReceiptsTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog("Receipts_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog("Receipts_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void PDCReceipts_PIC()
        {
            DataSet dsPIC = GetExternalData.getFn("getPDCReceiptsPIC");
            if (dsPIC.Tables.Count > 0)
            {
                BL_Registry.SetLog("getPDCReceiptsPIC" + dsPIC.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow Pic in dsPIC.Tables[0].Rows)
                {
                    docno = Pic["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = Pic["intDate"].ToString().Trim();
                    string CashBankAC__Name = Pic["CashBankAC"].ToString().Trim();
                    string RouteCode = Pic["Salesman"].ToString().Trim();
                    string Narration = Pic["Narration"].ToString().Trim();
                    string Grp = Pic["Grp"].ToString().Trim();
                    string sChequeNo = Pic["ChequeNo"].ToString().Trim();
                    string CustomerAc = Pic["CustomerAC"].ToString().Trim();
                    string currencyId = Pic["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", Pic["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    BL_Registry.SetLog("PDCReceipts_PIC header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsPIC.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("PDCReceipts_PIC item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("PDCReceipts_PIC Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("PDCReceipts_PIC Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Post-Dated Receipts";
                    BL_Registry.SetLog("PDCReceipts_PIC post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("PDCReceipts_PIC posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog(" PDCReceipts_PIC posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("PDCReceipts_PIC PDCReceipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " PDCReceipts_PIC PDCReceipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog(" PDCReceipts_PIC PDCReceipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setPDCReceiptsPIC", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" PDCReceipts_PIC FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" PDCReceipts_PIC FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void PDCReceipts_KIF()
        {
            DataSet dsKIF = GetExternalData.getFn("getPDCReceiptsKIF");
            if (dsKIF.Tables.Count > 0)
            {
                BL_Registry.SetLog("getPDCReceiptsKIF" + dsKIF.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow KIF in dsKIF.Tables[0].Rows)
                {
                    docno = KIF["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = KIF["intDate"].ToString().Trim();
                    string CashBankAC__Name = KIF["CashBankAC"].ToString().Trim();
                    string RouteCode = KIF["Salesman"].ToString().Trim();
                    string Narration = KIF["Narration"].ToString().Trim();
                    string Grp = KIF["Grp"].ToString().Trim();
                    string sChequeNo = KIF["ChequeNo"].ToString().Trim();
                    string CustomerAc = KIF["CustomerAC"].ToString().Trim();
                    string currencyId = KIF["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", KIF["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsKIF.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Post-Dated Receipts";
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("PDCReceipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + "PDCReceipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("PDCReceipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setPDCReceiptsKIF", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog(" PDCReceipts_KIF FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog(" PDCReceipts_KIF FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
                        }
                    }
                }

            }
        }

        private void PDCReceipts_TB()
        {
            DataSet dsTB = GetExternalData.getFn("getPDCReceiptsTB");
            if (dsTB.Tables.Count > 0)
            {
                BL_Registry.SetLog("getPDCReceiptsTB" + dsTB.Tables.Count.ToString());
                string docno = "";
                foreach (DataRow TB in dsTB.Tables[0].Rows)
                {
                    docno = TB["DocumentNo"].ToString().Trim();
                    BL_Registry.SetLog("docno" + docno);
                    string idate = TB["intDate"].ToString().Trim();
                    string CashBankAC__Name = TB["CashBankAC"].ToString().Trim();
                    string RouteCode = TB["Salesman"].ToString().Trim();
                    string Narration = TB["Narration"].ToString().Trim();
                    string Grp = TB["Grp"].ToString().Trim();
                    string sChequeNo = TB["ChequeNo"].ToString().Trim();
                    string CustomerAc = TB["CustomerAC"].ToString().Trim();
                    string currencyId = TB["currencyId"].ToString().Trim();
                    Hashtable header = new Hashtable
                                     {
                                         { "DocNo", TB["DocumentNo"].ToString().Trim() },
                                         { "Date", Convert.ToInt32(idate)},
                                         { "CashBankAC__Name", CashBankAC__Name},
                                         { "Company Master__Id", 3 },
                                         { "Route__Code", RouteCode },
                                         { "sNarration", Narration },
                                         { "Group Customer Master__Name", Grp },
                                         { "sChequeNo", sChequeNo },
                                         { "Currency__Id", currencyId },
                                         { "Salesman__Code", RouteCode }
                                     };
                    BL_Registry.SetLog("PDCReceipts_TB header Data ready");
                    List<Hashtable> lstBody = new List<Hashtable>();
                    DataRow[] rows = dsTB.Tables[1].Select("DocumentNo = '" + docno.Trim() + "'");
                    BL_Registry.SetLog("PDCReceipts_TB item rows count" + rows.Count().ToString());
                    foreach (DataRow item in rows)
                    {
                        List<Hashtable> listbillRef = new List<Hashtable>();
                        Hashtable billRef = new Hashtable();
                        billRef.Add("CustomerId", item["customerid"].ToString().Trim());
                        billRef.Add("Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()));
                        billRef.Add("reftype", 2);
                        billRef.Add("Reference", item["InvoiceNumber"].ToString().Trim());
                        listbillRef.Add(billRef);
                        Hashtable objBody = new Hashtable
                                     {
                                         { "DocNo", item["DocumentNo"].ToString().Trim() },
                                         { "Amount", Convert.ToDecimal(item["Amount"].ToString().Trim()) },
                                         { "Reference", listbillRef },
                                         { "Account__Id", item["customerid"].ToString().Trim() },
                                     };
                        lstBody.Add(objBody);
                    }
                    BL_Registry.SetLog("PDCReceipts_TB Body Data ready");
                    var postingData1 = new PostingData();
                    postingData1.data.Add(new Hashtable { { "Header", header }, { "Body", lstBody } });
                    string sContent1 = JsonConvert.SerializeObject(postingData1);
                    BL_Registry.SetLog("PDCReceipts_TB Content" + sContent1);
                    string err1 = "";

                    string Url1 = baseUrl + "/Transactions/Vouchers/Post-Dated Receipts";
                    BL_Registry.SetLog("PDCReceipts_TB post url" + Url1);
                    var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                    if (response1 != null)
                    {
                        BL_Registry.SetLog("PDCReceipts_TB posting Response" + response1.ToString());
                        var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                        if (responseData1.result == -1)
                        {
                            BL_Registry.SetLog("PDCReceipts_TB posting Response failed" + responseData1.result.ToString());
                            BL_Registry.SetLog("PDCReceipts_TB PDCReceipts Entry Posted Failed with DocNo: " + docno);
                            BL_Registry.SetLog2(response1 + "\n " + " PDCReceipts_TB PDCReceipts Entry Posted Failed with DocNo: " + docno + " \n Error Message : " + responseData1.message + "\n " + err1);
                        }
                        else
                        {
                            BL_Registry.SetLog("PDCReceipts_TB PDCReceipts Entry Posted Successfully with DocNo: " + docno);
                            int d = GetExternalData.setFn("setPDCReceiptsTB", docno);
                            if (d != 0)
                            {
                                BL_Registry.SetLog("PDCReceipts_TB FOCUS_WINIT DB updation successed with DocNo = " + docno);
                            }
                            else
                            {
                                BL_Registry.SetLog("PDCReceipts_TB FOCUS_WINIT DB Updation failed with DocNo=" + docno);
                            }
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
