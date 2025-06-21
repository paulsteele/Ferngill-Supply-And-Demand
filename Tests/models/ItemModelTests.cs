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
			MinPercentage = 0.2m,
			MaxPercentage = 0.8m,
		};

		_itemModel = new ItemModel("item1")
		{
			Supply = 100,
			DailyDelta = 10,
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

	private static IEnumerable<TestCaseData> TestGetPriceSource()
	{
		yield return new TestCaseData(.3m, 1.3m, 1000, 750, 100).Returns(55m);
		yield return new TestCaseData(.3m, 1.3m, 1000, 250, 100).Returns(105m);
		yield return new TestCaseData(.2m, 5m, 1000, 500, 100).Returns(260m);
		yield return new TestCaseData(6.5m, 7m, 1000, 1000, 100).Returns(650m);
		yield return new TestCaseData(3.5m, 5m, 1000, 0, 100).Returns(500m);
	}
	
	[TestCaseSource(nameof(TestGetPriceSource))]
	public decimal TestGetPrice(
		decimal minMultiplier,
		decimal maxMultiplier,
		int maxCalculatedSupply,
		int supply,
		int basePrice
	)
	{
		ConfigModel.Instance.MinPercentage = minMultiplier;
		ConfigModel.Instance.MaxPercentage = maxMultiplier;
		ConfigModel.Instance.MaxCalculatedSupply = maxCalculatedSupply;
		_itemModel.Supply = supply;
		
		_itemModel.UpdateMultiplier();
		return _itemModel.GetPrice(basePrice);
		
	}
}