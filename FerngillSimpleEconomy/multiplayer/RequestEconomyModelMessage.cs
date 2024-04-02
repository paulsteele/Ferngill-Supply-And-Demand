namespace fse.core.multiplayer;

public class RequestEconomyModelMessage : IMessage
{
	public const string StaticType = "fse.economy.model.request.message";
	public string Type => StaticType;
}