using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using MuseumTourManagement.Database;

namespace MuseumTourManagement.Forms
{
    public partial class DashboardForm : Form
    {
        private string userRole;

        private Label lblTotalEmployees = new Label();
        private Label lblTotalCustomers = new Label();

        public DashboardForm(string role)
        {
            InitializeComponent();
            this.userRole = role;
            RestrictAdminAccess();
            LoadDashboardData();
            ApplyProfessionalStyling();
        }

        private void RestrictAdminAccess()
        {
            if (userRole == "Admin")
                btnManageManagers.Visible = false;
        }

        private void LoadDashboardData()
        {
            lblRevenue.Text = "£" + Database.Database.GetTodaysEarningsRevenue().ToString("N2");
            lblUpcomingBookings.Text = Database.Database.GetUpcomingBookings().ToString();
            lblTotalEmployees.Text = Database.Database.GetAllEmployeesDataTable().Rows.Count.ToString();
            lblTotalCustomers.Text = Database.Database.GetAllCustomersDataTable().Rows.Count.ToString();
        }

        private void ApplyProfessionalStyling()
        {
            this.BackgroundImage = LoadImageFromProject("background.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.Black
            };

            PictureBox sidebarLogo = new PictureBox
            {
                Image = LoadImageFromProject("logo.png"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.Black
            };

            FlowLayoutPanel sidebarContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                AutoScroll = true
            };

            Panel[] sidebarButtons = {
        CreateSidebarButton("Expeditions", "exhibitions.png", BtnExpeditions_Click),
        CreateSidebarButton("Customers", "Customers.png", BtnCustomers_Click),
        CreateSidebarButton("Bookings", "bookings.png", btnBookings_Click),
        CreateSidebarButton("Earnings", "earning.png", BtnEarnings_Click),
        CreateSidebarButton("Manage Employee", "manage mangers.png", BtnManageEmployees_Click),
        CreateSidebarButton("Logout", "logout.png", BtnLogout_Click)
    };

            sidebarContainer.Controls.Add(sidebarLogo);
            foreach (var btn in sidebarButtons)
                sidebarContainer.Controls.Add(btn);
            sidebar.Controls.Add(sidebarContainer);

            // Create a centered 2x2 grid for dashboard
            TableLayoutPanel dashboardGrid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(60),  // was 40
                Margin = new Padding(60)
            };

            dashboardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            dashboardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            dashboardGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            dashboardGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Create cards
            AddCardToGrid(dashboardGrid, CreateCard("Revenue", "rev.png", lblRevenue), 0, 0);
            AddCardToGrid(dashboardGrid, CreateCard("Upcoming Bookings", "upcomingbooking.png", lblUpcomingBookings), 1, 0);
            AddCardToGrid(dashboardGrid, CreateCard("Employees", "employee.png", lblTotalEmployees), 0, 1);
            AddCardToGrid(dashboardGrid, CreateCard("Total Customers", "Customers.png", lblTotalCustomers), 1, 1);

            // Add everything to the form
            this.Controls.Clear();
            this.Controls.Add(dashboardGrid);
            this.Controls.Add(sidebar);
        }



        private void AddCardToGrid(TableLayoutPanel grid, Control card, int col, int row)
        {
            Panel wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.Transparent
            };

            wrapper.Paint += (s, e) =>
            {
                int radius = 20;
                Rectangle shadow = new Rectangle(5, 5, card.Width, card.Height);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(shadow.X, shadow.Y, radius, radius, 180, 90);
                    path.AddArc(shadow.Right - radius, shadow.Y, radius, radius, 270, 90);
                    path.AddArc(shadow.Right - radius, shadow.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(shadow.X, shadow.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(shadowBrush, path);
                    }
                }
            };

