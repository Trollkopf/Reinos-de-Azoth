# ⚔️ Reinos de Azoth – Reglas Beta 2.2

> Documento de reglas y diseño actualizado para el prototipo digital en Unity.  
> Estado de referencia: 25/09/2026.

---

# 1. Concepto general

**Reinos de Azoth** es un juego competitivo de alquimia, gestión de recursos y combate mágico.

Cada jugador controla a un alquimista que obtiene ingredientes, compra recursos en el Mercado, lanza hechizos, derrota criaturas, mejora sus hechizos mediante Maestría, combate contra otros jugadores y acumula Poder Arcano.

El objetivo final es convertirse en el **Rey Arcano de Azoth**.

Los ingredientes son el recurso central del juego. No existe un sistema clásico de puntos de acción.

Un jugador puede lanzar tantos hechizos como pueda pagar durante su turno, incluso repetir el mismo hechizo si vuelve a disponer de los ingredientes necesarios.

La capacidad ofensiva y defensiva está limitada principalmente por:

- la cantidad de ingredientes disponibles;
- el límite de mano;
- los costes de los hechizos;
- los estados activos;
- el Escudo;
- el riesgo de consumir todos los recursos en un turno agresivo.

---

# 2. Objetivo de la partida

Un jugador puede ganar de dos formas.

## 2.1 Victoria por Poder Arcano

Cuando un jugador alcanza **10 puntos de Poder Arcano**, inicia una **Coronación Arcana**.

Los demás jugadores vivos disponen de una última oportunidad para detenerlo.

Si al finalizar la Última Ronda el aspirante sigue vivo y mantiene al menos 10 puntos de Poder Arcano, puede ganar la partida según las reglas de resolución descritas más adelante.

## 2.2 Victoria por eliminación

Si en cualquier momento queda un único jugador vivo, gana inmediatamente la partida.

> Nota de implementación: las condiciones completas de victoria, eliminación de jugadores y Coronación todavía están pendientes de integrarse en el prototipo Unity.

---

# 3. Estadísticas iniciales

Cada jugador comienza con:

- **15 PV**;
- **3 monedas**;
- **3 ingredientes aleatorios**;
- **0 Poder Arcano**;
- **9 hechizos en Nivel 1**;
- **0 puntos de Maestría** en todos sus hechizos;
- **0 Escudo**.

La vida máxima normal es de **15 PV**.

El Escudo tiene un máximo actual de **6 puntos**.

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

El mazo principal contiene **108 ingredientes**:

- 25 × Hierba Roja;
- 25 × Agua Pura;
- 22 × Mineral Sulfuroso;
- 20 × Cristal de Aire;
- 16 × Polvo de Hueso.

Los ingredientes utilizados para pagar hechizos se envían a la **pila de descartes**.

Los ingredientes descartados manualmente también se envían a la pila de descartes.

Los ingredientes revelados mediante **Ilusión** y no conservados se envían igualmente al descarte.

Cuando el mazo queda vacío:

1. se toma toda la pila de descartes;
2. se convierte en el nuevo mazo;
3. se baraja;
4. continúa el robo normalmente.

Por tanto, los ingredientes no desaparecen del juego al utilizarse.

---

# 6. Mercado

El Mercado mantiene **5 cartas visibles**.

Cada compra se repone inmediatamente desde el mazo de Mercado.

Comprar no consume ninguna acción.

En la versión actual del prototipo un jugador puede realizar **varias compras durante su turno**, siempre que tenga monedas suficientes.

## 6.1 Distribución del mazo de Mercado

El mazo contiene **48 cartas**:

- 12 × 2 Hierbas Rojas;
- 12 × 2 Aguas Puras;
- 8 × 2 Minerales Sulfurosos;
- 8 × 2 Cristales de Aire;
- 8 × 1 Polvo de Hueso.

## 6.2 Precios

| Carta | Precio |
|---|---:|
| 2 Hierbas Rojas | 2 monedas |
| 2 Aguas Puras | 2 monedas |
| 2 Minerales Sulfurosos | 3 monedas |
| 2 Cristales de Aire | 3 monedas |
| 1 Polvo de Hueso | 4 monedas |

Los ingredientes adquiridos pasan inmediatamente a la mano.

---

# 7. Límite de mano

Cada jugador puede terminar su turno con un máximo de **10 ingredientes**.

