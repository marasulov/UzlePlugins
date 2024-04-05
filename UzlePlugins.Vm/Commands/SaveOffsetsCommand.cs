using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands;

public class SaveOffsetsCommand : CommandBase
{
    private readonly IOffsetManagerService _offsetManager;

    public SaveOffsetsCommand(IOffsetManagerService offsetManager)
    {
        _offsetManager = offsetManager;
    }

    public override void Execute(object parameter)
    {
        var obj = parameter as HolesVm;
        
        var ofssetsDto = new OffsetsDto(obj.DuctOffsets,obj.PipeOffsets);
        _offsetManager.Write(ofssetsDto);
    }
}