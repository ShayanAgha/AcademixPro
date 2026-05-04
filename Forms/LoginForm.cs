using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblError = null!;
        private Panel panelCard = null!;

        public LoginForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // ── Form Setup ────────────────────────────────────────────
            Text = "AcademixPro — Login";
            Size = new Size(520, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.Background;
            DoubleBuffered = true;

            // ── Close button ──────────────────────────────────────────
            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 14),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Location = new Point(Width - 40, 12)
            };
            btnClose.Click += (_, __) => Application.Exit();
            btnClose.MouseEnter += (_, __) => btnClose.ForeColor = AppColors.Danger;
            btnClose.MouseLeave += (_, __) => btnClose.ForeColor = AppColors.TextMuted;
            Controls.Add(btnClose);

            // ── Card Panel ────────────────────────────────────────────
            panelCard = new Panel
            {
                Size = new Size(420, 440),
                Location = new Point(50, 110),
                BackColor = AppColors.Surface,
            };
            Controls.Add(panelCard);

            // ── Logo / Title ──────────────────────────────────────────
            var lblLogo = new Label
            {
                Text = "🎓",
                Font = new Font("Segoe UI", 36),
                ForeColor = AppColors.Primary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            lblLogo.Location = new Point((panelCard.Width - 60) / 2, 20);
            panelCard.Controls.Add(lblLogo);

            var lblTitle = new Label
            {
                Text = "AcademixPro",
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            lblTitle.Location = new Point((panelCard.Width - lblTitle.PreferredWidth) / 2, 82);
            panelCard.Controls.Add(lblTitle);



            // ── Username ──────────────────────────────────────────────
            var lblUser = UIHelper.CreateLabel("Username", 9.5f, AppColors.TextSecondary);
            lblUser.Location = new Point(40, 165);
            panelCard.Controls.Add(lblUser);

            txtUsername = new TextBox
            {
                Size = new Size(340, 38),
                Location = new Point(40, 188),
                Font = new Font("Segoe UI", 11),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
            };
            panelCard.Controls.Add(txtUsername);

            // ── Password ──────────────────────────────────────────────
            var lblPass = UIHelper.CreateLabel("Password", 9.5f, AppColors.TextSecondary);
            lblPass.Location = new Point(40, 235);
            panelCard.Controls.Add(lblPass);

            txtPassword = new TextBox
            {
                Size = new Size(340, 38),
                Location = new Point(40, 258),
                Font = new Font("Segoe UI", 11),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
            };
            panelCard.Controls.Add(txtPassword);

            // ── Login Button ──────────────────────────────────────────
            btnLogin = new Button
            {
                Text = "Sign In",
                Size = new Size(340, 46),
                Location = new Point(40, 320),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = AppColors.PrimaryLight;
            btnLogin.Click += BtnLogin_Click;
            panelCard.Controls.Add(btnLogin);

            // ── Error Label ───────────────────────────────────────────
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppColors.Danger,
                AutoSize = false,
                Size = new Size(340, 30),
                Location = new Point(40, 375),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            panelCard.Controls.Add(lblError);

            // ── Enter key ─────────────────────────────────────────────
            AcceptButton = btnLogin;
        }

        // ── Paint rounded card ────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // subtle gradient top bar
            var rect = new Rectangle(0, 0, Width, 6);
            using var brush = new LinearGradientBrush(rect, AppColors.Primary, AppColors.Secondary, 0f);
            e.Graphics.FillRectangle(brush, rect);
        }

        // ── Login Logic ───────────────────────────────────────────────
        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblError.Text = "Please enter both username and password.";
                return;
            }

            string hash = ComputeSHA256(pass);

            var parms = new SqlParameter[]
            {
                new SqlParameter("@Username", user),
                new SqlParameter("@PasswordHash", hash),
            };

            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Auth.Login, parms);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                Session.UserID   = Convert.ToInt32(row["UserID"]);
                Session.Username = row["Username"].ToString()!;
                Session.FullName = row["FullName"].ToString()!;
                Session.Email    = row["Email"].ToString()!;
                Session.Role     = row["Role"].ToString()!;

                // Update last login
                DatabaseHelper.ExecuteNonQuery(SqlQueries.Auth.UpdateLastLogin,
                    new[] { new SqlParameter("@UserID", Session.UserID) });

                Hide();
                var main = new MainForm();
                main.FormClosed += (_, __) => Close();
                main.Show();
            }
            else
            {
                lblError.Text = "Invalid username or password.";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private static string ComputeSHA256(string input)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        // ── Allow dragging the borderless form ────────────────────────
        private Point _dragOffset;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
                _dragOffset = e.Location;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
                Location = new Point(Location.X + e.X - _dragOffset.X, Location.Y + e.Y - _dragOffset.Y);
        }
    }
}