Durante el turno puede superar temporalmente ese límite.

Antes de finalizar el turno debe descartar ingredientes hasta quedarse con **10 o menos**.

Los descartes van a la pila de descartes del mazo de ingredientes.

La cantidad total de ingredientes de cada jugador puede ser información pública.

Los tipos concretos de ingredientes que posee cada jugador son información privada.

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
   - 3 ingredientes aleatorios;
   - sus 9 hechizos;
   - 0 Poder Arcano;
   - 0 Escudo.
7. Elegir al jugador inicial.

---

# 9. Flujo de turno

## 9.1 Inicio del turno

Al comenzar un turno se resuelven los efectos cuyo disparador sea el inicio del turno.

Actualmente esto incluye especialmente:

- Quemaduras sobre jugadores;
- activación del límite de hechizos provocado por Raíces;
- otros estados que puedan añadirse más adelante.

Después el jugador roba su ingrediente normal del turno.

## 9.2 Robo

El jugador roba **1 ingrediente**.

## 9.3 Mercado

El jugador puede realizar compras mientras tenga monedas y existan cartas disponibles.

El Mercado se repone después de cada compra.

## 9.4 Fase principal

Durante la fase principal el jugador puede:

- comprar;
- lanzar hechizos contra criaturas;
- lanzar hechizos contra jugadores;
- curarse;
- obtener Escudo;
- utilizar hechizos de control;
- encadenar varios hechizos;
- repetir un hechizo si puede volver a pagar su coste.

No existe un límite global normal de hechizos por turno.

La excepción actual es **Raíces de Tierra**, que puede limitar temporalmente a un jugador a un único hechizo.

## 9.5 Final del turno

Antes de pasar el turno:

1. el jugador debe quedarse con un máximo de 10 ingredientes;
2. se resuelven las Quemaduras activas sobre criaturas;
3. se eliminan los estados de criatura que duren hasta final del turno, como Raíces y Corrosión;
4. se limpian los estados temporales de jugador que deban expirar;
5. pasa el turno al siguiente jugador vivo.

---

# 10. Lanzamiento de hechizos

Para lanzar un hechizo:

1. comprobar que el jugador puede lanzar hechizos;
2. comprobar que posee los ingredientes necesarios;
3. elegir un objetivo válido cuando sea necesario;
4. pagar los ingredientes;
5. enviar esos ingredientes al descarte;
6. resolver el efecto;
7. aplicar estados;
8. añadir **1 punto de Maestría**;
9. comprobar si el hechizo sube de nivel.

No existe actualmente la regla de “un uso por hechizo y turno”.

---

# 11. Maestría

Cada hechizo mejora al utilizarse.

| Nivel | Requisito |
|---|---:|
| Nivel 1 | Inicial |
| Nivel 2 | 3 usos |
| Nivel 3 | 6 usos |

La Maestría es independiente para cada hechizo y jugador.

La mejora se produce inmediatamente al alcanzar el requisito.

---

# 12. Hechizos

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

Inflige **3 de daño** y aplica **1 acumulación de Quemadura**.

### Quemadura en jugadores

Cada acumulación:

- dura **2 turnos del jugador afectado**;
- inflige **1 de daño al comienzo de cada uno de esos turnos**;
- el daño se aplica primero al Escudo;
- se acumula con otras Quemaduras;
- mantiene su propia duración independiente.

Varias Quemaduras pueden estar activas a la vez.

### Quemadura en criaturas

Cada acumulación:

- inflige **1 de daño al final de cada turno global**;
- dura **2 activaciones**;
- se acumula;
- conserva qué jugador la aplicó.

Las Quemaduras de una criatura se resuelven en el mismo orden en que fueron aplicadas.

Esto permite determinar correctamente quién obtiene la recompensa si una criatura muere por daño periódico.

Ejemplo:

```text
Jugador A aplica Quemadura A
Jugador B aplica Quemadura B
Criatura: 2 PV

Quemadura A → criatura queda a 1 PV
Quemadura B → criatura muere
→ Jugador B obtiene la recompensa
```

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

Inflige **3 de daño** e ignora completamente el Escudo.

La perforación de Plasma es una de sus principales diferencias frente a Bola de Fuego.

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

Recupera **4 PV** y limpia los estados negativos actuales del jugador.

Actualmente puede eliminar:

