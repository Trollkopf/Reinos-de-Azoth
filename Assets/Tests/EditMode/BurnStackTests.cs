using NUnit.Framework;
using UnityEngine;

namespace ReinosDeAzoth.Tests.EditMode
{
    public class BurnStackTests
    {
        [Test]
        public void Constructor_KeepsSourceAndStartsWithTwoIndependentTurns()
        {
            var owner = new GameObject("Burn owner");
            try
            {
                var source = owner.AddComponent<PlayerState>();
                var burn = new BurnStack(source);
                Assert.That(burn.source, Is.SameAs(source));
                Assert.That(burn.turnsRemaining, Is.EqualTo(2));
                var other = new BurnStack(source);
                burn.turnsRemaining--;
                Assert.That(other.turnsRemaining, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void Constructor_AllowsUnknownSource()
        {
            var burn = new BurnStack(null);
            Assert.That(burn.source, Is.Null);
            Assert.That(burn.turnsRemaining, Is.EqualTo(2));
        }
    }
}
