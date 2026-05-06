using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class AttendanceControl : UserControl
    {
        private DataGridView dgvAttendance = null!;
        private ComboBox cmbCourse = null!;
        private DataGridView dgvMark = null!;
        private DateTimePicker dtpDate = null!;
        private TabControl tabMain = null!;

        public AttendanceControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.Surface,
                Padding = new Padding(16, 0, 16, 0),
            };

            header.Controls.Add(new Label
            {
                Text = "Attendance Management",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16),
                BackColor = Color.Transparent,
            });
            Controls.Add(header);

            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = AppColors.SurfaceLight,
                Padding = new Padding(16, 8, 16, 8),
            };

            filterBar.Controls.Add(new Label
            {
                Text = "Course:",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(16, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
            });

            cmbCourse = new ComboBox
            {
                Location = new Point(16, 28),
                Size = new Size(360, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            UIHelper.StyleComboBox(cmbCourse);
            cmbCourse.SelectedIndexChanged += (_, __) => LoadAttendance();
            filterBar.Controls.Add(cmbCourse);

            filterBar.Controls.Add(new Label
            {
                Text = "Date:",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(392, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
            });

            dtpDate = UIHelper.CreateDatePicker();
            dtpDate.Location = new Point(392, 28);
            dtpDate.Size = new Size(160, 30);
            filterBar.Controls.Add(dtpDate);

            var btnMark = UIHelper.CreateButton("✅  Mark Attendance", AppColors.Primary, 180, 36);
            btnMark.Location = new Point(568, 24);
            btnMark.Click += BtnMark_Click;
            filterBar.Controls.Add(btnMark);

            var btnSummary = UIHelper.CreateButton("📊  Summary", AppColors.CardPurple, 130, 36);
            btnSummary.Location = new Point(758, 24);
            btnSummary.Click += BtnSummary_Click;
            filterBar.Controls.Add(btnSummary);

            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 100, 36);
            btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Location = new Point(898, 24);
            btnRefresh.Click += (_, __) => LoadAttendance();
            filterBar.Controls.Add(btnRefresh);

            Controls.Add(filterBar);
            header.BringToFront();
            filterBar.BringToFront();

            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
            };
            tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabMain.ItemSize = new Size(160, 36);
            tabMain.SizeMode = TabSizeMode.Fixed;
            tabMain.DrawItem += TabMain_DrawItem;
            Controls.Add(tabMain);

            var tabMark = new TabPage
            {
                Text = "📋  Mark Attendance",
                BackColor = AppColors.Background,
                Padding = new Padding(8),
            };

            var lblHint = new Label
            {
                Text = "Select a course above, then choose Present / Absent / Late for each student and click Mark Attendance.",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppColors.TextMuted,
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
            };
            tabMark.Controls.Add(lblHint);

            dgvMark = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvMark);
            dgvMark.ReadOnly = false;
            dgvMark.AllowUserToAddRows = false;
            dgvMark.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvMark.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabMark.Controls.Add(dgvMark);
            lblHint.BringToFront();
            tabMain.TabPages.Add(tabMark);

            var tabHistory = new TabPage
            {
                Text = "📜  Attendance History",
                BackColor = AppColors.Background,
                Padding = new Padding(8),
            };

            dgvAttendance = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvAttendance);
            dgvAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabHistory.Controls.Add(dgvAttendance);
            tabMain.TabPages.Add(tabHistory);

            tabMain.SendToBack();

            LoadCourses();
        }

        private void TabMain_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab = tabMain.TabPages[e.Index];
            var g = e.Graphics;
            bool selected = e.Index == tabMain.SelectedIndex;

            using var bgBrush = new SolidBrush(selected ? AppColors.Primary : AppColors.SurfaceLight);
            g.FillRectangle(bgBrush, e.Bounds);

            using var textBrush = new SolidBrush(selected ? Color.White : AppColors.TextSecondary);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(tab.Text, new Font("Segoe UI", 9.5f, selected ? FontStyle.Bold : FontStyle.Regular), textBrush, e.Bounds, sf);
        }

        private void LoadCourses()
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Courses.GetForDropdown);
            cmbCourse.DataSource = dt;
            cmbCourse.DisplayMember = "Display";
            cmbCourse.ValueMember = "CourseID";
        }

        private void LoadAttendance()
        {
            if (cmbCourse.SelectedValue == null || cmbCourse.SelectedValue is DataRowView) return;
            int courseId = Convert.ToInt32(cmbCourse.SelectedValue);

            var enrolled = DatabaseHelper.ExecuteQuery(
                SqlQueries.Attendance.GetEnrolledForCourse,
                new[] { new SqlParameter("@CourseID", courseId) });

            if (!enrolled.Columns.Contains("Status"))
            {
                enrolled.Columns.Add("Status", typeof(string));
                foreach (DataRow r in enrolled.Rows) r["Status"] = "Present";
            }
            if (!enrolled.Columns.Contains("Remarks"))
                enrolled.Columns.Add("Remarks", typeof(string));

            dgvMark.DataSource = null;
            dgvMark.Columns.Clear();
            dgvMark.DataSource = enrolled;

            if (dgvMark.Columns.Contains("EnrollmentID"))
                dgvMark.Columns["EnrollmentID"].Visible = false;

            if (dgvMark.Columns.Contains("StudentCode"))
            {
                dgvMark.Columns["StudentCode"].HeaderText = "Student ID";
                dgvMark.Columns["StudentCode"].ReadOnly = true;
                dgvMark.Columns["StudentCode"].Width = 120;
            }
            if (dgvMark.Columns.Contains("StudentName"))
            {
                dgvMark.Columns["StudentName"].HeaderText = "Student Name";
                dgvMark.Columns["StudentName"].ReadOnly = true;
                dgvMark.Columns["StudentName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (dgvMark.Columns.Contains("Status"))
            {
                int statusIdx = dgvMark.Columns["Status"].Index;
                dgvMark.Columns.RemoveAt(statusIdx);

                var cmbCol = new DataGridViewComboBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    Width = 130,
                    FlatStyle = FlatStyle.Flat,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                    DisplayIndex = statusIdx,
                };
                cmbCol.Items.AddRange("Present", "Absent", "Late");
                dgvMark.Columns.Insert(statusIdx, cmbCol);

                foreach (DataGridViewRow row in dgvMark.Rows)
                {
                    if (row.Cells["Status"].Value == null || row.Cells["Status"].Value == DBNull.Value)
                        row.Cells["Status"].Value = "Present";
                }
            }

            if (dgvMark.Columns.Contains("Remarks"))
            {
                dgvMark.Columns["Remarks"].HeaderText = "Remarks (optional)";
                dgvMark.Columns["Remarks"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            var history = DatabaseHelper.ExecuteQuery(
                SqlQueries.Attendance.GetByCourse,
                new[] { new SqlParameter("@CourseID", courseId) });

            dgvAttendance.DataSource = history;
            if (dgvAttendance.Columns.Contains("AttendanceID"))
                dgvAttendance.Columns["AttendanceID"].Visible = false;
            if (dgvAttendance.Columns.Contains("EnrollmentID"))
                dgvAttendance.Columns["EnrollmentID"].Visible = false;

            var histMap = new Dictionary<string, string>
            {
                ["StudentCode"]  = "Student ID",
                ["StudentName"]  = "Student Name",
                ["AttendDate"]   = "Date",
                ["Status"]       = "Status",
                ["Remarks"]      = "Remarks",
            };
            foreach (var kv in histMap)
                if (dgvAttendance.Columns.Contains(kv.Key))
                    dgvAttendance.Columns[kv.Key].HeaderText = kv.Value;
        }

        private void BtnMark_Click(object? s, EventArgs e)
        {
            if (dgvMark.Rows.Count == 0) { MessageBox.Show("No students to mark. Select a course first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            dgvMark.EndEdit();

            int count = 0;
            var errors = new List<string>();

            foreach (DataGridViewRow row in dgvMark.Rows)
            {
                if (row.IsNewRow) continue;

                object? enrollVal = null;
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("EnrollmentID"))
                    enrollVal = drv["EnrollmentID"];
                else
                    enrollVal = dgvMark.Columns.Contains("EnrollmentID") ? row.Cells["EnrollmentID"].Value : null;

                if (enrollVal == null || enrollVal == DBNull.Value) continue;
                int enrollId = Convert.ToInt32(enrollVal);

                string status = row.Cells["Status"].Value?.ToString() ?? "Present";
                if (status != "Present" && status != "Absent" && status != "Late") status = "Present";
                string remarks = row.Cells["Remarks"].Value?.ToString() ?? "";

                var p = new SqlParameter[]
                {
                    new("@EnrollmentID", enrollId),
                    new("@Date", dtpDate.Value.Date),
                    new("@Status", status),
                    new("@MarkedBy", Session.UserID),
                    new("@Remarks", remarks),
                };
                try
                {
                    if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Attendance.Insert, p) > 0) count++;
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if (errors.Count > 0)
                MessageBox.Show($"Marked {count} students.\nErrors: {string.Join("\n", errors)}", "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show($"Attendance marked for {count} student(s).", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadAttendance();
            tabMain.SelectedIndex = 1;
        }

        private void BtnSummary_Click(object? s, EventArgs e)
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Attendance.GetSummary);
            dgvAttendance.DataSource = dt;

            var sumMap = new Dictionary<string, string>
            {
                ["StudentCode"]  = "Student ID",
                ["StudentName"]  = "Student Name",
                ["CourseName"]   = "Course",
                ["TotalClasses"] = "Total Classes",
                ["Attended"]     = "Attended",
                ["Percentage"]   = "Attendance %",
            };
            foreach (var kv in sumMap)
                if (dgvAttendance.Columns.Contains(kv.Key))
                    dgvAttendance.Columns[kv.Key].HeaderText = kv.Value;

            tabMain.SelectedIndex = 1;
        }
    }
}
