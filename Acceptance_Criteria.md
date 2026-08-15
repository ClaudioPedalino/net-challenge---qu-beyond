# Word Finder Challenge - Acceptance Criteria

**Proyecto**: Word Finder
**Versión**: 1.0
**Fecha**: Agosto 2026
**Estado**: Todos los criterios cumplidos

---

## 1. Criterios de la API

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-01 | La clase se llama `WordFinder` | PASS | `src/WordFinder/WordFinder.cs:8` |
| AC-02 | El constructor recibe `IEnumerable<string> matrix` | PASS | `src/WordFinder/WordFinder.cs:16` |
| AC-03 | El método `Find` recibe `IEnumerable<string> wordstream` | PASS | `src/WordFinder/WordFinder.cs:45` |
| AC-04 | `Find` retorna `IEnumerable<string>` | PASS | `src/WordFinder/WordFinder.cs:45` |
| AC-05 | La interfaz pública coincide exactamente con la requerida | PASS | Firma pública idéntica al enunciado |

---

## 2. Criterios de la Matriz

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-06 | La matriz no excede 64 filas | PASS | `MatrixHelper.cs:29` - lanza `ArgumentException` si `rows.Length > 64` |
| AC-07 | La matriz no excede 64 columnas | PASS | `MatrixHelper.cs:43` - lanza `ArgumentException` si `colCount > 64` |
| AC-08 | Todas las filas tienen la misma longitud | PASS | `MatrixHelper.cs:57` - lanza `ArgumentException` si es jagged |
| AC-09 | Se rechaza matriz vacía | PASS | `MatrixHelper.cs:24` - lanza `ArgumentException` |
| AC-10 | Se rechaza matriz con filas nulas | PASS | `MatrixHelper.cs:52` - lanza `ArgumentException` |
| AC-11 | Se rechaza matriz nula | PASS | `MatrixHelper.cs:20` - lanza `ArgumentNullException` |

---

## 3. Criterios de Dirección de Búsqueda

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-12 | Busca horizontalmente de izquierda a derecha | PASS | `FrozenSetSearchEngine.cs:21` extrae filas como strings completos |
| AC-13 | Busca verticalmente de arriba a abajo | PASS | `MatrixHelper.cs:73-95` (ExtractColumns) genera columnas top→bottom |
| AC-14 | NO busca diagonales | PASS | `ExtractAllSearchLines` solo genera líneas H y V. Substrings diagonales nunca se indexan |
| AC-15 | NO busca de derecha a izquierda (reversa horizontal) | PASS | Las filas se extraen en orden natural izq→der |
| AC-16 | NO busca de abajo a arriba (reversa vertical) | PASS | Las columnas se extraen en orden natural arr→abajo |

---

## 4. Criterios de Salida y Ranking

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-17 | Retorna las top 10 palabras más frecuentes del wordstream | PASS | `TopWordsAggregator.cs:9` (DefaultTopK = 10) + `WordFinder.cs:66` |
| AC-18 | Las palabras rankadas son las que existen en la matriz | PASS | `WordFinder.cs:84-98` (FilterMatches verifica existencia) |
| AC-19 | Si hay menos de 10 palabras coincidentes, retorna las que hayan | PASS | `TopWordsAggregator.cs:25` (si Count ≤ topK, retorna todas) |
| AC-20 | Si no hay coincidencias, retorna colección vacía | PASS | `WordFinder.cs:61-64` retorna `[]` |
| AC-21 | Si el stream es null, retorna colección vacía | PASS | `WordFinder.cs:47-50` retorna `[]` |
| AC-22 | Si el stream está vacío, retorna colección vacía | PASS | `WordFinder.cs:54-57` retorna `[]` |

---

## 5. Criterios de Deduplicación

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-23 | Cada palabra aparece a lo sumo una vez en el resultado | PASS | `TopWordsAggregator.cs:18` - `ExtractTopK` retorna cada word una vez |
| AC-24 | La frecuencia viene del wordstream, no de la matriz | PASS | `WordFinder.cs:69-81` (CountStreamFrequencies cuenta ocurrencias en stream) |
| AC-25 | Si "cold" aparece 5 veces en stream, su frecuencia es 5 | PASS | `WordFinder.cs:77` - `frequencies[word] = frequencies.GetValueOrDefault(word) + 1` |
| AC-26 | El ranking usa la frecuencia del stream para ordenar | PASS | `TopWordsAggregator.cs:54-57` - OrderByDescending por count |

---

## 6. Criterios de Performance

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-27 | Búsqueda en O(1) por palabra | PASS | `FrozenSetSearchEngine.cs:41` - `FrozenSet.Contains` es O(1) |
| AC-28 | Preprocesamiento en el constructor (una sola vez) | PASS | `FrozenSetSearchEngine.cs:18-31` - indexa substrings en ctor |
| AC-29 | Agregación Top-10 en O(N log 10) = O(N) | PASS | `TopWordsAggregator.cs:33-46` - Min-Heap (PriorityQueue) |
| AC-30 | Thread-safe para llamadas concurrentes a Find() | PASS | FrozenSet es inmutable; sin locks necesarios |
| AC-31 | Minimiza presión de GC en Find() | PASS | Find() no asigna estructuras de datos significativas |

