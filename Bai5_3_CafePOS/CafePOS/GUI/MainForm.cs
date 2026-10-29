using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CafePOS.BUS;

namespace CafePOS.GUI
{
    public class MainForm : Form
    {
        private readonly CafeBUS bus = new CafeBUS();
        private readonly FlowLayoutPanel tables = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(8),
            WrapContents = true
        };
        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false
        };
        private readonly ComboBox menu = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 180
        };
        private readonly NumericUpDown quantity = new()
        {
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            Width = 65
        };
        private readonly Label selected = new()
        {
            AutoSize = true,
            Text = "Chưa chọn bàn",
            Padding = new Padding(4, 8, 4, 0)
        };
        private int selectedTable;

        public MainForm()
        {
            Text = "Bài 5.3 - Cafe POS";
            Width = 1100;
            Height = 650;
            MinimumSize = new Size(900, 500);
            StartPosition = FormStartPosition.CenterScreen;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(6)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(4)
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(3),
                Margin = new Padding(0)
            };

            top.Controls.Add(selected);
            top.Controls.Add(new Label { Text = "Món:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            top.Controls.Add(menu);
            top.Controls.Add(new Label { Text = "SL:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            top.Controls.Add(quantity);

            var add = new Button { Text = "Thêm món", AutoSize = true, Height = 30 };
            add.Click += (_, _) => AddItem();
            var pay = new Button { Text = "Thanh toán", AutoSize = true, Height = 30 };
            pay.Click += (_, _) => Pay();
            var refresh = new Button { Text = "Tải lại", AutoSize = true, Height = 30 };
            refresh.Click += (_, _) => RefreshData();
            var manageMenu = new Button { Text = "Quản lý món ăn", AutoSize = true, Height = 30 };
            manageMenu.Click += (_, _) => OpenMenuManager();

            top.Controls.Add(add);
            top.Controls.Add(pay);
            top.Controls.Add(refresh);
            top.Controls.Add(manageMenu);

            right.Controls.Add(top, 0, 0);
            right.Controls.Add(grid, 0, 1);
            root.Controls.Add(tables, 0, 0);
            root.Controls.Add(right, 1, 0);
            Controls.Add(root);

            Load += (_, _) =>
            {
                try
                {
                    LoadMenu();
                    LoadTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void LoadMenu()
        {
            DataTable data = bus.GetMenu();
            menu.DataSource = data;
            menu.DisplayMember = "TenMon";
            menu.ValueMember = "MaMon";
        }

        private void LoadTables()
        {
            tables.SuspendLayout();
            tables.Controls.Clear();
            foreach (DataRow row in bus.GetTables().Rows)
            {
                int id = Convert.ToInt32(row["MaBan"]);
                string name = Convert.ToString(row["TenBan"]) ?? "Bàn";
                string status = Convert.ToString(row["TrangThai"]) ?? "Trống";
                var button = new Button
                {
                    Text = name + Environment.NewLine + status,
                    Width = 95,
                    Height = 70,
                    Margin = new Padding(4),
                    BackColor = status == "Trống" ? Color.LightGreen : Color.LightCoral,
                    Tag = id
                };
                button.Click += (_, _) => SelectTable(id, name);
                tables.Controls.Add(button);
            }
            tables.ResumeLayout();
        }

        private void SelectTable(int id, string name)
        {
            selectedTable = id;
            selected.Text = name;
            grid.DataSource = bus.GetOrder(id);
        }

        private void AddItem()
        {
            try
            {
                if (selectedTable == 0 || menu.SelectedItem == null)
                    throw new Exception("Hãy chọn bàn và món ăn trước.");

                DataRowView row = (DataRowView)menu.SelectedItem;
                bus.AddItem(selectedTable, Convert.ToInt32(row["MaMon"]), (int)quantity.Value, Convert.ToDecimal(row["DonGia"]));
                grid.DataSource = bus.GetOrder(selectedTable);
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Pay()
        {
            try
            {
                if (selectedTable == 0)
                    throw new Exception("Hãy chọn bàn cần thanh toán.");

                decimal total = bus.Pay(selectedTable);
                MessageBox.Show("Tổng thanh toán: " + total.ToString("N0") + " đ", "Thanh toán thành công");
                grid.DataSource = null;
                selectedTable = 0;
                selected.Text = "Chưa chọn bàn";
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshData()
        {
            try
            {
                LoadMenu();
                LoadTables();
                if (selectedTable > 0)
                    grid.DataSource = bus.GetOrder(selectedTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu");
            }
        }

        private void OpenMenuManager()
        {
            using var form = new MenuManagerForm(bus);
            form.ShowDialog(this);
            RefreshData();
        }
    }

    internal class MenuManagerForm : Form
    {
        private readonly CafeBUS bus;
        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false
        };
        private readonly TextBox txtTen = new() { Width = 160 };
        private readonly ComboBox cboDanhMuc = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown numGia = new() { Width = 120, Minimum = 1, Maximum = 100000000, ThousandsSeparator = true };
        private int maMon;

        public MenuManagerForm(CafeBUS cafeBus)
        {
            bus = cafeBus;
            Text = "Quản lý món ăn";
            Width = 850;
            Height = 500;
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "Tên món:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            top.Controls.Add(txtTen);
            top.Controls.Add(new Label { Text = "Danh mục:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            top.Controls.Add(cboDanhMuc);
            top.Controls.Add(new Label { Text = "Đơn giá:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            top.Controls.Add(numGia);

            var add = new Button { Text = "Thêm mới", AutoSize = true };
            add.Click += (_, _) => AddItem();
            var save = new Button { Text = "Lưu sửa", AutoSize = true };
            save.Click += (_, _) => UpdateItem();
            var delete = new Button { Text = "Xóa món", AutoSize = true };
            delete.Click += (_, _) => DeleteItem();
            var clear = new Button { Text = "Làm mới", AutoSize = true };
            clear.Click += (_, _) => ClearFields();
            top.Controls.Add(add);
            top.Controls.Add(save);
            top.Controls.Add(delete);
            top.Controls.Add(clear);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(grid, 0, 1);
            Controls.Add(root);
            grid.SelectionChanged += (_, _) => SelectItem();

            Load += (_, _) =>
            {
                try
                {
                    cboDanhMuc.DataSource = bus.GetCategories();
                    cboDanhMuc.DisplayMember = "TenDanhMuc";
                    cboDanhMuc.ValueMember = "MaDanhMuc";
                    LoadItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi tải dữ liệu");
                }
            };
        }

        private void LoadItems()
        {
            grid.DataSource = bus.GetMenu();
            if (grid.Columns["MaDanhMuc"] != null)
                grid.Columns["MaDanhMuc"].HeaderText = "Mã danh mục";
            if (grid.Columns["MaMon"] != null)
                grid.Columns["MaMon"].HeaderText = "Mã món";
            if (grid.Columns["TenMon"] != null)
                grid.Columns["TenMon"].HeaderText = "Tên món";
            if (grid.Columns["DonGia"] != null)
                grid.Columns["DonGia"].HeaderText = "Đơn giá";
        }

        private void SelectItem()
        {
            if (grid.CurrentRow?.DataBoundItem is not DataRowView row)
                return;
            maMon = Convert.ToInt32(row["MaMon"]);
            txtTen.Text = Convert.ToString(row["TenMon"]) ?? "";
            numGia.Value = Math.Min(numGia.Maximum, Math.Max(numGia.Minimum, Convert.ToDecimal(row["DonGia"])));
            if (cboDanhMuc.DataSource != null)
                cboDanhMuc.SelectedValue = Convert.ToInt32(row["MaDanhMuc"]);
        }

        private void AddItem()
        {
            try
            {
                if (cboDanhMuc.SelectedValue == null)
                    throw new Exception("Chưa có danh mục. Hãy kiểm tra bảng DanhMuc.");
                bus.SaveMenuItem(0, txtTen.Text, Convert.ToInt32(cboDanhMuc.SelectedValue), numGia.Value);
                MessageBox.Show("Đã thêm món ăn.");
                LoadItems();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể thêm món");
            }
        }

        private void UpdateItem()
        {
            try
            {
                if (maMon == 0)
                    throw new Exception("Hãy chọn món cần sửa trong bảng.");
                bus.SaveMenuItem(maMon, txtTen.Text, Convert.ToInt32(cboDanhMuc.SelectedValue), numGia.Value);
                MessageBox.Show("Đã cập nhật món ăn.");
                LoadItems();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể sửa món");
            }
        }

        private void DeleteItem()
        {
            try
            {
                if (maMon == 0)
                    throw new Exception("Hãy chọn món cần xóa trong bảng.");
                var answer = MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                    return;
                bus.DeleteMenuItem(maMon);
                MessageBox.Show("Đã xóa món ăn.");
                LoadItems();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa món ăn. Nếu món đã nằm trong hóa đơn, SQL có thể không cho xóa.\n" + ex.Message, "Thông báo");
            }
        }

        private void ClearFields()
        {
            maMon = 0;
            txtTen.Clear();
            numGia.Value = numGia.Minimum;
            grid.ClearSelection();
            txtTen.Focus();
        }
    }
}
