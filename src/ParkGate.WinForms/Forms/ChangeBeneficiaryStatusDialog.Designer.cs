#nullable disable

namespace ParkGate.WinForms.Forms;

partial class ChangeBeneficiaryStatusDialog
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
        this._lblBeneficiaryInfo = new System.Windows.Forms.Label();
        this._lblCurrentStatus = new System.Windows.Forms.Label();
        this._lblTargetStatusPrompt = new System.Windows.Forms.Label();
        this._pnlStatusOptions = new System.Windows.Forms.FlowLayoutPanel();
        this._radActive = new System.Windows.Forms.RadioButton();
        this._radSuspended = new System.Windows.Forms.RadioButton();
        this._radBlocked = new System.Windows.Forms.RadioButton();
        this._lblReasonPrompt = new System.Windows.Forms.Label();
        this._txtReason = new System.Windows.Forms.TextBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnConfirm = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        this._pnlStatusOptions.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(500, 50);
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
        this._lblHeader.Size = new System.Drawing.Size(500, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "تغيير حالة المستفيد";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 1;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblBeneficiaryInfo, 0, 0);
        this._tblLayout.Controls.Add(this._lblCurrentStatus, 0, 1);
        this._tblLayout.Controls.Add(this._lblTargetStatusPrompt, 0, 2);
        this._tblLayout.Controls.Add(this._pnlStatusOptions, 0, 3);
        this._tblLayout.Controls.Add(this._lblReasonPrompt, 0, 4);
        this._tblLayout.Controls.Add(this._txtReason, 0, 5);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16);
        this._tblLayout.RowCount = 6;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Size = new System.Drawing.Size(500, 290);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblBeneficiaryInfo
        // 
        this._lblBeneficiaryInfo.AutoSize = true;
        this._lblBeneficiaryInfo.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblBeneficiaryInfo.Location = new System.Drawing.Point(365, 16);
        this._lblBeneficiaryInfo.Name = "_lblBeneficiaryInfo";
        this._lblBeneficiaryInfo.Size = new System.Drawing.Size(116, 19);
        this._lblBeneficiaryInfo.TabIndex = 0;
        this._lblBeneficiaryInfo.Text = "المستفيد: - (بطاقة: -)";
        // 
        // _lblCurrentStatus
        // 
        this._lblCurrentStatus.AutoSize = true;
        this._lblCurrentStatus.Location = new System.Drawing.Point(395, 48);
        this._lblCurrentStatus.Name = "_lblCurrentStatus";
        this._lblCurrentStatus.Size = new System.Drawing.Size(86, 19);
        this._lblCurrentStatus.TabIndex = 1;
        this._lblCurrentStatus.Text = "الحالة الحالية: -";
        // 
        // _lblTargetStatusPrompt
        // 
        this._lblTargetStatusPrompt.AutoSize = true;
        this._lblTargetStatusPrompt.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTargetStatusPrompt.Location = new System.Drawing.Point(344, 80);
        this._lblTargetStatusPrompt.Name = "_lblTargetStatusPrompt";
        this._lblTargetStatusPrompt.Size = new System.Drawing.Size(137, 19);
        this._lblTargetStatusPrompt.TabIndex = 2;
        this._lblTargetStatusPrompt.Text = "الحالة الجديدة المطلوبة:";
        // 
        // _pnlStatusOptions
        // 
        this._pnlStatusOptions.Controls.Add(this._radActive);
        this._pnlStatusOptions.Controls.Add(this._radSuspended);
        this._pnlStatusOptions.Controls.Add(this._radBlocked);
        this._pnlStatusOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlStatusOptions.Location = new System.Drawing.Point(19, 111);
        this._pnlStatusOptions.Name = "_pnlStatusOptions";
        this._pnlStatusOptions.Size = new System.Drawing.Size(462, 39);
        this._pnlStatusOptions.TabIndex = 3;
        // 
        // _radActive
        // 
        this._radActive.AutoSize = true;
        this._radActive.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._radActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._radActive.Location = new System.Drawing.Point(359, 3);
        this._radActive.Name = "_radActive";
        this._radActive.Size = new System.Drawing.Size(100, 23);
        this._radActive.TabIndex = 0;
        this._radActive.TabStop = true;
        this._radActive.Text = "ساري (نشط)";
        this._radActive.UseVisualStyleBackColor = true;
        // 
        // _radSuspended
        // 
        this._radSuspended.AutoSize = true;
        this._radSuspended.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._radSuspended.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._radSuspended.Location = new System.Drawing.Point(247, 3);
        this._radSuspended.Name = "_radSuspended";
        this._radSuspended.Size = new System.Drawing.Size(106, 23);
        this._radSuspended.TabIndex = 1;
        this._radSuspended.TabStop = true;
        this._radSuspended.Text = "موقوف مؤقتاً";
        this._radSuspended.UseVisualStyleBackColor = true;
        // 
        // _radBlocked
        // 
        this._radBlocked.AutoSize = true;
        this._radBlocked.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._radBlocked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._radBlocked.Location = new System.Drawing.Point(135, 3);
        this._radBlocked.Name = "_radBlocked";
        this._radBlocked.Size = new System.Drawing.Size(106, 23);
        this._radBlocked.TabIndex = 2;
        this._radBlocked.TabStop = true;
        this._radBlocked.Text = "محظور نهائياً";
        this._radBlocked.UseVisualStyleBackColor = true;
        // 
        // _lblReasonPrompt
        // 
        this._lblReasonPrompt.AutoSize = true;
        this._lblReasonPrompt.Location = new System.Drawing.Point(92, 153);
        this._lblReasonPrompt.Name = "_lblReasonPrompt";
        this._lblReasonPrompt.Size = new System.Drawing.Size(389, 19);
        this._lblReasonPrompt.TabIndex = 4;
        this._lblReasonPrompt.Text = "سبب تغيير الحالة (مطلوب عند الإيقاف أو الحظر - 5 أحرف على الأقل):";
        // 
        // _txtReason
        // 
        this._txtReason.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtReason.Location = new System.Drawing.Point(19, 184);
        this._txtReason.MaxLength = 500;
        this._txtReason.Multiline = true;
        this._txtReason.Name = "_txtReason";
        this._txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this._txtReason.Size = new System.Drawing.Size(462, 87);
        this._txtReason.TabIndex = 5;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnConfirm);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 340);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(500, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnConfirm
        // 
        this._btnConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnConfirm.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnConfirm.ForeColor = System.Drawing.Color.White;
        this._btnConfirm.Location = new System.Drawing.Point(368, 11);
        this._btnConfirm.Name = "_btnConfirm";
        this._btnConfirm.Size = new System.Drawing.Size(100, 32);
        this._btnConfirm.TabIndex = 0;
        this._btnConfirm.Text = "تأكيد التغيير";
        this._btnConfirm.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Location = new System.Drawing.Point(262, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // ChangeBeneficiaryStatusDialog
        // 
        this.AcceptButton = this._btnConfirm;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(500, 390);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ChangeBeneficiaryStatusDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "تغيير حالة المستفيد";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlStatusOptions.ResumeLayout(false);
        this._pnlStatusOptions.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblBeneficiaryInfo;
    private System.Windows.Forms.Label _lblCurrentStatus;
    private System.Windows.Forms.Label _lblTargetStatusPrompt;
    private System.Windows.Forms.FlowLayoutPanel _pnlStatusOptions;
    private System.Windows.Forms.RadioButton _radActive;
    private System.Windows.Forms.RadioButton _radSuspended;
    private System.Windows.Forms.RadioButton _radBlocked;
    private System.Windows.Forms.Label _lblReasonPrompt;
    private System.Windows.Forms.TextBox _txtReason;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnConfirm;
    private System.Windows.Forms.Button _btnCancel;
}
