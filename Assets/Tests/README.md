# Pruebas de hechizos y estados

Unity 6000.1.3f1; Unity Test Framework 1.5.1 (ya instalado).

## Ejecutar

En Unity: Window > General > Test Runner, pestañas EditMode y PlayMode,
Run All en cada una. Las integraciones PlayMode se ejecutan dentro del Editor:
cargan los SpellDefinition originales mediante AssetDatabase. No están destinadas
a un reproductor standalone; los casos que requieren assets se marcan como
omitidos allí.

Por consola (cerrar antes ese proyecto en Unity, o usar una copia independiente):

```powershell
$unity = 'I:/UnityHub/Unity Hub/Hub/Editor/6000.1.3f1/Editor/Unity.exe'
& $unity -batchmode -nographics -projectPath $PWD.Path -runTests -testPlatform EditMode -assemblyNames ReinosDeAzoth.EditMode.Tests -testResults "$PWD/Logs/TestResults/EditMode.xml" -logFile "$PWD/Logs/TestResults/EditMode.log"
& $unity -batchmode -nographics -projectPath $PWD.Path -runTests -testPlatform PlayMode -assemblyNames ReinosDeAzoth.PlayMode.Tests -testResults "$PWD/Logs/TestResults/PlayMode.xml" -logFile "$PWD/Logs/TestResults/PlayMode.log"
```

No añadir -quit: el Test Runner finaliza Unity cuando termina.

## Archivos y cobertura

| Archivo | Qué comprueba |
| --- | --- |
| EditMode/PlayerStatusEffectsTests.cs | Transferencia y limpieza de raíces, acumulación de quemaduras, limpieza de estados negativos y consumo único de reflejo. |
| EditMode/CreatureStatusEffectsTests.cs | Acumulación independiente; raíces y corrosión expiran sin eliminar quemaduras. |
| EditMode/BurnStackTests.cs | Autor, duración inicial de dos turnos e independencia de stacks. |
| EditMode/IngredientInventoryTests.cs | Altas, bajas, totales, eventos, costes duplicados, gasto atómico y coste vacío. |
| EditMode/PlayerStateTests.cs | Topes de escudo y vida, límites y contador de lanzamientos, reinicio de turno, limpieza mediante Healing Nv.3 y quemaduras durante dos turnos. |
| PlayMode/SpellIntegrationFixture.cs | Construye componentes, UI y mazos reales; carga assets; conecta dependencias y limpia objetos y estado aleatorio tras cada caso. |
| PlayMode/SpellCombatIntegrationTests.cs | Fireball, Plasma en tres niveles y tres cantidades de escudo, raíces y contraataques, bloqueo del segundo lanzamiento desde SpellPageView, corrosión, curación y reflejo. |
| PlayMode/BurnIntegrationTests.cs | Quemaduras escalonadas, FIFO, interrupción al morir, recompensa al autor correcto con ambos órdenes de jugadores y reemplazo de criatura. |
| PlayMode/IngredientSpellIntegrationTests.cs | Ilusión en tres niveles, duplicados, límite temporal de mano, destrucción de opciones, lanzamiento real y descarte exacto de costes. |
| EditMode/ReinosDeAzoth.EditMode.Tests.asmdef | Ensamblado de tests exclusivo del Editor. |
| PlayMode/ReinosDeAzoth.PlayMode.Tests.asmdef | Ensamblado de tests PlayMode. |
| ../Scripts/ReinosDeAzoth.Runtime.asmdef | Hace referenciables los scripts del juego; declara las dependencias existentes de TextMeshPro y UnityEngine.UI. |

Cada archivo y carpeta tiene su .meta versionado.

## Alcance y decisiones

- No se modifican scripts C#, reglas, valores, assets de hechizos ni escenas.
- El único cambio de configuración del código de producción es el asmdef runtime:
  los asmdef de tests no pueden referenciar el ensamblado predefinido Assembly-CSharp.
- No se amplía ninguna visibilidad. La reflexión queda en el fixture de pruebas
  para conectar campos privados de escena y preparar/inspeccionar mazos. No se
  invocan métodos privados para simular la resolución.
- Se usan GameObjects, MonoBehaviours y ScriptableObjects reales, sin mocks.
  Los objetos inactivos permiten configurar dependencias antes de Awake.
- Los assets de hechizos se leen sin modificarlos. Los valores artificiales de
  criaturas y manos son datos de prueba, no cambios de balance.
- Las descripciones de Healing y EarthRoots no coinciden con el código actual
  (un estado/dos hechizos en el texto frente a todos los estados/un hechizo en
  la implementación). Los tests fijan la implementación solicitada.
- La aplicación de corrosión se comprueba a través de AcidExplosion sobre
  criaturas. En jugadores se comprueba el daño posterior con el estado ya activo:
  Esta batería no cambia ni da por cubierta esa ruta ausente.


## Validacion realizada

Ejecutado con Unity 6000.1.3f1 en una copia temporal completa de Assets, Packages
 y ProjectSettings, utilizando las mismas dependencias del proyecto:

- EditMode: 33/33 correctos; 0 fallos y 0 omitidos.
- PlayMode: 38/38 correctos; 0 fallos y 0 omitidos.
- Total: 71/71 correctos.

Informes locales (ignorados por Git): Logs/TestResults/EditMode.xml y
Logs/TestResults/PlayMode.xml; registros completos junto a ellos en .log.
La copia temporal se elimina despues de la validacion.