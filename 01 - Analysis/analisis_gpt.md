Este challenge está bastante bien diseñado porque **no busca solamente que encuentres palabras**. Busca evaluar varias cosas al mismo tiempo:

1. Capacidad de análisis.
2. Diseño de software.
3. Performance.
4. Calidad de código.
5. Capacidad para justificar decisiones técnicas.
6. Madurez como Senior.

De hecho, la frase del mail es una pista enorme:

> "Candidates are welcome to use AI-assisted coding tools ... but they should be prepared to explain and defend the final solution, including the logic, design decisions, and tradeoffs behind it"

Eso significa que probablemente valoren más tu razonamiento que la implementación final.

---

# 1. Entendamos exactamente el problema

Te entregan una matriz:

```text
abcdc
fgwio
chill
pqnsd
uvdxy
```

Y un stream enorme de palabras:

```text
cold
wind
snow
chill
...
millones más
```

Debes devolver:

```text
cold
wind
chill
```

Porque son las palabras que aparecen en la matriz.

---

## Restricciones importantes

### Sólo se busca

Horizontal:

```text
abcd
```

de izquierda → derecha

---

Vertical:

```text
a
b
c
d
```

de arriba → abajo

---

NO:

* diagonal
* derecha → izquierda
* abajo → arriba

Si implementás eso estarías resolviendo más de lo pedido.

---

### Tamaño de matriz

Máximo:

```text
64 x 64
```

Eso es importante.

Porque significa:

```text
4096 caracteres
```

Nada más.

Es muy pequeña.

---

### Tamaño del stream

Acá está la trampa.

Dice:

> large stream of words

No limita tamaño.

Puede ser:

```text
10
1000
1000000
10000000
```

palabras.

Por lo tanto:

**el problema NO está en la matriz.**

El problema está en el stream.

---

# 2. Qué devuelve Find()

Esto suele generar confusión.

La consigna dice:

> return the top 10 most repeated words from the word stream found in the matrix

Eso significa:

Supongamos:

```text
stream:

cold
cold
cold
wind
wind
chill
snow
```

Y en la matriz existen:

```text
cold
wind
chill
```

Resultado:

```text
cold
wind
chill
```

Ordenado por frecuencia:

```text
cold = 3
wind = 2
chill = 1
```

---

Además:

> If any word in the word stream is found more than once within the stream, the search results should count it only once

Esta frase es ambigua.

Y de hecho muchos candidatos la interpretan distinto.

Yo prepararía una sección en el README explicando la interpretación elegida.

La interpretación más razonable es:

* la frecuencia se usa para calcular el top 10
* la palabra aparece una sola vez en el resultado final

Ejemplo:

```text
cold
cold
cold
```

Resultado:

```text
cold
```

no

```text
cold
cold
cold
```

---

# 3. Qué están evaluando realmente

Mucha gente hará algo así:

```csharp
foreach(word)
{
   buscarHorizontal(...)
   buscarVertical(...)
}
```

Y listo.

Funciona.

Pero es una mala solución para streams enormes.

---

Están evaluando si detectás que:

```text
Matriz = pequeña
Stream = enorme
```

Por lo tanto:

Hay que optimizar para muchas consultas.

---

# 4. Insight más importante

La matriz es inmutable.

Se construye una vez:

```csharp
new WordFinder(matrix)
```

y después:

```csharp
Find(...)
Find(...)
Find(...)
Find(...)
```

puede ejecutarse miles de veces.

---

Eso sugiere:

### Preprocesar la matriz

en el constructor.

Invertir trabajo al inicio.

Reducir trabajo en cada búsqueda.

---

# 5. Enfoques posibles

Yo benchmarkearía al menos 4.

---

## Approach 1

Búsqueda ingenua

Para cada palabra:

```text
buscar en filas
buscar en columnas
```

Complejidad:

```text
O(words × matrix)
```

Simple.

Fácil.

