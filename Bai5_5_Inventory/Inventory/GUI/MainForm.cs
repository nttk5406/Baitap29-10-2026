using System.Data;
using InventoryManagement.BUS;

namespace InventoryManagement.GUI
{
    public class MainForm : Form
    {
        private readonly InventoryBUS bus = new InventoryBUS();
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly TextBox id = new() { Width = 90, PlaceholderText = "VT005" };
        private readonly TextBox name = new() { Width = 160, PlaceholderText = "Tên vật tư" };
        private readonly TextBox unit = new() { Width = 80, PlaceholderText = "ĐVT" };
        private readonly TextBox search = new() { Width = 120, PlaceholderText = "Tìm mã/tên" };
        private readonly ComboBox type = new() { Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox supplier = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox qty = new() { Width = 70, PlaceholderText = "Số lượng" };
        private readonly TextBox price = new() { Width = 80, PlaceholderText = "Đơn giá" };
        private readonly DataTable details = new();
        private readonly DataGridView detailGrid = new() { Height = 150, Dock = DockStyle.Bottom, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly NumericUpDown threshold = new() { Minimum = 0, Maximum = 1000000, Value = 10, Width = 70 };

        public MainForm()
        {
            Text = "Bài 5.5 - Inventory Management"; Width = 1200; Height = 750; StartPosition = FormStartPosition.CenterScreen;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(6), AutoScroll = true };
            top.Controls.Add(id); top.Controls.Add(name); top.Controls.Add(unit);
            var save = new Button { Text = "Thêm/Sửa vật tư", AutoSize = true }; save.Click += (_,__) => SaveItem(); top.Controls.Add(save);
            var del = new Button { Text = "Xóa vật tư", AutoSize = true }; del.Click += (_,__) => DeleteItem(); top.Controls.Add(del);
            top.Controls.Add(search);
            var find = new Button { Text = "Tìm kiếm", AutoSize = true }; find.Click += (_,__) => LoadItems(); top.Controls.Add(find);
            type.Items.AddRange(new object[] { "NHAP", "XUAT" }); type.SelectedIndex = 0;
            top.Controls.Add(type); top.Controls.Add(supplier); top.Controls.Add(qty); top.Controls.Add(price);
            var add = new Button { Text = "Thêm dòng phiếu", AutoSize = true }; add.Click += (_,__) => AddDetail(); top.Controls.Add(add);
            var saveSlip = new Button { Text = "Lưu phiếu kho", AutoSize = true }; saveSlip.Click += (_,__) => SaveSlip(); top.Controls.Add(saveSlip);
            top.Controls.Add(new Label { Text = "Ngưỡng tồn:", AutoSize = true, Padding = new Padding(3,8,3,0) }); top.Controls.Add(threshold);
            var low = new Button { Text = "Tồn kho thấp", AutoSize = true }; low.Click += (_,__) => { try { grid.DataSource = bus.GetLowStock(threshold.Value); } catch(Exception ex) { MessageBox.Show(ex.Message); } }; top.Controls.Add(low);
            var value = new Button { Text = "Tổng giá trị tồn", AutoSize = true }; value.Click += (_,__) => { try { MessageBox.Show("Tổng số lượng tồn (chưa quy đổi theo giá vốn): " + bus.GetStockValue().ToString("N2")); } catch(Exception ex) { MessageBox.Show(ex.Message); } }; top.Controls.Add(value);
            details.Columns.Add("MaVatTu", typeof(string)); details.Columns.Add("SoLuong", typeof(decimal)); details.Columns.Add("DonGia", typeof(decimal));
            detailGrid.DataSource = details;
            var panel = new Panel { Dock = DockStyle.Fill }; panel.Controls.Add(grid); panel.Controls.Add(detailGrid);
            Controls.Add(panel); Controls.Add(top);
            grid.SelectionChanged += (_,__) => FillInputs();
            Load += (_,__) => { try { supplier.DataSource = bus.GetSuppliers(); supplier.DisplayMember = "TenNCC"; supplier.ValueMember = "MaNCC"; LoadItems(); } catch(Exception ex) { MessageBox.Show(ex.Message); } };
        }

        private void LoadItems() { grid.DataSource = bus.GetItems(search.Text.Trim()); }
        private void FillInputs()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells["MaVatTu"]?.Value == null) return;
            id.Text = Convert.ToString(grid.CurrentRow.Cells["MaVatTu"].Value);
            name.Text = Convert.ToString(grid.CurrentRow.Cells["TenVatTu"].Value);
            unit.Text = Convert.ToString(grid.CurrentRow.Cells["DVT"].Value);
        }
        private void SaveItem()
        {
            try { bus.SaveItem(id.Text, name.Text, unit.Text); LoadItems(); MessageBox.Show("Đã lưu vật tư."); }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void DeleteItem()
        {
            try { bus.DeleteItem(id.Text); LoadItems(); MessageBox.Show("Đã xóa vật tư."); }
            catch(Exception ex) { MessageBox.Show("Không thể xóa. Có thể vật tư đã nằm trong phiếu kho.\n" + ex.Message); }
        }
        private void AddDetail()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id.Text)) throw new Exception("Chọn hoặc nhập mã vật tư trước.");
                if (!decimal.TryParse(qty.Text, out decimal q) || q <= 0) throw new Exception("Số lượng phải lớn hơn 0.");
                if (!decimal.TryParse(price.Text, out decimal p) || p < 0) throw new Exception("Đơn giá không hợp lệ.");
                details.Rows.Add(id.Text.Trim(), q, p);
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void SaveSlip()
        {
            try
            {
                int ncc = supplier.SelectedValue == null ? 0 : Convert.ToInt32(supplier.SelectedValue);
                decimal total = bus.SaveStockSlip(type.SelectedItem.ToString(), ncc, details);
                details.Rows.Clear(); LoadItems();
                MessageBox.Show("Lưu phiếu thành công. Tổng tiền: " + total.ToString("N0") + " đ");
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
