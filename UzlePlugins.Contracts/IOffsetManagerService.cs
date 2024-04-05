using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Contracts;

public interface IOffsetManagerService
{
    OffsetsDto Read();
    void Write(OffsetsDto offsets);
}