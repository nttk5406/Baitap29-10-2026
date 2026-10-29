using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class HotelDAL
    {
        public DataTable GetRooms(string filter = "")
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM Phong ORDER BY SoPhong", cn);
            var dt = new DataTable();
            da.Fill(dt);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string safeFilter = filter.Replace("'", "''");
                dt.DefaultView.RowFilter = "LoaiPhong = '" + safeFilter + "' OR TrangThai = '" + safeFilter + "'";
            }
            return dt;
        }

        public DataTable GetServices()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaDV,TenDV,DonGia FROM DichVu ORDER BY TenDV", cn);
            var dt = new DataTable(); da.Fill(dt); return dt;
        }

        public List<ServiceDTO> GetServiceDTOs()
        {
            var services = new List<ServiceDTO>();
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT MaDV, TenDV, DonGia FROM DichVu ORDER BY TenDV", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                services.Add(new ServiceDTO
                {
                    MaDV = rd.GetInt32(0),
                    TenDV = rd.GetString(1),
                    DonGia = rd.GetDecimal(2)
                });
            }
            return services;
        }

        public void CheckIn(string room, KhachHangDTO khachHang)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tr = cn.BeginTransaction();
            try
            {
                int kh;
                using (var cmd = new SqlCommand("SELECT MaKH FROM KhachHang WHERE CCCD=@cccd", cn, tr))
                {
                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar,20).Value = khachHang.CCCD;
                    object o = cmd.ExecuteScalar();
                    if (o == null)
                    {
                        using var ins = new SqlCommand("INSERT INTO KhachHang(HoTen,CCCD,SoDienThoai) VALUES(@ten,@cccd,@sdt); SELECT CAST(SCOPE_IDENTITY() AS int)", cn, tr);
                        ins.Parameters.Add("@ten", SqlDbType.NVarChar,100).Value = khachHang.HoTen;
                        ins.Parameters.Add("@cccd", SqlDbType.VarChar,20).Value = khachHang.CCCD;
                        ins.Parameters.Add("@sdt", SqlDbType.VarChar,15).Value = khachHang.SoDienThoai ?? "";
                        kh = (int)ins.ExecuteScalar();
                    }
                    else kh = Convert.ToInt32(o);
                }
                using (var cmd = new SqlCommand("IF EXISTS(SELECT 1 FROM Phong WHERE SoPhong=@p AND TrangThai=N'Trống') BEGIN INSERT INTO PhieuDatPhong(SoPhong,MaKH,NgayNhanPhong) VALUES(@p,@kh,GETDATE()); UPDATE Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@p; END ELSE THROW 50001, N'Phòng không trống hoặc không tồn tại.', 1;", cn, tr))
                {
                    cmd.Parameters.Add("@p", SqlDbType.VarChar,10).Value = room;
                    cmd.Parameters.Add("@kh", SqlDbType.Int).Value = kh;
                    cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        public int GetActiveBooking(string room)
        {
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT TOP 1 MaPhieu FROM PhieuDatPhong WHERE SoPhong=@p AND NgayTraPhong IS NULL ORDER BY MaPhieu DESC", cn);
            cmd.Parameters.Add("@p", SqlDbType.VarChar,10).Value = room;
            cn.Open(); object o = cmd.ExecuteScalar();
            if (o == null) throw new Exception("Phòng chưa có phiếu nhận phòng.");
            return Convert.ToInt32(o);
        }

        public void AddService(SuDungDichVuDTO suDung)
        {
            int booking = GetActiveBooking(suDung.SoPhong);
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = new SqlCommand("INSERT INTO SuDungDichVu(MaPhieu,MaDV,SoLuong,ThoiGian) VALUES(@phieu,@dv,@sl,GETDATE())", cn);
            cmd.Parameters.Add("@phieu", SqlDbType.Int).Value = booking;
            cmd.Parameters.Add("@dv", SqlDbType.Int).Value = suDung.MaDV;
            cmd.Parameters.Add("@sl", SqlDbType.Int).Value = suDung.SoLuong;
            cmd.ExecuteNonQuery();
        }

        public decimal CheckOut(string room)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tr = cn.BeginTransaction();
            try
            {
                int booking; decimal rate; DateTime checkin;
                using (var cmd = new SqlCommand("SELECT TOP 1 b.MaPhieu,p.GiaTheoGio,b.NgayNhanPhong FROM PhieuDatPhong b JOIN Phong p ON p.SoPhong=b.SoPhong WHERE b.SoPhong=@p AND b.NgayTraPhong IS NULL ORDER BY b.MaPhieu DESC", cn, tr))
                {
                    cmd.Parameters.Add("@p", SqlDbType.VarChar,10).Value = room;
                    using var rd = cmd.ExecuteReader();
                    if (!rd.Read()) throw new Exception("Không tìm thấy phiếu đang ở.");
                    booking = rd.GetInt32(0); rate = rd.GetDecimal(1); checkin = rd.GetDateTime(2);
                }
                decimal serviceTotal;
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(s.SoLuong*d.DonGia),0) FROM SuDungDichVu s JOIN DichVu d ON d.MaDV=s.MaDV WHERE s.MaPhieu=@phieu", cn, tr))
                {
                    cmd.Parameters.Add("@phieu", SqlDbType.Int).Value = booking;
                    serviceTotal = Convert.ToDecimal(cmd.ExecuteScalar());
                }
                decimal hours = (decimal)Math.Max(1, Math.Ceiling((DateTime.Now - checkin).TotalHours));
                decimal total = hours * rate + serviceTotal;
                using (var cmd = new SqlCommand("UPDATE PhieuDatPhong SET NgayTraPhong=GETDATE(),TongTienThanhToan=@tong WHERE MaPhieu=@phieu; UPDATE Phong SET TrangThai=N'Đang dọn dẹp' WHERE SoPhong=@p", cn, tr))
                {
                    cmd.Parameters.Add("@tong", SqlDbType.Decimal).Value = total;
                    cmd.Parameters.Add("@phieu", SqlDbType.Int).Value = booking;
                    cmd.Parameters.Add("@p", SqlDbType.VarChar,10).Value = room;
                    cmd.ExecuteNonQuery();
                }
                tr.Commit(); return total;
            }
            catch { tr.Rollback(); throw; }
        }
    }
}
