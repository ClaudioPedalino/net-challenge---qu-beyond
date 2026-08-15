# Análisis del Challenge: Word Finder

## 📋 Reconstrucción del Problema

Basándome en la descripción de la imagen, el challenge requiere:

```csharp
public class WordFinder
{
    // Constructor: recibe la matriz de caracteres
    public WordFinder(IEnumerable<string> matrix) { }
    
    // Método: busca palabras y retorna top 10 más frecuentes
    public IEnumerable<string> Find(IEnumerable<string> wordstream) { }
}
```

**Matriz de ejemplo:**
```
c o l d
w i n d
o o o o
l l l l
```

Palabras encontradas: "cold", "wind" (horizontal), posiblemente otras verticales como "cool", "will"

---

## 🔍 Preguntas Clave para Entender el Problema

### 1. **¿Qué significa "frecuencia"?**
```
Escenario A: La palabra "cold" aparece 2 veces en la matriz → cuenta 2
Escenario B: La palabra "cold" aparece en el stream 5 veces → cuenta 5
Escenario C: Solo importa si existe o no (0 o 1)
```

> **Mi interpretación:** Es el **Escenario A** - contamos cuántas veces aparece en la matriz, no en el stream. El stream es la "lista de búsqueda", no la fuente de conteo.

### 2. **¿Direcciones permitidas?**
```
→ Horizontal: izquierda a derecha (cold)
← Horizontal: derecha a izquierda (dloc?)
↓ Vertical: arriba a abajo (cool)
↑ Vertical: abajo a arriba (looc?)
```

> **Mi interpretación:** Solo **izquierda→derecha** y **arriba→abajo**. Lo contrario sería "buscar en reversa" y probablemente lo mencionarían explícitamente.

### 3. **¿Si el stream tiene palabras repetidas?**
```
Stream: ["cold", "cold", "wind"]
¿Busco "cold" una vez o dos veces?
```

> **Mi interpretación:** Debería **deduplicar el stream** primero. Si "cold" está 3 veces en el stream pero 1 vez en la matriz, el resultado es 1.

### 4. **¿Top 10 de qué conjunto?**
```
Matriz contiene: cold(2), wind(1), cool(1)
Stream contiene: cold, wind, hello, world, foo, bar...

¿Top 10 de LAS ENCONTRADAS o del stream completo con 0s?
```

> **Mi interpretación:** Top 10 de **las efectivamente encontradas**, ordenadas por frecuencia descendente.

---

## 🧩 Análisis de Complejidad

### Variables:
- **M** = filas de la matriz
- **N** = columnas de la matriz  
- **W** = cantidad de palabras en el stream
- **L** = longitud promedio de palabra

### Enfoque 1: Naive (Búsqueda directa)
```
Por cada palabra en stream:
    Por cada fila:
        Buscar substring
    Por cada columna:
        Buscar substring
```
**Complejidad:** `O(W × M × N × L)` ❌ Lento para matrices grandes

### Enfoque 2: Preprocesamiento con HashSet
```
En constructor:
    Extraer TODAS las subcadenas posibles
    Guardar en HashSet<string>

En Find:
    Por cada palabra en stream:
        Si está en HashSet → encontrada
```
**Complejidad:** 
- Constructor: `O(M × N²)` - extraer todas las subcadenas
- Find: `O(W)` - búsqueda O(1) por palabra

**Problema:** No contamos FRECUENCIA, solo existencia ❌

### Enfoque 3: Preprocesamiento con Dictionary (⭐ RECOMENDADO)
```
En constructor:
    Por cada fila:
        Extraer todas las subcadenas
        Dictionary[subcadena]++
    Por cada columna:
        Igual

En Find:
    Por cada palabra ÚNICA del stream:
        Si existe en Dictionary → agregar al resultado con su conteo
    Ordenar por frecuencia descendente
    Tomar top 10
```
**Complejidad:**
- Constructor: `O(M × N²)` 
- Find: `O(W + K log K)` donde K = palabras encontradas

### Enfoque 4: Trie
```
Construir Trie con todas las subcadenas de la matriz
Cada nodo guarda el conteo de palabras que terminan ahí
```
**Complejidad:** Similar al Dictionary pero con más overhead de memoria

---

## 📊 Trade-offs por Enfoque

