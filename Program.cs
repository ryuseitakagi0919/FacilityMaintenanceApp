using FacilityMaintenanceApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQL Server LocalDBを使用するため、AppDbContextをDIコンテナに登録する。
// 接続文字列はappsettings.jsonのDefaultConnectionから取得する。
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVCのControllerとViewを使用するためのサービスを登録する。
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 本番環境でエラーが発生した場合は、共通エラー画面へ遷移する。
// 開発環境では詳細なエラー情報を表示し、原因を確認しやすくする。
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // HTTPS通信をブラウザに強制するための設定。
    app.UseHsts();
}

// HTTPアクセスをHTTPSへリダイレクトする。
app.UseHttpsRedirection();

// wwwroot配下のCSS、JavaScript、画像などの静的ファイルを使用できるようにする。
app.UseStaticFiles();

// URLに応じて、どのController・Actionを実行するかを判断する。
app.UseRouting();

// 認可処理を有効にする。
// 現時点ではログイン機能はないが、将来的な権限管理追加を見据えて残している。
app.UseAuthorization();

// デフォルトのルーティング設定。
// URL指定がない場合は、HomeControllerのIndexアクションを表示する。
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// アプリケーションを起動する。
app.Run();