- Quemaduras;
- Corrosión;
- limitación de hechizos provocada por Raíces.

La curación nunca permite superar los PV máximos.

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

Obtienes **3 puntos de Escudo** y activas **Reflejo**.

### Escudo

- El Escudo absorbe daño antes de los PV salvo que un efecto indique lo contrario.
- Se acumula hasta un máximo de **6**.
- El Escudo no desaparece automáticamente al comenzar el siguiente turno.
- Permanece hasta ser consumido por daño u otro efecto.

### Reflejo

El siguiente hechizo ofensivo directo que impacte al jugador protegido devuelve **1 de daño** al lanzador.

- Se consume tras activarse.
- Puede activarse aunque el Escudo haya absorbido todo el daño del impacto.
- No se activa por Quemaduras.
- No se activa por daño periódico.
- No se activa por ataques de criaturas.
- No se activa por Explosión Ácida al tratarse de un efecto de área.

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

El ingrediente descartado va a la pila de descartes.

### Contra criaturas

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

- Inflige **0 de daño**.
- El objetivo solo podrá lanzar **1 hechizo** en su siguiente turno.

#### Nivel 2

- Inflige **1 de daño**.
- El objetivo solo podrá lanzar **1 hechizo** en su siguiente turno.

#### Nivel 3

- Inflige **2 de daño**.
- El objetivo solo podrá lanzar **1 hechizo** en su siguiente turno.

El jugador afectado puede utilizar ese único hechizo para defenderse, curarse o limpiar estados si dispone de un efecto adecuado.

### Contra criaturas

#### Nivel 1

Inflige **0 de daño** y enraíza.

#### Nivel 2

Inflige **1 de daño** y enraíza.

#### Nivel 3

Inflige **2 de daño** y enraíza.

Una criatura enraizada:

- no contraataca al recibir Raíces;
- no puede contraatacar ningún otro hechizo durante el resto del turno;
- recupera su capacidad de contraatacar al finalizar el turno.

---

## 12.7 ☠️ Drenaje Vital

**Coste**

- ☠️ Polvo de Hueso
- 🌿 Hierba Roja

**Objetivos**

- jugador;
- criatura.

### Nivel 1

- Inflige **2 de daño**.
- Cura **1 PV** al lanzador.

### Nivel 2

- Inflige **2 de daño**.
- Cura **2 PV** al lanzador.

### Nivel 3

- Inflige **3 de daño**.
- Cura **2 PV** al lanzador.

La curación no puede superar los PV máximos.

---

## 12.8 🧪 Explosión Ácida

**Coste**

- 💧 Agua Pura
- ☠️ Polvo de Hueso

Es un hechizo de área.

### Contra criaturas

Afecta a **todas las criaturas activas**.

#### Nivel 1

Inflige **1 de daño** a cada criatura.

#### Nivel 2

Inflige **2 de daño** a cada criatura.

#### Nivel 3

Inflige **2 de daño** a cada criatura y aplica **Corrosión** a las que sobrevivan.

Las criaturas no contraatacan por el daño de Explosión Ácida.

### Contra jugadores

Afecta a **todos los demás jugadores vivos**.

El lanzador nunca se daña con su propia Explosión Ácida.

#### Nivel 1

Inflige **1 de daño**.

#### Nivel 2

Inflige **2 de daño**.

#### Nivel 3

Inflige **2 de daño** y aplica **Corrosión**.

### Corrosión

Mientras un objetivo está corroído:

> recibe **+1 de daño** de los siguientes hechizos ofensivos.

La Explosión Ácida aplica Corrosión después de resolver su propio daño, por lo que no se beneficia de la Corrosión que acaba de aplicar.

En jugadores, Corrosión desaparece al final del turno del jugador afectado o puede ser eliminada por Curación Nv.3.

En criaturas, Corrosión dura hasta el final del turno actual.

---

## 12.9 🌀 Ilusión

**Coste**

- 💎 Cristal de Aire
- ☠️ Polvo de Hueso

**Objetivo**

- uno mismo.

### Nivel 1

- Revela **2 ingredientes**.
- Conserva **1**.
- El otro va al descarte.

### Nivel 2

- Revela **2 ingredientes**.
- Conserva ambos.

### Nivel 3

- Revela **3 ingredientes**.
- Conserva **2**.
- El restante va al descarte.

Los ingredientes conservados pueden hacer que el jugador supere temporalmente el límite de mano.

