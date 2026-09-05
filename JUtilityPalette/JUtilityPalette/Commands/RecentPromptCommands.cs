using JUtilityPalette.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class DeleteRecentPromptCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public DeleteRecentPromptCommand(LibraryStore store, Guid id)
    {
        _store = store;
        _id = id;
        Name = "Remove from history";
    }

    public override CommandResult Invoke()
    {
        _store.DeleteRecentPrompt(_id);
        return CommandResult.ShowToast("Removed from recent prompts");
    }
}
