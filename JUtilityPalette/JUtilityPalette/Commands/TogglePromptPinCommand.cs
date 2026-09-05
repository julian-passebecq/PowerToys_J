using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class TogglePromptPinCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly PromptEntry _prompt;

    public TogglePromptPinCommand(LibraryStore store, PromptEntry prompt)
    {
        _store = store;
        _prompt = prompt;
        Name = prompt.IsPinned ? "Unpin" : "Pin";
    }

    public override CommandResult Invoke()
    {
        bool wasPinned = _prompt.IsPinned;
        _store.TogglePromptPinned(_prompt.Id);
        return CommandResult.ShowToast(wasPinned ? "Prompt unpinned" : "Prompt pinned");
    }
}
