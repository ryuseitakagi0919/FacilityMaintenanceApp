using System.Diagnostics;
using FacilityMaintenanceApp.Data;
using FacilityMaintenanceApp.Models;
using FacilityMaintenanceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacilityMaintenanceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // ダッシュボードで設備情報を集計するため、DbContextを使用する。
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index
        // ダッシュボード画面を表示する。
        // 設備の登録数、状態別件数、点検期限の状況を集計してViewに渡す。
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var soon = today.AddDays(7);

            // ダッシュボード表示用のViewModelを作成する。
            // Controller側で集計した値をまとめてViewに渡すために使用する。
            var viewModel = new DashboardViewModel
            {
                // 登録されている設備の総数を取得する。
                TotalCount = await _context.Equipments.CountAsync(),

                // 設備状態ごとの件数を集計する。
                // 現在の設備状況をダッシュボードで一目で把握できるようにするため。
                NormalCount = await _context.Equipments.CountAsync(e => e.Status == "正常"),
                WarningCount = await _context.Equipments.CountAsync(e => e.Status == "要対応"),
                FailureCount = await _context.Equipments.CountAsync(e => e.Status == "故障"),
                InProgressCount = await _context.Equipments.CountAsync(e => e.Status == "対応中"),

                // 次回点検日が今日より前の設備を「期限超過」として集計する。
                // 施設管理では、点検期限を過ぎた設備を優先的に確認する必要があるため。
                OverdueCount = await _context.Equipments.CountAsync(e =>
                    e.NextInspectionDate.HasValue &&
                    e.NextInspectionDate.Value < today),

                // 今日から7日以内に点検予定がある設備を集計する。
                // 近日中に対応が必要な設備を把握しやすくするため。
                DueSoonCount = await _context.Equipments.CountAsync(e =>
                    e.NextInspectionDate.HasValue &&
                    e.NextInspectionDate.Value >= today &&
                    e.NextInspectionDate.Value <= soon),

                // 注意が必要な設備を一覧表示するために取得する。
                // 「要対応」「故障」「期限超過」「7日以内の点検予定」を対象にしている。
                AttentionEquipments = await _context.Equipments
                    .Where(e =>
                        e.Status == "要対応" ||
                        e.Status == "故障" ||
                        e.NextInspectionDate.HasValue && e.NextInspectionDate.Value < today ||
                        e.NextInspectionDate.HasValue && e.NextInspectionDate.Value >= today && e.NextInspectionDate.Value <= soon)
                    .OrderBy(e => e.NextInspectionDate)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // このアプリについてページを表示する。
        // 作成目的、使用技術、主な機能、工夫点などを掲載するページ。
        public IActionResult Privacy()
        {
            return View();
        }

        // エラー発生時にエラー画面を表示する。
        // ResponseCacheを無効化し、古いエラー情報が表示されないようにする。
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}