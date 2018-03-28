namespace Exodus
{
    partial class ProjectInstaller
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectInstaller));
            this.ExodusServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ExodusServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ExodusServiceProcessInstaller
            // 
            this.ExodusServiceProcessInstaller.Password = null;
            this.ExodusServiceProcessInstaller.Username = null;
            // 
            // ExodusServiceInstaller
            // 
            this.ExodusServiceInstaller.Description = resources.GetString("ExodusServiceInstaller.Description");
            this.ExodusServiceInstaller.DisplayName = "Exodus Data Management Service";
            this.ExodusServiceInstaller.ServiceName = "Exodus";
            this.ExodusServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ExodusServiceProcessInstaller,
            this.ExodusServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ExodusServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ExodusServiceInstaller;
    }
}