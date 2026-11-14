#nullable disable

namespace ParkGate.WinForms.Forms;

partial class SelectOrganizationDialog
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
        this._lblHeader = new System.Windows.Forms.Label();
        this._lstOrganizations = new System.Windows.Forms.ListBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSelect = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // _lblHeader
        //
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._lblHeader.Size = new System.Drawing.Size(420, 44);
        this._lblHeader.TabIndex = 2;
        this._lblHeader.Text = "البطاقة مسجلة لدى أكثر من جهة. اختر الجهة:";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // _lstOrganizations
        //
        this._lstOrganizations.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lstOrganizations.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lstOrganizations.FormattingEnabled = true;
        this._lstOrganizations.IntegralHeight = false;
        this._lstOrganizations.ItemHeight = 26;
        this._lstOrganizations.Location = new System.Drawing.Point(0, 44);
        this._lstOrganizations.Name = "_lstOrganizations";
        this._lstOrganizations.Size = new System.Drawing.Size(420, 140);
        this._lstOrganizations.TabIndex = 0;
        //
        // _pnlButtons
        //
        this._pnlButtons.Controls.Add(this._btnSelect);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 184);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._pnlButtons.Size = new System.Drawing.Size(420, 56);
        this._pnlButtons.TabIndex = 1;
        //
        // _btnSelect
        //
        this._btnSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSelect.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSelect.FlatAppearance.BorderSize = 0;
        this._btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSelect.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSelect.ForeColor = System.Drawing.Color.White;
        this._btnSelect.Location = new System.Drawing.Point(15, 11);
        this._btnSelect.Name = "_btnSelect";
        this._btnSelect.Size = new System.Drawing.Size(120, 38);
        this._btnSelect.TabIndex = 0;
        this._btnSelect.Text = "اختيار";
        this._btnSelect.UseVisualStyleBackColor = false;
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
        this._btnCancel.Location = new System.Drawing.Point(141, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 38);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        //
        // SelectOrganizationDialog
        //
        this.AcceptButton = this._btnSelect;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(420, 240);
        this.Controls.Add(this._lstOrganizations);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._lblHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SelectOrganizationDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "اختيار الجهة";
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.ListBox _lstOrganizations;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSelect;
    private System.Windows.Forms.Button _btnCancel;
}
