﻿using System.Windows.Forms;

namespace Intersect.Editor.Forms
{
    public partial class frmLoadingContent : Form
    {
        public frmLoadingContent()
        {
            InitializeComponent();
        }

        public void SetProgress(int percent)
        {
            lblProgress.Text = "Downloading: " + percent + "% Complete";
        }
    }
}