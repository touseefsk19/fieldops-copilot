using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Services;

namespace FieldOps.Mobile.ViewModels;

// One line in the chat. Tools is empty for your own messages.
public record ChatLine(string Who, string Text, string Tools);

public partial class AgentViewModel(FieldOpsApi api) : ObservableObject
{
    [ObservableProperty] private string message = "";
    [ObservableProperty] private bool isBusy;

    public ObservableCollection<ChatLine> Lines { get; } = [];

    [RelayCommand]   // → SendCommand
    private async Task SendAsync()
    {
        var text = Message.Trim();
        if (IsBusy || text.Length == 0) return;

        Message = "";                               // clear the box (the Part 1 lesson: old text leaks)
        Lines.Add(new ChatLine("You", text, ""));
        IsBusy = true;
        try
        {
            var reply = await api.SendToAgentAsync(text);
            var tools = reply.ToolsCalled.Count == 0 ? "" : "Tools used: " + string.Join(", ", reply.ToolsCalled);
            Lines.Add(new ChatLine("FieldOps", reply.Reply, tools));
        }
        catch (ApiException ex)
        {
            Lines.Add(new ChatLine("Error", ex.Message, ""));
        }
        finally
        {
            IsBusy = false;
        }
    }
}