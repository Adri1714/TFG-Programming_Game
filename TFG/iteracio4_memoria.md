# 5.3.4. Quarta Iteració

Si les tres iteracions anteriors van establir el motor lògic, la fisicitat de les memòries i l'encadenament de nivells, aquesta quarta iteració s'ha centrat en fer el joc **llegible i guiat**: que el jugador sàpiga en tot moment on ha d'anar i quin component ha d'usar, i que la ALU comuniqui el seu estat de manera clara i interactiva. Les dues fites principals han estat el redisseny del panell visual de la ALU i la implementació d'un sistema de guia activa per a tots els dispositius de l'escenari.

---

## Panell visual de la ALU

A la iteració anterior, la ALU acceptava operands però no tenia cap interfície visual que indiqués al jugador el seu estat intern. En aquesta iteració s'ha redissenyat l'`ALUController` introduint una **màquina d'estats** i dos panells físics dins l'escenari.

L'script defineix quatre estats:

| Estat | Significat |
|---|---|
| `Idle` | Cap operand carregat; la ALU no fa res. |
| `ReadyToOpen` | Els dos operands han estat dipositats; s'espera que el jugador premi E per obrir el panell. |
| `AwaitingOperator` | El panell d'operador és visible; el jugador ha de triar +, −, × o ÷ i prémer Executar. |
| `ShowingResult` | El resultat s'ha generat; es mostra breument i la ALU es reinicia. |

Visualment, l'escenari té dos objectes solapats: `calculatorPanel` (estàtic, visible per defecte) i `iluminatedCalculatorPanel` (il·luminat, que s'activa quan els dos operands ja estan carregats). Aquest canvi de panell dóna un feedback visual immediat al jugador sense necessitat de cap element d'HUD extern.

