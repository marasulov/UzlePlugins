using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Vm.Base;
using UzlePlugins.Vm.Commands;

namespace UzlePlugins.Vm
{
    public class HolesVm : BaseViewModel
    {
        private List<ActualHoleDto> _actualHoles;
        private List<OutdatedFamilyDto> _outdatedHoles;
        private List<NewHoleDto> _newHoles;
        private List<NewHoleDto> _newHolesRect;

        private bool _canExecute = true;
        private string[] _holeFigureTypes;
        private bool _isAllActualHoleSelected;
        private bool _isAllNewHoleSelected = true;
        private bool _isAllNewRectHoleSelected = true;
        private bool _isAllOutdatedHoleSelected;
        private ObservableCollection<OffsetRanges> _ductOffsets;
        private ObservableCollection<OffsetRanges> _pipeOffsets;
        private ObservableCollection<string> _intersectingType;
        private string _selectedType = "Ducts";
        private ObservableCollection<OffsetRanges> _selectedOffsets ;
        private List<NewHoleDto> _newHolesCircle;


        public HolesVm (
            FindHolesCommand findHolesCommand,
            CreateHolesCommand createHolesCommand,
            ZoomToPointCommand zoomToPointCommand,
            FillOffsetSettingsCommand fillOffsetSettingsCommand, 
            SaveOffsetsCommand savetoJson)
        {
            HoleFigureTypes = new[] { "Square", "Circle" };
            IntersectingType = new ObservableCollection<string> { "Ducts", "Pipes", "Cable trays" };

            FindHolesCommand = findHolesCommand;
            findHolesCommand.ResultObtained += FindHolesCommand_ResultObtained;

            FillOffsetSettingsCommand = fillOffsetSettingsCommand;
            
            fillOffsetSettingsCommand.ResultObtained += FillOffsetSettingsCommand_ResultObtained;

            CreateHolesCommand = createHolesCommand;
            ZoomToPointCommand = zoomToPointCommand;
            UpdateSelectedOffsets();
            
            AddOffsetCommand =  new RelayCommand(AddToList);
            DeleteOffsetCommand = new RelayCommand(DeleteFromList);
            SaveOffsetsCommand = savetoJson;

        }

        private void DeleteFromList(object obj)
        {
            switch (SelectedType)
            {
                case "Ducts":
                {
                    var selected = SelectedOffsets.Where(s => s.IsDelete).ToList();
                    foreach (var offset in selected)
                    {
                        DuctOffsets.Remove(offset);
                    }

                    break;
                }
                case "Pipes":
                {
                    var selected = SelectedOffsets.Where(s => s.IsDelete).ToList();
                    foreach (var offset in selected)
                    {
                        PipeOffsets.Remove(offset);
                    }

                    break;
                }
            }
        }

        public RelayCommand DeleteOffsetCommand { get; set; }

        private void AddToList(object obj)
        {
            switch (SelectedType)
            {
                case "Ducts":
                    DuctOffsets.Add(new OffsetRanges(NewFrom, NewTo, NewOffset));
                    break;
                case "Pipes":
                    PipeOffsets.Add(new OffsetRanges(NewFrom, NewTo, NewOffset));
                    break;
            }
           
            NewFrom = default;
            NewTo = default;
            NewOffset = default;

            OnPropertyChanged(nameof(NewFrom));
            OnPropertyChanged(nameof(NewTo));
            OnPropertyChanged(nameof(NewOffset));

        }

        public RelayCommand AddOffsetCommand { get; set; }

        public bool IsAllNewHoleSelected
        {
            get => _isAllNewHoleSelected;
            set
            {
                _isAllNewHoleSelected = value;
                OnPropertyChanged();
                NewHoles.ForEach(x => x.IsInsert = IsAllNewHoleSelected);
            }
        }

        public bool IsAllNewRectHoleSelected {  
            get => _isAllNewRectHoleSelected;
            set
            {
                _isAllNewRectHoleSelected = value;
                OnPropertyChanged();
                NewHolesRect.ForEach(x => x.IsInsert = IsAllNewHoleSelected);
            } }
        
