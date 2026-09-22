# ⚔️ Reinos de Azoth – Reglas Beta 2.1

> Documento de diseño y especificación base para prototipo digital en Unity.

---

# 1. Concepto general

**Reinos de Azoth** es un juego competitivo de alquimia y combate mágico.

Cada jugador controla a un alquimista que obtiene ingredientes, lanza hechizos, derrota criaturas, mejora su dominio mágico y combate contra otros jugadores para convertirse en el **Rey Arcano de Azoth**.

El sistema está diseñado para que los ingredientes sean el recurso central del juego.

No existe un sistema clásico de puntos de acción para lanzar hechizos.

El límite natural de agresión y desarrollo viene determinado por:

- la cantidad de ingredientes disponibles;
- el límite de mano;
- el coste de los hechizos;
- el límite de un uso por hechizo y turno;
- el riesgo de quedarse sin recursos para defenderse.

---

# 2. Objetivo de la partida

Un jugador puede ganar de dos formas.

## 2.1 Victoria por Poder Arcano

Cuando un jugador alcanza **10 puntos de Poder Arcano**, inicia una **Coronación Arcana**.

Todos los demás jugadores vivos disponen de un último turno completo.

Si al terminar la Última Ronda el aspirante sigue con vida y mantiene al menos 10 puntos de Poder Arcano, gana la partida.

Si varios jugadores terminan la Última Ronda con 10 o más puntos de Poder Arcano, se aplican los desempates definidos más adelante.

---

## 2.2 Victoria por eliminación

Si en cualquier momento queda un único jugador vivo, este gana inmediatamente la partida.

---

# 3. Estadísticas iniciales

Cada jugador comienza con:

- **15 PV**
- **3 monedas**
- **3 ingredientes aleatorios**
- **0 Poder Arcano**
- **9 hechizos básicos en Nivel 1**
- **0 puntos de Maestría en todos los hechizos**

La vida máxima normal es de **15 PV**.

Un jugador no puede superar esta cantidad salvo que una carta o efecto indique expresamente lo contrario.

---

# 4. Ingredientes

Existen cinco ingredientes.

| Símbolo | Nombre | Rareza aproximada | Afinidad |
|---|---|---:|---|
| 🌿 | Hierba Roja | Común | Vida / Fuego |
| 💧 | Agua Pura | Común | Vida / Protección |
| 🌋 | Mineral Sulfuroso | Media | Fuego / Tierra |
| 💎 | Cristal de Aire | Media | Aire / Energía / Ilusión |
| ☠️ | Polvo de Hueso | Rara | Muerte / Corrupción |

---

# 5. Mazo de ingredientes

El mazo principal contiene **108 cartas**:

- 25 × Hierba Roja
- 25 × Agua Pura
- 22 × Mineral Sulfuroso
- 20 × Cristal de Aire
- 16 × Polvo de Hueso

Cuando el mazo de ingredientes se agota, se baraja la pila de descartes y se forma un nuevo mazo.

---

# 6. Mercado

El mercado contiene siempre **5 cartas visibles**.

Cada vez que un jugador compra una carta, se repone inmediatamente desde el mazo de mercado.

Cada jugador puede realizar como máximo **1 compra por turno**.

Comprar no consume ninguna acción ni impide lanzar hechizos.

## 6.1 Cartas de mercado

Distribución inicial:

- 12 × 2 Hierbas Rojas
- 12 × 2 Aguas Puras
- 8 × 2 Minerales Sulfurosos
- 8 × 2 Cristales de Aire
- 8 × 1 Polvo de Hueso

## 6.2 Precios

| Carta | Precio |
|---|---:|
| 2 Hierbas Rojas | 2 monedas |
| 2 Aguas Puras | 2 monedas |
| 2 Minerales Sulfurosos | 3 monedas |
| 2 Cristales de Aire | 3 monedas |
| 1 Polvo de Hueso | 4 monedas |

Los ingredientes adquiridos pasan inmediatamente a la mano del jugador.

---

# 7. Límite de mano

Cada jugador puede tener como máximo **7 ingredientes al final de su turno**.

Durante el turno puede superar temporalmente ese límite.

Al finalizar el turno debe descartar ingredientes hasta quedarse con 7.

