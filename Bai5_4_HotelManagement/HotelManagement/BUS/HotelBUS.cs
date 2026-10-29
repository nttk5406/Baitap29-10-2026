using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    public class HotelBUS
    {
        private readonly HotelDAL dal = new HotelDAL();

        public DataTable GetRooms(string filter = "") => dal.GetRooms(filter);
        public DataTable GetServices() => dal.GetServices();
        public List<ServiceDTO> GetServiceDTOs() => dal.GetServiceDTOs();

        public void CheckIn(string room, KhachHangDTO khachHang)
        {
            if (string.IsNullOrWhiteSpace(room) || khachHang == null ||
                string.IsNullOrWhiteSpace(khachHang.HoTen) ||
                string.IsNullOrWhiteSpace(khachHang.CCCD))
                throw new Exception("Phải nhập số phòng, họ tên và CCCD.");

            khachHang.HoTen = khachHang.HoTen.Trim();
            khachHang.CCCD = khachHang.CCCD.Trim();
            khachHang.SoDienThoai = khachHang.SoDienThoai?.Trim() ?? "";
            dal.CheckIn(room.Trim(), khachHang);
        }

        public void AddService(SuDungDichVuDTO suDung)
        {
            if (suDung == null || string.IsNullOrWhiteSpace(suDung.SoPhong) ||
                suDung.MaDV <= 0 || suDung.SoLuong <= 0)
                throw new Exception("Thông tin dịch vụ không hợp lệ.");
            suDung.SoPhong = suDung.SoPhong.Trim();
            dal.AddService(suDung);
        }

        public decimal CheckOut(string room)
        {
            if (string.IsNullOrWhiteSpace(room))
                throw new Exception("Hãy nhập số phòng cần trả.");
            return dal.CheckOut(room.Trim());
        }
    }
}
