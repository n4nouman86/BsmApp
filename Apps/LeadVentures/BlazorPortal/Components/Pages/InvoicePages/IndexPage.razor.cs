using BlazorPortal.Entities;
using BlazorPortal.Services;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class IndexPage
{
    private List<vwInvoice>? Invoices { get; set; }
    private IEnumerable<vwdAgent>? Agents { get; set; }
    private decimal UnpaidAmount { get; set; }
    private int UnpaidInvoices { get; set; }
    private int lastWeekBillables { get; set; }
    private int thisWeekBillables { get; set; }




    protected override async Task OnInitializedAsync()
    {
		using var context = contextFactory.CreateDbContext();
		Agents = await context.vwdAgents.ToListAsync();
		Invoices = await context.vwInvoices.OrderByDescending(x => x.CreateOn).Take(50).ToListAsync();

		await LoadDashboard();
        await GetWeeksBillables();
    }




    private async Task LoadDashboard()
	{
		using var context = contextFactory.CreateDbContext();

		UnpaidInvoices = await context.Invoices.Where(x => x.PaidOn == null && x.IsDefault == false).CountAsync();
		UnpaidAmount = await context.Invoices.Where(x => x.PaidOn == null && x.IsDefault == false).SumAsync(x => x.TotalCost);
	}




	private async Task GetWeeksBillables()
    {
		using var context = contextFactory.CreateDbContext();

        DateTime thisWeekDate = DateTime.Now.Date;
        while (thisWeekDate.DayOfWeek != DayOfWeek.Monday)
            thisWeekDate = thisWeekDate.AddDays(-1);

        thisWeekBillables = await context.vwLeads
            .Where(x => x.AgentTransferOnDate >= thisWeekDate && x.IsBillable == true)
            .CountAsync();

        DateTime lastWeekDate = thisWeekDate.AddDays(-7);
        lastWeekBillables = await context.vwLeads
            .Where(x => x.AgentTransferOnDate >= lastWeekDate && x.AgentTransferOnDate < thisWeekDate && x.IsBillable == true)
            .CountAsync();
    }




    private InputModel Input { get; set; } = new();

    private async Task SearchInvoice()
    {
        using var context = contextFactory.CreateDbContext();
        var query = context.vwInvoices
        .Where(x => x.CreateOn.Date >= Input.DateFrom.Date && x.CreateOn.Date <= Input.DateTo.Date && x.IsDefault == false);

        if (Input.AgentId.HasValue)
            query = query.Where(x => x.AgentId == Input.AgentId);

        if (!string.IsNullOrWhiteSpace(Input.PaidInvoice))
        {
            if (Input.PaidInvoice == "Paid") query = query.Where(x => x.PaidOn != null);
            if (Input.PaidInvoice == "Not Paid") query = query.Where(x => x.PaidOn == null);
        }

        Invoices = await query.OrderByDescending(x => x.InvoiceNo).ToListAsync();
    }




    private async Task SearchInvoiceByNo()
    {
        using var context = contextFactory.CreateDbContext();
        Invoices = await context.vwInvoices
            .Where(x => x.InvoiceNo == Input.InvoiceNo && x.IsDefault == false)
            .ToListAsync();
    }




    private async Task GetUnpaidInvoices()
    {
        using var context = contextFactory.CreateDbContext();
        Invoices = await context.vwInvoices
            .Where(x => x.PaidOn == null && x.IsDefault == false)
            .OrderByDescending(x => x.DueDate)
            .ToListAsync();
    }




    private async Task GetLateInvoices()
    {
        using var context = contextFactory.CreateDbContext();
        Invoices = await context.vwInvoices
            .Where(x => x.PaidOn == null && x.DueDate < DateTime.Now && x.IsDefault == false)
            .OrderByDescending(x => x.DueDate)
            .ToListAsync();
    }




    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add Invoice",
            options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

		if (result is Guid createdId)
		{
			using var context = contextFactory.CreateDbContext();

			var record = await context.vwInvoices.FirstOrDefaultAsync(x => x.Id == createdId);

			if (record is not null)
			{
				Invoices?.Insert(0, record);
				Invoices = new List<vwInvoice>(Invoices!);
				await LoadDashboard();
			}
		}
	}




    private async Task OpenEdit(Guid id)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit Invoice",
            new Dictionary<string, object?> { { "InvoiceId", id } },
            new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

		if (result is Guid updatedId)
		{
			using var context = contextFactory.CreateDbContext();

			var record = await context.vwInvoices.FirstOrDefaultAsync(x => x.Id == updatedId);

			if (record is not null)
			{
				var index = Invoices?.FindIndex(x => x.Id == updatedId) ?? -1;

				if (index >= 0)
				{
					Invoices![index] = record;
					Invoices = new List<vwInvoice>(Invoices);

					await LoadDashboard();
				}
			}
		}
	}




    private async Task SaveDefault(vwInvoice invoice)
    {
        var confirmed = await dialogService.OpenAsync<MarkDefaultDialog>(
            "Mark as Default",
            new Dictionary<string, object?> { { "Invoice", invoice } },
            new DialogOptions { Width = "400px", CloseDialogOnOverlayClick = true });

        if (confirmed is not true) return;

        using var context = contextFactory.CreateDbContext();
        var res = context.Invoices.Where(x => x.Id == invoice.Id)
            .ExecuteUpdate(s => s.SetProperty(x => x.IsDefault, true));

        if (res > 0)
            Invoices = Invoices?.Where(x => x.Id != invoice.Id).OrderByDescending(x => x.DueDate).ToList();
    }




    private void CsvExport()
    {
        if (Invoices is not null && Invoices.Any())
        {
            var exportService = new CsvInvoiceExportService();
            exportService.CreateCsv(jsRuntime, Invoices);
        }
    }




    private sealed class InputModel
    {
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public Guid? AgentId { get; set; }
        public string PaidInvoice { get; set; } = string.Empty;
        public int InvoiceNo { get; set; } = 2000;
    }
}
