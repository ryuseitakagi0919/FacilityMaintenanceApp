using System.ComponentModel.DataAnnotations;

namespace FacilityMaintenanceApp.Models
{
    // 点検履歴を表すModelクラス。
    // Inspectionsテーブルに対応し、設備ごとの点検結果を管理する。
    public class Inspection
    {
        // 点検履歴を一意に識別するID。
        // Entity Framework Coreにより主キーとして扱われる。
        public int Id { get; set; }

        // 点検対象となる設備のID。
        // Equipmentテーブルと紐づけるための外部キー。
        [Display(Name = "設備ID")]
        public int EquipmentId { get; set; }

        // 紐づく設備情報。
        // 設備詳細画面で、設備と点検履歴を関連付けて表示するために使用する。
        public Equipment? Equipment { get; set; }

        // 点検を実施した日付。
        // 点検履歴として必須の情報のため、必須入力にしている。
        [Required(ErrorMessage = "点検日は必須です")]
        [DataType(DataType.Date)]
        [Display(Name = "点検日")]
        public DateTime InspectionDate { get; set; } = DateTime.Today;

        // 点検結果。
        // 「異常なし」「要対応」「故障」などを管理し、設備の状態更新にも利用する。
        [Required(ErrorMessage = "点検結果は必須です")]
        [StringLength(20)]
        [Display(Name = "点検結果")]
        public string Result { get; set; } = "異常なし";

        // 点検を実施した担当者名。
        // 誰が点検したかを履歴として残すため、必須入力にしている。
        [Required(ErrorMessage = "点検者は必須です")]
        [StringLength(50, ErrorMessage = "点検者は50文字以内で入力してください")]
        [Display(Name = "点検者")]
        public string Inspector { get; set; } = string.Empty;

        // 点検時の補足コメント。
        // 状況や対応内容を簡単に記録できるようにする。
        [StringLength(200, ErrorMessage = "コメントは200文字以内で入力してください")]
        [Display(Name = "コメント")]
        public string? Comment { get; set; }

        // 次回点検予定日。
        // 点検登録時に入力された場合、設備情報側の次回点検日にも反映する。
        [DataType(DataType.Date)]
        [Display(Name = "次回点検予定日")]
        public DateTime? NextInspectionDate { get; set; }
    }
}