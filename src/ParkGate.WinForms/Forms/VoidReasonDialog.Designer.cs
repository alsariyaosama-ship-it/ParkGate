#nullable disable

namespace ParkGate.WinForms.Forms;

partial class VoidReasonDialog
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

    private void InitializeComponent()
    {
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._tblLayout = new System.Windows.Forms.TableLayoutPanel();
        this._lblAttemptInfo = new System.Windows.Forms.Label();
        this._lblWarning = new System.Windows.Forms.Label();
        this._lblReasonPrompt = new System.Windows.Forms.Label();
        this._txtReason = new System.Windows.Forms.TextBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnConfirm = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(480, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        this._lblHeader.Size = new System.Drawing.Size(480, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إلغاء عملية دخول";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 1;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblAttemptInfo, 0, 0);
        this._tblLayout.Controls.Add(this._lblWarning, 0, 1);
        this._tblLayout.Controls.Add(this._lblReasonPrompt, 0, 2);
        this._tblLayout.Controls.Add(this._txtReason, 0, 3);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16);
        this._tblLayout.RowCount = 4;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Size = new System.Drawing.Size(480, 230);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblAttemptInfo
        // 
        this._lblAttemptInfo.AutoSize = true;
        this._lblAttemptInfo.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblAttemptInfo.Location = new System.Drawing.Point(345, 16);
        this._lblAttemptInfo.Name = "_lblAttemptInfo";
        this._lblAttemptInfo.Size = new System.Drawing.Size(116, 19);
        this._lblAttemptInfo.TabIndex = 0;
        this._lblAttemptInfo.Text = "العملية: بطاقة - · الاسم -";
        // 
        // _lblWarning
        // 
        this._lblWarning.AutoSize = true;
        this._lblWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._lblWarning.Location = new System.Drawing.Point(23, 48);
        this._lblWarning.Name = "_lblWarning";
        this._lblWarning.Size = new System.Drawing.Size(438, 38);
        this._lblWarning.TabIndex = 1;
        this._lblWarning.Text = "تنبيه: سيؤدي إلغاء الحركة إلى إعادة احتساب آخر دخول مسموح للمستفيد تلقائياً والسماح بالدخول إذا استوفى الشروط.";
        // 
        // _lblReasonPrompt
        // 
        this._lblReasonPrompt.AutoSize = true;
        this._lblReasonPrompt.Location = new System.Drawing.Point(211, 88);
        this._lblReasonPrompt.Name = "_lblReasonPrompt";
        this._lblReasonPrompt.Size = new System.Drawing.Size(250, 19);
        this._lblReasonPrompt.TabIndex = 2;
        this._lblReasonPrompt.Text = "سبب الإلغاء (إلزامي - 5 أحرف على الأقل):";
        // 
        // _txtReason
        // 
        this._txtReason.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtReason.Location = new System.Drawing.Point(19, 119);
        this._txtReason.MaxLength = 500;
        this._txtReason.Multiline = true;
        this._txtReason.Name = "_txtReason";
        this._txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this._txtReason.Size = new System.Drawing.Size(442, 92);
        this._txtReason.TabIndex = 3;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnConfirm);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 280);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(480, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnConfirm
        // 
        this._btnConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnConfirm.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnConfirm.ForeColor = System.Drawing.Color.White;
        this._btnConfirm.Location = new System.Drawing.Point(348, 11);
        this._btnConfirm.Name = "_btnConfirm";
        this._btnConfirm.Size = new System.Drawing.Size(100, 32);
        this._btnConfirm.TabIndex = 0;
        this._btnConfirm.Text = "تأكيد الإلغاء";
        this._btnConfirm.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Location = new System.Drawing.Point(242, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "تراجع";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // VoidReasonDialog
        // 
        this.AcceptButton = this._btnConfirm;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(480, 330);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "VoidReasonDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إلغاء عملية دخول";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblAttemptInfo;
    private System.Windows.Forms.Label _lblWarning;
    private System.Windows.Forms.Label _lblReasonPrompt;
    private System.Windows.Forms.TextBox _txtReason;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnConfirm;
    private System.Windows.Forms.Button _btnCancel;
}
