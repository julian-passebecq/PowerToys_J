using JUtilityPalette.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class DeletePromptCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public DeletePromptCommand(LibraryStore store, Guid id)
    {
        _store = store;
        _id = id;
        Name = "Delete";
    }

    public override CommandResult Invoke()
    {
        _store.DeletePrompt(_id);
        return CommandResult.ShowToast("Prompt entry deleted");
    }
}