La IA resuelve Ilusión automáticamente y prioriza los ingredientes que considera más útiles para sus hechizos.

---

# 13. Estados actuales

Los estados principales del prototipo son:

```text
Burn / Quemadura
Rooted / Raíces
Corrosion / Corrosión
Reflect / Reflejo
Shield / Escudo
```

Los estados deben conservar, cuando corresponda:

- fuente;
- objetivo;
- duración;
- intensidad;
- momento de activación.

La fuente es especialmente importante para atribuir muertes causadas por efectos persistentes.

---

# 14. Criaturas

Siempre debe haber **3 criaturas visibles**.

Cada criatura dispone de:

- nombre;
- rango;
- PV actuales y máximos;
- ataque;
- habilidad;
- recompensa en monedas;
- recompensa en Poder Arcano;
- estados activos.

Las criaturas conservan el daño entre turnos.

Cuando una criatura muere:

1. no contraataca;
2. el jugador responsable recibe su recompensa;
3. se obtiene el Poder Arcano correspondiente;
4. la criatura se descarta;
5. se revela inmediatamente una nueva criatura.

---

# 15. Combate contra criaturas

Cuando una criatura sobrevive a un hechizo ofensivo dirigido contra ella, normalmente contraataca inmediatamente al lanzador.

El ataque de la criatura:

1. golpea primero al Escudo;
2. el daño restante reduce PV.

Excepciones actuales:

- una criatura enraizada no contraataca;
- Explosión Ácida no provoca contraataques;
- una criatura muerta no contraataca.

El jugador que inflige el golpe final recibe la recompensa.

Si la muerte se produce por Quemadura, la recompensa pertenece al propietario de la acumulación que produjo el golpe mortal.

---

# 16. Criaturas base

La lista de diseño contempla las siguientes criaturas.

## 16.1 Comunes

### 🐀 Rata de Sombra

- PV: 4
- Ataque: 1
- Recompensa: 1 moneda
- Poder Arcano: 1

### 🦇 Murciélago Viscoso

- PV: 4
- Ataque: 1
- Habilidad prevista: Vuela
- Recompensa: 1 moneda
- Poder Arcano: 1

### 💩 Gremlin de Barro

- PV: 4
- Ataque: 2
- Habilidad prevista: al morir, su asesino descarta 1 ingrediente aleatorio
- Recompensa: 2 monedas
- Poder Arcano: 1

### 💀 Esqueleto Roto

- PV: 5
- Ataque: 2
- Habilidad prevista: recibe 1 daño menos de Bola de Fuego
- Recompensa: 2 monedas
- Poder Arcano: 1

## 16.2 Intermedias

### 🐺 Lobo Corrompido

- PV: 6
- Ataque: 3
- Recompensa: 3 monedas
- Poder Arcano: 2

### 🌋 Guardián de Piedra

- PV: 7
- Ataque: 2
- Habilidad prevista: reduce en 1 el daño recibido
- Recompensa: 3 monedas
- Poder Arcano: 2

### 🧙‍♀️ Bruja del Pantano

- PV: 6
- Ataque: 2
- Habilidad prevista: presión sobre ingredientes
- Recompensa: 3 monedas + 1 ingrediente
- Poder Arcano: 2

### 🦴 Gólem Óseo

- PV: 8
- Ataque: 2
- Habilidad prevista: Regeneración
- Recompensa: 4 monedas
- Poder Arcano: 2

## 16.3 Épicas

### 🐉 Dragón de Azufre

- PV: 12
- Ataque: 4
- Habilidad prevista: resistencia a Bola de Fuego
- Recompensa: 6 monedas + carta de Mercado
- Poder Arcano: 3

### 🧬 Quimera Putrefacta

- PV: 10
- Ataque: 3
- Habilidad prevista: Veneno
- Recompensa: 5 monedas
- Poder Arcano: 3

### 👻 Señor Espectral

- PV: 9
- Ataque: 3
- Habilidad prevista: ignora Escudo
- Recompensa: 5 monedas + 1 Maestría
- Poder Arcano: 3

> En el prototipo actual ya están funcionando como criaturas de prueba la Rata de Sombra, el Gremlin de Barro y el Lobo Corrompido. Varias habilidades especiales del resto siguen pendientes de implementación.

---

# 17. Combate entre jugadores

