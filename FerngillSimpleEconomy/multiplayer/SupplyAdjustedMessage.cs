
namespace fse.core.multiplayer;

public class SupplyAdjustedMessage : IMessage
{
	public const string StaticType = "fse.supply.adjusted.message";
	public string Type => StaticType;
	
	public string ObjectId { get; }
	public int Amount { get; }
	
	public SupplyAdjustedMessage(string objectId, int amount)
	{
		ObjectId = objectId;
		Amount = amount;
	}
}