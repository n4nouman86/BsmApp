using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BlazorPortal.Components.Pages.InvoicePages;

public partial class MarkDefaultDialog
{
    [Parameter] public vwInvoice Invoice { get; set; } = default!;
    [Parameter] public string ConfirmText { get; set; } = "Are you sure you want to mark this invoice as default?";
    [Parameter] public string ActionLabel { get; set; } = "Yes, Mark";
    [Parameter] public ButtonStyle ActionStyle { get; set; } = ButtonStyle.Danger;
}
