using fse.core.models;

namespace fse.core.multiplayer;

public class EconomyModelMessage
{
	public const string Type = "fse.economy.model.message";
	public EconomyModel Model { get; }
	
	public EconomyModelMessage(EconomyModel model) => Model = model;
}