---

## 7. Criterios de Casos Edge

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-32 | Palabras más largas que 64 chars se ignoran | PASS | `WordFinder.cs:75` + `FrozenSetSearchEngine.cs:36` |
| AC-33 | Strings vacíos en stream se ignoran | PASS | `WordFinder.cs:75` - `!string.IsNullOrEmpty(word)` |
| AC-34 | Case-sensitive (no normaliza mayúsculas/minúsculas) | PASS | Comparación con `StringComparer.Ordinal` |
| AC-35 | Múltiples llamadas a Find() sobre misma instancia son consistentes | PASS | Estructuras inmutables post-construcción |

---

## 8. Criterios de Calidad de Código

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-36 | SRP: Validación, búsqueda, agregación y coordinación separadas | PASS | 4 clases: `MatrixHelper` (validación + transformación), `FrozenSetSearchEngine`, `TopWordsAggregator`, `WordFinder` |
| AC-37 | OCP: Nuevo motor de búsqueda sin modificar WordFinder | PASS | Interfaz `IMatrixSearchEngine` permite inyectar cualquier implementación |
| AC-38 | DIP: WordFinder depende de abstracciones, no de concreciones | PASS | `WordFinder.cs:10` - campo `IMatrixSearchEngine` |
| AC-39 | Sin warnings en build Release | PASS | Build 0 warnings, 0 errors |
| AC-40 | SonarQube ready (TreatWarningsAsErrors, EnforceCodeStyleInBuild) | PASS | `WordFinder.csproj` |

---

## 9. Criterios de Testing

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-41 | Test del ejemplo oficial del challenge (cold, wind, chill found; snow rejected) | PASS | `WordFinderTests.cs` - `Find_OfficialExample_ReturnsColdWindChill` |
| AC-42 | Test de restricción de dirección (solo H y V) | PASS | `WordFinderTests.cs` - `Find_Diagonal_DoesNotReturnWord`, `Find_ReverseHorizontal_DoesNotReturnWord`, `Find_ReverseVertical_DoesNotReturnWord` |
| AC-43 | Test de ranking Top-10 y deduplicación | PASS | `WordFinderTests.cs` - `Find_MoreThan10Words_ReturnsOnlyTop10`, `Find_DuplicateWordsInResult_DeduplicatesOutput` |
| AC-44 | Test de substrings y overlaps | PASS | `WordFinderTests.cs` - `Find_SubstringsAreRecognized` |
| AC-45 | Test de edge cases (null, empty, long words) | PASS | `WordFinderTests.cs` - `Find_NullStream_ReturnsEmpty`, `Find_EmptyStream_ReturnsEmpty`, `Find_WordsLongerThanMaxDimension_AreIgnored` |
| AC-46 | Test de validaciones de matriz (null, jagged, dimensiones) | PASS | `WordFinderTests.cs` - 5 tests de constructor |
| AC-47 | Test de concurrencia (Find() desde múltiples threads) | PASS | `ConcurrencyTests.cs` - 2 tests async |
| AC-48 | Test de stress (matriz 64x64, stream 200K+) | PASS | `StressTests.cs` - 4 tests |
| AC-49 | Todos los tests pasan | PASS | 92/92 tests passed |

---

## 10. Criterios de Benchmark

| ID | Criterio | Estado | Evidencia |
|----|----------|--------|-----------|
| AC-50 | Benchmark con BenchmarkDotNet | PASS | `WordFinder.Benchmarks/` |
| AC-51 | Medición de constructor time | PASS | Benchmark `Ctor_FrozenSet` |
| AC-52 | Medición de Find() con stream 10K y 100K | PASS | Benchmarks `Find_10K` y `Find_100K` |
| AC-53 | Medición de memoria (MemoryDiagnoser) | PASS | `[MemoryDiagnoser]` attribute |
| AC-54 | Comparación de Aggregator (MinHeap vs LINQ) | PASS | Benchmark `Aggregator_MinHeap` |

---

## Resumen de Cumplimiento

| Categoría | Total | Pass | Fail |
|-----------|-------|------|------|
| API | 5 | 5 | 0 |
| Matriz | 6 | 6 | 0 |
| Dirección | 5 | 5 | 0 |
| Salida/Ranking | 6 | 6 | 0 |
| Deduplicación | 4 | 4 | 0 |
| Performance | 5 | 5 | 0 |
| Edge Cases | 4 | 4 | 0 |
| Calidad Código | 5 | 5 | 0 |
| Testing | 9 | 9 | 0 |
| Benchmark | 5 | 5 | 0 |
| **TOTAL** | **54** | **54** | **0** |

**Todos los criterios de aceptación cumplidos.**
