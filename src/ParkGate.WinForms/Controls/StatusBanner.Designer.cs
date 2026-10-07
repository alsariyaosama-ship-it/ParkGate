#nullable disable

namespace ParkGate.WinForms.Controls;

partial class StatusBanner
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        this._layoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this._lblTitle = new System.Windows.Forms.Label();
        this._lblDetails = new System.Windows.Forms.Label();
        this._layoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // _layoutPanel
        // 
        this._layoutPanel.ColumnCount = 1;
        this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._layoutPanel.Controls.Add(this._lblTitle, 0, 0);
        this._layoutPanel.Controls.Add(this._lblDetails, 0, 1);
        this._layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._layoutPanel.Location = new System.Drawing.Point(0, 0);
        this._layoutPanel.Margin = new System.Windows.Forms.Padding(0);
        this._layoutPanel.Name = "_layoutPanel";
        this._layoutPanel.RowCount = 2;
        this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
        this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
        this._layoutPanel.Size = new System.Drawing.Size(800, 74);
        this._layoutPanel.TabIndex = 0;
        // 
        // _lblTitle
        // 
        this._lblTitle.AutoEllipsis = true;
        this._lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblTitle.Font = new System.Drawing.Font("Cairo", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTitle.ForeColor = System.Drawing.Color.White;
        this._lblTitle.Location = new System.Drawing.Point(0, 0);
        this._lblTitle.Margin = new System.Windows.Forms.Padding(0);
        this._lblTitle.Name = "_lblTitle";
        this._lblTitle.Size = new System.Drawing.Size(800, 48);
        this._lblTitle.TabIndex = 0;
        this._lblTitle.Text = "جاهز للاستخدام";
        this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _lblDetails
        // 
        this._lblDetails.AutoEllipsis = true;
        this._lblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblDetails.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
        this._lblDetails.Location = new System.Drawing.Point(0, 48);
        this._lblDetails.Margin = new System.Windows.Forms.Padding(0);
        this._lblDetails.Name = "_lblDetails";
        this._lblDetails.Size = new System.Drawing.Size(800, 26);
        this._lblDetails.TabIndex = 1;
        this._lblDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this._lblDetails.Visible = false;
        // 
        // StatusBanner
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this.Controls.Add(this._layoutPanel);
        this.DoubleBuffered = true;
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MinimumSize = new System.Drawing.Size(300, 60);
        this.Name = "StatusBanner";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(800, 74);
        this._layoutPanel.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel _layoutPanel;
    private System.Windows.Forms.Label _lblTitle;
    private System.Windows.Forms.Label _lblDetails;
}
