using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerIntegrationTests
{
    [UnityTest]
    public IEnumerator LastPlayerStanding_SetsGameOverAndWinner()
    {
        GameObject playerObject = new GameObject("Player");

        GameObject botObject = new GameObject("Bot");

        GameObject playerManagerObject = new GameObject("PlayerManager");

        GameObject gameManagerObject = new GameObject("GameManager");

        playerManagerObject.SetActive(false);
        gameManagerObject.SetActive(false);

        PlayerState player = playerObject.AddComponent<PlayerState>();

        PlayerState bot = botObject.AddComponent<PlayerState>();

        PlayerManager playerManager = playerManagerObject.AddComponent<PlayerManager>();

        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        try
        {
            player.currentHP = 1;
            bot.currentHP = 15;

            SetPrivateField(playerManager, "players", new List<PlayerState> { player, bot });

            SetPrivateField(gameManager, "playerManager", playerManager);

            playerManagerObject.SetActive(true);
            gameManagerObject.SetActive(true);

            // Dejamos que Unity ejecute OnEnable().
            yield return null;

            player.TakeDamage(1, bot);

            yield return null;

            Assert.That(player.IsAlive, Is.False);

            Assert.That(gameManager.IsGameOver, Is.True);

            Assert.That(gameManager.Winner, Is.SameAs(bot));
        }
        finally
        {
            Object.Destroy(playerObject);

            Object.Destroy(botObject);

            Object.Destroy(playerManagerObject);

            Object.Destroy(gameManagerObject);
        }

        yield return null;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target
            .GetType()
            .GetField(
                fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

        Assert.That(field, Is.Not.Null, $"No se encontró el campo privado '{fieldName}'.");

        field.SetValue(target, value);
    }

    [UnityTest]
    public IEnumerator ReachingTenArcanePower_StartsCoronation()
    {
        GameObject playerObject = new GameObject("Player");

        GameObject botObject = new GameObject("Bot");

        GameObject playerManagerObject = new GameObject("PlayerManager");

        GameObject gameManagerObject = new GameObject("GameManager");

        playerManagerObject.SetActive(false);
        gameManagerObject.SetActive(false);

        PlayerState player = playerObject.AddComponent<PlayerState>();

        PlayerState bot = botObject.AddComponent<PlayerState>();

        PlayerManager playerManager = playerManagerObject.AddComponent<PlayerManager>();

        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        try
        {
            player.arcanePower = 9;
            bot.arcanePower = 0;

            SetPrivateField(playerManager, "players", new List<PlayerState> { player, bot });

            SetPrivateField(gameManager, "playerManager", playerManager);

            playerManagerObject.SetActive(true);
            gameManagerObject.SetActive(true);

            yield return null;

            player.AddArcanePower(1);

            yield return null;

            Assert.That(gameManager.IsCoronationActive, Is.True);

            Assert.That(gameManager.CoronationPlayer, Is.SameAs(player));

            Assert.That(gameManager.IsGameOver, Is.False);

            Assert.That(gameManager.Winner, Is.Null);
        }
        finally
        {
            Object.Destroy(playerObject);

            Object.Destroy(botObject);

            Object.Destroy(playerManagerObject);

            Object.Destroy(gameManagerObject);
        }

        yield return null;
    }
}
