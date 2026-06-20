using System.ComponentModel.DataAnnotations;

namespace FacilityMaintenanceApp.Models
{
    // 設備情報を表すModelクラス。
    // Equipmentsテーブルに対応し、設備一覧・登録・編集・詳細表示で使用する。
    public class Equipment
    {
        // 設備を一意に識別するID。
        // Entity Framework Coreにより主キーとして扱われる。
        public int Id { get; set; }

        // 設備名。
        // 業務上必ず必要な情報のため必須入力とし、文字数も制限している。
        [Required(ErrorMessage = "設備名は必須です")]
        [StringLength(50, ErrorMessage = "設備名は50文字以内で入力してください")]
        [Display(Name = "設備名")]
        public string Name { get; set; } = string.Empty;

        // 設備が設置されている場所。
        // どこにある設備かを一覧や詳細画面で確認できるようにする。
        [Required(ErrorMessage = "設置場所は必須です")]
        [StringLength(50, ErrorMessage = "設置場所は50文字以内で入力してください")]
        [Display(Name = "設置場所")]
        public string Location { get; set; } = string.Empty;

        // 空調・照明・防災などの設備種別。
        // 検索や分類に使うため必須入力としている。
        [Required(ErrorMessage = "設備種別は必須です")]
        [StringLength(30, ErrorMessage = "設備種別は30文字以内で入力してください")]
        [Display(Name = "設備種別")]
        public string Category { get; set; } = string.Empty;

        // 設備の現在状態。
        // 「正常」「要対応」「故障」「対応中」などを管理し、一覧やダッシュボードで使用する。
        [Required(ErrorMessage = "状態は必須です")]
        [StringLength(20)]
        [Display(Name = "状態")]
        public string Status { get; set; } = "正常";

        // 次回点検予定日。
        // 期限超過や7日以内の点検予定を判定するために使用する。
        [DataType(DataType.Date)]
        [Display(Name = "次回点検日")]
        public DateTime? NextInspectionDate { get; set; }

        // 補足情報。
        // 長文になりすぎないよう、200文字以内に制限している。
        [StringLength(200, ErrorMessage = "メモは200文字以内で入力してください")]
        [Display(Name = "メモ")]
        public string? Memo { get; set; }

        // この設備に紐づく点検履歴の一覧。
        // 1つの設備に対して複数の点検履歴を持てるようにするためのナビゲーションプロパティ。
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}