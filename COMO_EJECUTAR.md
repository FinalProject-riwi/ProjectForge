# Cómo ejecutar ProjectForge (100% funcional)

## 1. Requisitos
- Docker y Docker Compose instalados.

## 2. Variables de entorno (.env)
El archivo `.env` ya viene configurado. Lo importante para la IA es tener **al menos una**
de estas claves válidas (el sistema usa fallback automático en este orden):

```
ANTHROPIC_API_KEY=PLACEHOLDER          # opcional
OPENAI_API_KEY=sk-...                  # ya configurada
GEMINI_API_KEY=AQ.A...                 # ya configurada
```

> Si las tres están en `PLACEHOLDER`, la app sigue funcionando: el wizard mostrará
> los patrones y librerías directamente desde la base de datos (sin sugerencias IA).
> Con OpenAI o Gemini configuradas, además verás las sugerencias marcadas con ✦ IA.

## 3. Arranque desde cero (IMPORTANTE)

Si ya habías ejecutado el proyecto antes, **borra el volumen viejo de la base de datos**.
Ese volumen guardaba un estado anterior en el que solo existía "Repository Pattern",
y por eso no aparecían el resto de patrones/librerías de Python, Java y TypeScript.

```bash
# Detener y BORRAR volúmenes (esto resetea la base de datos)
docker compose down -v

# Construir y levantar
docker compose up --build
```

La primera vez tarda un poco: descarga la imagen de SQL Server, compila la app .NET 10,
aplica las migraciones y ejecuta el **seeder idempotente** que garantiza que TODOS los
patrones y librerías existan.

## 4. Acceso
- App web: http://localhost:5000
- SQL Server: localhost:1433 (usuario `sa`)

## 5. ¿Qué quedó garantizado al 100%?

## 6. Nota de seguridad
Las claves que venían en el `.env` original (OpenAI, Gemini, GitHub) quedaron expuestas en
texto plano. **Revócalas/rótalas** desde sus consolas respectivas y genera unas nuevas para
la presentación. No subas el `.env` real a un repositorio público.

- **Patrones de diseño** completos por lenguaje (Python, Java, TypeScript) — ver el wizard.
- **Selección ÚNICA de patrón**: ahora solo se puede elegir UN patrón de diseño a la vez
  (radio buttons). Las librerías siguen permitiendo seleccionar varias.
- **Librerías recomendadas** completas por lenguaje.
- **IA con fallback** OpenAI → Anthropic → Gemini (OpenAI primero porque es tu key activa),
  tanto para sugerencias del wizard como para el chatbot. Si la IA falla, el banner ahora
  muestra **el motivo real** (ej: "OpenAI devolvió 401: invalid_api_key" o
  "insufficient_quota") en lugar del genérico "no disponibles", y cae a las opciones de BD.
- **La carpeta generada ahora refleja tus elecciones**:
  - Las **librerías** se escriben en el manifiesto real: `pom.xml` (Java), `requirements.txt`
    (Python) y `package.json` (TypeScript/JS) — antes en Java solo se descargaban al cache
    de Maven sin tocar el `pom.xml`, por eso la carpeta salía "vacía".
  - El **patrón** elegido genera su estructura de carpetas (ej. Repository → `domain/`,
    `repository/`, `infrastructure/`) y un archivo **`PATTERNS.md`** explicando lo aplicado.

## 6. Si la IA sigue sin funcionar

El banner ahora te dirá exactamente por qué. Los motivos típicos con OpenAI:
- `401 invalid_api_key`: la key es incorrecta o fue revocada → genera una nueva en
  https://platform.openai.com/api-keys y ponla en `OPENAI_API_KEY` del `.env`.
- `429 insufficient_quota`: la cuenta no tiene saldo/créditos → añade saldo o usa la key
  de Gemini (también con fallback).
- Tras cambiar el `.env`, reinicia: `docker compose down && docker compose up --build`.

## 7. Seguridad
Revoca y rota las claves que viajaron en el `.env` (OpenAI, Gemini, GitHub) y no subas el
`.env` real a un repositorio público.
