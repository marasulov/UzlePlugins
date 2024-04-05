using System;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Settings
{
    public class OffsetManagerService: IOffsetManagerService
    {
        private readonly SettingsReader<OffsetsDto> _reader;
        public string _filename;


        public OffsetManagerService()
        {
            _reader = new SettingsReader<OffsetsDto>();
            _filename = "Offset.json";
        }

        public OffsetsDto Read()
        {
            return  _reader.Read(_filename);
        }

        public void Write(OffsetsDto offsets)
        {
            _reader.WriteData(offsets, _filename);
        }
    }
}
