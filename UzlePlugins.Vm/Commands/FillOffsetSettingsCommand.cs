using System;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands
{
    public class FillOffsetSettingsCommand : CommandBase
    {
        private readonly IOffsetManagerService _offsetManager;

        public FillOffsetSettingsCommand(IOffsetManagerService offsetManager)
        {
            _offsetManager = offsetManager;
        }

        public event Action<OffsetsDto> ResultObtained;

        public override void Execute(object parameter)
        {
            var res = _offsetManager.Read();
            ResultObtained?.Invoke(res);
        }
    }
}
