#nullable disable

namespace ParkGate.WinForms.Controls;

partial class TodayEntriesPanel
{
    /// <summary>Required designer variable.</summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>Clean up any resources being used.</summary>
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
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        this._grpToday = new ParkGate.WinForms.Controls.RtlGroupBox();
        this._gridToday = new System.Windows.Forms.DataGridView();
        this._colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPersons = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._lblTodaySummary = new System.Windows.Forms.Label();
        this._pnlTodayActions = new System.Windows.Forms.FlowLayoutPanel();
        this._btnVoid = new System.Windows.Forms.Button();
        this._grpToday.SuspendLayout();
        this._pnlTodayActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).BeginInit();
        this.SuspendLayout();
        // 
        // _grpToday
        // 
        this._grpToday.BackColor = System.Drawing.Color.White;
        this._grpToday.Controls.Add(this._gridToday);
        this._grpToday.Controls.Add(this._pnlTodayActions);
        this._grpToday.Controls.Add(this._lblTodaySummary);
        this._grpToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpToday.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._grpToday.Location = new System.Drawing.Point(0, 0);
        this._grpToday.Name = "_grpToday";
        this._grpToday.Padding = new System.Windows.Forms.Padding(6);
        this._grpToday.Size = new System.Drawing.Size(988, 213);
        this._grpToday.TabIndex = 4;
        this._grpToday.TabStop = false;
        this._grpToday.Text = "سجل الدخول لهذا اليوم بهذا الشباك (آخر 20 محاولة)";
        // 
        // _gridToday
        // 
        this._gridToday.AllowUserToAddRows = false;
        this._gridToday.AllowUserToDeleteRows = false;
        this._gridToday.AllowUserToResizeRows = false;
        this._gridToday.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridToday.BackgroundColor = System.Drawing.Color.White;
        this._gridToday.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridToday.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
        this._gridToday.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(4);
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this._gridToday.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        this._gridToday.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        this._gridToday.ColumnHeadersHeight = 36;
        this._gridToday.RowTemplate.Height = 32;
        this._gridToday.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colTime,
            this._colCard,
            this._colName,
            this._colOrg,
            this._colPersons,
            this._colResult});
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(3);
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this._gridToday.DefaultCellStyle = dataGridViewCellStyle2;
        this._gridToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridToday.EnableHeadersVisualStyles = false;
        this._gridToday.Location = new System.Drawing.Point(6, 23);
        this._gridToday.MultiSelect = false;
        this._gridToday.Name = "_gridToday";
        this._gridToday.ReadOnly = true;
        this._gridToday.RowHeadersVisible = false;
        this._gridToday.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridToday.Size = new System.Drawing.Size(976, 152);
        this._gridToday.TabIndex = 0;
        // 
        // _pnlTodayActions
        // 
        this._pnlTodayActions.AutoSize = true;
        this._pnlTodayActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._pnlTodayActions.Controls.Add(this._btnVoid);
        this._pnlTodayActions.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlTodayActions.Location = new System.Drawing.Point(6, 23);
        this._pnlTodayActions.Margin = new System.Windows.Forms.Padding(0);
        this._pnlTodayActions.Name = "_pnlTodayActions";
        this._pnlTodayActions.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this._pnlTodayActions.Size = new System.Drawing.Size(976, 40);
        this._pnlTodayActions.TabIndex = 2;
        this._pnlTodayActions.WrapContents = false;
        // 
        // _btnVoid
        // 
        this._btnVoid.Enabled = false;
        this._btnVoid.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnVoid.Location = new System.Drawing.Point(776, 0);
        this._btnVoid.Margin = new System.Windows.Forms.Padding(0);
        this._btnVoid.MinimumSize = new System.Drawing.Size(200, 36);
        this._btnVoid.Name = "_btnVoid";
        this._btnVoid.Size = new System.Drawing.Size(200, 36);
        this._btnVoid.TabIndex = 0;
        this._btnVoid.Text = "إلغاء العملية… (F8)";
        this._btnVoid.UseVisualStyleBackColor = false;
        // 
        // _colTime
        // 
        this._colTime.FillWeight = 60F;
        this._colTime.HeaderText = "الوقت";
        this._colTime.Name = "_colTime";
        this._colTime.ReadOnly = true;
        // 
        // _colCard
        // 
        this._colCard.FillWeight = 80F;
        this._colCard.HeaderText = "رقم البطاقة";
        this._colCard.Name = "_colCard";
        this._colCard.ReadOnly = true;
        // 
        // _colName
        // 
        this._colName.FillWeight = 120F;
        this._colName.HeaderText = "اسم الزائر";
        this._colName.Name = "_colName";
        this._colName.ReadOnly = true;
        // 
        // _colOrg
        // 
        this._colOrg.FillWeight = 100F;
        this._colOrg.HeaderText = "الجهة";
        this._colOrg.Name = "_colOrg";
        this._colOrg.ReadOnly = true;
        // 
        // _colPersons
        // 
        this._colPersons.FillWeight = 50F;
        this._colPersons.HeaderText = "الأفراد";
        this._colPersons.Name = "_colPersons";
        this._colPersons.ReadOnly = true;
        // 
        // _colResult
        // 
        this._colResult.FillWeight = 70F;
        this._colResult.HeaderText = "النتيجة";
        this._colResult.Name = "_colResult";
        this._colResult.ReadOnly = true;
        // 
        // _lblTodaySummary
        // 
        this._lblTodaySummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._lblTodaySummary.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblTodaySummary.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTodaySummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._lblTodaySummary.Location = new System.Drawing.Point(6, 175);
        this._lblTodaySummary.Name = "_lblTodaySummary";
        this._lblTodaySummary.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
        this._lblTodaySummary.Size = new System.Drawing.Size(976, 32);
        this._lblTodaySummary.TabIndex = 1;
        this._lblTodaySummary.Text = "المقبول: 0 | التجاوز: 0 | المرفوض: 0 | إجمالي الأفراد: 0";
        this._lblTodaySummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // TodayEntriesPanel
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this._grpToday);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Margin = new System.Windows.Forms.Padding(0);
        this.Name = "TodayEntriesPanel";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(988, 213);
        this._grpToday.ResumeLayout(false);
        this._grpToday.PerformLayout();
        this._pnlTodayActions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).EndInit();
        this.ResumeLayout(false);
    }

    private ParkGate.WinForms.Controls.RtlGroupBox _grpToday;
    private System.Windows.Forms.DataGridView _gridToday;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colOrg;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPersons;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colResult;
    private System.Windows.Forms.Label _lblTodaySummary;
    private System.Windows.Forms.FlowLayoutPanel _pnlTodayActions;
    private System.Windows.Forms.Button _btnVoid;
}