Los hechizos que permitan objetivo jugador pueden utilizarse contra cualquier rival vivo válido.

Los ingredientes se pagan normalmente.

No existe un límite general de ataques.

Un jugador puede encadenar varios hechizos mientras:

- tenga ingredientes;
- siga vivo;
- no esté limitado por un estado;
- existan objetivos válidos.

El Escudo se aplica según las reglas de cada hechizo.

---

# 18. Resolución del daño

Como regla general:

1. determinar daño base;
2. aplicar bonificaciones, como Corrosión;
3. aplicar perforación de Escudo;
4. absorber con Escudo;
5. aplicar daño restante a PV;
6. resolver estados posteriores;
7. comprobar muerte;
8. atribuir recompensa o eliminación.

El daño mínimo es 0.

---

# 19. Escudo

El Escudo:

- se acumula;
- tiene un máximo de **6**;
- absorbe daño antes que los PV;
- no expira automáticamente al cambiar de turno;
- puede ser ignorado parcial o totalmente por algunos hechizos.

Quemadura de jugador daña primero el Escudo.

Rayo de Plasma puede perforarlo.

---

# 20. Eliminación de jugadores

Cuando un jugador llega a **0 PV**, queda eliminado.

Regla de diseño prevista:

- descarta sus ingredientes;
- pierde sus estados;
- deja de recibir turnos;
- el responsable de la eliminación obtiene **2 Poder Arcano**.

Si la muerte procede de un estado persistente, el crédito pertenece al jugador que aplicó el efecto responsable del daño mortal.

> Pendiente de implementación completa en Unity.

---

# 21. Poder Arcano

| Acción | Poder Arcano |
|---|---:|
| Derrotar criatura común | +1 |
| Derrotar criatura intermedia | +2 |
| Derrotar criatura épica | +3 |
| Eliminar a otro jugador | +2 |

El umbral de Coronación es **10 Poder Arcano**.

---

# 22. Coronación Arcana

Al alcanzar 10 Poder Arcano, un jugador se convierte en aspirante a la Coronación.

La regla prevista mantiene una Última Ronda para dar al resto de jugadores la oportunidad de:

- eliminar al aspirante;
- alcanzar también 10 Poder Arcano;
- preparar su propia victoria.

> Esta fase todavía no está implementada en el prototipo actual.

---

# 23. Desempates previstos

Si varios jugadores sobreviven a la Última Ronda con 10 o más Poder Arcano:

1. más Poder Arcano;
2. más PV;
3. más monedas;
4. mayor suma de niveles de hechizo;
5. más ingredientes;
6. si continúa el empate, victoria compartida en la beta.

---

# 24. IA del bot

La IA actual utiliza decisiones heurísticas.

Puede:

- evaluar criaturas;
- evaluar ataques contra jugadores;
- comprar varias veces en Mercado;
- curarse;
- generar Escudo;
- lanzar varios hechizos en un turno;
- respetar límites de hechizos;
- aprovechar daño y recompensas;
- utilizar Ilusión;
- escoger ingredientes útiles en Ilusión;
- descartar ingredientes al superar el límite de mano.

La IA vuelve a evaluar sus opciones durante el turno.

Actualmente prioriza especialmente:

- golpes letales;
- recompensas de criaturas;
- curación útil;
- protección;
- daño efectivo;
- compras que permiten completar hechizos.

---

# 25. Información pública y privada

## Pública

- PV;
- Escudo;
- monedas;
- Poder Arcano;
- cantidad de ingredientes;
- niveles y Maestría;
- estados;
- criaturas;
- Mercado.

## Privada

- composición exacta de la mano de ingredientes.

---

# 26. Aleatoriedad

La aleatoriedad se utiliza para:

- barajar mazos;
- robar ingredientes;
- descartes aleatorios;
- selección inicial;
- determinadas recompensas o efectos.

El daño base de los hechizos es determinista.

---

# 27. Principios de diseño

## 27.1 Los ingredientes son poder

Una mano grande representa opciones ofensivas y defensivas.

Con un límite de **10 ingredientes**, un jugador puede preparar turnos especialmente explosivos.

## 27.2 Atacar consume capacidad futura

No existe un coste artificial de acción.

Atacar consume ingredientes que podrían utilizarse para:

- curarse;
- protegerse;
- comprar tiempo;
- matar criaturas;
- preparar combos.

