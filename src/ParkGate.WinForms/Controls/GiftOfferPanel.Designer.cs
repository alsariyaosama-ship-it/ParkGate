#nullable disable

namespace ParkGate.WinForms.Controls;

partial class GiftOfferPanel
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
        this._grpMain = new System.Windows.Forms.GroupBox();
        this._tblContent = new System.Windows.Forms.TableLayoutPanel();
        this._flowButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnIssue = new System.Windows.Forms.Button();
        this._btnIssueOverride = new System.Windows.Forms.Button();
        this._pnlInfo = new System.Windows.Forms.Panel();
        this._lblDetails = new System.Windows.Forms.Label();
        this._lblStatus = new System.Windows.Forms.Label();
        this._grpMain.SuspendLayout();
        this._tblContent.SuspendLayout();
        this._flowButtons.SuspendLayout();
        this._pnlInfo.SuspendLayout();
        this.SuspendLayout();
        // 
        // _grpMain
        // 
        this._grpMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
        this._grpMain.Controls.Add(this._tblContent);
        this._grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpMain.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._grpMain.Location = new System.Drawing.Point(0, 0);
        this._grpMain.Name = "_grpMain";
        this._grpMain.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        this._grpMain.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._grpMain.Size = new System.Drawing.Size(988, 70);
        this._grpMain.TabIndex = 0;
        this._grpMain.TabStop = false;
        this._grpMain.Text = "قسيمة هدية الألعاب";
        // 
        // _tblContent
        // 
        this._tblContent.ColumnCount = 2;
        this._tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblContent.Controls.Add(this._flowButtons, 1, 0);
        this._tblContent.Controls.Add(this._pnlInfo, 0, 0);
        this._tblContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblContent.Location = new System.Drawing.Point(8, 28);
        this._tblContent.Margin = new System.Windows.Forms.Padding(0);
        this._tblContent.Name = "_tblContent";
        this._tblContent.RowCount = 1;
        this._tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblContent.Size = new System.Drawing.Size(972, 38);
        this._tblContent.TabIndex = 0;
        // 
        // _flowButtons
        // 
        this._flowButtons.AutoSize = true;
        this._flowButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._flowButtons.Controls.Add(this._btnIssue);
        this._flowButtons.Controls.Add(this._btnIssueOverride);
        this._flowButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this._flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._flowButtons.Location = new System.Drawing.Point(0, 0);
        this._flowButtons.Margin = new System.Windows.Forms.Padding(0);
        this._flowButtons.Name = "_flowButtons";
        this._flowButtons.Size = new System.Drawing.Size(378, 38);
        this._flowButtons.TabIndex = 1;
        this._flowButtons.WrapContents = false;
        // 
        // _btnIssue
        // 
        this._btnIssue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnIssue.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnIssue.FlatAppearance.BorderSize = 0;
        this._btnIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnIssue.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnIssue.ForeColor = System.Drawing.Color.White;
        this._btnIssue.Location = new System.Drawing.Point(216, 2);
        this._btnIssue.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
        this._btnIssue.Name = "_btnIssue";
        this._btnIssue.Size = new System.Drawing.Size(158, 34);
        this._btnIssue.TabIndex = 0;
        this._btnIssue.Text = "إصدار QR الهدية (F6)";
        this._btnIssue.UseVisualStyleBackColor = false;
        // 
        // _btnIssueOverride
        // 
        this._btnIssueOverride.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._btnIssueOverride.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnIssueOverride.FlatAppearance.BorderSize = 0;
        this._btnIssueOverride.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnIssueOverride.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnIssueOverride.ForeColor = System.Drawing.Color.White;
        this._btnIssueOverride.Location = new System.Drawing.Point(4, 2);
        this._btnIssueOverride.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
        this._btnIssueOverride.Name = "_btnIssueOverride";
        this._btnIssueOverride.Size = new System.Drawing.Size(204, 34);
        this._btnIssueOverride.TabIndex = 1;
        this._btnIssueOverride.Text = "إصدار بمدة مبكرة — مشرف (F7)";
        this._btnIssueOverride.UseVisualStyleBackColor = false;
        this._btnIssueOverride.Visible = false;
        // 
        // _pnlInfo
        // 
        this._pnlInfo.Controls.Add(this._lblDetails);
        this._pnlInfo.Controls.Add(this._lblStatus);
        this._pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlInfo.Location = new System.Drawing.Point(378, 0);
        this._pnlInfo.Margin = new System.Windows.Forms.Padding(0);
        this._pnlInfo.Name = "_pnlInfo";
        this._pnlInfo.Size = new System.Drawing.Size(594, 38);
        this._pnlInfo.TabIndex = 0;
        // 
        // _lblDetails
        // 
        this._lblDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblDetails.Font = new System.Drawing.Font("Cairo", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblDetails.Location = new System.Drawing.Point(4, 18);
        this._lblDetails.Name = "_lblDetails";
        this._lblDetails.Size = new System.Drawing.Size(586, 18);
        this._lblDetails.TabIndex = 1;
        // 
        // _lblStatus
        // 
        this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblStatus.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblStatus.Location = new System.Drawing.Point(4, 0);
        this._lblStatus.Name = "_lblStatus";
        this._lblStatus.Size = new System.Drawing.Size(586, 20);
        this._lblStatus.TabIndex = 0;
        this._lblStatus.Text = "جاري فحص استحقاق الهدية...";
        // 
        // GiftOfferPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this._grpMain);
        this.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Name = "GiftOfferPanel";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(988, 70);
        this._grpMain.ResumeLayout(false);
        this._tblContent.ResumeLayout(false);
        this._tblContent.PerformLayout();
        this._flowButtons.ResumeLayout(false);
        this._pnlInfo.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.GroupBox _grpMain;
    private System.Windows.Forms.TableLayoutPanel _tblContent;
    private System.Windows.Forms.Panel _pnlInfo;
    private System.Windows.Forms.Label _lblStatus;
    private System.Windows.Forms.Label _lblDetails;
    private System.Windows.Forms.FlowLayoutPanel _flowButtons;
    private System.Windows.Forms.Button _btnIssue;
    private System.Windows.Forms.Button _btnIssueOverride;
}
