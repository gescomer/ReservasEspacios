
namespace ReportingREC
{
    partial class GetSendInfo
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.stSendData = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.stSendData)).BeginInit();
            // 
            // stSendData
            // 
            this.stSendData.Enabled = true;
            this.stSendData.Interval = 1800000D;
            this.stSendData.Elapsed += new System.Timers.ElapsedEventHandler(this.stSendData_Elapsed);
            // 
            // GetSendInfo
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.stSendData)).EndInit();

        }

        #endregion

        private System.Timers.Timer stSendData;
    }
}
