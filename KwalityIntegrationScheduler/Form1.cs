using KwalityIntegrationLibrary;
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
                var currentTime = DateTime.Now;
                string scheduledtime = ConfigurationManager.AppSettings["StartTime"];
                DateTime t = DateTime.Parse(scheduledtime);
                TimeSpan ts = new TimeSpan();
                ts = t - System.DateTime.Now;

                string scheduledtime2 = ConfigurationManager.AppSettings["StartTime2"];
                DateTime t2 = DateTime.Parse(scheduledtime2);
                TimeSpan ts2 = new TimeSpan();
                ts2 = t2 - System.DateTime.Now;

                if (ts.TotalMilliseconds < 0 || ts2.TotalMilliseconds < 0)
                {
                    if (currentTime.Hour == t.Hour && currentTime.Minute == t.Minute && currentTime.Second == t.Second)
                    {
                        EventLog("Scheduled Time hit" + currentTime.ToString());
                        string screenNames = ConfigurationManager.AppSettings["ScreenNames"];
                        EventLog("ScreenNames from Scheduler = " + screenNames);
                        int CompanyId = Convert.ToInt32(ConfigurationManager.AppSettings["CompanyId"]);
                        EventLog("CompanyId from Scheduler = " + CompanyId.ToString());
                        //string screenNames = "Stock Transfer Issue - VAN,Stock Transfer Return - VAN,Sales Invoice - VAN,Sales Return - VAN,Damage Stock,Receipts,Post-Dated Receipts";
                        Trigger _trigger = new Trigger();
                        bool status = _trigger.Integration_Trigger(screenNames, CompanyId);
                        EventLog("Posting Status from Scheduler = " + status.ToString());
                    }
                    else if (currentTime.Hour == t2.Hour && currentTime.Minute == t2.Minute && currentTime.Second == t2.Second)
                    {
                        EventLog("Scheduled Time hit" + currentTime.ToString());
                        string screenNames = ConfigurationManager.AppSettings["ScreenNames"];
                        EventLog("ScreenNames from Scheduler = " + screenNames);
                        int CompanyId = Convert.ToInt32(ConfigurationManager.AppSettings["CompanyId"]);
                        EventLog("CompanyId from Scheduler = " + CompanyId.ToString());
                        //string screenNames = "Stock Transfer Issue - VAN,Stock Transfer Return - VAN,Sales Invoice - VAN,Sales Return - VAN,Damage Stock,Receipts,Post-Dated Receipts";
                        Trigger _trigger = new Trigger();
                        bool status = _trigger.Integration_Trigger(screenNames, CompanyId);
                        EventLog("Posting Status from Scheduler = " + status.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrLog(ex.ToString());
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
        public static void EventLog(string content)
        {
            StreamWriter objSw = null;
            try
            {
                string AppLocation = "";
                AppLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData); ;
                string folderName = AppLocation + "\\LogFiles";
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                string sFilePath = folderName + "\\Kwality_Integration_EventLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                objSw = new StreamWriter(sFilePath, true);
                objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objSw != null)
                {
                    objSw.Flush();
                    objSw.Dispose();
                }
            }
        }

        public static void ErrLog(string content)
        {
            StreamWriter objSw = null;
            try
            {
                string AppLocation = "";
                AppLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData); ;
                string folderName = AppLocation + "\\LogFiles";
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                string sFilePath = folderName + "\\Kwality_Integration_ErrorLog" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                objSw = new StreamWriter(sFilePath, true);
                objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objSw != null)
                {
                    objSw.Flush();
                    objSw.Dispose();
                }
            }
        }
    }
}
