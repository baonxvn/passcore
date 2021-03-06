﻿using Hpl.SaleOnlineDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static string GetMailListByMaPhongBan(string maPhongBan)
        {
            var db = new AbpHplDbContext();
            var query = db.HplPhongBans.FirstOrDefault(x => x.MaPhongBan == maPhongBan);
            if (query == null) return "";
            if (string.IsNullOrEmpty(query.MailingList)) return query.MailingList;

            db.Dispose();

            return "";
        }

        public static HplPhongBan GetAbpPhongBanByMaPhongBan(string maPhongBan)
        {
            var db = new AbpHplDbContext();
            var query = db.HplPhongBans.FirstOrDefault(x => x.MaPhongBan == maPhongBan);
            if (query == null) return null;

            db.Dispose();

            return query;
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
            var dt = DateTime.Now.AddDays(-7);

            return db.HplNhanVienLogs.Where(x => x.DateCreated.Value >= dt)
                .OrderByDescending(x => x.DateCreated).ToList();
        }

        public static List<HplPhongBan> GetListPhongBan()
        {
            var db = new AbpHplDbContext();

            return db.HplPhongBans.OrderBy(x => x.TenPhongBan).ToList();
        }
    }
}