using fse.core.actions;
using Moq;
using StardewModdingAPI;

namespace Tests.actions;

public class SafeActionTests
{

	[Test]
	public void SafeActionShouldInvokeValue()
	{
		var i = 0;
		var mockMonitor = new Mock<IMonitor>();

		void Func()
		{
			i++;
		}

		SafeAction.Run(Func, mockMonitor.Object);
		
		Assert.That(i, Is.EqualTo(1));
		mockMonitor.Verify(m => m.Log(It.IsAny<string>(), It.IsAny<LogLevel>()), Times.Never);
	}
	[Test]
	public void SafeActionShouldLogAndReturnDefaultValue()
	{
		var mockMonitor = new Mock<IMonitor>();

		int Func() => throw new Exception("ForcedError");

		var value = SafeAction.Run(Func, -1, mockMonitor.Object);
		
		Assert.That(value, Is.EqualTo(-1));
		
		mockMonitor.Verify(m => m.Log(It.IsAny<string>(), It.IsAny<LogLevel>()), Times.Exactly(3));
		
		mockMonitor.Verify(m => m.Log(nameof(SafeActionShouldLogAndReturnDefaultValue), LogLevel.Error), Times.Once);
		mockMonitor.Verify(m => m.Log("ForcedError", LogLevel.Error), Times.Once);
	}
}