| Enfoque | Tiempo Constructor | Tiempo Find | Memoria | Complejidad Código |
|---------|-------------------|-------------|---------|-------------------|
| Naive | O(1) | O(W×M×N×L) | Baja | Baja |
| HashSet | O(M×N²) | O(W) | Alta | Media |
| **Dictionary** | **O(M×N²)** | **O(W+KlogK)** | **Media-Alta** | **Media** |
| Trie | O(M×N²) | O(W×L) | Alta | Alta |

---

## 🎯 Patrones de Diseño a Considerar

### 1. **Strategy Pattern** - Para direcciones de búsqueda
```csharp
interface ISearchDirection
{
    IEnumerable<string> ExtractSequences(char[,] matrix);
}

class HorizontalSearch : ISearchDirection { }
class VerticalSearch : ISearchDirection { }
// Futuro: DiagonalSearch sin modificar WordFinder
```
**Justificación:** Open/Closed Principle - si mañana piden diagonales, agregamos una estrategia nueva.

### 2. **Builder Pattern** - Para construcción opcional
```csharp
var finder = new WordFinderBuilder(matrix)
    .WithDirection(Horizontal)
    .WithDirection(Vertical)
    .WithMaxResults(10)
    .Build();
```
**Justificación:** Si el challenge crece en requisitos, esto escala bien.

### 3. **Template Method** - Para el algoritmo de búsqueda
```csharp
abstract class BaseSearchStrategy
{
    public IEnumerable<string> Search(char[,] matrix)
    {
        var sequences = ExtractSequences(matrix); // abstract
        return ProcessSequences(sequences);        // concreto
    }
}
```

---

## 🏗️ Estructura de Código Propuesta

```
src/
├── WordFinder/
│   ├── WordFinder.cs                 # Clase principal (Facade)
│   ├── Models/
│   │   ├── SearchDirection.cs        # Enum
│   │   └── SearchResult.cs           # Record para resultado
│   ├── Strategies/
│   │   ├── ISearchStrategy.cs        # Interface
│   │   ├── HorizontalSearchStrategy.cs
│   │   └── VerticalSearchStrategy.cs
│   ├── Builders/
│   │   └── WordFinderBuilder.cs      # Opcional
│   └── Extensions/
│       ├── MatrixExtensions.cs       # Métodos de extensión
│       └── CollectionExtensions.cs   # TakeTop, etc.
├── WordFinder.Tests/
│   └── WordFinderTests.cs
└── WordFinder.Benchmarks/
    └── FinderBenchmarks.cs
```

---

## ❓ Preguntas que te pueden hacer en la defensa

1. **"¿Por qué preprocesas en el constructor y no buscas en cada Find?"**
   > Trade-off memoria/tiempo. Asumo que Find se llama múltiples veces con diferentes streams, así que amortizo el costo.

2. **"¿Qué pasa si la matriz es 1000x1000?"**
   > El preprocesamiento genera O(N³) subcadenas potencialmente. Podríamos optimizar con un límite de longitud máxima basado en las palabras del stream... pero eso requeriría conocer el stream en el constructor (diseño diferente).

3. **"¿Por qué Dictionary y no Trie?"**
   > Para este caso, las lookups son exactas (no prefijos). Dictionary tiene mejor cache locality y menos overhead en .NET. Trie sería mejor si buscáramos prefijos o autocomplete.

4. **"¿Manejas casos edge?"**
   > - Matriz vacía
   > - Stream vacío  
   > - Palabras más largas que la matriz
   > - Caracteres especiales/unicode
   > - Null inputs

5. **"¿Es thread-safe?"**
   > El Dictionary se llena en el constructor (single-thread) y solo se lee en Find. Dictionary en .NET es seguro para lecturas concurrentes una vez poblado.

---

## ✅ Próximos Pasos

1. **¿Confirmamos las interpretaciones del problema?** - Especialmente el tema de "frecuencia"
2. **¿Vamos con el enfoque Dictionary?** - O prefieres explorar otro
3. **¿Level de patrones de diseño?** - Quiero mantenerlo pragmático, no over-engineered
4. **¿Empezamos con el código?**

¿Qué te parece el análisis? ¿Hay algún punto que quieras profundizar o que veas diferente?