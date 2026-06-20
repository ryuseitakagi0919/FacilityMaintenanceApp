using FacilityMaintenanceApp.Models;

namespace FacilityMaintenanceApp.ViewModels
{
    // ダッシュボード画面に表示する情報をまとめるViewModel。
    // Controllerで集計した設備件数や注意が必要な設備一覧をViewへ渡すために使用する。
    public class DashboardViewModel
    {
        // 登録されている設備の総数。
        public int TotalCount { get; set; }

        // 状態が「正常」の設備件数。
        public int NormalCount { get; set; }

        // 状態が「要対応」の設備件数。
        public int WarningCount { get; set; }

        // 状態が「故障」の設備件数。
        public int FailureCount { get; set; }

        // 状態が「対応中」の設備件数。
        public int InProgressCount { get; set; }

        // 次回点検日を過ぎている設備件数。
        // 優先的に確認すべき設備数としてダッシュボードに表示する。
        public int OverdueCount { get; set; }

        // 今日から7日以内に点検予定がある設備件数。
        // 近日中に対応が必要な設備数としてダッシュボードに表示する。
        public int DueSoonCount { get; set; }

        // 注意が必要な設備一覧。
        // 「要対応」「故障」「期限超過」「7日以内の点検予定」の設備を表示するために使用する。
        public List<Equipment> AttentionEquipments { get; set; } = new();
    }
}