La cantidad de ingredientes que posee cada jugador es información pública.

El tipo exacto de ingredientes es información privada.

---

# 8. Preparación de partida

1. Barajar el mazo de Ingredientes.
2. Barajar el mazo de Mercado.
3. Revelar 5 cartas de Mercado.
4. Barajar el mazo de Criaturas.
5. Revelar 3 criaturas.
6. Cada jugador recibe:
   - 15 PV;
   - 3 monedas;
   - 3 ingredientes;
   - sus 9 hechizos;
   - su tablero personal;
   - 0 Poder Arcano.
7. Elegir aleatoriamente al jugador inicial.

---

# 9. Flujo de turno

Cada turno se divide en las siguientes fases.

## 9.1 Fase de inicio

Se resuelven los efectos que indiquen:

- "al comienzo de tu turno";
- daño periódico;
- veneno;
- quemadura;
- recuperación;
- regeneración;
- efectos similares.

Después se eliminan los efectos que hayan expirado.

---

## 9.2 Fase de robo

El jugador roba **1 ingrediente** del mazo principal.

---

## 9.3 Fase de mercado

El jugador puede comprar como máximo **1 carta del Mercado**.

La compra es opcional.

---

## 9.4 Fase principal

Durante esta fase, el jugador puede lanzar tantos hechizos como pueda pagar.

No existe un límite global de hechizos por turno.

Sin embargo:

> **Cada hechizo concreto solo puede lanzarse una vez por turno.**

Ejemplo válido:

- Bola de Fuego
- Curación
- Escudo Arcano
- Drenaje Vital

Ejemplo no válido:

- Bola de Fuego
- Bola de Fuego

El jugador decide libremente cuándo dejar de lanzar hechizos.

---

## 9.5 Fase final

Al finalizar el turno:

1. se comprueba el límite de mano;
2. se descartan ingredientes hasta quedarse con 7;
3. se eliminan los efectos que indiquen "hasta el final del turno";
4. se reinicia el registro de hechizos usados en ese turno;
5. pasa el turno al siguiente jugador vivo.

---

# 10. Lanzamiento de hechizos

Para lanzar un hechizo:

1. elegir un hechizo válido;
2. comprobar que no haya sido utilizado ya durante ese turno;
3. comprobar que el jugador posee los ingredientes necesarios;
4. seleccionar un objetivo válido;
5. descartar los ingredientes;
6. resolver el efecto;
7. añadir **1 punto de Maestría** al hechizo;
8. marcar el hechizo como utilizado durante ese turno.

Un hechizo no puede lanzarse si su objetivo deja de ser válido antes de resolverse.

---

# 11. Maestría de hechizos

Cada hechizo mejora al utilizarse.

| Nivel | Requisito |
|---|---:|
| Nivel 1 | Inicial |
| Nivel 2 | 3 usos |
| Nivel 3 | 6 usos |

La mejora es permanente durante toda la partida.

Cuando un hechizo alcanza el requisito de un nuevo nivel, mejora inmediatamente.

---

# 12. Hechizos

---

## 12.1 🔥 Bola de Fuego

**Coste**

- 🌿 Hierba Roja
- 🌋 Mineral Sulfuroso

**Objetivos**

- jugador;
- criatura.

### Nivel 1

Inflige **2 de daño**.

### Nivel 2

Inflige **3 de daño**.

### Nivel 3

Inflige **4 de daño** y aplica **Quemadura**.

### Quemadura

El objetivo recibe **1 de daño al comienzo de su próximo turno**.

Después, Quemadura desaparece.

Las criaturas reciben el daño de Quemadura cuando se active su próximo evento de turno global.

---

## 12.2 ⚡ Rayo de Plasma

**Coste**

- 💧 Agua Pura
- 💎 Cristal de Aire

**Objetivos**

- jugador;
- criatura.

### Nivel 1

Inflige **2 de daño**.

### Nivel 2

Inflige **3 de daño** e ignora **1 punto de Escudo**.

### Nivel 3

Inflige **3 de daño** e ignora completamente los Escudos.

---

## 12.3 💚 Curación

**Coste**

- 💧 Agua Pura
- 🌿 Hierba Roja

**Objetivo**

- uno mismo.

### Nivel 1

