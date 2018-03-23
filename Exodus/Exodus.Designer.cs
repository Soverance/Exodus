﻿// © 2018 Soverance Studios
// Scott McCutchen
// soverance.com
// scott.mccutchen@soverance.com

namespace Exodus
{
    partial class Exodus
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExodusEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.ExodusEventLog)).BeginInit();
            // 
            // ExodusEventLog
            // 
            this.ExodusEventLog.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.ExodusEventLog_EntryWritten);
            // 
            // Exodus
            // 
            this.ServiceName = "Exodus";
            ((System.ComponentModel.ISupportInitialize)(this.ExodusEventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog ExodusEventLog;
    }
}