Lento.

---

## Approach 2

Generar todas las filas y columnas

Constructor:

```text
row1
row2
row3

col1
col2
col3
```

Guardar:

```csharp
List<string>
```

Luego:

```csharp
Contains(word)
```

sobre cada línea.

Mejor.

Pero sigue siendo repetitivo.

---

## Approach 3

HashSet de todas las palabras posibles

Constructor:

Generar todos los substrings posibles de:

* filas
* columnas

Guardar:

```csharp
HashSet<string>
```

Luego:

```csharp
hashSet.Contains(word)
```

O(1)

---

Y acá aparece algo interesante:

Matriz máxima:

```text
64x64
```

Cantidad máxima de substrings:

```text
64 filas

64 * (64*65/2)
≈ 133.120
```

Duplicás por columnas:

```text
266.240
```

Muy poco.

Perfectamente viable.

---

Entonces:

Constructor:

```text
O(266k)
```

Una sola vez.

---

Búsqueda:

```text
O(1)
```

por palabra.

---

Para streams gigantes suele ser excelente.

---

## Approach 4 (el más interesante)

Trie

o mejor aún:

Aho-Corasick

---

Preprocesás la matriz.

Construís un automata.

Luego procesás palabras eficientemente.

---

Problema:

Para una matriz de 64x64 es probablemente overengineering.

---

Y ahí está la discusión senior:

> ¿Puedo hacerlo?
>
> Sí.
>
> ¿Debo hacerlo?
>
> Probablemente no.

---

# 6. Mi hipótesis de la solución ganadora

Si yo fuera reviewer, me impresionaría más:

### Solución principal

HashSet de todos los substrings posibles.

Porque:

* extremadamente rápida
* simple
* mantenible
* fácil de testear
* aprovecha la restricción 64x64

---

### Benchmark

Comparar:

| Approach           | Tiempo búsqueda | Memoria | Complejidad |
| ------------------ | --------------- | ------- | ----------- |
| Naive              | Alto            | Baja    | Baja        |
| Filas/Columnas     | Medio           | Baja    | Baja        |
| HashSet Substrings | Muy bajo        | Media   | Media       |
| Trie/Aho-Corasick  | Muy bajo        | Alta    | Alta        |

Y justificar por qué elegiste HashSet.

Eso demuestra criterio de ingeniería.

---

# 7. Qué artefactos enviaría yo

Además del código.

```text
/README.md
```

Explicando:

* interpretación de la consigna
* supuestos
* complejidades
* tradeoffs

---

```text
/tests
```

Unit tests.

Casos:

* palabra horizontal
* palabra vertical
* inexistente
* duplicados
* top 10
* matriz vacía
* stream vacío

---

```text
/benchmarks
```

BenchmarkDotNet.

Comparando:

* Naive
* RowColumn
* HashSet

---

```text
/docs
```

Diagrama simple:

```text
WordFinder
    |
    +--> MatrixIndexer
    |
    +--> WordSearchEngine
```

si finalmente tiene sentido la separación.

---

# Preguntas que probablemente te hagan

1. ¿Por qué no usaste Trie?
2. ¿Qué complejidad tiene tu solución?
3. ¿Por qué preprocesar la matriz?
4. ¿Qué pasa si la matriz fuera 10.000 x 10.000?
5. ¿Qué pasa si Find() se llama una sola vez?
6. ¿Qué consume más memoria?
7. ¿Por qué HashSet y no Dictionary?
8. ¿Cómo manejás duplicados?
9. ¿Cómo probaste performance?
10. ¿Qué cambiarías si el tamaño máximo de matriz desapareciera?

La respuesta a varias de esas preguntas surge directamente de entender el punto central del challenge:

> La matriz es pequeña y estable. El stream es potencialmente enorme y variable. Por lo tanto conviene invertir trabajo una vez en el constructor para minimizar el costo de búsqueda posterior.
