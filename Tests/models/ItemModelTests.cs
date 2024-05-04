using fse.core.models;

namespace Tests.models;

public class ItemModelTests : HarmonyTestBase
{
	private ItemModel _itemModel;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		ConfigModel.Instance = new ConfigModel
		{
			MaxCalculatedSupply = 200,
			MinDelta = -100,
			MaxDelta = 20,
			MinPercentage = 0.2f,
			MaxPercentage = 0.8f,
		};

		_itemModel = new ItemModel
		{
			ObjectId = "item1",
			Supply = 100,
			DailyDelta = 10
		};
	}

	[Test]
	public void TestAdvanceOneDay()
	{
		_itemModel.AdvanceOneDay();
		Assert.That(_itemModel.Supply, Is.EqualTo(110));

		_itemModel.DailyDelta = -20;
		
		_itemModel.AdvanceOneDay();
		Assert.That(_itemModel.Supply, Is.EqualTo(90));
	}

	[Test]
	public void TestUpdateMultiplier()
	{
		_itemModel.UpdateMultiplier();
		Assert.That(_itemModel.GetPrice(100), Is.EqualTo(50));
		_itemModel.Supply = 50;
		Assert.That(_itemModel.GetPrice(100), Is.EqualTo(50));
		_itemModel.UpdateMultiplier();
		Assert.That(_itemModel.GetPrice(100), Is.EqualTo(65));
	}

	[Test]
	public void TestCapSupply()
	{
		_itemModel.Supply = 250;
		_itemModel.CapSupply();
		Assert.That(_itemModel.Supply, Is.EqualTo(200));
	}

	[Test]
	public void TestGetObjectInstance()
	{
		var obj = _itemModel.GetObjectInstance();
		Assert.That(obj.ItemId, Is.EqualTo(_itemModel.ObjectId));
	}
}