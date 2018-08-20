namespace Eu.EDelivery.AS4.WindowsService
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
            this.as4ServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.as4ServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // as4ServiceProcessInstaller
            // 
            this.as4ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.as4ServiceProcessInstaller.Password = null;
            this.as4ServiceProcessInstaller.Username = null;
            // 
            // as4ServiceInstaller
            // 
            this.as4ServiceInstaller.Description = "AS4.NET Message Service Handler for sending and receiving AS4 Messages";
            this.as4ServiceInstaller.DisplayName = "AS4.NET Message Service Handler";
            this.as4ServiceInstaller.ServiceName = "AS4Service";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.as4ServiceProcessInstaller,
            this.as4ServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller as4ServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller as4ServiceInstaller;
    }
}