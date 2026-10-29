
using System;
using System.Data;
using System.Windows.Forms;
using CafePOS.BUS;

namespace CafePOS.GUI
{
    public class MenuForm : Form
    {
        private readonly CafeBUS bus = new CafeBUS();

        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false
        };

        private readonly TextBox txtTen = new() { Width = 150 };
        private readonly ComboBox cboDanhMuc = new()
        {
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        private readonly NumericUpDown numGia = new()
        {
            Width = 120,
            Maximum = 100000000,
            Minimum = 0,
            ThousandsSeparator = true
        };

        private int maMon = 0;

        public MenuForm()
        {
            Text = "Quản lý món ăn";
            Width = 850;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(8),
                AutoScroll = true
            };

            top.Controls.Add(new Label
            {
                Text = "Tên món:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });
            top.Controls.Add(txtTen);

            top.Controls.Add(new Label
            {
                Text = "Danh mục:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });
            top.Controls.Add(cboDanhMuc);

            top.Controls.Add(new Label
            {
                Text = "Đơn giá:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });
            top.Controls.Add(numGia);

            var btnThem = new Button { Text = "Thêm mới", AutoSize = true };
            var btnLuu = new Button { Text = "Lưu sửa", AutoSize = true };
            var btnXoa = new Button { Text = "Xóa món", AutoSize = true };
            var btnMoi = new Button { Text = "Làm mới", AutoSize = true };

            btnThem.Click += (_, _) => ThemMon();
            btnLuu.Click += (_, _) => LuuSua();
            btnXoa.Click += (_, _) => XoaMon();
            btnMoi.Click += (_, _) => LamMoi();

            top.Controls.Add(btnThem);
            top.Controls.Add(btnLuu);
            top.Controls.Add(btnXoa);
            top.Controls.Add(btnMoi);

            Controls.Add(grid);
            Controls.Add(top);

            grid.SelectionChanged += (_, _) => ChonMon();

            Load += (_, _) =>
            {
                try
                {
                    cboDanhMuc.DataSource = bus.GetCategories();
                    cboDanhMuc.DisplayMember = "TenDanhMuc";
                    cboDanhMuc.ValueMember = "MaDanhMuc";
                    TaiDanhSach();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi tải dữ liệu");
                }
            };
        }

        private void TaiDanhSach()
        {
            grid.DataSource = bus.GetMenu();
        }

        private void ChonMon()
        {
            if (grid.CurrentRow == null ||
                grid.CurrentRow.DataBoundItem == null)
                return;

            DataRowView row =
                (DataRowView)grid.CurrentRow.DataBoundItem;

            maMon = Convert.ToInt32(row["MaMon"]);
            txtTen.Text = row["TenMon"].ToString();
            numGia.Value = Convert.ToDecimal(row["DonGia"]);
            cboDanhMuc.SelectedValue = Convert.ToInt32(row["MaDanhMuc"]);
        }

        private void ThemMon()
        {
            try
            {
                if (cboDanhMuc.SelectedValue == null)
                    throw new Exception("Chưa có danh mục món ăn.");

                bus.SaveMenuItem(
                    0,
                    txtTen.Text,
                    Convert.ToInt32(cboDanhMuc.SelectedValue),
                    numGia.Value
                );

                MessageBox.Show("Đã thêm món ăn.");
                TaiDanhSach();
                LamMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo");
            }
        }

        private void LuuSua()
        {
            try
            {
                if (maMon == 0)
                    throw new Exception("Hãy chọn món cần sửa trong bảng.");

                bus.SaveMenuItem(
                    maMon,
                    txtTen.Text,
                    Convert.ToInt32(cboDanhMuc.SelectedValue),
                    numGia.Value
                );

                MessageBox.Show("Đã cập nhật món ăn.");
                TaiDanhSach();
                LamMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo");
            }
        }

        private void XoaMon()
        {
            try
            {
                if (maMon == 0)
                    throw new Exception("Hãy chọn món cần xóa.");

                DialogResult kq = MessageBox.Show(
                    "Bạn có chắc muốn xóa món này?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (kq != DialogResult.Yes)
                    return;

                bus.DeleteMenuItem(maMon);

                MessageBox.Show("Đã xóa món ăn.");
                TaiDanhSach();
                LamMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể xóa món ăn.\n" + ex.Message,
                    "Thông báo"
                );
            }
        }

        private void LamMoi()
        {
            maMon = 0;
            txtTen.Clear();
            numGia.Value = 0;
            grid.ClearSelection();
            txtTen.Focus();
        }
    }
}
