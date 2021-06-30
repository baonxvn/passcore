using System;
using System.Collections.Generic;
using System.Linq;
using Hpl.HrmDatabase.ViewModels;

namespace Hpl.HrmDatabase.Services
{
    public class UserService
    {
        public static NhanVienViewModel GetNhanVienByUsername(string username)
        {
            var db = new HrmDbContext();
            var nhanVien = new NhanVienViewModel();
            //var qr1 = db.SysNguoiDungs.Where(x => x.TenDangNhap == username);

            var query = from u in db.SysNguoiDungs.ToList()
                        join n in db.NhanViens on u.NhanVienId equals n.NhanVienId into table1
                        from n in table1.ToList()
                        join cv in db.NsDsChucVus on n.ChucVuId equals cv.ChucVuId into table2
                        from cv in table2.ToList()
                        join cd in db.NsDsChucDanhs on n.ChucDanhId equals cd.ChucDanhId into table3
                        from cd in table3.ToList()
                        join p in db.PhongBans on n.PhongBanId equals p.PhongBanId into table4
                        from p in table4.ToList()
                        where u.TenDangNhap == username
                        select new NhanVienViewModel
                        {
                            NhanVienID = n.NhanVienId,
                            Ho = n.Ho,
                            Ten = n.HoTen,
                            GioiTinh = n.GioiTinh,
                            MaNhanVien = n.MaNhanVien,
                            TenDangNhap = u.TenDangNhap,
                            Email = n.Email,
                            DienThoai = n.DienThoai,
                            CMTND = n.Cmtnd,
                            TenChucVu = cv.TenChucVu,
                            TenChucDanh = cd.TenChucDanh,
                            PhongBanId = p.PhongBanId,
                            TenPhongBan = p.Ten,
                            MaPhongBan = p.MaPhongBan
                        };

            var nvList = query.ToList();
            switch (nvList.ToList().Count)
            {
                case > 1:
                    nhanVien.TenDangNhap = "Username này bị trùng lặp, yêu cầu kiểm tra lại";
                    break;
                case 1:
                    nhanVien = nvList.FirstOrDefault();
                    var pbList = db.PhongBans.ToList();

                    var child = pbList.First(x => nhanVien != null && x.PhongBanId == nhanVien.PhongBanId);
                    var parents = FindAllParents(pbList, child).ToList();

                    int index = 0;
                    if (nhanVien != null)
                    {
                        foreach (var phongBan in parents)
                        {
                            index++;
                            switch (index)
                            {
                                case 1:
                                    nhanVien.PhongBanCha = phongBan.Ten;
                                    nhanVien.MaCha = phongBan.MaPhongBan;
                                    break;
                                case 2:
                                    nhanVien.PhongBanOng = phongBan.Ten;
                                    nhanVien.MaOng = phongBan.MaPhongBan;
                                    break;
                                case 3:
                                    nhanVien.PhongBanCo = phongBan.Ten;
                                    nhanVien.MaCo = phongBan.MaPhongBan;
                                    break;
                                case 4:
                                    nhanVien.PhongBanKy = phongBan.Ten;
                                    nhanVien.MaKy = phongBan.MaPhongBan;
                                    break;
                                case 5:
                                    nhanVien.PhongBan6 = phongBan.Ten;
                                    nhanVien.MaPb6 = phongBan.MaPhongBan;
                                    break;
                            }
                        }
                    }

                    break;

                default:
                    nhanVien = null;
                    break;
            }

            return nhanVien;
        }

        /// <summary>
        /// Lấy thông tin phòng ban Cấp 1 của Nhân Viên theo Ma Nhân Viên
        /// </summary>
        /// <param name="maNhanVien"></param>
        /// <returns></returns>
        public static PhongBan GetPhongBanCap1CuaNhanVien(string maNhanVien)
        {
            var db = new HrmDbContext();
            var dbHpl = new AbpHplDbContext();
            var phongBan = new PhongBan();
            var listNvs = db.NhanViens.Where(x => x.MaNhanVien == maNhanVien).ToList();

            if (listNvs.Count == 1)
            {
                var pb = listNvs.FirstOrDefault();
                if (pb != null & pb.PhongBanId != null)
                {
                    int phongBanId = pb.PhongBanId.Value;
                    var listIdHrm = GetAllChildrenAndParents(phongBanId).Select(x => x.PhongBanId).ToList();
                    var listIdAbp = dbHpl.HplPhongBans.Select(x => x.PhongBanId).ToList();
                    var listIds = listIdHrm.Intersect(listIdAbp).ToList();
                    switch (listIds.Count())
                    {
                        case 1:
                            phongBan = db.PhongBans.FirstOrDefault(x => listIds.Contains(x.PhongBanId));
                            break;
                        case < 1:
                        case > 1:
                            //TODO
                            break;
                    }
                }
            }

            return phongBan;
        }