Recupera **2 PV**.

### Nivel 2

Recupera **3 PV**.

### Nivel 3

Recupera **4 PV** y elimina **1 estado negativo**.

La curación no permite superar los PV máximos.

---

## 12.4 🛡️ Escudo Arcano

**Coste**

- 🌋 Mineral Sulfuroso
- 💧 Agua Pura

**Objetivo**

- uno mismo.

### Nivel 1

Obtienes **2 puntos de Escudo**.

### Nivel 2

Obtienes **3 puntos de Escudo**.

### Nivel 3

Obtienes **3 puntos de Escudo**.

Además, el primer atacante que consiga infligirte daño antes de que expire el Escudo recibe **1 de daño**.

### Duración

El Escudo permanece activo hasta el comienzo de tu siguiente turno.

El daño recibido se resta primero del Escudo.

El Escudo restante desaparece al comienzo de tu siguiente turno.

---

## 12.5 🌪️ Látigo de Viento

**Coste**

- 💎 Cristal de Aire
- 🌿 Hierba Roja

### Contra jugadores

#### Nivel 1

- Inflige **1 de daño**.
- El objetivo descarta **1 ingrediente aleatorio**.

#### Nivel 2

- Inflige **2 de daño**.
- El objetivo descarta **1 ingrediente aleatorio**.

#### Nivel 3

- Inflige **2 de daño**.
- El objetivo descarta **1 ingrediente aleatorio**.
- El lanzador roba **1 ingrediente**.

### Contra criaturas

Las criaturas no poseen mano.

Por ello, el descarte se sustituye por daño adicional.

#### Nivel 1

Inflige **2 de daño**.

#### Nivel 2

Inflige **3 de daño**.

#### Nivel 3

Inflige **3 de daño** y el lanzador roba **1 ingrediente**.

---

## 12.6 🌱 Raíces de Tierra

**Coste**

- 🌋 Mineral Sulfuroso
- 🌋 Mineral Sulfuroso
- 💧 Agua Pura

### Contra jugadores

#### Nivel 1

El objetivo queda **Enraizado**.

En su próximo turno solo podrá lanzar un máximo de **2 hechizos**.

#### Nivel 2

Como Nivel 1 y además recibe **1 de daño**.

#### Nivel 3

Como Nivel 1 y además recibe **2 de daño**.

### Contra criaturas

Una criatura afectada por Raíces de Tierra no contraataca tras el siguiente hechizo que reciba.

Después, el efecto desaparece.

Las criaturas con **Vuela** son inmunes.

> Nota de diseño:
> En esta versión no se hace perder el turno completo a ningún jugador.

---

# 13.7 ☠️ Drenaje Vital

**Coste**

- ☠️ Polvo de Hueso
- 🌿 Hierba Roja

**Objetivos**

- jugador;
- criatura.

### Nivel 1

- Inflige **2 de daño**.
- Recuperas **1 PV**.

### Nivel 2

- Inflige **2 de daño**.
- Recuperas **2 PV**.

### Nivel 3

- Inflige **3 de daño**.
- Recuperas **2 PV**.

La curación se aplica después del daño.

---

# 13.8 🧪 Explosión Ácida

**Coste**

- 💧 Agua Pura
- ☠️ Polvo de Hueso

### Nivel 1

Todos los demás jugadores reciben **1 de daño**.

### Nivel 2

Todos los demás jugadores reciben **2 de daño**.

### Nivel 3

Todos los demás jugadores reciben **2 de daño**.

Además, todas las criaturas reciben el estado **Corrosión** hasta el final del turno.

### Corrosión

La criatura recibe **+1 daño** de cada hechizo ofensivo que la golpee mientras dure el efecto.

El lanzador nunca recibe daño de su propia Explosión Ácida.

---

# 13.9 🌀 Ilusión

**Coste**

- 💎 Cristal de Aire
- ☠️ Polvo de Hueso

**Objetivo**

- uno mismo.

### Nivel 1

Roba **2 ingredientes**.

Elige **1** y descarta el otro.

### Nivel 2

Roba **2 ingredientes** y conserva ambos.

### Nivel 3

Roba **3 ingredientes**.

Conserva **2** y descarta 1.

Los ingredientes robados pueden superar temporalmente el límite de mano.

