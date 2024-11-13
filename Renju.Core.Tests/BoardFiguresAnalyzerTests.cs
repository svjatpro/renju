using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests
{
    [TestFixture]
    public class BoardAnalyzerTests
    {
        [Test]
        public void TestMethod1()
        {
            var analyser = new BoardFiguresAnalyser( new Board.Board( 15 ), Stone.Black );
            
            
        }
    }
}
