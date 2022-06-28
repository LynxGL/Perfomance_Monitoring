using System;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;

namespace Monitoring
{
    public partial class PerfForm : Form
    {
        public PerfForm()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
           out ulong lpFreeBytesAvailableToCaller,
           out ulong lpTotalNumberOfBytes,
           out ulong lpTotalNumberOfFreeBytes);

        private int count = 0;
        private PerformanceCounter pNetS = new PerformanceCounter();
        private PerformanceCounter pNetR = new PerformanceCounter();
        private ManagementObjectSearcher objectsearcherIP = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration"); //gets all the info for network adapters
        //private ManagementObjectSearcher objectsearcherCores = new ManagementObjectSearcher("Select * from Win32_Processor");
        //ManagementObjectSearcher searchDisk = new ManagementObjectSearcher("SELECT * FROM Win32_Volume");

        // private ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
        
        // private PerformanceCounter perfSystemCounter = new PerformanceCounter();

        private void GetHardWareInfo(string key, ListView list)
        {
            list.Items.Clear();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + key);

            try
            {
                foreach (ManagementBaseObject obj in searcher.Get())
                {
                    ListViewGroup listViewGroup;

                    try
                    {
                        listViewGroup = list.Groups.Add(obj["Name"].ToString(), obj["Name"].ToString());
                    }
                    catch (Exception ex)
                    {
                        listViewGroup = list.Groups.Add(obj.ToString(), obj.ToString());
                    }

                    if (obj.Properties.Count == 0)
                    {
                        MessageBox.Show("Failed to get information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;

                    }
                    foreach (PropertyData data in obj.Properties)
                    {
                        ListViewItem item = new ListViewItem(listViewGroup);

                        if (list.Items.Count % 2 != 0)
                        {
                            item.BackColor = Color.White;
                        }
                        else
                        {
                            item.BackColor = Color.WhiteSmoke;
                        }
                        item.Text = data.Name;

                        if (data.Value != null && !string.IsNullOrEmpty(data.Value.ToString()))
                        {
                            switch (data.Value.GetType().ToString())
                            {
                                case "System.String[]":

                                    string[] stringData = data.Value as string[];

                                    string restStr1 = string.Empty;

                                    foreach (string s in stringData)
                                    {
                                        restStr1 += $"{s} ";
                                    }

                                    item.SubItems.Add(restStr1);

                                    break;
                                case "System.UInt16[]":

                                    ushort[] ushortData = data.Value as ushort[];

                                    string resStr2 = string.Empty;

                                    foreach (ushort u in ushortData)
                                    {
                                        resStr2 += $"{Convert.ToString(u)}";
                                    }

                                    item.SubItems.Add(resStr2);
                                    break;
                                default:
                                    item.SubItems.Add(data.Value.ToString());
                                    break;

                            }
                            list.Items.Add(item);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CPU_count(int count)
        {
            float fcpu = PersіЗук.NextValue();
            float fhandle = pHandle.NextValue();
            float fprocess = pProcess.NextValue();

            labelCPU.Text = string.Format("{0:0.00}%", fcpu);
            labelHandle.Text = string.Format("{0:0}", fhandle);
            labelProcess.Text = string.Format("{0:0}", fprocess);
          
            if (count >= 100)
                chartCPU.Series["CPU"].Points.RemoveAt(0);
            chartCPU.Series["CPU"].Points.AddY(fcpu);
            object result = Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "ProcessorNameString", "");
            if (result != null)
            {
                labelNameCPU.Text = result.ToString();
            }
            object result1 = Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "~MHz", "");
            if (result1 != null)
            {
                BasicSpeed.Text = "Basic speed: " + result1 + " GHz".ToString();
            }


            int process = Environment.ProcessorCount;

            NumProcess.Text = "Logic processors: " + process.ToString();
            NumCores.Text = "Cores: " + (process / 2).ToString();

            // int coreCount = 0;

            //   if(objectsearcherCores != null)
            //      {
            //       NumOfCore.Text = objectsearcherCores.ToString();
            //      }
            //


            /*foreach (ManagementObject item in objectsearcherCores.Get())
            {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                    NumOfCore.Text = coreCount.ToString();
            }*/

            //NumOfCore.Text = coreCount.ToString();
        }

        private void RAM_count(int count)
        {
            ComputerInfo cpI = new ComputerInfo();
            float avaiRAM = cpI.TotalPhysicalMemory / (1024 * 1024 * 1024); //GB
            float fram = pRAM.NextValue();
            float framCmt = pRAMcmt.NextValue();
            float framAvai = pRAMavai.NextValue();
            float fCached = pCached.NextValue();
            float fPaged = pPaged.NextValue();
            float fNPaged = pNPaged.NextValue();


            labelRAM.Text = string.Format("{0:0.0} GB", (fram / 100) * avaiRAM);
            labelCmt.Text = string.Format("{0:0.0} GB", framCmt / (1024 * 1024 * 1024));
            labelRamAvai.Text = string.Format("{0:0.0} GB", framAvai / 1024);
            labelCached.Text = string.Format("{0:0.0} GB", fCached / (1024 * 1024 * 1024));
            labelPaged.Text = string.Format("{0:0} MB", fPaged / (1024 * 1024));
            labelNPaged.Text = string.Format("{0:0} MB", fNPaged / (1024 * 1024));

            if (count >= 100)
                chartRAM.Series["RAM"].Points.RemoveAt(0);
            chartRAM.Series["RAM"].Points.AddY(fram);
        }

        private void DISK_count(int count)
        {
            float fdisk = pDISK.NextValue();
            float fdRead = pReadSpd.NextValue();
            float fdWrite = pWriteSpd.NextValue();

            labelDiskUse.Text = string.Format("{0:0}%", fdisk);
            labelReadSpd.Text = string.Format("{0:0.0} KB/s", fdRead / 1024);
            labelWriteSpd.Text = string.Format("{0:0.0} KB/s", fdWrite / 1024);

            if (count >= 100)
            {
                chartDiskUse.Series["DiskUse"].Points.RemoveAt(0);
                chartDiskRate.Series["DiskRead"].Points.RemoveAt(0);
                chartDiskRate.Series["DiskWrite"].Points.RemoveAt(0);
            }
            chartDiskUse.Series["DiskUse"].Points.AddY(fdisk);
            chartDiskRate.Series["DiskRead"].Points.AddY(fdRead / 1024);            
            chartDiskRate.Series["DiskWrite"].Points.AddY(fdWrite / 1024);


            DriveInfo[] allDrives = DriveInfo.GetDrives();

            string[] strArr = new string[2];

            for (int i = 0; i < allDrives.Length; i++){
                strArr[i] = allDrives[i].Name;
            }

            string disk = strArr[0];
            string disk2 = strArr[1];

            ulong lpTotalNumberOfFreeBytes, lpTotalNumberOfBytes, lpFreeBytesAvailableToCaller;
            ulong lpTotalNumberOfFreeBytes1, lpTotalNumberOfBytes1, lpFreeBytesAvailableToCaller1;
            GetDiskFreeSpaceEx(disk, out lpFreeBytesAvailableToCaller, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);
            GetDiskFreeSpaceEx(disk2, out lpFreeBytesAvailableToCaller1, out lpTotalNumberOfBytes1, out lpTotalNumberOfFreeBytes1);

            label19.Text = strArr[0];
            DiskSize.Text = ("Size: " + lpTotalNumberOfBytes / 1024 / 1024 / 1024) + " GB".ToString();
            label21.Text = strArr[1];
            DiskSize2.Text = ("Size: " + lpTotalNumberOfBytes1 / 1024 / 1024 / 1024) + " GB".ToString();
        }

        private void INTERNET_count(int count)
        {
            NetworkInterface nic = getNIC();
            String name = String.Copy(nic.Description);
            name = name.Replace("(", "[");
            name = name.Replace(")", "]");
            try
            {
                pNetS.CategoryName = "Network Interface";
                pNetS.CounterName = "Bytes Sent/sec";
                pNetS.InstanceName = name;
                pNetR.CategoryName = "Network Interface";
                pNetR.CounterName = "Bytes Received/sec";
                pNetR.InstanceName = name;
                float fsend = pNetS.NextValue();
                float freceive = pNetR.NextValue();

                labelNetS.Text = string.Format("{0:0.0} Kbps", fsend * 8 / 1024);
                labelNetR.Text = string.Format("{0:0.0} Kbps", freceive * 8 / 1024);

                if (count >= 100)
                {
                    chartInternet.Series["Send"].Points.RemoveAt(0);
                    chartInternet.Series["Receive"].Points.RemoveAt(0);
                }
                chartInternet.Series["Send"].Points.AddY(fsend * 8 / 1024);
                chartInternet.Series["Receive"].Points.AddY(freceive * 8 / 1024);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
            labelNetworkType.Text = String.Copy(nic.Description);
            labelNetworkType1.Text = "Adapter: " + String.Copy(nic.Name);
       
            //GetIP IPv4 and IPv6
            foreach (ManagementObject queryObj in objectsearcherIP.Get())
            {
                if (queryObj["IPAddress"] == null) //finds the IP address info 
                {

                }
                else
                {
                    String[] arrIPAddress = (String[])(queryObj["IPAddress"]);
                    foreach (String arrValue in arrIPAddress)
                    {
                        if (arrValue.StartsWith("192") || arrValue.StartsWith("172") || arrValue.StartsWith("10."))
                        { //Checks if the IP is v4 or v6
                            //Console.WriteLine("IPv4: {0}", arrValue);
                            IPv4.Text = "IPv4: " + arrValue;
                        }
                       // else
                      //  {
                            //  Console.WriteLine("IPv6: {0}", arrValue);
                            //lblip6.Text = "IPv6: " + arrValue;
                      //  }
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            CPU_count(count);
            RAM_count(count);
            DISK_count(count);
          //  DISK_count2(count);
            INTERNET_count(count);
            count++;
           // SysUpTime.Text = (float)perfSystemCounter.NextValue() / 60 / 60 + "Hours";
        }

        public NetworkInterface getNIC() 
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if (nic.OperationalStatus.ToString().Equals("Up"))
                {
                    return nic;
                }
            }
            return null;
        }

        private void PerfForm_Load(object sender, EventArgs e)
        {
            if (getNIC() == null)
                tabPerformance.TabPages.Remove(tabInternet);
            timer.Start();

            comboBox1.SelectedIndex = 0;//Индекс процессора
        }
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string key = string.Empty;

            switch (comboBox1.SelectedItem.ToString())
            {
                case "CPU":
                    key = "Win32_Processor";
                    break;
                case "GPU":
                    key = "Win32_VideoController";
                    break;
                case "Socket":
                    key = "Win32_IDEController";
                    break;
                case "BIOS":
                    key = "Win32_BIOS";
                    break;
                case "RAM":
                    key = "Win32_PhysicalMemory";
                    break;
                case "Cache":
                    key = "Win32_CacheMemory";
                    break;
                case "USB":
                    key = "Win32_USBController";
                    break;
                case "DiskDrive":
                    key = "Win32_DiskDrive";
                    break;
                case "LogicalDisk":
                    key = "Win32_LogicalDisk";
                    break;
                case "Keyboard":
                    key = "Win32_Keyboard";
                    break;
                case "NetworkAdapter":
                    key = "Win32_NetworkAdapter";
                    break;
                case "Account":
                    key = "Win32_Account";
                    break;
                default:
                    key = "Win32_Processor";
                    break;

            }
            GetHardWareInfo(key, listView1);
        }

        private void tabInternet_Click(object sender, EventArgs e)
        {

        }

        private void labelDiskUse_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabCPU_Click(object sender, EventArgs e)
        {

        }

        private void NumCores_Click(object sender, EventArgs e)
        {

        }
        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void tabDisk_Click(object sender, EventArgs e)
        {

        }

        private void labelCached_Click(object sender, EventArgs e)
        {

        }
    }
}
