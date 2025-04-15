using fse.core.models;

namespace fse.core.multiplayer;

public class EconomyModelMessage : IMessage
{
	public const string StaticType = "fse.economy.model.message";
	public string Type => StaticType;
	public EconomyModel Model { get; }
	
	public EconomyModelMessage(EconomyModel model) => Model = model;
}