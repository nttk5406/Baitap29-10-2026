using System;
using System.Data;
using CafePOS.DAL;

namespace CafePOS.BUS
{
    public class CafeBUS
    {
        private readonly CafeDAL dal = new CafeDAL();

        public DataTable GetTables() => dal.GetTables();
        public DataTable GetMenu() => dal.GetMenu();
        public DataTable GetCategories() => dal.GetCategories();
        public DataSet GetCafeDataSet() => dal.GetCafeDataSet();
        public DataTable GetOrder(int maBan) => dal.GetOrder(maBan);

        public void AddItem(int ban, int mon, int sl, decimal gia)
        {
            if (ban <= 0 || mon <= 0 || sl <= 0 || gia < 0)
                throw new Exception("Thông tin gọi món không hợp lệ.");
            dal.AddItem(ban, mon, sl, gia);
        }

        public decimal Pay(int ban)
        {
            if (ban <= 0)
                throw new Exception("Hãy chọn bàn cần thanh toán.");
            return dal.Pay(ban);
        }

        public void SaveMenuItem(int ma, string ten, int dm, decimal gia)
        {
            if (string.IsNullOrWhiteSpace(ten))
                throw new Exception("Tên món không được để trống.");
            if (dm <= 0)
                throw new Exception("Hãy chọn danh mục món ăn.");
            if (gia <= 0)
                throw new Exception("Đơn giá phải lớn hơn 0.");
            dal.SaveMenuItem(ma, ten.Trim(), dm, gia);
        }

        public void DeleteMenuItem(int ma)
        {
            if (ma <= 0)
                throw new Exception("Mã món không hợp lệ.");
            dal.DeleteMenuItem(ma);
        }
    }
}
