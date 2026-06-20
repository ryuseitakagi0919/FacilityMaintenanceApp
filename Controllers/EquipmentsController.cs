using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacilityMaintenanceApp.Models;
using FacilityMaintenanceApp.Data;

public class EquipmentsController : Controller
{
    // データベース操作を行うためのDbContext。
    // EquipmentsテーブルやInspectionsテーブルにアクセスする。
    private readonly AppDbContext _context;


    public EquipmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Equipments
    // 設備一覧画面を表示する。
    // キーワード、状態、点検期限の条件を受け取り、該当する設備だけを絞り込む。
    public async Task<IActionResult> Index(string? keyword, string? status, string? inspectionFilter)
    {
        var today = DateTime.Today;
        var soon = today.AddDays(7);

        // AsQueryableを使うことで、検索条件に応じてWhere句を後から追加できるようにしている。
        var equipments = _context.Equipments.AsQueryable();

        // キーワードが入力されている場合、設備名・設置場所・設備種別・状態を対象に検索する。
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            equipments = equipments.Where(e =>
                e.Name.Contains(keyword) ||
                e.Location.Contains(keyword) ||
                e.Category.Contains(keyword) ||
                e.Status.Contains(keyword));
        }

        // 状態が選択されている場合、「正常」「要対応」「故障」「対応中」などで絞り込む。
        if (!string.IsNullOrWhiteSpace(status))
        {
            equipments = equipments.Where(e => e.Status == status);
        }

        // 点検期限が過ぎている設備のみを表示する。
        // 施設管理では、期限超過設備を早めに把握することが重要なため。
        if (inspectionFilter == "overdue")
        {
            equipments = equipments.Where(e =>
                e.NextInspectionDate.HasValue &&
                e.NextInspectionDate.Value < today);
        }
        // 7日以内に点検予定がある設備のみを表示する。
        // 近日中に対応が必要な設備を見つけやすくするため。
        else if (inspectionFilter == "soon")
        {
            equipments = equipments.Where(e =>
                e.NextInspectionDate.HasValue &&
                e.NextInspectionDate.Value >= today &&
                e.NextInspectionDate.Value <= soon);
        }

        // 検索後も入力内容を画面に残すため、ViewDataに検索条件を渡す。
        ViewData["CurrentKeyword"] = keyword;
        ViewData["CurrentStatus"] = status;
        ViewData["CurrentInspectionFilter"] = inspectionFilter;

        // 次回点検日が近い順に並べて、対応が必要な設備を確認しやすくする。
        return View(await equipments
            .OrderBy(e => e.NextInspectionDate)
            .ToListAsync());
    }

    // GET: Equipments/Details/5
    // 設備の詳細情報を表示する。
    // Includeを使って、設備に紐づく点検履歴も一緒に取得する。
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipment = await _context.Equipments
            .Include(e => e.Inspections)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (equipment == null)
        {
            return NotFound();
        }

        // 点検履歴は新しいものから確認できるよう、点検日の降順で並べ替える。
        equipment.Inspections = equipment.Inspections
            .OrderByDescending(i => i.InspectionDate)
            .ToList();

        return View(equipment);
    }

    // GET: Equipments/CreateInspection
    // 指定された設備に対して、点検履歴登録画面を表示する。
    public async Task<IActionResult> CreateInspection(int equipmentId)
    {
        var equipment = await _context.Equipments.FindAsync(equipmentId);

        if (equipment == null)
        {
            return NotFound();
        }

        // 登録画面で対象設備名を表示するため、ViewDataに渡す。
        ViewData["EquipmentName"] = equipment.Name;

        // 点検日は初期値として今日の日付を設定する。
        var inspection = new Inspection
        {
            EquipmentId = equipmentId,
            InspectionDate = DateTime.Today
        };

        return View(inspection);
    }

    // POST: Equipments/CreateInspection
    // 点検履歴を登録する。
    // 点検結果に応じて、設備側の現在状態や次回点検日も更新する。
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInspection(
        [Bind("EquipmentId,InspectionDate,Result,Inspector,Comment,NextInspectionDate")] Inspection inspection)
    {
        if (ModelState.IsValid)
        {
            // 点検履歴をInspectionsテーブルに追加する。
            _context.Inspections.Add(inspection);

            // 点検履歴の登録対象となる設備を取得する。
            var equipment = await _context.Equipments.FindAsync(inspection.EquipmentId);

            if (equipment != null)
            {
                // 点検結果に応じて、設備一覧やダッシュボードに表示される現在状態を更新する。
                // 履歴を残すだけでなく、最新の設備状態にも反映させるため。
                if (inspection.Result == "異常なし")
                {
                    equipment.Status = "正常";
                }
                else if (inspection.Result == "要対応")
                {
                    equipment.Status = "要対応";
                }
                else if (inspection.Result == "故障")
                {
                    equipment.Status = "故障";
                }

                // 点検時に次回点検予定日が入力された場合は、設備情報にも反映する。
                if (inspection.NextInspectionDate.HasValue)
                {
                    equipment.NextInspectionDate = inspection.NextInspectionDate;
                }
            }

            await _context.SaveChangesAsync();

            // 登録後は、対象設備の詳細画面へ戻り、点検履歴を確認できるようにする。
            return RedirectToAction(nameof(Details), new { id = inspection.EquipmentId });
        }

        return View(inspection);
    }

    // GET: Equipments/Create
    // 設備の新規登録画面を表示する。
    public IActionResult Create()
    {
        return View();
    }

    // POST: Equipments/Create
    // 入力された設備情報をデータベースに登録する。
    // Bindで登録対象のプロパティを明示し、不要な項目が更新されないようにする。
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Location,Category,Status,NextInspectionDate,Memo")] Equipment equipment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(equipment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    // GET: Equipments/Edit/5
    // 指定された設備の編集画面を表示する。
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipment = await _context.Equipments.FindAsync(id);

        if (equipment == null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    // POST: Equipments/Edit/5
    // 編集画面で入力された内容をデータベースに反映する。
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Location,Category,Status,NextInspectionDate,Memo")] Equipment equipment)
    {
        // URLのIDとフォームから送信された設備IDが一致しない場合は不正なアクセスとして扱う。
        if (id != equipment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(equipment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 更新対象の設備が存在しない場合は404を返す。
                // 存在する場合は別のDB更新エラーとして再スローする。
                if (!EquipmentExists(equipment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    // GET: Equipments/Delete/5
    // 削除確認画面を表示する。
    // いきなり削除せず、確認画面を挟むことで誤操作を防ぐ。
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipment = await _context.Equipments
            .FirstOrDefaultAsync(m => m.Id == id);

        if (equipment == null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    // POST: Equipments/Delete/5
    // 削除確認後、対象設備をデータベースから削除する。
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var equipment = await _context.Equipments.FindAsync(id);

        if (equipment != null)
        {
            _context.Equipments.Remove(equipment);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // 指定されたIDの設備が存在するか確認する。
    // 編集処理で、更新対象が削除済みではないかを判定するために使用する。
    private bool EquipmentExists(int? id)
    {
        return _context.Equipments.Any(e => e.Id == id);
    }

}
