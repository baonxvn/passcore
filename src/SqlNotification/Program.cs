using System;
using System.Data;
using Hpl.HrmDatabase;
using Microsoft.Data.SqlClient;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;


namespace SqlNotification
{
    class Program
    {
        //private static readonly string _con = "Server=54.251.3.45; Database=HPL_ACM; User ID=sa; Password=Zm*3_E}7gaR83+_G";
        private static readonly string _con = "Server=54.251.3.45; Database=HRM_db; User ID=hrm; Password=H@iphat2021";
        //<add key="ConnStr" value="server=54.251.3.45,1433;database=HRM_db;persist security info=True; uid=hrm; pwd=H@iphat2021" />

        static void Main(string[] args)
        {
            var mapper = new ModelToTableMapper<NhanVien>();
            mapper.AddMapping(c => c.HoTen, "HoTen");
            mapper.AddMapping(c => c.Ho, "Ho");

            using (var dep = new SqlTableDependency<NhanVien>(_con, "NhanVien", mapper: mapper))
            {
                dep.OnChanged += Changed;
                dep.Start();

                Console.WriteLine("Press a key to exit");
                Console.ReadKey();

                dep.Stop();
            }
        }

        public static void Changed(object sender, RecordChangedEventArgs<NhanVien> e)
        {
            var changedEntity = e.Entity;

            Console.WriteLine("DML operation: " + e.ChangeType);
            Console.WriteLine("ID: " + changedEntity.NhanVienId);
            Console.WriteLine("Name: " + changedEntity.HoTen);
            Console.WriteLine("Surname: " + changedEntity.Ho);

            if (e.ChangeType == ChangeType.Update)
            {
                //TODO

            }
        }
    }
}
