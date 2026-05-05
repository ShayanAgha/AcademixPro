using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class UsersControl : UserControl
    {
        private DataGridView dgvUsers = null!;
        private TextBox txtUsername = null!, txtFullName = null!, txtEmail = null!, txtPassword = null!;
        private ComboBox cmbRole = null!;

        public UsersControl()
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

            var btnToggle = UIHelper.CreateButton("🔒 Toggle Status", AppColors.CardAmber, 160, 36);
            btnToggle.Location = new Point(120, 10);
            btnToggle.Click += BtnToggle_Click;
            topPanel.Controls.Add(btnToggle);

            Controls.Add(topPanel);

            // ── Split ─────────────────────────────────────────────────
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

            // Grid
            dgvUsers = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvUsers);
            split.Panel1.Controls.Add(dgvUsers);

            // Form
            var fp = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Surface,
                AutoScroll = true,
                Padding = new Padding(20),
            };
            split.Panel2.Controls.Add(fp);

            fp.Controls.Add(new Label
            {
                Text = "Add New User",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 16),
                BackColor = Color.Transparent,
            });

            int y = 52, x = 20, w = 260;

            fp.Controls.Add(FL("Username *", x, y));
            txtUsername = STB(x, y + 22, w);
            fp.Controls.Add(txtUsername);
            y += 58;

            fp.Controls.Add(FL("Full Name *", x, y));
            txtFullName = STB(x, y + 22, w);
            fp.Controls.Add(txtFullName);
            y += 58;

            fp.Controls.Add(FL("Email *", x, y));
            txtEmail = STB(x, y + 22, w);
            fp.Controls.Add(txtEmail);
            y += 58;

            fp.Controls.Add(FL("Password *", x, y));
            txtPassword = new TextBox
            {
                Location = new Point(x, y + 22),
                Size = new Size(w, 30),
                PasswordChar = '●',
            };
            UIHelper.StyleTextBox(txtPassword);
            fp.Controls.Add(txtPassword);
            y += 58;

            fp.Controls.Add(FL("Role *", x, y));
            cmbRole = new ComboBox
            {
                Location = new Point(x, y + 22),
                Size = new Size(w, 30),
            };
            UIHelper.StyleComboBox(cmbRole);
            cmbRole.Items.AddRange(new object[] { "Admin", "Teacher", "Student" });
            cmbRole.SelectedIndex = 2;
            fp.Controls.Add(cmbRole);
            y += 58;

            var btnAdd = UIHelper.CreateButton("➕ Create User", AppColors.Primary, 160, 40);
            btnAdd.Location = new Point(x, y);
            btnAdd.Click += BtnAdd_Click;
            fp.Controls.Add(btnAdd);

            var btnClear = UIHelper.CreateButton("🔄 Clear", AppColors.SurfaceLight, 110, 40);
            btnClear.ForeColor = AppColors.TextSecondary;
            btnClear.Location = new Point(x + 170, y);
            btnClear.Click += (_, __) => ClearForm();
            fp.Controls.Add(btnClear);

            LoadData();
        }

        private void LoadData()
        {
            dgvUsers.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Auth.GetAllUsers);
            if (dgvUsers.Columns.Contains("UserID"))
                dgvUsers.Columns["UserID"].Visible = false;
            var map = new Dictionary<string, string>
            {
                ["Username"]  = "Username",
                ["FullName"]  = "Full Name",
                ["Email"]     = "Email",
                ["Role"]      = "Role",
                ["IsActive"]  = "Active",
                ["LastLogin"] = "Last Login",
                ["CreatedAt"] = "Created On",
            };
            foreach (var kv in map)
                if (dgvUsers.Columns.Contains(kv.Key))
                    dgvUsers.Columns[kv.Key].HeaderText = kv.Value;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("All fields are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hash = ComputeSHA256(txtPassword.Text.Trim());

            var parms = new SqlParameter[]
            {
                new("@Username", txtUsername.Text.Trim()),
                new("@PasswordHash", hash),
                new("@FullName", txtFullName.Text.Trim()),
                new("@Email", txtEmail.Text.Trim()),
                new("@Role", cmbRole.SelectedItem!.ToString()!),
            };

            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Auth.InsertUser, parms) > 0)
            {
                MessageBox.Show("User created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Select a user first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int userId = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);

            if (MessageBox.Show("Toggle this user's active status?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DatabaseHelper.ExecuteNonQuery(SqlQueries.Auth.ToggleUserStatus,
                new[] { new SqlParameter("@UserID", userId) });

            LoadData();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 2;
        }

        private static string ComputeSHA256(string input)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private Label FL(string t, int x, int y) => new()
        {
            Text = t,
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.TextSecondary,
            Location = new Point(x, y),
            AutoSize = true,
            BackColor = Color.Transparent,
        };

        private TextBox STB(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30) };
            UIHelper.StyleTextBox(tb);
            return tb;
        }
    }
}
