using fse.core.models;
using StardewValley;
using Object = StardewValley.Object;

namespace Tests.models;

[TestFixture]
public class EconomyModelTests : HarmonyTestBase
{
	private EconomyModel _economyModel;
	
	private ItemModel _itemModel1;
	private ItemModel _itemModel2;
	private ItemModel _itemModel3;
	private ItemModel _itemModel4;
	private ItemModel _itemModel5;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		_economyModel = new EconomyModel();
		
		_itemModel1 = new ItemModel() {ObjectId = "o1"};
		_itemModel2 = new ItemModel() {ObjectId = "o2"};
		_itemModel3 = new ItemModel() {ObjectId = "o3"};
		_itemModel4 = new ItemModel() {ObjectId = "o4"};
		_itemModel5 = new ItemModel() {ObjectId = "o5"};
		
		_economyModel.CategoryEconomies = new Dictionary<int, Dictionary<string, ItemModel>>()
		{
			{
				1, new Dictionary<string, ItemModel>()
				{
					{"o1", _itemModel1},
					{"o2", _itemModel2},
				}
			},
			{
				2, new Dictionary<string, ItemModel>()
				{
					{"o3", _itemModel3},
					{"o4", _itemModel4},
					{"o5", _itemModel5},
				}
			},
		};

		Game1.content = new LocalizedContentManager(null, null, null);
	}

	[Test]
	public void ShouldReturnItemModelForObject()
	{
		var o1 = new Object("o1", 1) { Category = 1 };
		var o2 = new Object("o2", 1) { Category = 1 };
		var o3 = new Object("o3", 1) { Category = 2 };
		var o4 = new Object("o4", 1) { Category = 2 };
		var o5 = new Object("o5", 1) { Category = 2 };
		var o6 = new Object("o6", 1) { Category = 2 };

		var itemModel1 = _economyModel.GetItem(o1);
		var itemModel2 = _economyModel.GetItem(o2);
		var itemModel3 = _economyModel.GetItem(o3);
		var itemModel4 = _economyModel.GetItem(o4);
		var itemModel5 = _economyModel.GetItem(o5);
		var itemModel6 = _economyModel.GetItem(o6);

		Assert.Multiple(() =>
		{
		 Assert.That(itemModel1, Is.EqualTo(_itemModel1));
		 Assert.That(itemModel2, Is.EqualTo(_itemModel2));
		 Assert.That(itemModel3, Is.EqualTo(_itemModel3));
		 Assert.That(itemModel4, Is.EqualTo(_itemModel4));
		 Assert.That(itemModel5, Is.EqualTo(_itemModel5));
		 Assert.That(itemModel6, Is.Null);
		}); 
	}
	
	[Test]
	public void ShouldReturnItemModelForId()
	{
		var itemModel1 = _economyModel.GetItem("o1");
		var itemModel2 = _economyModel.GetItem("o2");
		var itemModel3 = _economyModel.GetItem("o3");
		var itemModel4 = _economyModel.GetItem("o4");
		var itemModel5 = _economyModel.GetItem("o5");
		var itemModel6 = _economyModel.GetItem("o6");

		Assert.Multiple(() =>
		{
		 Assert.That(itemModel1, Is.EqualTo(_itemModel1));
		 Assert.That(itemModel2, Is.EqualTo(_itemModel2));
		 Assert.That(itemModel3, Is.EqualTo(_itemModel3));
		 Assert.That(itemModel4, Is.EqualTo(_itemModel4));
		 Assert.That(itemModel5, Is.EqualTo(_itemModel5));
		 Assert.That(itemModel6, Is.Null);
		}); 
	}

	[Test]
	public void ShouldBeAbleToActOnAllItems()
	{
		var objSupply = 400;
		_economyModel.ForAllItems(i => i.Supply = objSupply);

		Assert.Multiple(() =>
		{
			Assert.That(_itemModel1.Supply, Is.EqualTo(objSupply));
			Assert.That(_itemModel2.Supply, Is.EqualTo(objSupply));
			Assert.That(_itemModel3.Supply, Is.EqualTo(objSupply));
			Assert.That(_itemModel4.Supply, Is.EqualTo(objSupply));
			Assert.That(_itemModel5.Supply, Is.EqualTo(objSupply));
		});
	}

	[Test]
	public void ShouldIndicateEconomiesHaveSameItems()
	{
		var newModel = new EconomyModel
		{
			CategoryEconomies = new Dictionary<int, Dictionary<string, ItemModel>>()
			{
				{
					1, new Dictionary<string, ItemModel>()
					{
						{"o1", _itemModel1},
						{"o2", _itemModel2},
					}
				},
				{
					2, new Dictionary<string, ItemModel>()
					{
						{"o3", _itemModel3},
						{"o4", _itemModel4},
						{"o5", _itemModel5},
					}
				},
			},
		};
		
		Assert.That(_economyModel.HasSameItems(newModel), Is.True);
	}
	
	[Test]
	public void ShouldIndicateEconomiesHaveDifferentCategories()
	{
		var newModel = new EconomyModel
		{
			CategoryEconomies = new Dictionary<int, Dictionary<string, ItemModel>>()
			{
				{
					1, new Dictionary<string, ItemModel>()
					{
						{"o1", _itemModel1},
						{"o2", _itemModel2},
					}
				},
				{
					3, new Dictionary<string, ItemModel>()
					{
						{"o3", _itemModel3},
						{"o4", _itemModel4},
						{"o5", _itemModel5},
					}
				},
			},
		};
		
		Assert.That(_economyModel.HasSameItems(newModel), Is.False);
	}
	
	[Test]
	public void ShouldIndicateEconomiesHaveDifferentItems()
	{
		var newModel = new EconomyModel
		{
			CategoryEconomies = new Dictionary<int, Dictionary<string, ItemModel>>()
			{
				{
					1, new Dictionary<string, ItemModel>()
					{
						{"o1", _itemModel1},
						{"o2", _itemModel2},
					}
				},
				{
					2, new Dictionary<string, ItemModel>()
					{
						{"o3", _itemModel3},
						{"o6", _itemModel4},
						{"o5", _itemModel5},
					}
				},
			},
		};
		
		Assert.That(_economyModel.HasSameItems(newModel), Is.False);
	}
}