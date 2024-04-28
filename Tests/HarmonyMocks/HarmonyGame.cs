using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public static class HarmonyGame
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetPlayer))
		);
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetActiveClickableMenu))
		);
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.options)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetOptions))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getOnlineFarmers)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetOnlineFarmers))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getAllFarmers)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetAllFarmers))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getFarm)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetFarm))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetSourceRectForStandardTileSheet))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.playSound), new []{typeof(string), typeof(int?)}),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockPlaySound))
		);
		harmony.Patch(
			AccessTools.Method(
				typeof(Game1), 
				nameof(Game1.drawDialogueBox), 
				new []
				{
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(bool),
					typeof(bool),
					typeof(string),
					typeof(bool),
					typeof(bool),
					typeof(int),
					typeof(int),
					typeof(int)
				}
			),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockDrawDialogueBox))
		);
		DrawDialogueBoxCalls.Clear();
	}

	public static Farmer GetPlayerResult { get; set; }
	static bool MockGetPlayer(ref Farmer __result)
	{
		__result = GetPlayerResult;
		return false;
	}
	
	public static FarmerCollection GetOnlineFarmersResults { get; set; }
	static bool MockGetOnlineFarmers(ref FarmerCollection __result)
	{
		__result = GetOnlineFarmersResults;
		return false;
	}
	
	public static IEnumerable<Farmer> GetAllFarmersResults { get; set; }
	static bool MockGetAllFarmers(ref IEnumerable<Farmer> __result)
	{
		__result = GetAllFarmersResults;
		return false;
	}
	
	public static Farm GetFarmResult { get; set; }
	static bool MockGetFarm(ref Farm __result)
	{
		__result = GetFarmResult;
		return false;
	}
	
	public static IClickableMenu GetActiveClickableMenuResult { get; set; }
	static bool MockGetActiveClickableMenu(ref IClickableMenu __result)
	{
		__result = GetActiveClickableMenuResult;
		return false;
	}
	public static Options GetOptionsResult { get; set; }
	static bool MockGetOptions(ref Options __result)
	{
		__result = GetOptionsResult;
		return false;
	}

	public static List<(int x, int y, int width, int height, bool speaker, bool drawOnlyBox)>
		DrawDialogueBoxCalls { get; } = [];

	static bool MockDrawDialogueBox
	(
		int x,
		int y,
		int width,
		int height,
		bool speaker,
		bool drawOnlyBox
	)
	{
		DrawDialogueBoxCalls.Add(new (x, y, width, height, speaker, drawOnlyBox));
		return false;
	}
	
	static bool MockGetSourceRectForStandardTileSheet(ref Rectangle __result)
	{
		__result = new Rectangle();
		return false;
	}
	
	static bool MockPlaySound(string cueName)
	{
		return false;
	}
}