using System.Data;
using InventoryManagement.DAL;

namespace InventoryManagement.BUS
{
    public class InventoryBUS
    {
        private readonly InventoryDAL dal = new InventoryDAL();
        public DataTable GetItems(string keyword = "") => dal.GetItems(keyword);
        public DataTable GetSuppliers() => dal.GetSuppliers();
        public void SaveItem(string id, string name, string unit)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(unit))
                throw new Exception("Mã, tên vật tư và đơn vị tính không được để trống.");
            dal.SaveItem(id.Trim(), name.Trim(), unit.Trim());
        }
        public void DeleteItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new Exception("Nhập mã vật tư cần xóa.");
            dal.DeleteItem(id.Trim());
        }
        public decimal SaveStockSlip(string type, int supplier, DataTable details) => dal.SaveStockSlip(type, supplier, details);
        public decimal GetStockValue() => dal.GetStockValue();
        public DataTable GetLowStock(decimal threshold) => dal.GetLowStock(threshold);
    }
}
