#nullable disable

namespace ParkGate.WinForms.Forms;

partial class EditAgeCategoryDialog
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
        this._lblNameAr = new System.Windows.Forms.Label();
        this._txtNameAr = new System.Windows.Forms.TextBox();
        this._lblDescriptionAr = new System.Windows.Forms.Label();
        this._txtDescriptionAr = new System.Windows.Forms.TextBox();
        this._lblSortOrder = new System.Windows.Forms.Label();
        this._numSortOrder = new System.Windows.Forms.NumericUpDown();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numSortOrder)).BeginInit();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
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
        this._lblHeader.Text = "بيانات الفئة العمرية";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblNameAr, 0, 0);
        this._tblLayout.Controls.Add(this._txtNameAr, 1, 0);
        this._tblLayout.Controls.Add(this._lblDescriptionAr, 0, 1);
        this._tblLayout.Controls.Add(this._txtDescriptionAr, 1, 1);
        this._tblLayout.Controls.Add(this._lblSortOrder, 0, 2);
        this._tblLayout.Controls.Add(this._numSortOrder, 1, 2);
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
        // _lblNameAr
        // 
        this._lblNameAr.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblNameAr.AutoSize = true;
        this._lblNameAr.Location = new System.Drawing.Point(320, 24);
        this._lblNameAr.Name = "_lblNameAr";
        this._lblNameAr.Size = new System.Drawing.Size(69, 19);
        this._lblNameAr.TabIndex = 0;
        this._lblNameAr.Text = "اسم الفئة:";
        // 
        // _txtNameAr
        // 
        this._txtNameAr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtNameAr.Location = new System.Drawing.Point(19, 21);
        this._txtNameAr.MaxLength = 100;
        this._txtNameAr.Name = "_txtNameAr";
        this._txtNameAr.Size = new System.Drawing.Size(262, 25);
        this._txtNameAr.TabIndex = 1;
        // 
        // _lblDescriptionAr
        // 
        this._lblDescriptionAr.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblDescriptionAr.AutoSize = true;
        this._lblDescriptionAr.Location = new System.Drawing.Point(320, 60);
        this._lblDescriptionAr.Name = "_lblDescriptionAr";
        this._lblDescriptionAr.Size = new System.Drawing.Size(53, 19);
        this._lblDescriptionAr.TabIndex = 2;
        this._lblDescriptionAr.Text = "الوصف:";
        // 
        // _txtDescriptionAr
        // 
        this._txtDescriptionAr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtDescriptionAr.Location = new System.Drawing.Point(19, 57);
        this._txtDescriptionAr.MaxLength = 250;
        this._txtDescriptionAr.Name = "_txtDescriptionAr";
        this._txtDescriptionAr.Size = new System.Drawing.Size(262, 25);
        this._txtDescriptionAr.TabIndex = 3;
        // 
        // _lblSortOrder
        // 
        this._lblSortOrder.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblSortOrder.AutoSize = true;
        this._lblSortOrder.Location = new System.Drawing.Point(320, 96);
        this._lblSortOrder.Name = "_lblSortOrder";
        this._lblSortOrder.Size = new System.Drawing.Size(52, 19);
        this._lblSortOrder.TabIndex = 4;
        this._lblSortOrder.Text = "الترتيب:";
        // 
        // _numSortOrder
        // 
        this._numSortOrder.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numSortOrder.Location = new System.Drawing.Point(161, 93);
        this._numSortOrder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        this._numSortOrder.Name = "_numSortOrder";
        this._numSortOrder.Size = new System.Drawing.Size(120, 25);
        this._numSortOrder.TabIndex = 5;
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
        this._pnlButtons.Size = new System.Drawing.Size(460, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(328, 11);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(100, 32);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Location = new System.Drawing.Point(222, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // EditAgeCategoryDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(460, 250);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditAgeCategoryDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "بيانات الفئة العمرية";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numSortOrder)).EndInit();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblNameAr;
    private System.Windows.Forms.TextBox _txtNameAr;
    private System.Windows.Forms.Label _lblDescriptionAr;
    private System.Windows.Forms.TextBox _txtDescriptionAr;
    private System.Windows.Forms.Label _lblSortOrder;
    private System.Windows.Forms.NumericUpDown _numSortOrder;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
