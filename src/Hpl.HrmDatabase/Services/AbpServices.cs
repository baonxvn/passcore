using System.Collections.Generic;
using System.Linq;

namespace Hpl.HrmDatabase.Services
{
    public class AbpServices
    {
        public static void AddSyncLogAbp(HplSyncLog model)
        {
            AbpHplDbContext db = new AbpHplDbContext();

            db.HplSyncLogs.Add(model);
            db.SaveChanges();
            db.Dispose();
        }

        public static List<HplPhongBan> GetListPhongBan()
        {
            var db = new AbpHplDbContext();

            return db.HplPhongBans.OrderBy(x => x.TenPhongBan).ToList();
        }
    }
}