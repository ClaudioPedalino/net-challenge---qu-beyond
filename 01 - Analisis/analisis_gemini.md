Análisis Técnico y Plan de Arquitectura: Challenge "Word Finder"

Candidato: Senior .NET Developer

Objetivo: Desarrollar un análisis exhaustivo, diseño de arquitectura y plan de solución para el challenge de búsqueda de palabras en matriz de caracteres de alto rendimiento.

1. Entendimiento Profundo del Problema

Contexto y Requerimientos

Se debe implementar la siguiente interfaz exigida por el enunciado:

public class WordFinder
{
    public WordFinder(IEnumerable<string> matrix)
    {
        // Constructor: Recibe la matriz de caracteres
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        // Retorna las Top 10 palabras más repetidas del wordstream que se encuentren en la matriz
    }
}



Reglas e Invariantes Clave

Dimensiones de la Matriz: Máximo de $64 \times 64$ caracteres. Todas las cadenas tienen la misma longitud.

Direcciones de Búsqueda: Únicamente de Izquierda a Derecha (Horizontal) y de Arriba a Abajo (Vertical). No se requiere búsqueda diagonal ni en sentido inverso (Right-to-Left o Bottom-to-Top).

Naturaleza del wordstream: Se define como un "stream de palabras grande" (IEnumerable<string>). Puede contener miles o millones de elementos y debe procesarse con alta eficiencia de CPU y memoria (baja presión en el Garbage Collector).

Resultado Retornado:

Las Top 10 palabras más frecuentes dentro del wordstream que existan en la matriz.

Si no hay coincidencias, retornar un conjunto vacío (Enumerable.Empty<string>()).

Si se encuentran menos de 10 palabras válidas, retornar las que hayan.

2. Análisis de Ambigüedades y Casos Borde (Crucial para la Defensa)

Durante la lectura de la consigna existe una regla clave a clarificar y justificar ante los evaluadores:

"If any word in the word stream is found more than once within the stream, the search results should count it only once" (o variaciones de conteo).

Interpretación Correcta

Aparición múltiple en la Matriz: Si la palabra "cold" aparece 2 veces en la matriz (ej. una vez horizontal y una vertical), no debe sumar doble ponderación. La matriz solo dictamina EXISTENCIA (true/false).

Frecuencia en el Stream: El conteo/frecuencia proviene de cuántas veces aparece dicha palabra en el wordstream.

Deduplicación del Conteo: Si la palabra "cold" viene en el wordstream 5 veces y existe en la matriz, su conteo final de frecuencia es 5. Si la palabra "snow" viene 10 veces pero no existe en la matriz, se ignora.

Casos Borde a Contemplar

wordstream es null o está vacío.

Matriz vacía o con dimensiones inconsistentes ($0 \times 0$, cadenas de distinto tamaño).

Palabras con longitud mayor a 64 caracteres (imposibles de encontrar en la matriz, se pueden descartar tempranamente).

Manejo de Mayúsculas/Minúsculas (Case Sensitivity): Asumiremos Ordinal o Case Insensitive según configuración, siendo Ordinal el estándar de performance.

Empates en el Top 10 (misma frecuencia entre la décima y undécima palabra).

3. La Clave de Rendimiento para un Senior: Ratio de Dimensiones ($64 \times 64$ vs Stream Infinito)

El mayor error de nivel Junior/Semi-Senior es iterar la matriz por cada palabra del stream ($O(N \times \text{Filas} \times \text{Cols})$).

Comparación de Escalas

Matriz: Máximo $64 \times 64 = 4,096$ caracteres.

64 Filas horizontales (longitud 64).

64 Columnas verticales (longitud 64).

Total de cadenas continuas en la matriz: Únicamente $128$ cadenas de 64 caracteres.

Word Stream: Cientos de miles o millones de palabras.

Estrategia de Preprocesamiento en el Constructor

Dado que la matriz es fija y minúscula comparada con el wordstream, el trabajo pesado debe realizarse en el Constructor WordFinder(matrix), preparando una estructura de datos ultrarrápida. Luego, la ejecución de Find(wordstream) consistirá en un lookup de $O(1)$ o $O(L)$ por cada palabra del stream.

4. Estrategias de Solución para los Benchmarks (3 o 4 Approaches)

Para justificar tu elección final mediante métricas con BenchmarkDotNet, evaluaremos los siguientes enfoques:

+-----------------------------------------------------------------------------------+
| APPROACH 1: Naive / String.Contains                                              |
| Extrae 128 cadenas (64 filas + 64 columnas) en el ctor.                          |
| En Find(), por cada palabra del stream hace 128 string.Contains().               |
| Complejidad: O(N * 128 * L) - Lento, alta asignación de memoria.                  |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| APPROACH 2: Substring Ingestion into HashSet / FrozenSet                         |
| Genera TODOS los substrings posibles de las 128 cadenas en el ctor y los          |
| guarda en un HashSet<string> (o FrozenSet<string> en .NET 8).                     |
| En Find(), cada palabra se evalúa con hashSet.Contains(word) -> O(1).             |
| Complejidad: Ctor O(128 * L^2), Find O(N). Extremadamente rápido.                |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| APPROACH 3: Compressed Trie / Suffix Trie de la Matriz                            |
| Inserta todas las filas y columnas en un Trie (Árbol de Prefijos/Sufijos).        |
| En Find(), se navega el Trie caracter por caracter -> O(L) por palabra.           |
| Complejidad: Ctor eficiente, Búsqueda O(N * L_palabra), sin allocations.         |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| APPROACH 4: Parallel & Zero-Allocation (PLINQ / Spans / MemoryPool)               |
| Mismo principio de búsqueda $O(1)$, pero paralelizando la lectura del stream      |
| en chunks y utilizando `ReadOnlySpan<char>` o `FrozenSet<string>` optimizado.     |
+-----------------------------------------------------------------------------------+



5. Diseño C#, Patrones y Buenas Prácticas (SOLID)

Principios SOLID Aplicados

Single Responsibility Principle (SRP):

MatrixIndexer / MatrixTransposer: Se encarga exclusivamente de convertir la matriz de entrada en las estructuras requeridas (extraer columnas, limpiar strings).

WordFinder: Encargado de coordinar la búsqueda y agregación de métricas/frecuencias.

TopWordsAggregator: Encargado de mantener el Top 10 de palabras sin ordenar la lista completa repetidamente (ej. usando un PriorityQueue / Min-Heap).

Open/Closed Principle (OCP) & Strategy Pattern:

Definir una interfaz IMatrixSearchEngine (ej. HashSetSearchEngine, TrieSearchEngine). Esto permite intercambiar algoritmos sin modificar WordFinder, perfecto para ejecutar los benchmarks.

Dependency Inversion Principle (DIP):

WordFinder depende de abstracciones de búsqueda e indexación.

Extension Methods Sugeridos

matrix.ExtractColumns(): Método de extensión sobre IEnumerable<string> para transponer las columnas a filas de forma elegante y diferida (yield return).

6. Plan de Acción Pasos a Seguir

Fase 1 (Acordada): Validar el entendimiento del problema y la estructura teórica.

Fase 2 (Siguiente paso): Diseñar el esqueleto de código en C# siguiendo arquitectura limpia y SOLID (Interfaces, Métodos de extensión, Implementación de WordFinder).

Fase 3: Crear la suite de Benchmarks con BenchmarkDotNet simulando streams masivos de datos (ej. 100,000 a 1,000,000 de palabras).

Fase 4: Redactar el documento final de entrega justificando métricas, consumo de RAM (Allocations/GC) y throughput (Ops/s).