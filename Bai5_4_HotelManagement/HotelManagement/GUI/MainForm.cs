using System.Data;
using HotelManagement.BUS;
using HotelManagement.DTO;

namespace HotelManagement.GUI
{
    public class MainForm : Form
    {
        private readonly HotelBUS bus = new HotelBUS();
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly TextBox room = new() { Width = 80, PlaceholderText = "P101" };
        private readonly TextBox name = new() { Width = 140, PlaceholderText = "Họ tên" };
        private readonly TextBox cccd = new() { Width = 130, PlaceholderText = "CCCD" };
        private readonly TextBox phone = new() { Width = 110, PlaceholderText = "Điện thoại" };
        private readonly ComboBox service = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown qty = new() { Minimum = 1, Maximum = 999, Value = 1, Width = 60 };
        private readonly ComboBox filter = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };

        public MainForm()
        {
            Text = "Bài 5.4 - Hotel Management"; Width = 1100; Height = 650; StartPosition = FormStartPosition.CenterScreen;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(8), AutoScroll = true };
            top.Controls.Add(new Label { Text = "Phòng:", AutoSize = true, Padding = new Padding(3,8,3,0) }); top.Controls.Add(room);
            top.Controls.Add(new Label { Text = "Họ tên:", AutoSize = true, Padding = new Padding(3,8,3,0) }); top.Controls.Add(name);
            top.Controls.Add(new Label { Text = "CCCD:", AutoSize = true, Padding = new Padding(3,8,3,0) }); top.Controls.Add(cccd);
            top.Controls.Add(phone);
            var checkin = new Button { Text = "Check-in", AutoSize = true }; checkin.Click += (_,__) => CheckIn();
            top.Controls.Add(checkin);
            top.Controls.Add(new Label { Text = "Dịch vụ:", AutoSize = true, Padding = new Padding(3,8,3,0) }); top.Controls.Add(service); top.Controls.Add(qty);
            var add = new Button { Text = "Gọi dịch vụ", AutoSize = true }; add.Click += (_,__) => AddService(); top.Controls.Add(add);
            var checkout = new Button { Text = "Check-out", AutoSize = true }; checkout.Click += (_,__) => CheckOut(); top.Controls.Add(checkout);
            filter.Items.AddRange(new object[] { "Tất cả", "Đơn", "Đôi", "VIP", "Trống", "Đang ở", "Đang dọn dẹp" }); filter.SelectedIndex = 0;
            var search = new Button { Text = "Lọc phòng", AutoSize = true }; search.Click += (_,__) => LoadRooms();
            top.Controls.Add(filter); top.Controls.Add(search);
            Controls.Add(grid); Controls.Add(top);
            Load += (_,__) => { try { service.DataSource = bus.GetServiceDTOs(); service.DisplayMember = "TenDV"; service.ValueMember = "MaDV"; LoadRooms(); } catch(Exception ex) { MessageBox.Show(ex.Message); } };
        }

        private void LoadRooms()
        {
            string f = filter.SelectedItem?.ToString() ?? "Tất cả";
            grid.DataSource = bus.GetRooms(f == "Tất cả" ? "" : f);
        }
        private void CheckIn()
        {
            try { bus.CheckIn(room.Text, new KhachHangDTO { HoTen = name.Text, CCCD = cccd.Text, SoDienThoai = phone.Text }); LoadRooms(); MessageBox.Show("Check-in thành công."); }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void AddService()
        {
            try
            {
                if (service.SelectedItem == null) throw new Exception("Chọn dịch vụ.");
                bus.AddService(new SuDungDichVuDTO { SoPhong = room.Text, MaDV = Convert.ToInt32(service.SelectedValue), SoLuong = (int)qty.Value });
                MessageBox.Show("Đã ghi nhận dịch vụ.");
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void CheckOut()
        {
            try { decimal total = bus.CheckOut(room.Text); LoadRooms(); MessageBox.Show("Tổng thanh toán: " + total.ToString("N0") + " đ"); }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
