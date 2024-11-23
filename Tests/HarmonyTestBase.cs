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
		HarmonyFarmerTeam.Setup(harmony);
		HarmonyFarm.Setup(harmony);
		HarmonyObject.Setup(harmony);
		HarmonyModMessageReceivedEventArgs.Setup(harmony);
		HarmonySpriteFont.Setup(harmony);
		HarmonyUtility.Setup(harmony);
		HarmonySpriteBatch.Setup(harmony);
		HarmonyGameMenu.Setup(harmony);
		HarmonyOptions.Setup(harmony);
		HarmonyTexture2D.Setup(harmony);
		HarmonyIClickableMenu.Setup(harmony);
		HarmonyMapPage.Setup(harmony);
		HarmonyCollectionsPage.Setup(harmony);
		HarmonyLetterViewMenu.Setup(harmony);
		HarmonyOptionsDropDown.Setup(harmony);
		HarmonyOptionsCheckbox.Setup(harmony);
		HarmonyGraphicsDeviceManager.Setup(harmony);
		HarmonyClickableTextureComponent.Setup(harmony);
		HarmonyItem.Setup(harmony);
		HarmonyLocalizedContentManager.Setup(harmony);
		HarmonyOptionsTextEntry.Setup(harmony);
		HarmonyTextBox.Setup(harmony);
		HarmonyExitPage.Setup(harmony);
	}
}