using System.Diagnostics;
using NUnit.Framework;
using Renju.Core.BoardAnalyser;
using Renju.Core.Players;

namespace Renju.Core.Tests;

[TestFixture]
public class PcPlayerTests
{
    [TestCase( AiType.Plain )]
    [TestCase( AiType.Graph )]
    public void BothAiTypes_PlayAGame( AiType type )
    {
        var game = new RenjuGame.RenjuGame( 9,
            new PcPlayer( "black", type ),
            new PcPlayer( "white", type ) );

        for ( var i = 0; i < 10 && !game.Referee.IsGameOver; i++ )
        {
            Assert.That( game.TryProceedMove(), Is.True, $"move {i} failed" );
        }
    }

    [Test]
    public void GraphConfig_IsHonored_TimeoutCapsExplosiveExpansion()
    {
        // depth 8 × topK 50 would take practically forever without the cap;
        // with 100 ms budget the first move must come back almost immediately.
        var config = new GraphConfig { Depth = 8, TopK = 50, TimeoutMs = 100 };
        var game = new RenjuGame.RenjuGame( 15,
            new PcPlayer( "black", AiType.Graph, config ),
            new PcPlayer( "white", AiType.Graph, config ) );

        var watch = Stopwatch.StartNew();
        Assert.That( game.TryProceedMove(), Is.True );
        Assert.That( game.TryProceedMove(), Is.True );
        watch.Stop();

        // generous margin: 2 moves × (100 ms budget + one last depth pass overshoot)
        Assert.That( watch.ElapsedMilliseconds, Is.LessThan( 5000 ),
            "timeout did not cap the expansion" );
        Assert.That( game.Board.LastMove, Is.Not.Null, "board should have stones placed" );
    }
}
