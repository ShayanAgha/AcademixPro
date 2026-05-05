using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class AuditLogControl : UserControl
    {
        private DataGridView dgvLog = null!;

        public AuditLogControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            DoubleBuffered = true;
            Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            // ── Top bar ───────────────────────────────────────────────
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent,
            };

            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 36);
            btnRefresh.Location = new Point(0, 10);
            btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadData();
            topPanel.Controls.Add(btnRefresh);

            Controls.Add(topPanel);

            // ── Info label ────────────────────────────────────────────
            var lblInfo = new Label
            {
                Text = "🧾 Audit log captures all INSERT, UPDATE, and DELETE operations on the Students table via database triggers.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextMuted,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = AppColors.SurfaceLight,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
            };
            Controls.Add(lblInfo);

            // ── Grid ──────────────────────────────────────────────────
            dgvLog = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvLog);
            Controls.Add(dgvLog);

            lblInfo.BringToFront();
            topPanel.BringToFront();

            LoadData();
        }

        private void LoadData()
        {
            dgvLog.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.AuditLog.GetAll);
            if (dgvLog.Columns.Contains("LogID"))
                dgvLog.Columns["LogID"].Visible = false;
            var map = new Dictionary<string, string>
            {
                ["TableName"]  = "Table",
                ["Action"]     = "Action",
                ["RecordID"]   = "Record ID",
                ["ChangedBy"]  = "Changed By",
                ["ChangeTime"] = "Timestamp",
                ["Details"]    = "Details",
            };
            foreach (var kv in map)
                if (dgvLog.Columns.Contains(kv.Key))
                    dgvLog.Columns[kv.Key].HeaderText = kv.Value;
        }
    }
}
