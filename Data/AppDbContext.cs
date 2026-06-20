using FacilityMaintenanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacilityMaintenanceApp.Data
{
    // Entity Framework Coreでデータベース操作を行うためのクラス。
    // ModelクラスとSQL Serverのテーブルを紐づける役割を持つ。
    public class AppDbContext : DbContext
    {
        // Program.csで設定した接続情報を受け取り、DbContextを初期化する。
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 設備情報を管理するテーブル。
        // Equipmentモデルに対応し、設備の登録・編集・削除・検索で使用する。
        public DbSet<Equipment> Equipments { get; set; }

        // 点検履歴を管理するテーブル。
        // Inspectionモデルに対応し、設備ごとの点検結果や次回点検日を保存する。
        public DbSet<Inspection> Inspections { get; set; }
    }
}