        /// <summary>
        /// Trả về danh sách nhân viên đã chuẩn hóa
        /// </summary>
        /// <param name="phongBanId"></param>
        /// <returns>List NhanVienViewModel</returns>
        public static List<NhanVienViewModel> GetAllNhanVienCuaPhongBan(int phongBanId)
        {
            var db = new HrmDbContext();
            PhongBan phongBan = db.PhongBans.Single(x => x.PhongBanId == phongBanId);

            var listPbs = GetAllChildren1(db.PhongBans.ToList(), phongBanId);
            listPbs.Add(db.PhongBans.Single(x => x.PhongBanId == phongBanId));
            var listPbIds = listPbs.Select(x => x.PhongBanId).ToList();

            var query = db.NhanViens.Where(x => listPbIds.Contains((int)x.PhongBanId)).ToList();

            //var listNvs = from nv in db.NhanViens.ToList()
            var listNvs = from nv in query
                          join nd in db.SysNguoiDungs on nv.NhanVienId equals nd.NhanVienId into tb1
                          from nd in tb1
                          join cv in db.NsDsChucVus on nv.ChucVuId equals cv.ChucVuId into tb2
                          from cv in tb2.ToList()
                          join cd in db.NsDsChucDanhs on nv.ChucDanhId equals cd.ChucDanhId into tb3
                          from cd in tb3.ToList()
                              //join pb in db.PhongBans on nv.PhongBanId equals pb.PhongBanId into table4
                              //from pb in table4.ToList()
                          select new NhanVienViewModel
                          {
                              NhanVienID = nv.NhanVienId,
                              Ho = nv.Ho,
                              Ten = nv.HoTen,
                              GioiTinh = nv.GioiTinh,
                              MaNhanVien = nv.MaNhanVien,
                              TenDangNhap = nd.TenDangNhap,
                              Email = nv.Email,
                              DienThoai = nv.DienThoai,
                              CMTND = nv.Cmtnd,
                              TenChucVu = cv.TenChucVu,
                              TenChucDanh = cd.TenChucDanh,
                              PhongBanId = phongBan.PhongBanId,
                              TenPhongBan = phongBan.Ten,
                              MaPhongBan = phongBan.MaPhongBan
                          };

            return listNvs.ToList();
        }

        /// <summary>
        /// Trả về danh sách nhân viên trên DB HRM
        /// </summary>
        /// <param name="listMaNhanVien"></param>
        /// <returns>List NhanVienViewModel</returns>
        public static List<NhanVienViewModel> GetAllNhanVienTheoMa(List<string> listMaNhanVien)
        {
            var db = new HrmDbContext();
            try
            {
                var listNvs = from nv in db.NhanViens
                              join nd in db.SysNguoiDungs on nv.NhanVienId equals nd.NhanVienId into tb1
                              from nd in tb1.DefaultIfEmpty()
                              join cv in db.NsDsChucVus on nv.ChucVuId equals cv.ChucVuId into tb2
                              from cv in tb2.DefaultIfEmpty()
                              join cd in db.NsDsChucDanhs on nv.ChucDanhId equals cd.ChucDanhId into tb3
                              from cd in tb3.DefaultIfEmpty()
                              join pb in db.PhongBans on nv.PhongBanId equals pb.PhongBanId into table4
                              from pb in table4.DefaultIfEmpty()
                              join pb2 in db.PhongBans on pb.PhongBanChaId equals pb2.PhongBanId into table5
                              from pb2 in table5.DefaultIfEmpty()
                              join pb3 in db.PhongBans on pb2.PhongBanChaId equals pb3.PhongBanId into table6
                              from pb3 in table6.DefaultIfEmpty()
                              join pb4 in db.PhongBans on pb3.PhongBanChaId equals pb4.PhongBanId into table7
                              from pb4 in table7.DefaultIfEmpty()
                              join pb5 in db.PhongBans on pb4.PhongBanChaId equals pb5.PhongBanId into table8
                              from pb5 in table8.DefaultIfEmpty()
                              join pb6 in db.PhongBans on pb5.PhongBanChaId equals pb6.PhongBanId into table9
                              from pb6 in table9.DefaultIfEmpty()
                              where listMaNhanVien.Contains(nv.MaNhanVien) & !string.IsNullOrEmpty(nv.MaNhanVien)
                               
                              select new NhanVienViewModel
                              {
                                  NhanVienID = nv.NhanVienId,
                                  Ho = nv.Ho,
                                  Ten = nv.HoTen,
                                  GioiTinh = nv.GioiTinh,
                                  MaNhanVien = nv.MaNhanVien,
                                  TenDangNhap = nd.TenDangNhap,
                                  Email = nv.Email,
                                  DienThoai = nv.DienThoai,
                                  CMTND = nv.Cmtnd,
                                  TenChucVu = cv.TenChucVu,
                                  TenChucDanh = cd.TenChucDanh,
                                  PhongBanId = pb.PhongBanId,
                                  TenPhongBan = pb.Ten,
                                  MaPhongBan = pb.MaPhongBan,
                                  PhongBanCha = pb2.Ten,
                                  MaCha = pb.MaPhongBan,
                                  PhongBanOng = pb3.Ten,
                                  MaOng = pb3.MaPhongBan,
                                  PhongBanCo = pb4.Ten,
                                  MaCo = pb4.MaPhongBan,
                                  PhongBanKy = pb5.Ten,
                                  MaKy = pb5.MaPhongBan,
                                  PhongBan6 = pb6.Ten,
                                  MaPb6 = pb6.MaPhongBan
                              };

                return listNvs.ToList();
            }
            catch (Exception e)
            {
                string abc = e.Message;
                return new List<NhanVienViewModel>();
            }
        }

