using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class InstructorsControl : UserControl
    {
        private DataGridView dgvInstructors = null!;
        private TextBox txtName = null!, txtEmail = null!, txtPhone = null!, txtSpecialization = null!;
        private ComboBox cmbDept = null!;
        private DateTimePicker dtpJoining = null!;
        private Label lblCode = null!;
        private int _selectedId = 0;

        public InstructorsControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            DoubleBuffered = true;
            Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 36);
            btnRefresh.Location = new Point(0, 10);
            btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadData();
            topPanel.Controls.Add(btnRefresh);
            Controls.Add(topPanel);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 8,
            };
            split.Panel1.BackColor = AppColors.Background;
            split.Panel2.BackColor = AppColors.Background;
            Controls.Add(split);
            topPanel.BringToFront();
            split.SizeChanged += (_, __) =>
            {
                int target = (int)(split.Width * 0.6);
                if (target > 0 && target < split.Width - 50)
                    try { split.SplitterDistance = target; } catch { }
            };

            dgvInstructors = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvInstructors);
            dgvInstructors.SelectionChanged += DgvSelectionChanged;
            split.Panel1.Controls.Add(dgvInstructors);

            var formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Surface,
                AutoScroll = true,
                Padding = new Padding(20),
            };
            split.Panel2.Controls.Add(formPanel);

            var lblTitle = new Label
            {
                Text = "Instructor Details",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true, Location = new Point(20, 16),
                BackColor = Color.Transparent,
            };
            formPanel.Controls.Add(lblTitle);

            int y = 52, x = 20, w = 280;

            formPanel.Controls.Add(FL("Instructor Code", x, y));
            lblCode = new Label
            {
                Text = "(auto-generated)", Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.TextMuted, Location = new Point(x, y + 22),
                AutoSize = true, BackColor = Color.Transparent,
            };
            formPanel.Controls.Add(lblCode);
            y += 54;

            formPanel.Controls.Add(FL("Full Name *", x, y));
            txtName = STB(x, y + 22, w); formPanel.Controls.Add(txtName); y += 60;

            formPanel.Controls.Add(FL("Email *", x, y));
            txtEmail = STB(x, y + 22, w); formPanel.Controls.Add(txtEmail); y += 60;

            formPanel.Controls.Add(FL("Phone", x, y));
            txtPhone = STB(x, y + 22, w); formPanel.Controls.Add(txtPhone); y += 60;

            formPanel.Controls.Add(FL("Specialization", x, y));
            txtSpecialization = STB(x, y + 22, w); formPanel.Controls.Add(txtSpecialization); y += 60;

            formPanel.Controls.Add(FL("Department *", x, y));
            cmbDept = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 30) };
            UIHelper.StyleComboBox(cmbDept);
            formPanel.Controls.Add(cmbDept); y += 60;

            formPanel.Controls.Add(FL("Joining Date", x, y));
            dtpJoining = UIHelper.CreateDatePicker();
            dtpJoining.Location = new Point(x, y + 22);
            dtpJoining.Size = new Size(w, 30);
            formPanel.Controls.Add(dtpJoining); y += 60;

            var btnAdd = UIHelper.CreateButton("➕ Add", AppColors.Primary, 130, 38);
            btnAdd.Location = new Point(20, y);
            btnAdd.Click += BtnAdd_Click;
            formPanel.Controls.Add(btnAdd);

            var btnUpdate = UIHelper.CreateButton("✏️ Update", AppColors.CardEmerald, 130, 38);
            btnUpdate.Location = new Point(160, y);
            btnUpdate.Click += BtnUpdate_Click;
            formPanel.Controls.Add(btnUpdate);

            var btnDeactivate = UIHelper.CreateButton("🗑️ Deactivate", AppColors.Danger, 130, 38);
            btnDeactivate.Location = new Point(20, y + 48);
            btnDeactivate.Click += BtnDeactivate_Click;
            formPanel.Controls.Add(btnDeactivate);

            var btnClear = UIHelper.CreateButton("🔄 Clear", AppColors.SurfaceLight, 130, 38);
            btnClear.ForeColor = AppColors.TextSecondary;
            btnClear.Location = new Point(160, y + 48);
            btnClear.Click += (_, __) => ClearForm();
            formPanel.Controls.Add(btnClear);

            LoadDepartments();
            LoadData();
            GenerateCode();
        }

        private void LoadData()
        {
            dgvInstructors.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Instructors.GetAll);
            if (dgvInstructors.Columns.Contains("InstructorID"))
                dgvInstructors.Columns["InstructorID"].Visible = false;
            ApplyColumnHeaders();
        }

        private void ApplyColumnHeaders()
        {
            var map = new Dictionary<string, string>
            {
                ["InstructorCode"] = "Instructor ID",
                ["FullName"] = "Full Name",
                ["Email"] = "Email",
                ["Phone"] = "Phone",
                ["Specialization"] = "Specialization",
                ["Department"] = "Department",
                ["JoiningDate"] = "Joining Date",
                ["IsActive"] = "Active",
                ["CoursesAssigned"] = "Courses",
            };
            foreach (var kv in map)
                if (dgvInstructors.Columns.Contains(kv.Key))
                    dgvInstructors.Columns[kv.Key].HeaderText = kv.Value;
        }

        private void LoadDepartments()
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Departments.GetForDropdown);
            cmbDept.DataSource = dt;
            cmbDept.DisplayMember = "DeptName";
            cmbDept.ValueMember = "DeptID";
        }

        private void GenerateCode()
        {
            var result = DatabaseHelper.ExecuteScalar(SqlQueries.Instructors.GetNextCode);
            lblCode.Text = result?.ToString() ?? "INS-NEW";
        }

        private void DgvSelectionChanged(object? sender, EventArgs e)
        {
            if (dgvInstructors.CurrentRow == null) return;
            var row = dgvInstructors.CurrentRow;
            _selectedId = Convert.ToInt32(row.Cells["InstructorID"].Value);
            lblCode.Text = row.Cells["InstructorCode"].Value?.ToString() ?? "";
            txtName.Text = row.Cells["FullName"].Value?.ToString() ?? "";
            txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
            txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? "";
            txtSpecialization.Text = row.Cells["Specialization"].Value?.ToString() ?? "";

            string? dept = row.Cells["Department"].Value?.ToString();
            if (!string.IsNullOrEmpty(dept))
            {
                for (int i = 0; i < cmbDept.Items.Count; i++)
                    if (((DataRowView)cmbDept.Items[i])["DeptName"].ToString() == dept)
                    { cmbDept.SelectedIndex = i; break; }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            { MessageBox.Show("Name and Email are required."); return; }

            var parms = new SqlParameter[]
            {
                new("@Code", lblCode.Text),
                new("@FullName", txtName.Text.Trim()),
                new("@Email", txtEmail.Text.Trim()),
                new("@Phone", txtPhone.Text.Trim()),
                new("@Specialization", txtSpecialization.Text.Trim()),
                new("@DeptID", cmbDept.SelectedValue),
                new("@JoiningDate", dtpJoining.Value.Date),
            };

            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Instructors.Insert, parms) > 0)
            {
                MessageBox.Show("Instructor added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm(); LoadData(); GenerateCode();
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0) { MessageBox.Show("Select an instructor first."); return; }

            var parms = new SqlParameter[]
            {
                new("@InstructorID", _selectedId),
                new("@FullName", txtName.Text.Trim()),
                new("@Email", txtEmail.Text.Trim()),
                new("@Phone", txtPhone.Text.Trim()),
                new("@Specialization", txtSpecialization.Text.Trim()),
                new("@DeptID", cmbDept.SelectedValue),
            };

            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Instructors.Update, parms) > 0)
            {
                MessageBox.Show("Instructor updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnDeactivate_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0) { MessageBox.Show("Select an instructor first."); return; }
            if (MessageBox.Show("Deactivate this instructor?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            DatabaseHelper.ExecuteNonQuery(SqlQueries.Instructors.Deactivate,
                new[] { new SqlParameter("@InstructorID", _selectedId) });
            MessageBox.Show("Instructor deactivated.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm(); LoadData();
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtName.Clear(); txtEmail.Clear(); txtPhone.Clear(); txtSpecialization.Clear();
            dtpJoining.Value = DateTime.Today;
            GenerateCode();
        }

        private Label FL(string text, int x, int y) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary,
            Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent,
        };

        private TextBox STB(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30) };
            UIHelper.StyleTextBox(tb); return tb;
        }
    }
}
