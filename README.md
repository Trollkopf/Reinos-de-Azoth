# Reinos de Azoth

Prototipo digital de un juego competitivo de alquimia y combate mágico por turnos, desarrollado en **Unity y C#**. Cada alquimista reúne ingredientes, compra recursos, mejora sus hechizos y combate contra criaturas y rivales para acumular Poder Arcano.

Los ingredientes son el recurso central: decidir cuándo gastarlos para atacar, curarse o protegerse es la base del juego.

## Estado del proyecto

En desarrollo, con un prototipo local de **un jugador humano contra un bot**. El diseño completo está recogido en las [reglas Beta 2.1](Assets/Projet%20Info/Reinos_de_Azoth_Reglas_Beta_2_1.md); su implementación todavía es parcial.

El proyecto incluye:

- Inventario con cinco ingredientes: Hierba Roja, Agua Pura, Mineral Sulfuroso, Cristal de Aire y Polvo de Hueso.
- Mercado con cinco cartas visibles, compra con monedas y reposición de cartas.
- Libro con nueve hechizos: Bola de Fuego, Rayo de Plasma, Curación, Escudo Arcano, Látigo de Viento, Raíces de Tierra, Drenaje Vital, Explosión Ácida e Ilusión.
- Maestría de hechizos: nivel 2 al acumular 3 puntos y nivel 3 al acumular 6.
- Tres criaturas visibles, selección de objetivo, contraataques y recompensas. Los datos actuales incluyen Rata de Sombra, Gremlin de Barro y Lobo Corrompido.
- Bot con decisiones heurísticas para comprar ingredientes, curarse, protegerse, atacar y descartar.
- Definiciones de hechizos, criaturas y cartas de mercado mediante `ScriptableObject`.

### Diferencias con las reglas Beta 2.1

| Aspecto | Estado del prototipo |
| --- | --- |
| Límite de mano | `PlayerState.MaxHandSize` está fijado en **10**, frente a los 7 de las reglas. |
| Compras por turno | El mercado no aplica todavía el límite de una compra por turno. |
| Uso de hechizos | No se registra todavía el límite de un uso por hechizo y turno. |
| Combate entre jugadores | La resolución admite efectos de tipo `Damage`; la interacción del libro del jugador humano sigue orientada a seleccionar criaturas. |
| Victoria | La Coronación Arcana, la última ronda y la resolución completa del fin de partida están pendientes. |

Las reglas plantean ganar por eliminación o alcanzar 10 de Poder Arcano y superar la Coronación Arcana. Estos objetivos describen el diseño previsto, no un ciclo de partida completo ya implementado.

## Requisitos

- **Unity Hub**.
- **Unity Editor 6000.1.3f1**, versión indicada en [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- Un editor compatible con C# para modificar los scripts.
- Conexión a Internet para resolver los paquetes durante la primera apertura.

Entre las dependencias declaradas en [Packages/manifest.json](Packages/manifest.json) están Universal Render Pipeline **17.1.0**, Input System **1.14.0**, Unity UI **2.0.0** y Unity Test Framework **1.5.1**. Unity gestiona su instalación al abrir el proyecto.

## Abrir y ejecutar

1. Clona o descarga el repositorio.
2. En Unity Hub, añade la carpeta raíz del proyecto: la que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Ábrela con **Unity 6000.1.3f1**.
4. Espera a que termine la importación de recursos y la compilación de scripts.
5. Abre [Assets/Scenes/SampleScene.unity](Assets/Scenes/SampleScene.unity).
6. Pulsa **Play** en el editor.

`SampleScene` es la escena habilitada en la configuración de compilación. La partida se inicializa desde `GameManager`, que reparte tres ingredientes al jugador y al bot.

### Probar el flujo de juego

1. Consulta el inventario y navega por las páginas del libro de hechizos.
2. Abre el mercado para comprar ingredientes si tienes monedas suficientes.
3. Selecciona una criatura antes de lanzar un hechizo que requiera ese objetivo.
4. Haz clic en el hechizo para lanzarlo si puedes pagar su coste.
5. Si superas los diez ingredientes, utiliza los botones de descarte del inventario antes de terminar el turno.
6. Termina el turno para que actúe el bot. Después comienza un nuevo turno del jugador con el robo de un ingrediente.

La consola de Unity muestra las acciones, las decisiones del bot y los motivos por los que una acción no puede ejecutarse.

## Estructura

```text
Assets/
├── Art/                  Recursos gráficos
├── Audio/                Recursos de audio
├── Prefabs/              Elementos reutilizables de la escena y la interfaz
├── Projet Info/          Reglas y documentación de diseño
├── Scenes/               Escenas de Unity
├── ScriptableObjects/    Datos de criaturas, ingredientes, mercado y hechizos
├── Scripts/
│   ├── Core/             Inicialización de partida y gestión de turnos
│   ├── Creatures/        Mazos, estado, selección y vistas de criaturas
│   ├── Ingredients/      Inventario, mazo e interfaz de ingredientes
│   ├── Market/           Cartas, compras y vistas del mercado
│   ├── Player/           Estado del jugador, libro de hechizos y bot
│   └── Spells/           Definiciones, maestría, efectos y libro visual
└── Settings/             Configuración gráfica
Packages/                 Dependencias de Unity
ProjectSettings/          Configuración y versión del editor
```

### Puntos de entrada del código

| Archivo | Responsabilidad |
| --- | --- |
| [GameManager.cs](Assets/Scripts/Core/GameManager.cs) | Reparto inicial y actualización de las vistas al comenzar. |
| [TurnManager.cs](Assets/Scripts/Core/TurnManager.cs) | Alternancia entre jugador y bot, robo y comprobación del límite de mano. |
| [PlayerState.cs](Assets/Scripts/Player/PlayerState.cs) | Vida, escudo, monedas, Poder Arcano e inventario. |
| [BotController.cs](Assets/Scripts/Player/BotController.cs) | Evaluación y ejecución de las acciones del bot. |
| [SpellResolver.cs](Assets/Scripts/Spells/SpellResolver.cs) | Resolución de los efectos de los hechizos. |
| [SpellInstance.cs](Assets/Scripts/Spells/SpellInstance.cs) | Nivel y maestría de cada hechizo. |

## Modificar contenido

Las definiciones existentes están en `Assets/ScriptableObjects`. Se pueden editar desde el Inspector para ajustar costes, valores por nivel, estadísticas y recompensas.

Para crear definiciones nuevas, utiliza las opciones de **Create > Reinos de Azoth**: `Spell Definition`, `Creature Definition` y `Market Card`. Después asigna los nuevos recursos a las listas y referencias correspondientes de la escena. Crear un recurso por sí solo no lo incorpora a la partida.

## Comprobación manual

Tras modificar el proyecto, abre `SampleScene`, comprueba que la consola no muestre errores y verifica el reparto inicial, una compra, el lanzamiento de un hechizo, una recompensa por derrotar a una criatura y el paso de turno al bot.

El paquete Unity Test Framework está declarado, pero actualmente no hay una suite de pruebas automatizadas en `Assets`.

Al trabajar con Git, conserva los archivos `.meta` junto a sus recursos. Las carpetas generadas por Unity, como `Library`, `Temp` y `Logs`, están excluidas mediante `.gitignore`.
