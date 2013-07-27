using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PlumePad
{
    public partial class AboutDialog : Form
    {
        // アドレスを直書きから変数に変更
        string strHomeUrl = "http://www.angel-teatime.com/";

        public AboutDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //リンク先に移動したことにする
            linkLabel1.LinkVisited = true;

            //ブラウザで開く
            System.Diagnostics.Process.Start(strHomeUrl);
        }
    }
}