---

# 14. Criaturas

Siempre debe haber **3 criaturas visibles**.

Cada criatura posee:

- nombre;
- rango;
- PV máximos;
- PV actuales;
- ataque;
- habilidad;
- recompensa;
- Poder Arcano.

Las criaturas mantienen el daño recibido entre turnos.

Cualquier jugador puede atacar una criatura ya dañada por otro jugador.

El jugador que inflige el golpe final obtiene toda la recompensa.

---

# 15. Combate contra criaturas

Cuando un jugador lanza un hechizo ofensivo contra una criatura:

1. paga el coste;
2. resuelve el daño;
3. aplica efectos;
4. comprueba si la criatura ha muerto.

Si la criatura muere:

- no contraataca;
- el jugador recibe la recompensa;
- obtiene el Poder Arcano correspondiente;
- la criatura se descarta;
- se revela una nueva criatura inmediatamente.

Si la criatura sigue viva:

- contraataca inmediatamente;
- el jugador recibe el daño indicado en su carta;
- se aplican las habilidades especiales correspondientes.

---

# 16. Criaturas comunes

## 🐀 Rata de Sombra

- Rango: Común
- PV: 4
- Ataque: 1
- Habilidad: ninguna
- Recompensa: 1 moneda
- Poder Arcano: 1

---

## 🦇 Murciélago Viscoso

- Rango: Común
- PV: 4
- Ataque: 1
- Habilidad: Vuela
- Inmune a Raíces de Tierra
- Recompensa: 1 moneda
- Poder Arcano: 1

---

## 💩 Gremlin de Barro

- Rango: Común
- PV: 4
- Ataque: 2
- Habilidad: al morir, su asesino descarta 1 ingrediente aleatorio
- Recompensa: 2 monedas
- Poder Arcano: 1

---

## 💀 Esqueleto Roto

- Rango: Común
- PV: 5
- Ataque: 2
- Habilidad: recibe 1 daño menos de Bola de Fuego
- Recompensa: 2 monedas
- Poder Arcano: 1

---

# 17. Criaturas intermedias

## 🐺 Lobo Corrompido

- Rango: Intermedia
- PV: 6
- Ataque: 3
- Habilidad: ninguna
- Recompensa: 3 monedas
- Poder Arcano: 2

---

## 🌋 Guardián de Piedra

- Rango: Intermedia
- PV: 7
- Ataque: 2
- Habilidad: reduce en 1 todo el daño recibido
- Recompensa: 3 monedas
- Poder Arcano: 2

El daño mínimo recibido por un hechizo es 0.

---

## 🧙‍♀️ Bruja del Pantano

- Rango: Intermedia
- PV: 6
- Ataque: 2
- Habilidad: cada jugador que la ataque debe descartar 1 ingrediente adicional aleatorio
- Recompensa:
  - 3 monedas
  - 1 ingrediente aleatorio
- Poder Arcano: 2

Si el jugador no tiene ingredientes adicionales después de pagar el hechizo, puede atacarla igualmente.

---

## 🦴 Gólem Óseo

- Rango: Intermedia
- PV: 8
- Ataque: 2
- Habilidad: regeneración
- Recompensa: 4 monedas
- Poder Arcano: 2

### Regeneración

Al comienzo del turno del jugador inicial, recupera 1 PV.

No puede superar sus PV máximos.

---

# 18. Criaturas épicas

## 🐉 Dragón de Azufre

- Rango: Épica
- PV: 12
- Ataque: 4
- Habilidad: recibe 1 daño menos de Bola de Fuego
- Recompensa:
  - 6 monedas
  - 1 carta visible del Mercado gratis
- Poder Arcano: 3

La carta elegida del Mercado se repone inmediatamente.

---

## 🧬 Quimera Putrefacta

- Rango: Épica
- PV: 10
- Ataque: 3
- Habilidad: Veneno
- Recompensa: 5 monedas
- Poder Arcano: 3

### Veneno

Cuando la Quimera hace daño a un jugador, este recibe Veneno.

El jugador afectado recibe **1 de daño al comienzo de su próximo turno**.

Después, Veneno desaparece.

---

## 👻 Señor Espectral

