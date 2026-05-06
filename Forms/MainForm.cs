using System.Drawing.Drawing2D;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class MainForm : Form
    {
        private Panel panelSidebar = null!;
        private Panel panelContent = null!;
        private Panel panelTopBar = null!;
        private Label lblPageTitle = null!;
        private Label lblUserInfo = null!;
        private Button? _activeNavButton;

        public MainForm()
        {
            InitializeUI();
            NavigateTo("Dashboard");
        }

        private void InitializeUI()
        {
            Text = "AcademixPro — Student Management System";
            Size = new Size(1400, 820);
            MinimumSize = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppColors.Background;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10);

            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = AppColors.Surface,
            };

            var panelBrand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.Transparent,
            };
            panelSidebar.Controls.Add(panelBrand);

            var lblBrand = new Label
            {
                Text = "🎓  AcademixPro",
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = false,
                Size = new Size(250, 72),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            panelBrand.Controls.Add(lblBrand);

            var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppColors.Border };
            panelSidebar.Controls.Add(sep);

            var navPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 12, 12, 12),
            };
            panelSidebar.Controls.Add(navPanel);

            panelBrand.BringToFront();
            sep.BringToFront();

            string[][] navItems = new[]
            {
                new[] { "📊", "Dashboard" },
                new[] { "👨‍🎓", "Students" },
                new[] { "🧑‍🏫", "Instructors" },
                new[] { "📚", "Courses" },
                new[] { "🔗", "Enrollments" },
                new[] { "🗓️", "Attendance" },
                new[] { "📝", "Results" },
                new[] { "🏛️", "Departments" },
                new[] { "🔔", "Notifications" },
                new[] { "🧾", "Audit Log" },
            };

            if (Session.Role == "Admin")
            {
                navItems = navItems.Append(new[] { "👤", "Users" }).ToArray();
            }

            foreach (var item in navItems)
            {
                var btn = CreateNavButton(item[0], item[1]);
                btn.Click += (s, _) => NavigateTo(item[1], (Button)s!);
                navPanel.Controls.Add(btn);
            }

            var panelUser = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = AppColors.SurfaceLight,
                Padding = new Padding(16, 10, 16, 10),
            };
            panelSidebar.Controls.Add(panelUser);

            lblUserInfo = new Label
            {
                Text = $"👤 {Session.FullName}\n    {Session.Role}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
            };
            panelUser.Controls.Add(lblUserInfo);

            var btnLogout = new Label
            {
                Text = "⏻ Logout",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.Danger,
                AutoSize = true,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
            };
            btnLogout.Click += (_, __) =>
            {
                Session.Clear();
                Close();
            };
            panelUser.Controls.Add(btnLogout);

            panelTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.Surface,
                Padding = new Padding(24, 0, 24, 0),
            };

            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent,
            };
            panelTopBar.Controls.Add(lblPageTitle);

            var topBarSep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = AppColors.Border,
            };

            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Padding = new Padding(24),
            };

            Controls.Add(panelContent); 
            Controls.Add(topBarSep);
            Controls.Add(panelTopBar);    
            Controls.Add(panelSidebar);   
        }

        private Button CreateNavButton(string icon, string text)
        {
            var btn = new Button
            {
                Text = $"  {icon}  {text}",
                Size = new Size(224, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = AppColors.TextSecondary,
                Font = new Font("Segoe UI", 10.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),
                Tag = text,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AppColors.SurfaceHover;
            return btn;
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeNavButton != null)
            {
                _activeNavButton.BackColor = Color.Transparent;
                _activeNavButton.ForeColor = AppColors.TextSecondary;
            }
            btn.BackColor = AppColors.SurfaceHover;
            btn.ForeColor = AppColors.Primary;
            _activeNavButton = btn;
        }

        private void NavigateTo(string page, Button? navBtn = null)
        {
            if (navBtn != null) SetActiveNav(navBtn);
            else
            {
                foreach (Control c in panelSidebar.Controls)
                {
                    if (c is FlowLayoutPanel fp)
                    {
                        foreach (Control b in fp.Controls)
                        {
                            if (b is Button bt && bt.Tag?.ToString() == page)
                            {
                                SetActiveNav(bt);
                                break;
                            }
                        }
                    }
                }
            }

            lblPageTitle.Text = page;
            panelContent.Controls.Clear();

            UserControl? content = page switch
            {
                "Dashboard" => new DashboardControl(),
                "Students" => new StudentsControl(),
                "Instructors" => new InstructorsControl(),
                "Courses" => new CoursesControl(),
                "Enrollments" => new EnrollmentsControl(),
                "Attendance" => new AttendanceControl(),
                "Results" => new ResultsControl(),
                "Departments" => new DepartmentsControl(),
                "Notifications" => new NotificationsControl(),
                "Audit Log" => new AuditLogControl(),
                "Users" => new UsersControl(),
                _ => null,
            };

            if (content != null)
            {
                content.Dock = DockStyle.Fill;
                panelContent.Controls.Add(content);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, Width, 3), AppColors.Primary, AppColors.Secondary, 0f);
            e.Graphics.FillRectangle(brush, 0, 0, Width, 3);
        }
    }
}