Un punt rellevant és la **guàrdia d'ús**: la ALU comprova la propietat `CurrentTaskNeedsAlu` del `GameManager` abans d'acceptar cap interacció. Això evita que el jugador operi valors fora del moment correcte del codi (per exemple, que sumi variables abans d'haver-les declarat). Només quan l'expressió de la tasca actual conté un operador aritmètic (`+`, `−`, `*`, `/`) el `GameManager` activa aquest indicador.

---

## Sistema de guia visual

El repte pedagògic central del joc és que el jugador sàpiga, en tot moment, quin component de l'escenari necessita usar per executar la instrucció activa. Per resoldre-ho sense sobrecarregar la interfície amb text explicatiu, s'ha dissenyat un sistema de guia visual basat en el **patró Observer**: el `GameManager` publica canvis d'estat de tasca, i els dispositius de l'escenari s'hi subscriuen per il·luminar-se o apagar-se de manera autòmatica.

El sistema es compon de tres scripts que treballen conjuntament:

### `GuidedDevice`

Component que s'afegeix a cada dispositiu interactuable de l'escenari (ROM, RAM, ALU, Unitat de Salt, STDOUT). Declara a quina **categoria** pertany el dispositiu mitjançant l'enumeració `DeviceCategory`:

```csharp
public enum DeviceCategory { CodeMemory, WorkMemory, Output, JumpUnit, ALU }
```

Quan el `TaskGuideManager` determina que un dispositiu és rellevant per a la tasca activa, crida `SetGuided(true)`, que activa el component `InteractableHighlight` i, opcionalment, un indicador visual addicional (llum, halo, fletxa) configurable per escena a través de `activeIndicator`. En registrar-se (`OnEnable`) i desregistrar-se (`OnDisable`), `GuidedDevice` manté una llista estàtica centralitzada que el `TaskGuideManager` pot recórrer sense necessitat de referències explícites entre objectes.

### `TaskGuideManager`

És el cervell del sistema de guia. En iniciar-se, es subscriu a l'event `OnTaskStateChanged` del `GameManager`. Cada vegada que l'estat de tasca canvia, recorre tots els `GuidedDevice` registrats i determina si cadascun és rellevant per a l'estat actual:

- **`DIM_MEM` / `WRITE_MEM`**: s'il·luminen la memòria de codi (ROM) i la memòria de treball (RAM). Si la tasca requereix una operació aritmètica (`CurrentTaskNeedsAlu == true`), la ALU s'il·lumina també.
- **`WRITE_IO`**: s'il·lumina la unitat de sortida (STDOUT), i la ALU si cal calcular el valor.
- **`PRESS_JMP`**: s'il·lumina únicament la Unitat de Salt.
- **`NONE`**: tots els dispositius s'apaguen.

Aquest disseny desacobla completament la lògica de guia de la lògica de joc: afegir un nou tipus de dispositiu o un nou estat de tasca només requereix modificar la funció `IsRelevant` del `TaskGuideManager`, sense tocar cap dels scripts de dispositiu existents.

### `InteractableHighlight` (ampliat)

Aquest script ja existia des de la segona iteració per ressaltar en groc els objectes quan el jugador s'hi apropa. En aquesta iteració s'hi ha afegit un **segon mode de ressaltat**: el mode guia (`SetGuided`), que activa una emissió pulsant de color cian sobre el material de l'objecte. El pols es calcula amb una sinusoide sobre `Time.unscaledTime`, de manera que continua animant-se fins i tot quan el joc està en pausa:

```csharp
float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
mat.SetColor("_EmissionColor", guideColor * Mathf.Lerp(minEmission, maxEmission, t));
```

Els dos modes (proximitat i guia) són independents: un objecte pot estar alhora ressaltat en groc (perquè el jugador hi és a prop) i pulsant en cian (perquè és el dispositiu que toca usar), sense que cap dels dos interferexi amb l'altre.

---

## Canvis al `GameManager`

Per donar suport a les dues funcionalitats anteriors, s'han afegit tres elements al `GameManager`:

- **Propietat `CurrentTaskNeedsAlu`**: indica si el valor esperat de la tasca activa prové d'una expressió aritmètica. Es calcula cridant `HasOperator(expr)`, que comprova si l'expressió conté algun dels operadors `+`, `−`, `*`, `/`.
- **Event `OnTaskStateChanged`**: es llença cada vegada que l'estat de tasca canvia (tant quan s'assigna una nova tasca com quan es completa). El `TaskGuideManager` el escolta per saber quan actualitzar el ressaltat dels dispositius.
- **Mètode `HasOperator(string expr)`**: utilitat privada que retorna `true` si l'expressió requereix pas per la ALU, evitant duplicar aquesta lògica a múltiples llocs del codi.

---

## Estructura d'scripts d'aquesta iteració

La Figura X mostra el diagrama de classes dels scripts nous i modificats en aquesta iteració, posant èmfasi en les relacions entre ells. El patró Observer entre `GameManager` i `TaskGuideManager` és l'eix central del disseny: el `GameManager` no coneix els dispositius de l'escenari, i els dispositius no coneixen la lògica de joc; la comunicació es fa exclusivament a través d'events i de la interfície de registre de `GuidedDevice`.

> **[Figura X: Diagrama de classes de la quarta iteració]**
> *Inclou: `GameManager` (event `OnTaskStateChanged`, propietat `CurrentTaskNeedsAlu`, mètode `HasOperator`), `TaskGuideManager` (subscripció a l'event, llista estàtica de `GuidedDevice`, lògica `IsRelevant`), `GuidedDevice` (categoria, `SetGuided`), `InteractableHighlight` (modes `SetHighlight` i `SetGuided`), `ALUController` (màquina d'estats, panells visuals, guàrdia `CurrentTaskNeedsAlu`).*

Com a resultat, el jugador rep ara una retroalimentació contínua i sense ambigüitat sobre on ha d'actuar, reduint la fricció de navegació i permetent que l'atenció es pugui centrar en entendre el codi, que és l'objectiu pedagògic del joc.
