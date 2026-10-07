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

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.Label _lblHeaderSubtitle;
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

    private void InitializeComponent()
    {
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._lblHeaderSubtitle = new System.Windows.Forms.Label();
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
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblHeaderSubtitle);
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Margin = new System.Windows.Forms.Padding(0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(500, 64);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(16, 8);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Size = new System.Drawing.Size(468, 26);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "بيانات الفئة العمرية";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _lblHeaderSubtitle
        // 
        this._lblHeaderSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblHeaderSubtitle.Font = new System.Drawing.Font("Cairo", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
        this._lblHeaderSubtitle.Location = new System.Drawing.Point(16, 34);
        this._lblHeaderSubtitle.Name = "_lblHeaderSubtitle";
        this._lblHeaderSubtitle.Size = new System.Drawing.Size(468, 20);
        this._lblHeaderSubtitle.TabIndex = 1;
        this._lblHeaderSubtitle.Text = "تحديد اسم الفئة العمرية ووصفها وترتيب ظهورها في النظام";
        this._lblHeaderSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblNameAr, 0, 0);
        this._tblLayout.Controls.Add(this._txtNameAr, 1, 0);
        this._tblLayout.Controls.Add(this._lblDescriptionAr, 0, 1);
        this._tblLayout.Controls.Add(this._txtDescriptionAr, 1, 1);
        this._tblLayout.Controls.Add(this._lblSortOrder, 0, 2);
        this._tblLayout.Controls.Add(this._numSortOrder, 1, 2);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 64);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
        this._tblLayout.RowCount = 3;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        this._tblLayout.Size = new System.Drawing.Size(500, 186);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblNameAr
        // 
        this._lblNameAr.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblNameAr.AutoSize = true;
        this._lblNameAr.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblNameAr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this._lblNameAr.Location = new System.Drawing.Point(370, 26);
        this._lblNameAr.Name = "_lblNameAr";
        this._lblNameAr.Size = new System.Drawing.Size(73, 20);
        this._lblNameAr.TabIndex = 0;
        this._lblNameAr.Text = "اسم الفئة *:";
        // 
        // _txtNameAr
        // 
        this._txtNameAr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtNameAr.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._txtNameAr.Location = new System.Drawing.Point(23, 21);
        this._txtNameAr.MaxLength = 100;
        this._txtNameAr.Name = "_txtNameAr";
        this._txtNameAr.Size = new System.Drawing.Size(324, 30);
        this._txtNameAr.TabIndex = 1;
        // 
        // _lblDescriptionAr
        // 
        this._lblDescriptionAr.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblDescriptionAr.AutoSize = true;
        this._lblDescriptionAr.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDescriptionAr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this._lblDescriptionAr.Location = new System.Drawing.Point(370, 72);
        this._lblDescriptionAr.Name = "_lblDescriptionAr";
        this._lblDescriptionAr.Size = new System.Drawing.Size(56, 20);
        this._lblDescriptionAr.TabIndex = 2;
        this._lblDescriptionAr.Text = "الوصف:";
        // 
        // _txtDescriptionAr
        // 
        this._txtDescriptionAr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtDescriptionAr.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._txtDescriptionAr.Location = new System.Drawing.Point(23, 67);
        this._txtDescriptionAr.MaxLength = 250;
        this._txtDescriptionAr.Name = "_txtDescriptionAr";
        this._txtDescriptionAr.Size = new System.Drawing.Size(324, 30);
        this._txtDescriptionAr.TabIndex = 3;
        // 
        // _lblSortOrder
        // 
        this._lblSortOrder.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblSortOrder.AutoSize = true;
        this._lblSortOrder.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblSortOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this._lblSortOrder.Location = new System.Drawing.Point(370, 118);
        this._lblSortOrder.Name = "_lblSortOrder";
        this._lblSortOrder.Size = new System.Drawing.Size(54, 20);
        this._lblSortOrder.TabIndex = 4;
        this._lblSortOrder.Text = "الترتيب:";
        // 
        // _numSortOrder
        // 
        this._numSortOrder.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numSortOrder.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._numSortOrder.Location = new System.Drawing.Point(227, 113);
        this._numSortOrder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        this._numSortOrder.Name = "_numSortOrder";
        this._numSortOrder.Size = new System.Drawing.Size(120, 30);
        this._numSortOrder.TabIndex = 5;
        this._numSortOrder.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 250);
        this._pnlButtons.Margin = new System.Windows.Forms.Padding(0);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(500, 52);
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
        this._btnSave.Location = new System.Drawing.Point(324, 8);
        this._btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(144, 36);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ البيانات (Enter)";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        this._btnCancel.Location = new System.Drawing.Point(212, 8);
        this._btnCancel.Margin = new System.Windows.Forms.Padding(0);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(104, 36);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء الأمر";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // EditAgeCategoryDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(500, 302);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditAgeCategoryDialog";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "بيانات الفئة العمرية - ParkGate";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numSortOrder)).EndInit();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
