using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.EmployeePages;

public partial class IndexPage
{
    private List<Employee>? Employees { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Employees = await db.Employees.OrderBy(x => x.Name).ToListAsync();
    }

    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add Employee",
            options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "500px" });

        if (result is int createdId)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var record = await db.Employees.FindAsync(createdId);

            if (record is not null)
            {
                Employees?.Add(record);
                Employees = Employees is null ? null : new List<Employee>(Employees.OrderBy(x => x.Name));
            }
        }
    }

    private async Task OpenEdit(int id)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit Employee",
            new Dictionary<string, object?> { { "Id", id } },
            new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "500px" });

        if (result is int updatedId)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var record = await db.Employees.FindAsync(updatedId);

            if (record is not null)
            {
                var index = Employees?.FindIndex(x => x.Id == updatedId) ?? -1;
                if (index >= 0)
                {
                    Employees![index] = record;
                    Employees = new List<Employee>(Employees.OrderBy(x => x.Name));
                }
            }
        }
    }
}
