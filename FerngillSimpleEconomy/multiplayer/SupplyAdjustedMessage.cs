
namespace fse.core.multiplayer;

public class SupplyAdjustedMessage(string objectId, int amount) : IMessage
{
	public const string StaticType = "fse.supply.adjusted.message";
	public string Type => StaticType;
	
	public string ObjectId { get; } = objectId;
	public int Amount { get; } = amount;
}