        /// <summary>
        /// Trả về danh sách nhân viên trên DB HRM
        /// </summary>
        /// <param name="maNhanVien"></param>
        /// <returns>List NhanVienViewModel</returns>
        public static List<NhanVienViewModel> GetAllNhanVienTheoMa(string maNhanVien)
        {
            var db = new HrmDbContext();
            try
            {
                var listNvs = from nv in db.NhanViens
                              join nd in db.SysNguoiDungs on nv.NhanVienId equals nd.NhanVienId into tb1
                              from nd in tb1.DefaultIfEmpty()
                              join cv in db.NsDsChucVus on nv.ChucVuId equals cv.ChucVuId into tb2
                              from cv in tb2.DefaultIfEmpty()
                              join cd in db.NsDsChucDanhs on nv.ChucDanhId equals cd.ChucDanhId into tb3
                              from cd in tb3.DefaultIfEmpty()
                              join pb in db.PhongBans on nv.PhongBanId equals pb.PhongBanId into table4
                              from pb in table4.DefaultIfEmpty()
                              join pb2 in db.PhongBans on pb.PhongBanChaId equals pb2.PhongBanId into table5
                              from pb2 in table5.DefaultIfEmpty()
                              join pb3 in db.PhongBans on pb2.PhongBanChaId equals pb3.PhongBanId into table6
                              from pb3 in table6.DefaultIfEmpty()
                              join pb4 in db.PhongBans on pb3.PhongBanChaId equals pb4.PhongBanId into table7
                              from pb4 in table7.DefaultIfEmpty()
                              join pb5 in db.PhongBans on pb4.PhongBanChaId equals pb5.PhongBanId into table8
                              from pb5 in table8.DefaultIfEmpty()
                              join pb6 in db.PhongBans on pb5.PhongBanChaId equals pb6.PhongBanId into table9
                              from pb6 in table9.DefaultIfEmpty()
                              where nv.MaNhanVien.Equals(maNhanVien)
                              select new NhanVienViewModel
                              {
                                  NhanVienID = nv.NhanVienId,
                                  Ho = nv.Ho,
                                  Ten = nv.HoTen,
                                  GioiTinh = nv.GioiTinh,
                                  MaNhanVien = nv.MaNhanVien,
                                  TenDangNhap = nd.TenDangNhap,
                                  Email = nv.Email,
                                  DienThoai = nv.DienThoai,
                                  CMTND = nv.Cmtnd,
                                  TenChucVu = cv.TenChucVu,
                                  TenChucDanh = cd.TenChucDanh,
                                  PhongBanId = pb.PhongBanId,
                                  TenPhongBan = pb.Ten,
                                  MaPhongBan = pb.MaPhongBan,
                                  PhongBanCha = pb2.Ten,
                                  MaCha = pb.MaPhongBan,
                                  PhongBanOng = pb3.Ten,
                                  MaOng = pb3.MaPhongBan,
                                  PhongBanCo = pb4.Ten,
                                  MaCo = pb4.MaPhongBan,
                                  PhongBanKy = pb5.Ten,
                                  MaKy = pb5.MaPhongBan,
                                  PhongBan6 = pb6.Ten,
                                  MaPb6 = pb6.MaPhongBan
                              };

                return listNvs.Take(10).ToList();
            }
            catch (Exception e)
            {
                string abc = e.Message;
                return new List<NhanVienViewModel>();
            }
        }

