namespace fse.core.models;

public readonly record struct UpdateFrequencyModel(bool ShouldUpdateSupply, bool ShouldUpdateDelta, Seasons UpdateSeason);