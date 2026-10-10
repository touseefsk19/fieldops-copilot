using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Models;

namespace FieldOps.Mobile.ViewModels;

// One row in the list: knows whether IT can be approved, and which command to run
public class RequestItem(RequestDto dto, bool canApprove, IAsyncRelayCommand<RequestItem> approve)
{
    public RequestDto Dto { get; } = dto;
    public string Title => Dto.Title;
    public string Status => Dto.Status;

    // SQL gives back a DateTime with no "Z", so mark it as UTC before converting to local time
    public string Subtitle =>
        $"#{Dto.Id} · {Dto.Equipment} · {DateTime.SpecifyKind(Dto.CreatedAtUtc, DateTimeKind.Utc).ToLocalTime():dd MMM HH:mm}";

    public bool CanApprove { get; } = canApprove;
    public IAsyncRelayCommand<RequestItem> ApproveCommand { get; } = approve;
}