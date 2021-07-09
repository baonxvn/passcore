using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hpl.SaleOnlineDatabase;

namespace Hpl.HrmDatabase.Services
{
    public class AbpServices
    {
        public static void AddLogNhanVien(List<HplNhanVienLog> listNhanVien)
        {
            var db = new AbpHplDbContext();

            db.HplNhanVienLogs.AddRange(listNhanVien);
            db.SaveChanges();
            db.Dispose();
        }

        public static void UpdateBranch()
        {
            var db = new SaleOnlineDbContext();
            var dbApb = new AbpHplDbContext();
            var listPbs = dbApb.HplPhongBans;
            foreach (var pb in listPbs)
            {
                var branch = db.Branches.FirstOrDefault(x => x.BranchCode == pb.MaPhongBan);
                if (branch != null)
                {
                    pb.BranchId = branch.BranchId;
                    pb.BranchName = branch.BranchName;
                    pb.BranchCode = branch.BranchCode;
                }
            }
            //SaleOnlineServices
            dbApb.SaveChanges();
            dbApb.Dispose();
        }

        public static void AddSyncLogAbp(HplSyncLog model)
        {
            AbpHplDbContext db = new AbpHplDbContext();

            db.HplSyncLogs.Add(model);
            db.SaveChanges();
            db.Dispose();
        }

        public static void AddSyncLogAbp(List<HplSyncLog> listLogs)
        {
            AbpHplDbContext db = new AbpHplDbContext();

            db.HplSyncLogs.AddRange(listLogs);
            db.SaveChanges();
            db.Dispose();
        }

        public static List<HplNhanVienLog> GetAllLogNhanVien()
        {
            var db = new AbpHplDbContext();

            return db.HplNhanVienLogs.OrderByDescending(x=>x.DateCreated).ToList();
        }

        public static List<HplPhongBan> GetListPhongBan()
        {
            var db = new AbpHplDbContext();

            return db.HplPhongBans.OrderBy(x => x.TenPhongBan).ToList();
        }
    }
}