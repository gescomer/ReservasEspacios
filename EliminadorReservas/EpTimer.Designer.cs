
namespace EliminadorReservas
{
    partial class EpTimer
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
            this.tmInit = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.tmInit)).BeginInit();
            // 
            // tmInit
            // 
            this.tmInit.Enabled = true;
            this.tmInit.Interval = 120000D;
            this.tmInit.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
            // 
            // EpTimer
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.tmInit)).EndInit();

        }

        #endregion

        private System.Timers.Timer tmInit;
    }
}
