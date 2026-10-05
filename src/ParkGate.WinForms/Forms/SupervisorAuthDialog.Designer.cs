#nullable disable

namespace ParkGate.WinForms.Forms;

partial class SupervisorAuthDialog
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._lblUsername = new System.Windows.Forms.Label();
        this._txtUsername = new System.Windows.Forms.TextBox();
        this._lblPassword = new System.Windows.Forms.Label();
        this._txtPassword = new System.Windows.Forms.TextBox();
        this._lblReason = new System.Windows.Forms.Label();
        this._txtReason = new System.Windows.Forms.TextBox();
        this._lblError = new System.Windows.Forms.Label();
        this._btnApprove = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
        this._pnlHeader.Size = new System.Drawing.Size(464, 60);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(16, 12);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Size = new System.Drawing.Size(432, 36);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "هذا الإجراء يتطلب مصادقة مشرف الوردية أو مدير النظام.";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _lblUsername
        // 
        this._lblUsername.AutoSize = true;
        this._lblUsername.Location = new System.Drawing.Point(28, 80);
        this._lblUsername.Name = "_lblUsername";
        this._lblUsername.Size = new System.Drawing.Size(126, 19);
        this._lblUsername.TabIndex = 1;
        this._lblUsername.Text = "اسم مستخدم المشرف:";
        // 
        // _txtUsername
        // 
        this._txtUsername.Location = new System.Drawing.Point(28, 105);
        this._txtUsername.MaxLength = 50;
        this._txtUsername.Name = "_txtUsername";
        this._txtUsername.Size = new System.Drawing.Size(408, 25);
        this._txtUsername.TabIndex = 2;
        // 
        // _lblPassword
        // 
        this._lblPassword.AutoSize = true;
        this._lblPassword.Location = new System.Drawing.Point(28, 145);
        this._lblPassword.Name = "_lblPassword";
        this._lblPassword.Size = new System.Drawing.Size(78, 19);
        this._lblPassword.TabIndex = 3;
        this._lblPassword.Text = "كلمة المرور:";
        // 
        // _txtPassword
        // 
        this._txtPassword.Location = new System.Drawing.Point(28, 170);
        this._txtPassword.MaxLength = 100;
        this._txtPassword.Name = "_txtPassword";
        this._txtPassword.Size = new System.Drawing.Size(408, 25);
        this._txtPassword.TabIndex = 4;
        this._txtPassword.UseSystemPasswordChar = true;
        // 
        // _lblReason
        // 
        this._lblReason.AutoSize = true;
        this._lblReason.Location = new System.Drawing.Point(28, 210);
        this._lblReason.Name = "_lblReason";
        this._lblReason.Size = new System.Drawing.Size(193, 19);
        this._lblReason.TabIndex = 5;
        this._lblReason.Text = "سبب الإجراء (5 أحرف على الأقل):";
        // 
        // _txtReason
        // 
        this._txtReason.Location = new System.Drawing.Point(28, 235);
        this._txtReason.MaxLength = 500;
        this._txtReason.Multiline = true;
        this._txtReason.Name = "_txtReason";
        this._txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this._txtReason.Size = new System.Drawing.Size(408, 65);
        this._txtReason.TabIndex = 6;
        // 
        // _lblError
        // 
        this._lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._lblError.Location = new System.Drawing.Point(28, 308);
        this._lblError.Name = "_lblError";
        this._lblError.Size = new System.Drawing.Size(408, 40);
        this._lblError.TabIndex = 7;
        this._lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _btnApprove
        // 
        this._btnApprove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnApprove.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnApprove.FlatAppearance.BorderSize = 0;
        this._btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnApprove.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnApprove.ForeColor = System.Drawing.Color.White;
        this._btnApprove.Location = new System.Drawing.Point(246, 360);
        this._btnApprove.Name = "_btnApprove";
        this._btnApprove.Size = new System.Drawing.Size(190, 40);
        this._btnApprove.TabIndex = 8;
        this._btnApprove.Text = "موافقة واعتماد";
        this._btnApprove.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._btnCancel.Location = new System.Drawing.Point(28, 360);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(120, 40);
        this._btnCancel.TabIndex = 9;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // SupervisorAuthDialog
        // 
        this.AcceptButton = this._btnApprove;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(464, 420);
        this.Controls.Add(this._btnCancel);
        this.Controls.Add(this._btnApprove);
        this.Controls.Add(this._lblError);
        this.Controls.Add(this._txtReason);
        this.Controls.Add(this._lblReason);
        this.Controls.Add(this._txtPassword);
        this.Controls.Add(this._lblPassword);
        this.Controls.Add(this._txtUsername);
        this.Controls.Add(this._lblUsername);
        this.Controls.Add(this._pnlHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SupervisorAuthDialog";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "موافقة المشرف";
        this._pnlHeader.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.Label _lblUsername;
    private System.Windows.Forms.TextBox _txtUsername;
    private System.Windows.Forms.Label _lblPassword;
    private System.Windows.Forms.TextBox _txtPassword;
    private System.Windows.Forms.Label _lblReason;
    private System.Windows.Forms.TextBox _txtReason;
    private System.Windows.Forms.Label _lblError;
    private System.Windows.Forms.Button _btnApprove;
    private System.Windows.Forms.Button _btnCancel;
}