            wrapper.Controls.Add(card);
            grid.Controls.Add(wrapper, col, row);
        }


        private Panel CreateSidebarButton(string text, string imagePath, EventHandler clickHandler)
        {
            Panel panel = new Panel
            {
                Width = 220,
                Height = 60,
                BackColor = Color.Black,
                Margin = new Padding(5)
            };

            PictureBox icon = new PictureBox
            {
                Image = LoadImageFromProject(imagePath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(30, 30),
                Location = new Point(10, 15)
            };

            Button btn = new Button
            {
                Text = text,
                Location = new Point(50, 15),
                Width = 150,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickHandler;

            panel.Controls.Add(icon);
            panel.Controls.Add(btn);
            return panel;
        }

        private Panel CreateCardBase()
        {
            Panel card = new Panel
            {
                Width = 300,  // was 240
                Height = 200, // was 160
                BackColor = Color.Transparent,
                Padding = new Padding(10),
                Margin = new Padding(30) // was 20
            };

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 20;
                    Rectangle bounds = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                    path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                    path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
                    path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (LinearGradientBrush brush = new LinearGradientBrush(bounds,
                        Color.FromArgb(200, Color.White),
                        Color.FromArgb(160, Color.LightGray),
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillPath(brush, path);
                    }

                    using (Pen pen = new Pen(Color.FromArgb(120, 0, 0, 0), 1))
                        g.DrawPath(pen, path);
                }
            };

            return card;
        }

        private Panel CreateCard(string title, string imagePath, Label dataLabel)
        {
            Panel card = CreateCardBase();

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // Title
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));    // Icon
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));    // Number

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Arial", 12F, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            PictureBox icon = new PictureBox
            {
                Image = LoadImageFromProject(imagePath),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            dataLabel.Font = new Font("Arial", 14F, FontStyle.Bold);
            dataLabel.ForeColor = Color.Black;
            dataLabel.Dock = DockStyle.Fill;
            dataLabel.TextAlign = ContentAlignment.TopCenter;

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(icon, 0, 1);
            layout.Controls.Add(dataLabel, 0, 2);

            card.Controls.Add(layout);
            return card;
        }


        private Panel CreateCardButton(string title, string imagePath, EventHandler clickHandler)
        {
            Panel card = CreateCardBase();

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            PictureBox icon = new PictureBox
            {
                Image = LoadImageFromProject(imagePath),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };

            Button btn = new Button
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.TopCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickHandler;

            layout.Controls.Add(icon, 0, 0);
            layout.Controls.Add(btn, 0, 1);

            card.Controls.Add(layout);
            return card;
        }

        private Image LoadImageFromProject(string fileName)
        {
            string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string projectFolder = Path.Combine(solutionRoot, "MuseumTourManagement");
            string fullPath = Path.Combine(projectFolder, "images", fileName);

            if (!File.Exists(fullPath))
            {
                MessageBox.Show($"IMAGE NOT FOUND:\n{fullPath}", "Missing Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return SystemIcons.Warning.ToBitmap();
            }

            return Image.FromFile(fullPath);
        }
        public void RefreshRevenueLabel()
        {
            lblRevenue.Text = "£" + Database.Database.GetTodaysEarningsRevenue().ToString("N2");
        }

        public void RefreshEmployeeCount()
        {
            lblTotalEmployees.Text = Database.Database.GetAllEmployeesDataTable().Rows.Count.ToString();
        }


        // Event Handlers
        private void BtnExpeditions_Click(object sender, EventArgs e) => new ExpeditionForm().Show();
        private void BtnCustomers_Click(object sender, EventArgs e) => new CustomerForm().Show();
        private void btnBookings_Click(object sender, EventArgs e) => new BookingsForm().Show();
        private void BtnManageEmployees_Click(object sender, EventArgs e) => new ManageEmployeesForm(this).Show();
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();

            this.Close();
        }

        private void BtnEarnings_Click(object sender, EventArgs e) =>
            new EarningsForm(this).Show();
        private void BtnEmployees_Click(object sender, EventArgs e) => new EmployeesForm().Show();
        private void BtnAvailableExhibitions_Click(object sender, EventArgs e) => new AvailableExhibitionsForm().Show();
    }
}
