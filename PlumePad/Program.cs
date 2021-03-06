﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlumePad
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0) {
                Application.Run(new MainForm(args[0]));
            }
            else {
                Application.Run(new MainForm());
            }
        }
    }
}