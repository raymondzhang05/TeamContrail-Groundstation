
namespace Contrail
{
    partial class FormPopupData
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbCO2Popup = new System.Windows.Forms.TextBox();
            this.tbTVOCPopup = new System.Windows.Forms.TextBox();
            this.tbCOPopup = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Max Concentration of CO2 (ppm):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(170, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Max Concentration of TVOC (ppb):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(411, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Max Concentration of CO (ppm):";
            // 
            // tbCO2Popup
            // 
            this.tbCO2Popup.Location = new System.Drawing.Point(44, 51);
            this.tbCO2Popup.Name = "tbCO2Popup";
            this.tbCO2Popup.ReadOnly = true;
            this.tbCO2Popup.Size = new System.Drawing.Size(100, 20);
            this.tbCO2Popup.TabIndex = 4;
            // 
            // tbTVOCPopup
            // 
            this.tbTVOCPopup.Location = new System.Drawing.Point(233, 49);
            this.tbTVOCPopup.Name = "tbTVOCPopup";
            this.tbTVOCPopup.ReadOnly = true;
            this.tbTVOCPopup.Size = new System.Drawing.Size(100, 20);
            this.tbTVOCPopup.TabIndex = 5;
            // 
            // tbCOPopup
            // 
            this.tbCOPopup.Location = new System.Drawing.Point(442, 49);
            this.tbCOPopup.Name = "tbCOPopup";
            this.tbCOPopup.ReadOnly = true;
            this.tbCOPopup.Size = new System.Drawing.Size(100, 20);
            this.tbCOPopup.TabIndex = 6;
            // 
            // FormPopupData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(606, 255);
            this.Controls.Add(this.tbCOPopup);
            this.Controls.Add(this.tbTVOCPopup);
            this.Controls.Add(this.tbCO2Popup);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormPopupData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Analyzed Gas Concentration Data Window";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbCO2Popup;
        private System.Windows.Forms.TextBox tbTVOCPopup;
        private System.Windows.Forms.TextBox tbCOPopup;
    }
}