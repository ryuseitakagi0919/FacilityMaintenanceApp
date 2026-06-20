namespace FacilityMaintenanceApp.Models
{
    // エラー画面で表示する情報を管理するViewModel。
    // 予期しないエラーが発生した際に、リクエストIDを画面へ渡すために使用する。
    public class ErrorViewModel
    {
        // エラーが発生したリクエストを識別するためのID。
        // 開発時やログ確認時に、どのリクエストでエラーが起きたかを追跡しやすくする。
        public string? RequestId { get; set; }

        // RequestIdが存在する場合のみ、画面に表示するための判定プロパティ。
        // nullや空文字の場合は、不要な表示を出さないようにする。
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}