        public bool IsAllActualHoleSelected
        {
            get => _isAllActualHoleSelected;
            set
            {
                _isAllActualHoleSelected = value;
                OnPropertyChanged();
                ActualHoles.ForEach(x => x.IsDelete = IsAllActualHoleSelected);
            }
        } 

        public bool IsAllOutdatedHoleSelected
        {
            get => _isAllOutdatedHoleSelected;
            set
            {
                _isAllOutdatedHoleSelected = value;
                OnPropertyChanged();
                OutdatedHoles.ForEach(x => x.IsDelete = IsAllOutdatedHoleSelected);
            }
        }

        private void FillOffsetSettingsCommand_ResultObtained(OffsetsDto obj)
        {
            DuctOffsets = obj.Duct;
            PipeOffsets = obj.Pipe;
        }

        private void FindHolesCommand_ResultObtained(AllHolesDto obj)
        {
            ActualHoles = obj.ActualFamiliesDtos;
            NewHoles = obj.NewFamiliesDtos;
            OutdatedHoles = obj.OutdatedFamiliesDtos;
        }

        public string[] HoleFigureTypes
        {
            get => _holeFigureTypes;
            set => Set(ref _holeFigureTypes, value);
        }

        public List<NewHoleDto> NewHoles
        {
            get => _newHoles;
            set
            {
                Set(ref _newHoles, value);
                UpdateFilteredLists();
            }
        }

        private void UpdateFilteredLists()
        {
            
            NewHolesCircle?.Clear();
            NewHolesRect?.Clear();

            foreach (var hole in NewHoles)
            {
                switch (hole.Intersection.IntersectingElementShape)
                {
                    case "Round":
                        NewHolesCircle.Add(hole);
                        break;
                    case "Rectangular":
                        NewHolesRect.Add(hole);
                        break;
                }
            }
        }
        public List<NewHoleDto> NewHolesCircle
        {
            get => _newHolesCircle ?? (_newHolesCircle = new List<NewHoleDto>());
            set => Set(ref _newHolesCircle, value);
        }

        public List<NewHoleDto> NewHolesRect
        {
            get => _newHolesRect ?? (_newHolesRect = new List<NewHoleDto>());
            set => Set(ref _newHolesRect, value);
        }

        public List<ActualHoleDto> ActualHoles
        {
            get => _actualHoles;
            set => Set(ref _actualHoles, value);
        }

        public List<OutdatedFamilyDto> OutdatedHoles
        {
            get => _outdatedHoles;
            set => Set(ref _outdatedHoles, value);
        }

        public ObservableCollection<OffsetRanges> DuctOffsets
        {
            get => _ductOffsets;
            set => Set(ref _ductOffsets, value);
        }

        public ObservableCollection<OffsetRanges> PipeOffsets
        {
            get => _pipeOffsets;
            set => Set(ref _pipeOffsets, value);
        }

        public Action CloseAction { get; set; }

        public FillOffsetSettingsCommand FillOffsetSettingsCommand { get; }
        public FindHolesCommand FindHolesCommand { get; }

        public CreateHolesCommand CreateHolesCommand { get; }

        public ZoomToPointCommand ZoomToPointCommand { get; }

        public ObservableCollection<string> IntersectingType
        {
            get=>_intersectingType;
            set => Set(ref _intersectingType, value);
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
                UpdateSelectedOffsets();
            }
        }

        private void UpdateSelectedOffsets()
        {
            
            SelectedOffsets = SelectedType switch
            {
                "Ducts" => DuctOffsets,
                "Pipes" => PipeOffsets,
                _ => SelectedOffsets
            };
        }

        public ObservableCollection<OffsetRanges> SelectedOffsets
        {
            get => _selectedOffsets;
            set => Set(ref _selectedOffsets, value);
        }

        public int NewFrom { get; set; }
        public int NewTo { get; set; }
        public int NewOffset { get; set; }
        public SaveOffsetsCommand SaveOffsetsCommand { get; }

    }
}
