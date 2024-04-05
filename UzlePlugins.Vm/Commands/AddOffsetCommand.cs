using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands;

public class AddOffsetCommand : CommandBase
{
    private readonly IOffsetManagerService _offsetManager;
    public AddOffsetCommand(IOffsetManagerService offsetManager)
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