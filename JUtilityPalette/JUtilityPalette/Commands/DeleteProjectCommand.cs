using JUtilityPalette.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class DeleteProjectCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public DeleteProjectCommand(LibraryStore store, Guid id)
    {
        _store = store;
        _id = id;
        Name = "Delete project row";
    }

    public override CommandResult Invoke()
    {
        _store.DeleteProject(_id);
        return CommandResult.ShowToast("Project row deleted");
    }
}
