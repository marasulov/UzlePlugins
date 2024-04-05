using System;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands
{
    /// <summary>
    /// Команда для поиска отверстий.
    /// </summary>
    public class FindHolesCommand : CommandBase
    {
        private readonly IFindHoleService _findHoleService;

        /// <summary>
        /// Создает новый экземпляр класса FindHolesCommand.
        /// </summary>
        /// <param name="findHoleService">Сервис поиска отверстий.</param>
        public FindHolesCommand(IFindHoleService findHoleService)
        {
            _findHoleService = findHoleService;
        }


        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Событие, возникающее при получении результата поиска отверстий.
        /// </summary>
        public event Action<AllHolesDto> ResultObtained;

        /// <summary>
        /// Выполняет команду поиска отверстий.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        public override void Execute(object parameter)
        {
            var res = _findHoleService.FindHoles();

            ResultObtained?.Invoke(res);
        }
    }
}
