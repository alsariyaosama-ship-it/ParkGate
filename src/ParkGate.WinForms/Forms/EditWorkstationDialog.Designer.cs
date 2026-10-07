#nullable disable

namespace ParkGate.WinForms.Forms;

partial class EditWorkstationDialog
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
        this._lblMachineNameTitle = new System.Windows.Forms.Label();
        this._lblMachineNameValue = new System.Windows.Forms.Label();
        this._lblDisplayName = new System.Windows.Forms.Label();
        this._txtDisplayName = new System.Windows.Forms.TextBox();
        this._lblType = new System.Windows.Forms.Label();
        this._cmbType = new System.Windows.Forms.ComboBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(460, 50);
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
        this._lblHeader.Size = new System.Drawing.Size(460, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "تعديل محطة العمل";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblMachineNameTitle, 0, 0);
        this._tblLayout.Controls.Add(this._lblMachineNameValue, 1, 0);
        this._tblLayout.Controls.Add(this._lblDisplayName, 0, 1);
        this._tblLayout.Controls.Add(this._txtDisplayName, 1, 1);
        this._tblLayout.Controls.Add(this._lblType, 0, 2);
        this._tblLayout.Controls.Add(this._cmbType, 1, 2);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16);
        this._tblLayout.RowCount = 3;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.Size = new System.Drawing.Size(460, 150);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblMachineNameTitle
        // 
        this._lblMachineNameTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblMachineNameTitle.AutoSize = true;
        this._lblMachineNameTitle.Location = new System.Drawing.Point(320, 24);
        this._lblMachineNameTitle.Name = "_lblMachineNameTitle";
        this._lblMachineNameTitle.Size = new System.Drawing.Size(124, 19);
        this._lblMachineNameTitle.TabIndex = 0;
        this._lblMachineNameTitle.Text = "اسم الجهاز بالشبكة:";
        // 
        // _lblMachineNameValue
        // 
        this._lblMachineNameValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblMachineNameValue.AutoSize = true;
        this._lblMachineNameValue.Font = new System.Drawing.Font("Cairo", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblMachineNameValue.Location = new System.Drawing.Point(230, 25);
        this._lblMachineNameValue.Name = "_lblMachineNameValue";
        this._lblMachineNameValue.Size = new System.Drawing.Size(51, 17);
        this._lblMachineNameValue.TabIndex = 1;
        this._lblMachineNameValue.Text = "DESK-1";
        // 
        // _lblDisplayName
        // 
        this._lblDisplayName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblDisplayName.AutoSize = true;
        this._lblDisplayName.Location = new System.Drawing.Point(320, 60);
        this._lblDisplayName.Name = "_lblDisplayName";
        this._lblDisplayName.Size = new System.Drawing.Size(126, 19);
        this._lblDisplayName.TabIndex = 2;
        this._lblDisplayName.Text = "الاسم التعريفي للمحطة:";
        // 
        // _txtDisplayName
        // 
        this._txtDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtDisplayName.Location = new System.Drawing.Point(19, 57);
        this._txtDisplayName.MaxLength = 100;
        this._txtDisplayName.Name = "_txtDisplayName";
        this._txtDisplayName.Size = new System.Drawing.Size(262, 25);
        this._txtDisplayName.TabIndex = 3;
        // 
        // _lblType
        // 
        this._lblType.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblType.AutoSize = true;
        this._lblType.Location = new System.Drawing.Point(320, 96);
        this._lblType.Name = "_lblType";
        this._lblType.Size = new System.Drawing.Size(73, 19);
        this._lblType.TabIndex = 4;
        this._lblType.Text = "نوع المحطة:";
        // 
        // _cmbType
        // 
        this._cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbType.FormattingEnabled = true;
        this._cmbType.Location = new System.Drawing.Point(19, 93);
        this._cmbType.Name = "_cmbType";
        this._cmbType.Size = new System.Drawing.Size(262, 25);
        this._cmbType.TabIndex = 5;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 200);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(460, 56);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSave.FlatAppearance.BorderSize = 0;
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(318, 9);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(110, 38);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnCancel.Location = new System.Drawing.Point(208, 9);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 38);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // EditWorkstationDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(460, 256);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditWorkstationDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "تعديل محطة العمل";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblMachineNameTitle;
    private System.Windows.Forms.Label _lblMachineNameValue;
    private System.Windows.Forms.Label _lblDisplayName;
    private System.Windows.Forms.TextBox _txtDisplayName;
    private System.Windows.Forms.Label _lblType;
    private System.Windows.Forms.ComboBox _cmbType;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
