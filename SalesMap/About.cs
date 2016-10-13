﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalesMap
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();

            this.labelVersion.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version;

            WebClient client = new WebClient();
            string url = "https://github.com/derekantrican/SalesMap/releases/tag/" + Common.ThisVersion;
            string html = "";
            try
            {
                html = client.DownloadString(url);
                html = html.Substring(html.IndexOf("<div class=\"markdown-body\">") + ("< div class=\"markdown-body\">").Length);
                html = html.Substring(0, html.IndexOf("</div>"));
                richTextBoxChangelog.Text = Regex.Replace(html, "<.*?>", string.Empty);
            }
            catch
            {
                Common.Log("Attempted to get the changlog for version " + Common.ThisVersion + " and failed...");
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabelFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Feedback feedback = new Feedback();
            feedback.ShowDialog();
        }

        private void linkLabelUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string GitVersionString = Common.checkGitHub();
            double GitVersion = Convert.ToDouble(GitVersionString.Split('v').Last());
            double thisVersion = Convert.ToDouble(Common.ThisVersion.Split('v').Last());

            if (GitVersion > thisVersion)
            {
                Common.Log("Prompted for new update. Current: " + thisVersion + "  Online: " + GitVersion);

                MessageBox messageBox = new MessageBox("New Update Available!", "A new version is available!\n\nThe current version is " + GitVersionString + " and you are running " + Common.ThisVersion +
                                    "\n\nDo you want to update to the new version?", "No", Common.MessageBoxResult.No, true, "Yes", Common.MessageBoxResult.Yes);
                messageBox.ShowDialog();
                if (Common.DialogResult == Common.MessageBoxResult.Yes)
                {
                    Common.Log("User selected \"Yes\" for the new update");
                    Update(GitVersionString);
                }
                else
                {
                    Common.Log("User selected \"No\" for the new update");
                }
            }
            else
            {
                MessageBox messageBox2 = new MessageBox("Most Current Version", "Congrats, you have the most current version! You are running version " + Common.ThisVersion, "OK", Common.MessageBoxResult.OK);
                messageBox2.ShowDialog();
            }
        }

        private void Update(string version)
        {
            Updater updater = new Updater(version);
            updater.ShowDialog();
        }

        private void richTextBoxChangelog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void linkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:derek.antrican@sigmanest.com&Subject=SalesMap%20Feedback");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Common.Log("Composing a Skype message to developer");

            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C start im:\"<sip:derek.antrican@sigmatek.net>\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.FileName = "cmd.exe";
            Process infoProcess = Process.Start(Info);
        }
    }
}
