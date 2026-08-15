### Thought Process

**Core technical challenge:** No es buscar un string en una matriz (trivial) — es diseñar una clase reutilizable donde el constructor pre-procesa una matriz fija una sola vez, y `Find` puede invocarse N veces con streams de palabras potencialmente enormes, devolviendo el top-10 por frecuencia. El desafío real de "high performance" está en **dónde** pagás el costo computacional: en la construcción (una vez) vs. en cada `Find` (muchas veces).

**Edge cases identificados:** ambigüedad de ranking, duplicados en el stream, validación de matriz, mayúsculas/minúsculas, concurrencia.

**Impacto arquitectónico:** el diseño de la interfaz (ctor recibe matrix, `Find` se puede llamar múltiples veces) es la pista más fuerte del enunciado sobre qué optimizar.

---

## 1. Qué pide realmente el enunciado

**Entrada:**
- `matrix`: `IEnumerable<string>` — cada string es una **fila**, cada carácter una columna. Máx 64x64, todas las filas con igual longitud (garantizado por el enunciado, pero conviene validarlo defensivamente igual).
- `wordstream`: `IEnumerable<string>` — stream "grande" de palabras, con posibles repeticiones.

**Direcciones de búsqueda — solo 2, explícitas:**
- Horizontal, izquierda → derecha (dentro de una fila)
- Vertical, arriba → abajo (dentro de una columna)

**Sin diagonales, sin derecha→izquierda, sin abajo→arriba.** Esto está confirmado por la imagen: "snow" tiene una flecha diagonal en el diagrama y es la única palabra del stream que **no** aparece en el resultado (`chill`, `cold`, `wind` sí). Es una pista visual deliberada, no decorativa — vale la pena mencionarlo en el análisis que envíes, muestra que leíste el enunciado con precisión.

**Salida:** las top 10 palabras (por frecuencia en el stream) que efectivamente existan en la matriz. Si ninguna aparece, set vacío.

## 2. El punto ambiguo — y cómo lo vamos a defender

Esta frase es la trampa del challenge:

> *"the Find method should return the top 10 most repeated words from the word stream found in the matrix [...] If any word in the word stream is found more than once within the stream, the search results should count it only once"*

Interpretación que propongo (y que es la estándar para este challenge, que además es coherente con el resto del texto):

1. Contás la **frecuencia de cada palabra dentro del wordstream** (cuántas veces aparece "wind" en el stream de entrada).
2. Filtrás esa lista a las palabras que **existen en la matriz** (booleano: encontrada / no encontrada — no importa cuántas veces aparece *dentro de la matriz*, ni en cuántas direcciones).
3. Ordenás por esa frecuencia, descendente.
4. Devolvés el top 10, **cada palabra una sola vez** en el resultado — no un elemento repetido por cada ocurrencia en el stream.

O sea: "found more than once within the stream → count it only once" se refiere al **resultado de salida** (dedupe del output), no a la lógica de ranking, que sigue siendo por frecuencia. Es la única lectura que hace que "most repeated" tenga sentido — si no hubiera conteo de frecuencia, "most repeated" no significaría nada.

**Esto es exactamente el tipo de cosa que te van a preguntar en la entrevista** ("¿por qué interpretaste esto así?"), así que documentalo como una **assumption explícita** en tu entrega (README o comentario XML en la clase), no lo dejes implícito.

Otras assumptions a documentar:
- Case-sensitivity: asumo comparación exacta (case-sensitive) salvo que digan lo contrario — es más seguro que asumir case-insensitive sin evidencia.
- Empate en frecuencia dentro del top 10: orden estable, se resuelve por orden de primera aparición en el stream (hay que decidirlo y ser consistente, el enunciado no lo especifica).

## 3. Dónde está el verdadero cuello de botella de performance

Datos del enunciado:
- Matriz: **acotada y chica** — máx 64×64 = 4096 celdas. Esto es fijo y pequeño sin importar qué tan grande sea el desafío real.
- Wordstream: **"large"**, sin cota — potencialmente millones de palabras, y `Find` puede llamarse más de una vez sobre la misma matriz (así lo sugiere el diseño del constructor separado).

Conclusión: el costo que importa optimizar es el de **cada llamada a `Find`**, no la matriz en sí. La estrategia ganadora es:

