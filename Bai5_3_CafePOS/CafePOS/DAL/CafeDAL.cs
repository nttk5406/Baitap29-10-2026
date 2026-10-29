using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOS.DAL
{
    public class CafeDAL
    {
        public DataTable GetTables()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaBan, TenBan, TrangThai FROM BanAn ORDER BY MaBan", cn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetMenu()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaMon, TenMon, MaDanhMuc, DonGia FROM MonAn ORDER BY TenMon", cn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetCategories()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc ORDER BY MaDanhMuc", cn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataSet GetCafeDataSet()
        {
            using var cn = DatabaseHelper.GetConnection();

            var ds = new DataSet("CafePOS");

            using (var da = new SqlDataAdapter(
                "SELECT MaBan, TenBan, TrangThai FROM BanAn", cn))
            {
                da.Fill(ds, "BanAn");
            }

            using (var da = new SqlDataAdapter(
                "SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc", cn))
            {
                da.Fill(ds, "DanhMuc");
            }

            using (var da = new SqlDataAdapter(
                "SELECT MaMon, TenMon, MaDanhMuc, DonGia FROM MonAn", cn))
            {
                da.Fill(ds, "MonAn");
            }

            return ds;
        }


        public DataTable GetOrder(int maBan)
        {
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT c.MaMon, m.TenMon, c.SoLuong, c.DonGia,
                       c.SoLuong * c.DonGia AS ThanhTien
                FROM HoaDon h
                JOIN ChiTietHoaDon c ON h.MaHoaDon = c.MaHoaDon
                JOIN MonAn m ON c.MaMon = m.MaMon
                WHERE h.MaBan = @ban AND h.TrangThai = 0
                ORDER BY m.TenMon", cn);
            cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void AddItem(int maBan, int maMon, int soLuong, decimal donGia)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tr = cn.BeginTransaction();
            try
            {
                int maHD;
                using (var cmd = new SqlCommand("SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC", cn, tr))
                {
                    cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                    object value = cmd.ExecuteScalar();
                    if (value == null || value == DBNull.Value)
                    {
                        using var insert = new SqlCommand("INSERT INTO HoaDon(MaBan,GioVao,TongTien,TrangThai) VALUES(@ban,GETDATE(),0,0); SELECT CAST(SCOPE_IDENTITY() AS int);", cn, tr);
                        insert.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                        maHD = Convert.ToInt32(insert.ExecuteScalar());
                    }
                    else
                    {
                        maHD = Convert.ToInt32(value);
                    }
                }

                using (var cmd = new SqlCommand(@"
                    IF EXISTS(SELECT 1 FROM ChiTietHoaDon WHERE MaHoaDon=@hd AND MaMon=@mon)
                        UPDATE ChiTietHoaDon SET SoLuong=SoLuong+@sl WHERE MaHoaDon=@hd AND MaMon=@mon
                    ELSE
                        INSERT INTO ChiTietHoaDon(MaHoaDon,MaMon,SoLuong,DonGia) VALUES(@hd,@mon,@sl,@gia)", cn, tr))
                {
                    cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHD;
                    cmd.Parameters.Add("@mon", SqlDbType.Int).Value = maMon;
                    cmd.Parameters.Add("@sl", SqlDbType.Int).Value = soLuong;
                    var price = cmd.Parameters.Add("@gia", SqlDbType.Decimal);
                    price.Precision = 18;
                    price.Scale = 2;
                    price.Value = donGia;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("UPDATE BanAn SET TrangThai=N'Có khách' WHERE MaBan=@ban", cn, tr))
                {
                    cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                    cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }

        public decimal Pay(int maBan)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tr = cn.BeginTransaction();
            try
            {
                int maHD;
                using (var cmd = new SqlCommand("SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@ban AND TrangThai=0 ORDER BY MaHoaDon DESC", cn, tr))
                {
                    cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                    object value = cmd.ExecuteScalar();
                    if (value == null || value == DBNull.Value)
                        throw new Exception("Bàn chưa có hóa đơn.");
                    maHD = Convert.ToInt32(value);
                }

                decimal total;
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(SoLuong * DonGia),0) FROM ChiTietHoaDon WHERE MaHoaDon=@hd", cn, tr))
                {
                    cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHD;
                    total = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                using (var cmd = new SqlCommand(@"
                    UPDATE HoaDon SET GioRa=GETDATE(), TongTien=@tong, TrangThai=1 WHERE MaHoaDon=@hd;
                    UPDATE BanAn SET TrangThai=N'Trống' WHERE MaBan=@ban;", cn, tr))
                {
                    cmd.Parameters.Add("@tong", SqlDbType.Decimal).Value = total;
                    cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHD;
                    cmd.Parameters.Add("@ban", SqlDbType.Int).Value = maBan;
                    cmd.ExecuteNonQuery();
                }
                tr.Commit();
                return total;
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }

        public void SaveMenuItem(int maMon, string tenMon, int maDanhMuc, decimal gia)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            string sql = maMon == 0
                ? "INSERT INTO MonAn(TenMon,MaDanhMuc,DonGia) VALUES(@ten,@dm,@gia)"
                : "UPDATE MonAn SET TenMon=@ten,MaDanhMuc=@dm,DonGia=@gia WHERE MaMon=@ma";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = tenMon;
            cmd.Parameters.Add("@dm", SqlDbType.Int).Value = maDanhMuc;
            var price = cmd.Parameters.Add("@gia", SqlDbType.Decimal);
            price.Precision = 18;
            price.Scale = 2;
            price.Value = gia;
            if (maMon != 0)
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maMon;
            cmd.ExecuteNonQuery();
        }

        public void DeleteMenuItem(int maMon)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var cmd = new SqlCommand("DELETE FROM MonAn WHERE MaMon=@ma", cn);
            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maMon;
            cmd.ExecuteNonQuery();
        }
    }
}