- Rango: Épica
- PV: 9
- Ataque: 3
- Habilidad: su daño ignora Escudos
- Recompensa:
  - 5 monedas
  - +1 punto de Maestría en cualquier hechizo
- Poder Arcano: 3

El punto de Maestría puede provocar inmediatamente una subida de nivel.

---

# 19. Combate entre jugadores

Cualquier hechizo ofensivo puede utilizarse contra otro jugador vivo si su descripción lo permite.

Los ingredientes se pagan normalmente.

No existe límite global de ataques por turno.

El límite viene determinado por:

- recursos disponibles;
- un uso máximo de cada hechizo por turno.

---

# 20. Daño

El daño se resuelve en este orden:

1. modificadores del hechizo;
2. resistencias del objetivo;
3. efectos que ignoren Escudo;
4. absorción de Escudo;
5. reducción de PV;
6. efectos posteriores al daño.

Si un efecto reduce el daño por debajo de 0, se considera 0.

---

# 21. Eliminación de jugadores

Cuando un jugador llega a **0 PV**, queda eliminado inmediatamente.

Al ser eliminado:

- descarta todos sus ingredientes;
- pierde todas sus monedas;
- deja de realizar turnos;
- pierde sus estados activos;
- conserva sus datos únicamente para estadísticas de partida.

El jugador responsable directo de su eliminación obtiene:

- **2 Poder Arcano**

Si un jugador muere por un estado causado anteriormente por otro jugador, el crédito de eliminación pertenece al jugador que aplicó dicho estado.

Si no existe un atacante identificable, nadie recibe Poder Arcano por la eliminación.

---

# 22. Poder Arcano

El Poder Arcano representa prestigio, dominio mágico y autoridad.

## Fuentes principales

| Acción | Poder Arcano |
|---|---:|
| Derrotar criatura común | +1 |
| Derrotar criatura intermedia | +2 |
| Derrotar criatura épica | +3 |
| Eliminar a otro jugador | +2 |

---

# 23. Coronación Arcana

Cuando un jugador alcanza **10 Poder Arcano**:

1. se marca como Aspirante al Trono;
2. la ronda actual continúa normalmente;
3. al terminar la ronda actual se inicia la Última Ronda;
4. todos los jugadores vivos excepto el aspirante reciben un turno completo adicional.

Durante la Última Ronda pueden:

- atacar al aspirante;
- ganar Poder Arcano;
- curarse;
- combatir criaturas;
- alcanzar también 10 Poder Arcano.

---

# 24. Resolución de la partida

Al finalizar la Última Ronda:

## Caso 1

Solo un jugador vivo tiene 10 o más Poder Arcano.

Ese jugador gana.

## Caso 2

Varios jugadores vivos tienen 10 o más Poder Arcano.

Gana quien tenga más Poder Arcano.

## Caso 3

Hay empate de Poder Arcano.

Se aplican los siguientes desempates:

1. más PV actuales;
2. más monedas;
3. mayor suma total de niveles de Maestría;
4. mayor cantidad de ingredientes;
5. si persiste el empate, victoria compartida en la beta.

---

# 25. Estados

Estados iniciales soportados:

```text
Burn
Poison
Shield
Rooted
Corrosion
Reflect
```

Cada estado debe almacenar al menos:

```text
id
sourcePlayerId
targetId
duration
value
trigger
```

---

# 26. Información pública y privada

## Información pública

- PV
- Escudo
- Monedas
- Poder Arcano
- cantidad de ingredientes en mano
- nivel de hechizos
- Maestría
- estados
- criaturas activas
- mercado

## Información privada

- tipos concretos de ingredientes en mano

---

# 27. Reglas de aleatoriedad

La versión digital utiliza RNG únicamente para:

- orden de mazos;
- robo de ingredientes;
- descartes aleatorios;
- selección inicial de jugador;
- recompensas aleatorias.

El combate no utiliza dados.

Todo el daño base es determinista.

---

# 28. Entidades recomendadas para Unity

Estas entidades sirven como referencia de arquitectura.

---

## 28.1 Player

```csharp
Player
{
    int id;
    string playerName;

    int currentHP;
    int maxHP;

    int coins;
    int arcanePower;

    List<Ingredient> hand;
    List<SpellInstance> spells;
    List<StatusEffect> statusEffects;

    bool isAlive;
    bool isCoronationCandidate;
}
```