- **Pre-procesar la matriz una sola vez en el constructor** (es chica, el costo de pre-procesamiento es irrelevante en términos absolutos, pero se paga una sola vez).
- Que cada `Find` resuelva membership de cada palabra en **tiempo proporcional a la longitud de la palabra**, no a la longitud/tamaño de la matriz — eso es lo que te permite escalar con streams gigantes y con múltiples llamadas a `Find`.

Esto descarta de entrada el enfoque naive (para cada palabra del stream, escanear la matriz con `string.Contains` sobre cada fila/columna) — sería O(palabras × tamaño de matriz) en cada `Find`, repitiendo trabajo de escaneo de matriz cada vez que se llama.

Familias de solución que tiene sentido comparar en tus benchmarks futuros (y que quiero que veas antes de que armemos código):

| Enfoque | Pre-proceso (ctor) | Costo por palabra en `Find` | Memoria extra |
|---|---|---|---|
| Naive (`Contains` fila/col por palabra) | O(1) | O(tamaño matriz) | ninguna |
| HashSet de todos los substrings de filas+columnas | O(filas × L²) | O(1) esperado | alta (~cientos de miles de strings) |
| Trie de substrings (filas+columnas) | O(filas × L²) | O(largo palabra) | media |
| Aho-Corasick construido *desde el wordstream* | — (se construye en `Find`) | amortizado, pero se reconstruye en cada llamada | depende del stream |
| Suffix Automaton generalizado (filas+columnas) | O(caracteres matriz) — óptimo | O(largo palabra) | baja (proporcional a la matriz, no al stream) |

La fila de Aho-Corasick es interesante pero **no encaja bien con la interfaz dada**: construye el autómata en función del stream, no de la matriz, así que si `Find` se llama varias veces con streams distintos, pagás la construcción del autómata cada vez — contradice la idea de "pre-procesar una vez en el constructor". Por eso mi candidato de "mejor solución teórica" es el Suffix Automaton generalizado, y el candidato de "mejor balance costo/complejidad para un límite de 64×64" es HashSet o Trie de substrings — esto es justo el tipo de trade-off que después vamos a poder sostener con números reales en el benchmark.

## 4. Boceto de arquitectura (sin código todavía)

Para que puedas defender con solidez tanto el diseño como el futuro benchmark de 3-4 approaches sin tocar la firma pública (`WordFinder` tal como la pide el enunciado):

- **Strategy Pattern**: la lógica de "¿esta palabra existe en la matriz?" se aísla detrás de una abstracción (algo como `IMatrixSearchIndex` con un método `Contains(string word)`). `WordFinder` no sabe *cómo* se busca, solo *que* se puede buscar. Esto es lo que te permite, a futuro, benchmarkear HashSet vs Trie vs Suffix Automaton sin tocar el contrato público que pide el challenge — cambiás la estrategia inyectada, no la clase.
- **SRP** aplicado en 3 responsabilidades separadas: validación/normalización de la matriz, el índice de búsqueda (la estrategia), y la agregación/ranking del stream (conteo de frecuencia + top 10).
- **Inmutabilidad**: una vez construido, `WordFinder` no debería mutar estado — el índice se construye una vez y es de solo lectura. Esto de paso te da **thread-safety gratis** si `Find` se llama concurrentemente, algo que vale la pena mencionar como criterio de diseño senior aunque el enunciado no lo pida explícitamente.
- **Extension methods** con sentido real (no por poner): por ejemplo, transponer la matriz (filas → columnas) es una operación reutilizable y testeable de forma aislada — buen candidato a extension method sobre `IEnumerable<string>`, en vez de método privado enterrado en la clase.

## 5. Lo que necesito confirmar con vos antes de seguir

Antes de pasar a diseño de clases concreto, dos preguntas que probablemente también te hagan a vos en la entrevista — mejor que las resolvamos ahora:

1. ¿Coincidís con la interpretación de "top 10 por frecuencia en el stream, deduplicado en el output" del punto 2? Es la que voy a asumir salvo que me digas otra cosa.
2. Para las palabras superpuestas (una palabra que cabe dentro de otra, ej. "cold" dentro de "scolder" si existiera en la matriz) — ¿asumimos que alcanza con ser substring contiguo exacto en la dirección permitida, sin exigir separadores/bordes de palabra? Coincide con cómo está planteado el ejemplo de la imagen, solo confirmo que estamos alineados.

Con esto ya tenemos el problema completamente mapeado. El próximo paso natural es que definamos juntos el diseño de clases/interfaces (todavía sin código, solo el "plano") antes de implementar la primera versión.