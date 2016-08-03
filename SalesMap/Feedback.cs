﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalesMap
{
    public partial class Feedback : Form
    {
        bool feature = false;
        bool bug = false;
        RichTextBox textBox = new RichTextBox();

        public Feedback()
        {
            InitializeComponent();
        }

        private void buttonFeedback_Click(object sender, EventArgs e)
        {
            Process.Start("mailto:derek.antrican@sigmanest.com&Subject=SalesMap%20Feedback");
            this.Close();
        }

        private void buttonFeature_Click(object sender, EventArgs e)
        {
            feature = true;
            showSubmissionForm();
        }

        private void buttonBug_Click(object sender, EventArgs e)
        {
            bug = true;
            showSubmissionForm();
        }

        private void showSubmissionForm()
        {
            this.Height += 100;
            textBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            textBox.Size = new Size(157, 75);
            textBox.Location = new Point(12, 98);
            this.Controls.Add(textBox);
            Button buttonOk = new Button();
            buttonOk.Size = new Size(50, 23);
            buttonOk.Text = "Submit";
            buttonOk.Location = new Point(127, 178);
            this.Controls.Add(buttonOk);
            buttonOk.Click += ButtonOk_Click;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            string subject = textBox.Text;
            string email = "derekantrican+jtqvwnmoer0jrzmbktqy@boards.trello.com";

            if (bug)
            {
                //Send email

                MessageBox messageBug = new MessageBox("Added to Trello!", "Your bug report has been added to the SalesMap Trello board!", "Go to Trello", Common.MessageBoxResult.Yes, true, "OK", Common.MessageBoxResult.Ok);
                messageBug.ShowDialog();

                if (Common.DialogResult == Common.MessageBoxResult.Yes)
                    Process.Start("https://trello.com/b/mvRhnwaF/salesmap");

                this.Close();
            }
            else if (feature)
            {
                //Send email

                MessageBox messageFeature = new MessageBox("Added to Trello!", "Your feature request has been added to the SalesMap Trello board!", "Go to Trello", Common.MessageBoxResult.Yes, true, "OK", Common.MessageBoxResult.Ok);
                messageFeature.ShowDialog();

                if (Common.DialogResult == Common.MessageBoxResult.Yes)
                    Process.Start("https://trello.com/b/mvRhnwaF/salesmap");

                this.Close();
            }
        }
    }
}
