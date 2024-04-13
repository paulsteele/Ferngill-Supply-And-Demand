using HarmonyLib;
using Tests.HarmonyMocks;

namespace Tests;

public class HarmonyTestBase
{
	[SetUp]
	public virtual void Setup()
	{
		var harmony = new Harmony("fse.tests");

		HarmonyFarmer.Setup(harmony);
		HarmonyGame.Setup(harmony);
		HarmonyFarmerCollection.Setup(harmony);
	}
}