﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace SalesMap
{
    public partial class Updater : Form
    {
        public Updater(string updateURL)
        {
            InitializeComponent();

            string progName = Application.ExecutablePath.Substring(Application.ExecutablePath.LastIndexOf("\\") + 1);
            string progLoc = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\") + 1);
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SalesMap", true);
            key.SetValue("Updating", true);


            if (File.Exists(progLoc + "SalesMap-old.exe"))
                File.Delete(progLoc + "SalesMap-old.exe");

            try
            {
                File.Move(progLoc + progName, progLoc + "SalesMap-old.exe");
            }
            catch (Exception ex)
            {
                Common.Log("[UPDATER] Problem renaming the old executable: " + ex.Message, false);
            }

            Common.Log("[UPDATER] Downloading new version... (" + updateURL + ")", false);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.DownloadFileAsync(new Uri(updateURL), Path.Combine(progLoc, progName));
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            this.Text = "Updating... (" + e.ProgressPercentage + "%)";
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string progName = Application.ExecutablePath.Substring(Application.ExecutablePath.LastIndexOf("\\") + 1);
            string progLoc = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\") + 1);

            if ((new FileInfo(Path.Combine(progLoc, progName))).Length == 0)
                throw new Exception("Downloaded file is 0 bytes");

            Common.Log("[UPDATER] Download has completed....restarting", false);

            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C ping 127.0.0.1 -n 2 && \"" + progLoc + progName + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }
    }
}