        /// <summary>
        /// Trả về danh sách nhân viên dữ liệu thô theo table của DB
        /// </summary>
        /// <param name="phongBanId"></param>
        /// <returns>List NhanVien</returns>
        public static List<NhanVien> GetAllNhanVienCuaPhongBanRaw(int phongBanId)
        {
            var db = new HrmDbContext();

            var listPbs = GetAllChildren1(db.PhongBans.ToList(), phongBanId);
            listPbs.Add(db.PhongBans.Single(x => x.PhongBanId == phongBanId));
            var listPbIds = listPbs.Select(x => x.PhongBanId).ToList();

            return db.NhanViens.Where(x => listPbIds.Contains((int)x.PhongBanId)).ToList();
        }

        public static List<HplPhongBan> GetAllHplPhongBan()
        {
            var db = new AbpHplDbContext();
            var listPbs = db.HplPhongBans.ToList();

            return listPbs;

        }

        /// <summary>
        /// Lấy Danh sách Phòng Ban bao gồm danh sách con và cả danh sách cha (theo PhongBanId)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<PhongBan> GetAllChildrenAndParents(int id)
        {
            var db = new HrmDbContext();
            //Get all parents
            PhongBan pb = db.PhongBans.FirstOrDefault(x => x.PhongBanId == id);
            var listPbs = FindAllParents(db.PhongBans.ToList(), pb).ToList();
            //Add chính nó
            listPbs.Add(db.PhongBans.Single(x => x.PhongBanId == id));
            //Add danh sách con
            listPbs.AddRange(GetAllChildren1(db.PhongBans.ToList(), id));

            return listPbs;
        }

        /// <summary>
        /// Lấy Danh sách Phòng Ban bao gồm danh sách con và cả danh sách cha (theo MaNhanVien của nhân sự)
        /// </summary>
        /// <param name="maNhanVien">maNhanVien</param>
        /// <returns></returns>
        public static List<PhongBan> GetAllChildrenAndParents(string maNhanVien)
        {
            var db = new HrmDbContext();
            var listPbs = new List<PhongBan>();
            //Get all parents
            var lstNvs = db.NhanViens.Where(x => x.MaNhanVien == maNhanVien);
            switch (lstNvs.Count())
            {
                case 1:
                    var nhanVien = lstNvs.FirstOrDefault();
                    if (nhanVien is { PhongBanId: { } })
                    {
                        //Add danh sách con
                        listPbs = GetAllChildrenAndParents(nhanVien.PhongBanId.Value);
                    }

                    break;
                case < 1:
                case > 1:
                    //chưa xử lý gì
                    break;
            }

            return listPbs.OrderBy(x => x.PhongBanId).ToList();
        }

        /// <summary>
        /// Lấy bao gồm danh sách con và cả chính nó
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<PhongBan> GetAllChildrenAndSelf(int id)
        {
            var db = new HrmDbContext();
            var listPbs = GetAllChildren1(db.PhongBans.ToList(), id);
            listPbs.Add(db.PhongBans.Single(x => x.PhongBanId == id));

            return listPbs;
        }

        /// <summary>
        /// Lấy danh sách phòng ban Cha trở lên (không bao gồm chính nó)
        /// </summary>
        /// <param name="allData"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        private static IEnumerable<PhongBan> FindAllParents(List<PhongBan> allData, PhongBan child)
        {
            var parent = allData.FirstOrDefault(x => x.PhongBanId == child.PhongBanChaId);

            if (parent == null)
                return Enumerable.Empty<PhongBan>();

            return new[] { parent }.Concat(FindAllParents(allData, parent));

        }

        /// <summary>
        /// Lấy danh sách Phòng ban con (không bao gồm chính nó)
        /// </summary>
        /// <param name="listPb"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<PhongBan> GetAllChildren1(List<PhongBan> listPb, int id)
        {
            var query = listPb
                .Where(x => x.PhongBanChaId == id)
                .Union(listPb.Where(x => x.PhongBanChaId == id)
                    .SelectMany(y => GetAllChildren1(listPb, y.PhongBanId))
                ).ToList();

            return query.ToList();
        }
    }
}