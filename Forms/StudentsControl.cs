using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class StudentsControl : UserControl
    {
        private DataGridView dgvStudents = null!;
        private TextBox txtSearch = null!;
        private TextBox txtName = null!, txtEmail = null!, txtPhone = null!, txtAddress = null!;
        private ComboBox cmbDept = null!, cmbGender = null!, cmbSemester = null!;
        private DateTimePicker dtpDOB = null!, dtpAdmission = null!;
        private Label lblStudentCode = null!;
        private int _selectedStudentId = 0;

        public StudentsControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            DoubleBuffered = true;
            Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            // ── Top bar: Search + Add ─────────────────────────────────
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 8),
            };

            txtSearch = new TextBox
            {
                Size = new Size(320, 36),
                Location = new Point(0, 10),
                PlaceholderText = "🔍 Search by name, code, or email...",
                Font = new Font("Segoe UI", 10.5f),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
            };
            txtSearch.TextChanged += (_, __) => SearchStudents();
            topPanel.Controls.Add(txtSearch);

            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 36);
            btnRefresh.Location = new Point(340, 10);
            btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadStudents();
            topPanel.Controls.Add(btnRefresh);

            Controls.Add(topPanel);

            // ── Split: Grid left, Form right ──────────────────────────
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = AppColors.Background,
                SplitterWidth = 8,
            };
            splitContainer.Panel1.BackColor = AppColors.Background;
            splitContainer.Panel2.BackColor = AppColors.Background;
            Controls.Add(splitContainer);
            topPanel.BringToFront();
            // Defer splitter distance until control is sized
            splitContainer.SizeChanged += (_, __) =>
            {
                int target = (int)(splitContainer.Width * 0.6);
                if (target > 0 && target < splitContainer.Width - 50)
                    try { splitContainer.SplitterDistance = target; } catch { }
            };

            // ── Data Grid ─────────────────────────────────────────────
            dgvStudents = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvStudents);
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;
            splitContainer.Panel1.Controls.Add(dgvStudents);

            // ── Form Panel ────────────────────────────────────────────
            var formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Surface,
                AutoScroll = true,
                Padding = new Padding(20),
            };
            splitContainer.Panel2.Controls.Add(formPanel);

            var lblFormTitle = new Label
            {
                Text = "Student Details",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 16),
                BackColor = Color.Transparent,
            };
            formPanel.Controls.Add(lblFormTitle);

            int y = 52;
            int lblX = 20, inputX = 20, inputW = 280;

            // Student Code (auto-generated)
            formPanel.Controls.Add(CreateFieldLabel("Student Code", lblX, y));
            lblStudentCode = new Label
            {
                Text = "(auto-generated)",
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.TextMuted,
                Location = new Point(inputX, y + 22),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            formPanel.Controls.Add(lblStudentCode);
            y += 54;

            // Full Name
            formPanel.Controls.Add(CreateFieldLabel("Full Name *", lblX, y));
            txtName = CreateStyledTextBox(inputX, y + 22, inputW);
            formPanel.Controls.Add(txtName);
            y += 60;

            // Email
            formPanel.Controls.Add(CreateFieldLabel("Email *", lblX, y));
            txtEmail = CreateStyledTextBox(inputX, y + 22, inputW);
            formPanel.Controls.Add(txtEmail);
            y += 60;

            // Phone
            formPanel.Controls.Add(CreateFieldLabel("Phone", lblX, y));
            txtPhone = CreateStyledTextBox(inputX, y + 22, inputW);
            formPanel.Controls.Add(txtPhone);
            y += 60;

            // Gender
            formPanel.Controls.Add(CreateFieldLabel("Gender", lblX, y));
            cmbGender = new ComboBox
            {
                Location = new Point(inputX, y + 22),
                Size = new Size(inputW, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            UIHelper.StyleComboBox(cmbGender);
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            formPanel.Controls.Add(cmbGender);
            y += 60;

            // DOB
            formPanel.Controls.Add(CreateFieldLabel("Date of Birth", lblX, y));
            dtpDOB = UIHelper.CreateDatePicker();
            dtpDOB.Location = new Point(inputX, y + 22);
            dtpDOB.Size = new Size(inputW, 30);
            formPanel.Controls.Add(dtpDOB);
            y += 60;

            // Department
            formPanel.Controls.Add(CreateFieldLabel("Department *", lblX, y));
            cmbDept = new ComboBox
            {
                Location = new Point(inputX, y + 22),
                Size = new Size(inputW, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            UIHelper.StyleComboBox(cmbDept);
            formPanel.Controls.Add(cmbDept);
            y += 60;

            // Semester
            formPanel.Controls.Add(CreateFieldLabel("Semester", lblX, y));
            cmbSemester = new ComboBox
            {
                Location = new Point(inputX, y + 22),
                Size = new Size(inputW, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            UIHelper.StyleComboBox(cmbSemester);
            for (int i = 1; i <= 12; i++) cmbSemester.Items.Add(i);
            cmbSemester.SelectedIndex = 0;
            formPanel.Controls.Add(cmbSemester);
            y += 60;

            // Admission Date
            formPanel.Controls.Add(CreateFieldLabel("Admission Date", lblX, y));
            dtpAdmission = UIHelper.CreateDatePicker();
            dtpAdmission.Location = new Point(inputX, y + 22);
            dtpAdmission.Size = new Size(inputW, 30);
            formPanel.Controls.Add(dtpAdmission);
            y += 60;

            // Address
            formPanel.Controls.Add(CreateFieldLabel("Address", lblX, y));
            txtAddress = CreateStyledTextBox(inputX, y + 22, inputW);
            formPanel.Controls.Add(txtAddress);
            y += 60;

            // Buttons
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
            LoadStudents();
            GenerateStudentCode();
        }

        // ── Data Loading ──────────────────────────────────────────────
        private void LoadStudents()
        {
            dgvStudents.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Students.GetAll);
            if (dgvStudents.Columns.Contains("StudentID"))
                dgvStudents.Columns["StudentID"].Visible = false;
        }

        private void LoadDepartments()
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Departments.GetForDropdown);
            cmbDept.DataSource = dt;
            cmbDept.DisplayMember = "DeptName";
            cmbDept.ValueMember = "DeptID";
        }

        private void GenerateStudentCode()
        {
            var result = DatabaseHelper.ExecuteScalar(SqlQueries.Students.GetNextCode);
            lblStudentCode.Text = result?.ToString() ?? "STU-NEW";
        }

        private void SearchStudents()
        {
            string term = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(term)) { LoadStudents(); return; }

            var parms = new[] { new SqlParameter("@Term", "%" + term + "%") };
            dgvStudents.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Students.Search, parms);
        }

        // ── Selection Changed ─────────────────────────────────────────
        private void DgvStudents_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null) return;
            var row = dgvStudents.CurrentRow;

            _selectedStudentId = Convert.ToInt32(row.Cells["StudentID"].Value);
            lblStudentCode.Text = row.Cells["StudentCode"].Value?.ToString() ?? "";
            txtName.Text = row.Cells["FullName"].Value?.ToString() ?? "";
            txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
            txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? "";
            txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";

            string? gender = row.Cells["Gender"].Value?.ToString();
            if (!string.IsNullOrEmpty(gender)) cmbGender.SelectedItem = gender;

            if (row.Cells["DateOfBirth"].Value != DBNull.Value)
                dtpDOB.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value);

            string? dept = row.Cells["Department"].Value?.ToString();
            if (!string.IsNullOrEmpty(dept))
            {
                for (int i = 0; i < cmbDept.Items.Count; i++)
                {
                    if (((DataRowView)cmbDept.Items[i])["DeptName"].ToString() == dept)
                    { cmbDept.SelectedIndex = i; break; }
                }
            }

            if (row.Cells["Semester"].Value != DBNull.Value)
                cmbSemester.SelectedItem = Convert.ToInt32(row.Cells["Semester"].Value);
        }

        // ── CRUD Operations ───────────────────────────────────────────
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var parms = new SqlParameter[]
            {
                new("@StudentCode", lblStudentCode.Text),
                new("@FullName", txtName.Text.Trim()),
                new("@Email", txtEmail.Text.Trim()),
                new("@Phone", txtPhone.Text.Trim()),
                new("@DOB", dtpDOB.Value.Date),
                new("@Gender", cmbGender.SelectedItem?.ToString() ?? "Other"),
                new("@Address", txtAddress.Text.Trim()),
                new("@DeptID", cmbDept.SelectedValue),
                new("@AdmissionDate", dtpAdmission.Value.Date),
                new("@Semester", cmbSemester.SelectedItem),
            };

            int result = DatabaseHelper.ExecuteNonQuery(SqlQueries.Students.Insert, parms);
            if (result > 0)
            {
                MessageBox.Show("Student added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadStudents();
                GenerateStudentCode();
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedStudentId == 0) { MessageBox.Show("Please select a student first."); return; }
            if (!ValidateForm()) return;

            var parms = new SqlParameter[]
            {
                new("@StudentID", _selectedStudentId),
                new("@FullName", txtName.Text.Trim()),
                new("@Email", txtEmail.Text.Trim()),
                new("@Phone", txtPhone.Text.Trim()),
                new("@DOB", dtpDOB.Value.Date),
                new("@Gender", cmbGender.SelectedItem?.ToString() ?? "Other"),
                new("@Address", txtAddress.Text.Trim()),
                new("@DeptID", cmbDept.SelectedValue),
                new("@Semester", cmbSemester.SelectedItem),
            };

            int result = DatabaseHelper.ExecuteNonQuery(SqlQueries.Students.Update, parms);
            if (result > 0)
            {
                MessageBox.Show("Student updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudents();
            }
        }

        private void BtnDeactivate_Click(object? sender, EventArgs e)
        {
            if (_selectedStudentId == 0) { MessageBox.Show("Please select a student first."); return; }

            var confirm = MessageBox.Show("Are you sure you want to deactivate this student?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            DatabaseHelper.ExecuteNonQuery(SqlQueries.Students.Deactivate,
                new[] { new SqlParameter("@StudentID", _selectedStudentId) });

            MessageBox.Show("Student deactivated.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
            LoadStudents();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Name is required."); txtName.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            { MessageBox.Show("Email is required."); txtEmail.Focus(); return false; }
            if (cmbDept.SelectedValue == null)
            { MessageBox.Show("Please select a department."); return false; }
            return true;
        }

        private void ClearForm()
        {
            _selectedStudentId = 0;
            txtName.Clear(); txtEmail.Clear(); txtPhone.Clear(); txtAddress.Clear();
            cmbGender.SelectedIndex = -1;
            if (cmbSemester.Items.Count > 0) cmbSemester.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Today;
            dtpAdmission.Value = DateTime.Today;
            GenerateStudentCode();
        }

        // ── Helpers ───────────────────────────────────────────────────
        private Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
        }

        private TextBox CreateStyledTextBox(int x, int y, int w)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 30),
            };
            UIHelper.StyleTextBox(tb);
            return tb;
        }
    }
}