---

## 28.2 Ingredient

```csharp
Ingredient
{
    IngredientType type;
}
```

```csharp
enum IngredientType
{
    RedHerb,
    PureWater,
    SulfurMineral,
    AirCrystal,
    BoneDust
}
```

---

## 28.3 SpellDefinition

Preferiblemente implementado mediante `ScriptableObject`.

```csharp
SpellDefinition
{
    string id;
    string spellName;

    List<IngredientCost> cost;

    TargetType targetType;

    SpellEffect level1;
    SpellEffect level2;
    SpellEffect level3;
}
```

---

## 28.4 SpellInstance

Estado del hechizo perteneciente a un jugador.

```csharp
SpellInstance
{
    SpellDefinition definition;

    int mastery;
    int level;

    bool usedThisTurn;
}
```

---

## 28.5 CreatureDefinition

Preferiblemente `ScriptableObject`.

```csharp
CreatureDefinition
{
    string id;
    string creatureName;

    CreatureRank rank;

    int maxHP;
    int attack;

    CreatureAbility ability;

    int coinReward;
    int arcanePowerReward;
}
```

---

## 28.6 CreatureInstance

```csharp
CreatureInstance
{
    CreatureDefinition definition;

    int currentHP;

    List<StatusEffect> statusEffects;
}
```

---

## 28.7 StatusEffect

```csharp
StatusEffect
{
    StatusType type;

    int value;
    int remainingDuration;

    int sourcePlayerId;

    StatusTrigger trigger;
}
```

---

# 29. GameState

El estado completo de una partida debería poder representarse mediante una única estructura.

```csharp
GameState
{
    List<Player> players;

    List<Ingredient> ingredientDeck;
    List<Ingredient> ingredientDiscard;

    List<MarketCard> marketDeck;
    List<MarketCard> visibleMarket;

    List<CreatureDefinition> creatureDeck;
    List<CreatureInstance> activeCreatures;

    int currentPlayerIndex;
    int roundNumber;

    GamePhase currentPhase;

    bool finalRoundActive;
}
```

---

# 30. Fases recomendadas

```csharp
enum GamePhase
{
    StartTurn,
    Draw,
    Market,
    Main,
    EndTurn,
    FinalRound,
    GameOver
}
```

---

# 31. Acciones de juego

Toda interacción debería convertirse en una acción concreta.

Ejemplos:

```text
DrawIngredientAction
BuyMarketCardAction
CastSpellAction
SelectTargetAction
DiscardIngredientAction
EndTurnAction
```

Idealmente:

```csharp
GameAction -> Validation -> Resolution -> GameResult
```

Ejemplo conceptual:

```csharp
GameResult Execute(GameState state, GameAction action);
```

Esto permite reutilizar exactamente las mismas reglas para:

- jugador humano;
- bot;
- simulador automático;
- multiplayer futuro.

---

# 32. Validación de hechizos

Antes de permitir lanzar un hechizo debe comprobarse:

```text
PlayerAlive
CorrectPhase
SpellNotUsedThisTurn
EnoughIngredients
ValidTarget
TargetAlive
SpecialRestrictions
```

---

# 33. Prioridad de efectos

Para evitar ambigüedades:

```text
1. Validar acción
2. Pagar coste
3. Aplicar efecto principal
4. Aplicar modificadores
5. Resolver daño
6. Resolver muerte
7. Resolver recompensas
8. Aplicar estados
9. Añadir Maestría
10. Comprobar subida de nivel
11. Comprobar Coronación
12. Comprobar fin de partida
```

Cuando sea necesario, una carta puede indicar expresamente una prioridad distinta.

---

# 34. IA del bot

La primera IA no necesita machine learning.

Puede utilizar un sistema heurístico.

Cada posible acción recibe una puntuación.

Ejemplo:

```text
Matar jugador                +100
Evitar muerte propia          +90
Iniciar Coronación            +80
Matar criatura épica          +60
Matar criatura intermedia     +40
Matar criatura común          +20
Hacer daño rival              +damage * 5
Curarse                       +missingHP * 3
Comprar ingrediente útil      +10
Gastar ingrediente raro       -5
Quedarse sin cartas           -20
```

