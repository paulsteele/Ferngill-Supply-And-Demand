using System;
using System.Collections.Generic;
using MailFrameworkMod.Api;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.Letters
{
	public class StartingLetter : ILetter
	{
		private readonly IModHelper _modHelper;
		public string Id => "fse.starting.letter";
		public string Text => _modHelper.Translation.Get("fse.startingLetter.text");
		public string GroupId => "pts.fse";
		public string Title => _modHelper.Translation.Get("fse.startingLetter.title");
		public List<Item> Items => default;
		public string Recipe => default;
		public int WhichBG => default;
		public Texture2D LetterTexture => default;
		public int? TextColor => default;
		public Texture2D UpperRightCloseButtonTexture => default;
		public bool AutoOpen => default;
		public ITranslationHelper I18N => default;

		public StartingLetter(IModHelper modHelper) => _modHelper = modHelper;

		public bool Condition(ILetter letter) => !"read".Equals(_modHelper.Data.ReadSaveData<string>(Id));

		public void OnRead(ILetter letter)
		{
			_modHelper.Data.WriteSaveData(Id, "read");
		}
	}
}