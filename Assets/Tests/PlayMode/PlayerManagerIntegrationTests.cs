using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerManagerIntegrationTests
{
    [UnityTest]
    public IEnumerator PlayerDeath_AwardsTwoArcanePowerToKiller()
    {
        GameObject managerObject = new GameObject("PlayerManager");

        GameObject victimObject = new GameObject("Victim");

        GameObject killerObject = new GameObject("Killer");

        managerObject.SetActive(false);

        PlayerManager manager = managerObject.AddComponent<PlayerManager>();

        PlayerState victim = victimObject.AddComponent<PlayerState>();

        PlayerState killer = killerObject.AddComponent<PlayerState>();

        victim.currentHP = 1;
        killer.arcanePower = 0;

        SetPlayers(manager, new List<PlayerState> { killer, victim });

        // Ahora sí estamos en runtime.
        // Al activar el objeto Unity ejecutará OnEnable().
        managerObject.SetActive(true);

        // Dejamos que Unity procese el ciclo de vida.
        yield return null;

        victim.TakeDamage(1, killer);

        Assert.That(victim.currentHP, Is.EqualTo(0));

        Assert.That(victim.IsAlive, Is.False);

        Assert.That(killer.arcanePower, Is.EqualTo(PlayerManager.PlayerKillArcanePowerReward));

        Object.Destroy(managerObject);
        Object.Destroy(victimObject);
        Object.Destroy(killerObject);

        yield return null;
    }

    private void SetPlayers(PlayerManager manager, List<PlayerState> players)
    {
        var field = typeof(PlayerManager).GetField(
            "players",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        Assert.That(field, Is.Not.Null);

        field.SetValue(manager, players);
    }
}
