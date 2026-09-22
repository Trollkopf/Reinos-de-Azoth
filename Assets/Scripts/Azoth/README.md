# Núcleo inicial de Reinos de Azoth

`Azoth.Core` es C# independiente de Unity. Esta entrega implementa preparación para 2–4 jugadores, distribución de 108 ingredientes, mercado de 48 ofertas, robo, compra, fases, descarte obligatorio, rondas y catálogo de nueve hechizos con costes, objetivos y Maestría.

## Uso

```csharp
using Azoth.Core;

var game = new GameSession(new[] { "Humano", "Bot" }, seed: 42);
int player = game.CurrentPlayer.Id;
// La sesión ya ha repartido 3 ingredientes y robado 1 para el primer turno.
var purchase = game.Execute(new GameAction(player, ActionKind.BuyMarketCard, 0));
var main = game.Execute(new GameAction(player, ActionKind.BeginMainPhase));
var end = game.Execute(new GameAction(player, ActionKind.EndTurn));
// Si Phase == Discard, enviar DiscardIngredient con el índice elegido de la mano.
// El último descarte necesario avanza automáticamente al siguiente jugador.
```

Consultar siempre `ActionResult.Success` y `Error`. Las acciones rechazadas no cambian el estado. Los índices del mercado y de la mano corresponden al estado actual, no son identificadores permanentes.

## Decisiones provisionales

- Las compras generan ingredientes del tipo comprado, sin retirarlos del mazo principal. Cuando se descartan, entran en el descarte principal: el total puede aumentar por compras.
- El mazo de mercado se repone barajando las ofertas compradas al agotarse.
- Si no quedan ingredientes ni descartes, el robo no entrega carta.
- La construcción de la sesión resuelve reparto, elección del jugador inicial y primer robo. Inicio y robo son automáticos por ahora; falta integrar los estados.
- La misma semilla permite reproducir partidas dentro del mismo entorno de ejecución; no se garantiza idéntico RNG entre plataformas o versiones de .NET.

## Alcance pendiente

Todavía no es una partida jugable: faltan efectos de hechizos, criaturas, estados, muertes, coronación, bot e interfaz. `SpellProgress.RecordCast` es una pieza para el futuro resolutor; no paga ingredientes ni ejecuta el hechizo. No se expone una acción de lanzamiento hasta implementar esa resolución.

Las manos y el mazo completo están disponibles al código de confianza. Esta API aún no ofrece una vista filtrada para multijugador ni serialización de partidas. La representación futura para UI/bot debe respetar la información privada.

## Pruebas

Abrir **Window > General > Test Runner > EditMode** y ejecutar `Azoth.Core.Tests`. Las pruebas cubren reparto, semilla, validación sin mutaciones, compra, avance de turnos, descarte, reciclaje durante 300 turnos, Maestría y costes repetidos.
