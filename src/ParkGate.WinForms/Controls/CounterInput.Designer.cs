#nullable disable

namespace ParkGate.WinForms.Controls;

partial class CounterInput
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
        this._btnDecrement = new System.Windows.Forms.Button();
        this._txtValue = new System.Windows.Forms.TextBox();
        this._btnIncrement = new System.Windows.Forms.Button();
        this._layoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // _layoutPanel
        // 
        this._layoutPanel.ColumnCount = 4;
        this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        this._layoutPanel.Controls.Add(this._lblTitle, 0, 0);
        this._layoutPanel.Controls.Add(this._btnDecrement, 1, 0);
        this._layoutPanel.Controls.Add(this._txtValue, 2, 0);
        this._layoutPanel.Controls.Add(this._btnIncrement, 3, 0);
        this._layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._layoutPanel.Location = new System.Drawing.Point(0, 0);
        this._layoutPanel.Margin = new System.Windows.Forms.Padding(0);
        this._layoutPanel.Name = "_layoutPanel";
        this._layoutPanel.RowCount = 1;
        this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._layoutPanel.Size = new System.Drawing.Size(320, 42);
        this._layoutPanel.TabIndex = 0;
        // 
        // _lblTitle
        // 
        this._lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._lblTitle.AutoEllipsis = true;
        this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._lblTitle.Location = new System.Drawing.Point(148, 8);
        this._lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this._lblTitle.Name = "_lblTitle";
        this._lblTitle.Size = new System.Drawing.Size(168, 25);
        this._lblTitle.TabIndex = 0;
        this._lblTitle.Text = "الفئة:";
        this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _btnDecrement
        // 
        this._btnDecrement.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._btnDecrement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnDecrement.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnDecrement.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnDecrement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnDecrement.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnDecrement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._btnDecrement.Location = new System.Drawing.Point(105, 3);
        this._btnDecrement.Margin = new System.Windows.Forms.Padding(0);
        this._btnDecrement.Name = "_btnDecrement";
        this._btnDecrement.Size = new System.Drawing.Size(36, 36);
        this._btnDecrement.TabIndex = 1;
        this._btnDecrement.Text = "−";
        this._btnDecrement.UseVisualStyleBackColor = false;
        // 
        // _txtValue
        // 
        this._txtValue.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._txtValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._txtValue.Location = new System.Drawing.Point(46, 6);
        this._txtValue.Margin = new System.Windows.Forms.Padding(0);
        this._txtValue.MaxLength = 3;
        this._txtValue.Name = "_txtValue";
        this._txtValue.Size = new System.Drawing.Size(52, 29);
        this._txtValue.TabIndex = 2;
        this._txtValue.Text = "0";
        this._txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // _btnIncrement
        // 
        this._btnIncrement.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._btnIncrement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnIncrement.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnIncrement.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnIncrement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnIncrement.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnIncrement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._btnIncrement.Location = new System.Drawing.Point(3, 3);
        this._btnIncrement.Margin = new System.Windows.Forms.Padding(0);
        this._btnIncrement.Name = "_btnIncrement";
        this._btnIncrement.Size = new System.Drawing.Size(36, 36);
        this._btnIncrement.TabIndex = 3;
        this._btnIncrement.Text = "+";
        this._btnIncrement.UseVisualStyleBackColor = false;
        // 
        // CounterInput
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this._layoutPanel);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MinimumSize = new System.Drawing.Size(200, 42);
        this.Name = "CounterInput";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(320, 42);
        this.Leave += new System.EventHandler(this.OnControlLeave);
        this._layoutPanel.ResumeLayout(false);
        this._layoutPanel.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel _layoutPanel;
    private System.Windows.Forms.Label _lblTitle;
    private System.Windows.Forms.Button _btnDecrement;
    private System.Windows.Forms.TextBox _txtValue;
    private System.Windows.Forms.Button _btnIncrement;
}