El bot:

1. genera todas las acciones válidas;
2. calcula una puntuación;
3. escoge la mejor;
4. vuelve a evaluar el estado;
5. repite hasta decidir finalizar turno.

---

# 35. Métricas de balance

Durante las pruebas deben registrarse automáticamente:

```text
Winner
NumberOfTurns
NumberOfRounds

DamageDealtPerPlayer
DamageReceivedPerPlayer

HealingPerPlayer

IngredientsDrawn
IngredientsSpent
IngredientsDiscarded

MarketPurchases

SpellsCast
SpellUsageByType

AverageSpellLevel

CreaturesKilled
CreaturesKilledByRank

PlayerKills

ArcanePowerSources

TurnsWithNoUsefulAction

AverageHandSize

HPAtEndOfGame

CoinsAtEndOfGame
```

---

# 36. Objetivos iniciales de balance

No son reglas definitivas.

Sirven como referencia para las pruebas.

## Duración

### 2 jugadores

15-30 minutos.

### 3 jugadores

25-45 minutos.

### 4 jugadores

35-60 minutos.

---

## Ritmo esperado

Un jugador debería poder lanzar aproximadamente:

- 1 hechizo en un turno pobre;
- 2 hechizos en un turno normal;
- 3 o más hechizos en un turno preparado.

Los turnos explosivos deben ser posibles.

Sin embargo, deberían dejar al jugador significativamente más vulnerable al consumir buena parte de su mano.

---

# 37. Principios de diseño

## 37.1 Los ingredientes son poder

Tener muchas cartas debe dar miedo a los rivales.

Un jugador con 7 ingredientes representa una amenaza potencial.

---

## 37.2 Atacar tiene un coste real

No existe un coste artificial de acción.

El coste es consumir recursos que podrían utilizarse para:

- defenderse;
- curarse;
- combatir criaturas;
- preparar turnos posteriores.

---

## 37.3 Quedarse vacío debe ser peligroso

Un turno extremadamente agresivo puede proporcionar una gran ventaja.

Pero también puede dejar al jugador sin capacidad de respuesta.

---

## 37.4 Especializarse debe sentirse bien

Utilizar repetidamente un hechizo lo mejora.

Un jugador puede desarrollar una identidad durante la partida.

---

## 37.5 Ningún jugador debe perder varios turnos

Los estados de control deben limitar opciones.

No deben impedir jugar durante largos periodos.

---

## 37.6 Las criaturas generan conflicto indirecto

Una criatura dañada puede ser rematada por otro jugador.

La recompensa siempre pertenece al jugador que consigue el golpe final.

Esto permite:

- robo de recompensas;
- presión;
- faroles;
- decisiones tácticas.

---

# 38. Posibles expansiones futuras

Estas mecánicas NO forman parte todavía de la beta base.

Podrían añadirse más adelante:

- Artefactos
- Reliquias
- Escuelas de magia
- Hechizos avanzados
- Ingredientes legendarios
- Catalizadores
- Pociones
- Criaturas únicas
- Jefes
- Personajes con habilidades propias
- Reinos
- Eventos globales
- Alquimia experimental
- Transmutación
- Sangre de criaturas
- Equipamiento permanente

---

# 39. Alcance recomendado del primer prototipo Unity

Para la primera versión jugable:

```text
2 jugadores
1 humano
1 bot

9 hechizos

5 ingredientes

8-11 tipos de criatura

5 cartas visibles de mercado

15 PV

10 Poder Arcano para ganar

Sin animaciones complejas

Sin multiplayer

Sin artefactos

Sin eventos

Sin personajes especiales
```

El objetivo del primer prototipo no es crear el juego final.

El objetivo es comprobar:

> **¿Es divertido conseguir ingredientes, decidir cuándo gastarlos y enfrentarse al rival?**

Si esa respuesta es sí, el resto del juego puede crecer alrededor de este núcleo.

---

# 40. Versión

```text
Game: Reinos de Azoth
Rules Version: Beta 2.1
Primary Target: Unity prototype
Players: 2-4
Initial Digital Prototype: 2 players
Victory Threshold: 10 Arcane Power
Max HP: 15
Max Hand: 7
```