## 27.3 Los combos deben importar

Estados como Quemadura, Corrosión, Raíces y Reflejo deben alterar las decisiones del rival.

Ejemplo:

```text
Explosión Ácida Nv.3
→ rivales quedan corroídos
→ comienza una guerra de hechizos
→ cada hechizo ofensivo posterior gana +1 daño
```

## 27.4 El control limita, pero no elimina el turno

Raíces no impide jugar.

Obliga al afectado a escoger cuidadosamente su único hechizo.

## 27.5 Las criaturas generan conflicto indirecto

Las criaturas mantienen su daño.

Esto permite:

- preparar una muerte;
- robar una recompensa;
- dejar una criatura quemándose;
- obligar al rival a decidir si rematarla;
- competir por el Poder Arcano.

---

# 28. Ciclo de ingredientes

El ciclo correcto es:

```text
Mazo
 ↓
Robo
 ↓
Mano / Inventario
 ↓
Uso o descarte
 ↓
Pila de descartes
 ↓
Mazo agotado
 ↓
Barajar descartes
 ↓
Nuevo mazo
```

No deben existir ingredientes que desaparezcan al pagar un hechizo o resolver Ilusión.

---

# 29. Estado actual del prototipo Unity

Configuración actual:

```text
Jugadores: 2
- 1 humano
- 1 bot

PV iniciales: 15
Monedas iniciales: 3
Ingredientes iniciales: 3
Mano máxima al final del turno: 10
Escudo máximo: 6
Poder Arcano para Coronación: 10

Hechizos: 9
Ingredientes: 5
Criaturas visibles: 3
Cartas visibles de Mercado: 5
```

Sistemas actualmente construidos:

- mazo de ingredientes;
- descartes y reconstrucción del mazo;
- Mercado;
- libro de hechizos;
- Maestría;
- niveles;
- combate contra criaturas;
- PvP;
- bot;
- estados;
- Quemadura;
- Corrosión;
- Raíces;
- Reflejo;
- Escudo;
- selección de objetivos;
- Ilusión;
- recompensas de criaturas.

Pendiente fuera del sistema de hechizos:

- eliminación completa de jugadores;
- recompensa por matar jugadores;
- Coronación;
- Última Ronda;
- resolución de victoria;
- varias habilidades especiales de criaturas;
- batería completa de tests.

---

# 30. Objetivo del prototipo

El prototipo debe responder principalmente a esta pregunta:

> **¿Es divertido obtener ingredientes, decidir cuándo gastarlos, encadenar hechizos, disputar criaturas y presionar al rival?**

El objetivo inmediato no es construir todo el contenido final, sino validar el núcleo jugable.

---

# 31. Versión

```text
Game: Reinos de Azoth
Rules Version: Beta 2.2
Primary Target: Unity prototype
Players: 2-4
Initial Digital Prototype: 2 players
Victory Threshold: 10 Arcane Power
Max HP: 15
Max Shield: 6
Max Hand: 10
Visible Creatures: 3
Visible Market Cards: 5
Ingredient Deck: 108
Market Deck: 48
Spells: 9
```

---

# 32. Cambios principales respecto a Beta 2.1

- Mano máxima aumentada de **7 a 10**.
- Eliminado el límite de un uso por hechizo y turno.
- Se permiten múltiples compras de Mercado durante un turno.
- Escudo Arcano pasa a persistir hasta ser consumido y se limita a 6.
- Bola de Fuego Nv.3 pasa de 4 de daño a **3 + Quemadura acumulable**.
- Quemadura pasa a tener duración de 2 activaciones y puede acumularse.
- Quemaduras de criaturas conservan fuente y orden de aplicación.
- Curación Nv.3 limpia estados negativos.
- Escudo Arcano Nv.3 incorpora Reflejo.
- Látigo de Viento queda diferenciado entre PvP y criaturas.
- Raíces limita a jugadores a **1 hechizo** en su siguiente turno.
- Raíces impide todos los contraataques de una criatura durante el resto del turno.
- Explosión Ácida Nv.3 aplica Corrosión.
- Corrosión añade +1 al daño de hechizos ofensivos posteriores.
- Explosión Ácida dispone de resolución de área PvE y PvP.
- Ilusión dispone de resolución humana y automática para la IA.
- Los ingredientes utilizados o descartados regresan correctamente a la pila de descartes.
