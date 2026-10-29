using System.Data;
using Microsoft.Data.SqlClient;

namespace InventoryManagement.DAL
{
    public class InventoryDAL
    {
        public DataTable GetItems(string keyword = "")
        {
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT MaVatTu,TenVatTu,DVT,SoLuongTon FROM VatTu WHERE MaVatTu LIKE @kw OR TenVatTu LIKE @kw ORDER BY MaVatTu", cn);
            cmd.Parameters.Add("@kw", SqlDbType.NVarChar,150).Value = "%" + keyword + "%";
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable(); da.Fill(dt); return dt;
        }

        public DataTable GetSuppliers()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaNCC,TenNCC,DienThoai,DiaChi FROM NhaCungCap ORDER BY MaNCC", cn);
            var dt = new DataTable(); da.Fill(dt); return dt;
        }

        public void SaveItem(string id, string name, string unit)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM VatTu WHERE MaVatTu=@ma)
                UPDATE VatTu SET TenVatTu=@ten,DVT=@dvt WHERE MaVatTu=@ma
                ELSE INSERT INTO VatTu(MaVatTu,TenVatTu,DVT,SoLuongTon) VALUES(@ma,@ten,@dvt,0)", cn);
            cmd.Parameters.Add("@ma", SqlDbType.VarChar,20).Value = id;
            cmd.Parameters.Add("@ten", SqlDbType.NVarChar,150).Value = name;
            cmd.Parameters.Add("@dvt", SqlDbType.NVarChar,20).Value = unit;
            cmd.ExecuteNonQuery();
        }

        public void DeleteItem(string id)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = new SqlCommand("DELETE FROM VatTu WHERE MaVatTu=@ma", cn);
            cmd.Parameters.Add("@ma", SqlDbType.VarChar,20).Value = id;
            cmd.ExecuteNonQuery();
        }

        public decimal SaveStockSlip(string type, int supplierId, DataTable details)
        {
            if (type != "NHAP" && type != "XUAT") throw new Exception("Loại phiếu không hợp lệ.");
            if (details.Rows.Count == 0) throw new Exception("Phiếu chưa có vật tư.");
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tr = cn.BeginTransaction();
            try
            {
                decimal total = 0;
                foreach (DataRow r in details.Rows)
                {
                    decimal qty = Convert.ToDecimal(r["SoLuong"]);
                    decimal price = Convert.ToDecimal(r["DonGia"]);
                    if (qty <= 0 || price < 0) throw new Exception("Số lượng phải > 0 và đơn giá không âm.");
                    total += qty * price;
                }
                int slipId;
                using (var cmd = new SqlCommand("INSERT INTO PhieuKho(LoaiPhieu,MaNCC,TongTien) VALUES(@loai,@ncc,@tong); SELECT CAST(SCOPE_IDENTITY() AS int)", cn, tr))
                {
                    cmd.Parameters.Add("@loai", SqlDbType.NVarChar,10).Value = type;
                    if (type == "NHAP" && supplierId > 0) cmd.Parameters.Add("@ncc", SqlDbType.Int).Value = supplierId;
                    else cmd.Parameters.Add("@ncc", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@tong", SqlDbType.Decimal).Value = total;
                    slipId = (int)cmd.ExecuteScalar();
                }
                foreach (DataRow r in details.Rows)
                {
                    string id = Convert.ToString(r["MaVatTu"]);
                    decimal qty = Convert.ToDecimal(r["SoLuong"]);
                    decimal price = Convert.ToDecimal(r["DonGia"]);
                    using (var cmd = new SqlCommand("INSERT INTO ChiTietPhieuKho(MaPhieu,MaVatTu,SoLuong,DonGia) VALUES(@p,@ma,@sl,@gia)", cn, tr))
                    {
                        cmd.Parameters.Add("@p", SqlDbType.Int).Value = slipId;
                        cmd.Parameters.Add("@ma", SqlDbType.VarChar,20).Value = id;
                        cmd.Parameters.Add("@sl", SqlDbType.Decimal).Value = qty;
                        cmd.Parameters.Add("@gia", SqlDbType.Decimal).Value = price;
                        cmd.ExecuteNonQuery();
                    }
                    string update = type == "NHAP"
                        ? "UPDATE VatTu SET SoLuongTon=SoLuongTon+@sl WHERE MaVatTu=@ma"
                        : "UPDATE VatTu SET SoLuongTon=SoLuongTon-@sl WHERE MaVatTu=@ma AND SoLuongTon>=@sl";
                    using (var cmd = new SqlCommand(update, cn, tr))
                    {
                        cmd.Parameters.Add("@sl", SqlDbType.Decimal).Value = qty;
                        cmd.Parameters.Add("@ma", SqlDbType.VarChar,20).Value = id;
                        int affected = cmd.ExecuteNonQuery();
                        if (affected == 0 && type == "XUAT") throw new Exception("Không đủ hàng trong kho hoặc mã vật tư không tồn tại.");
                        if (affected == 0) throw new Exception("Mã vật tư không tồn tại.");
                    }
                }
                tr.Commit();
                return total;
            }
            catch { tr.Rollback(); throw; }
        }

        public decimal GetStockValue()
        {
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(SoLuongTon * 1.0),0) FROM VatTu", cn);
            cn.Open(); return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public DataTable GetLowStock(decimal threshold)
        {
            using var cn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT MaVatTu,TenVatTu,DVT,SoLuongTon FROM VatTu WHERE SoLuongTon<@nguong ORDER BY SoLuongTon", cn);
            cmd.Parameters.Add("@nguong", SqlDbType.Decimal).Value = threshold;
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable(); da.Fill(dt); return dt;
        }
    }
}
