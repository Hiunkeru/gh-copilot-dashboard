# Guía: Dashboard de Auditoría de Uso de GitHub Copilot

## Contexto y Estado Actual del Ecosistema de Métricas

Antes de construir nada, es importante entender el panorama actual de las APIs de GitHub Copilot, que ha cambiado significativamente en los últimos meses.

### APIs disponibles (Marzo 2026)

GitHub ofrece actualmente **tres superficies de datos**, pero no son intercambiables:

| API | Estado | Granularidad | Formato | Recomendación |
|-----|--------|-------------|---------|---------------|
| **Copilot Usage Metrics API** (nueva) | ✅ GA (Feb 2026) | Enterprise / Org / User / Day | NDJSON | **Usar esta** |
| **Copilot Metrics API** (legacy) | ⚠️ Deprecada 2 Abril 2026 | Enterprise / Org / Team | JSON array | Migrar antes de abril |
| **Copilot User Management API** | ✅ Activa | Asignación de seats, `last_activity_at` | JSON | Complementaria para seats |

**La recomendación clara de GitHub es usar la nueva Copilot Usage Metrics API** para cualquier integración nueva. Es la que tiene granularidad a nivel de usuario individual por día, que es exactamente lo que necesitas para auditar quién usa y quién no.

### Métricas disponibles

Las métricas se agrupan en estas categorías:

**Adopción:**
- Daily Active Users (DAU) y Weekly Active Users (WAU)
- Usuarios activos por feature (completions, chat, agent mode, CLI)
- Agent adoption rate

**Engagement:**
- Profundidad de uso por feature
- Frecuencia de interacción
- Breadth across features (cuántas features usa cada usuario)

**Acceptance Rate:**
- Ratio de sugerencias aceptadas vs. totales
- A nivel de lenguaje, editor y modelo

**Lines of Code (LoC):**
- Líneas sugeridas, añadidas y eliminadas
- Desglose por completions, chat y agent
- Actividad iniciada por usuario vs. por agente

**Pull Request Lifecycle** (enterprise-level):
- PRs creadas totales vs. creadas por Copilot
- PRs revisadas totales vs. revisadas por Copilot

**Desglose dimensional:**
- Por lenguaje de programación
- Por editor/IDE (VS Code, JetBrains, Neovim, etc.)
- Por modelo de IA usado
- Por usuario individual (en reports user-level)

---

## Paso 1: Prerequisitos y Configuración en GitHub

### 1.1 Tipo de licencia necesario

Necesitas **GitHub Copilot Business** o **GitHub Copilot Enterprise**. La API de métricas no está disponible en planes individuales.

### 1.2 Habilitar la política de métricas

Alguien con rol de Enterprise Owner debe:

1. Ir a **github.com** → tu cuenta Enterprise
2. Pestaña **AI Controls**
3. Sidebar izquierdo: **Copilot**
4. Scroll hasta la sección **Metrics**
5. Cambiar **Copilot usage metrics** a **Enabled**

Sin este paso, ni el dashboard nativo ni la API devuelven datos.

### 1.3 Asegurar telemetría habilitada en los IDEs

**Dato crítico**: las métricas de uso del IDE solo se capturan si los usuarios tienen la telemetría habilitada en su editor. Si un dev la desactiva, no aparece en los informes. Esto es algo que debes comunicar al equipo.

- **VS Code**: `telemetry.telemetryLevel` debe estar en `all` o al menos no en `off`
- **JetBrains**: verificar que data sharing esté habilitado
- Los usuarios deben tener la **última versión** de su IDE y del plugin de Copilot

### 1.4 Crear el Personal Access Token (PAT)

Ve a **Settings → Developer settings → Personal access tokens → Tokens (classic)**:

- Scopes necesarios: `manage_billing:copilot` o `read:enterprise`
- Si usas **Fine-grained PAT**: necesitas el permiso `View Enterprise Copilot Metrics` (asignable vía custom enterprise role)
- Alternativa: GitHub App Installation (solo da acceso a user-level metrics vía `users-1-day`)

**Tip de seguridad para la app**: No hardcodees el PAT. Guárdalo en Azure Key Vault y referéncialo como variable de entorno o secreto de la aplicación.

### 1.5 Permisos: quién puede ver qué

Puedes crear un **custom enterprise role** con el permiso `View Enterprise Copilot Metrics` para dar visibilidad a personas como tú (IA Lead) sin necesidad de ser Enterprise Admin. También existe el equivalente a nivel de org: `View organization Copilot metrics`.

---

## Paso 2: Entender las Fuentes de Datos

Tienes **tres formas** de obtener los datos, dependiendo de tu enfoque:

### Opción A: Descarga manual del NDJSON (quick & dirty)

Desde el dashboard nativo de GitHub (Enterprise → Insights → Copilot usage), hay un botón de **descarga NDJSON**. Te da un fichero con registros diarios que puedes cargar en tu app.

**Pros**: No requiere código, inmediato.
**Contras**: Manual, no automatizable, ventana de 28 días.

### Opción B: API REST — Copilot Usage Metrics (recomendada)

Endpoints principales:

```
# Enterprise-level aggregado (28 días)
GET /enterprises/{enterprise}/copilot/metrics/reports/enterprise-28-day/latest

# Enterprise-level por día específico
GET /enterprises/{enterprise}/copilot/metrics/reports/enterprise-1-day?day=YYYY-MM-DD

# User-level (28 días) — ESTE ES EL CLAVE para auditoría individual
GET /enterprises/{enterprise}/copilot/metrics/reports/users-28-day/latest

# User-level por día
GET /enterprises/{enterprise}/copilot/metrics/reports/users-1-day?day=YYYY-MM-DD

# Organization-level equivalentes
GET /orgs/{org}/copilot/metrics/reports/org-28-day/latest
GET /orgs/{org}/copilot/metrics/reports/users-28-day/latest
```

**Flujo de la nueva API:**

1. Haces GET al endpoint → te devuelve `download_links` (URLs firmadas con expiración)
2. Descargas cada URL → obtienes ficheros **NDJSON** (una línea JSON por registro)
3. Parseas línea a línea (no es un JSON array estándar)

Ejemplo de respuesta del paso 1:
```json
{
  "download_links": [
    "https://example.com/copilot-usage-report-1.json",
    "https://example.com/copilot-usage-report-2.json"
  ],
  "report_start_day": "2026-02-18",
  "report_end_day": "2026-03-17"
}
```

**Headers necesarios:**
```
Accept: application/vnd.github+json
Authorization: Bearer <YOUR-TOKEN>
X-GitHub-Api-Version: 2026-03-10
```

### Opción C: Copilot Metrics API legacy + User Management API

Estos endpoints aún funcionan pero se cierran el 2 de abril 2026:

```
# Métricas agregadas por org
GET /orgs/{org}/copilot/metrics

# Métricas por team
GET /orgs/{org}/team/{team_slug}/copilot/metrics

# Seats y last_activity_at
GET /orgs/{org}/copilot/billing/seats
```

La API de seats sigue siendo útil como complemento: te da la lista completa de licencias asignadas con el campo `last_activity_at` (timestamp de la última actividad, retención de 90 días). Esto te permite cruzar "quién tiene licencia" con "quién aparece en las métricas de uso".

### Opción D: CSV de la página de Access Management

Desde GitHub.com → tu org → Copilot → Access Management, puedes descargar un **CSV** con datos de actividad por usuario. Es la opción más sencilla pero limitada en profundidad de métricas.

---

## Paso 3: Arquitectura de la Aplicación

Dado tu stack Azure y que son ~50 devs, te propongo dos caminos:

### Camino 1: Solución Ligera (MVP rápido)

**Stack**: Python + Streamlit o React SPA + Azure Static Web App

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  GitHub API      │────▶│  Python Backend   │────▶│  Dashboard UI   │
│  (Usage Metrics) │     │  (FastAPI/Flask)   │     │  (React/Streamlit)│
└─────────────────┘     └──────────────────┘     └─────────────────┘
                               │
                        ┌──────┴──────┐
                        │  SQLite /   │
                        │  CosmosDB   │
                        └─────────────┘
```

- **Ingesta**: Script Python que llama a la API, descarga NDJSON, parsea y almacena
- **Storage**: SQLite para MVP, CosmosDB o Azure SQL si quieres escalar
- **Dashboard**: Streamlit si quieres algo rápido, React + Recharts si quieres algo más pulido
- **Hosting**: Azure Static Web Apps (gratis para el frontend) + Azure Functions para el backend

### Camino 2: Solución con el Accelerator de Microsoft

Microsoft tiene un **solution accelerator** oficial en GitHub: `microsoft/copilot-metrics-dashboard`. Es una app Next.js que:

- Se conecta directamente a la API de Copilot
- Muestra acceptance rate, active users, adoption rate, seats info
- Permite filtrar por fecha, lenguaje, editor, team
- Se despliega en Azure con `azd up`
- Soporta scope enterprise u organization

**Repo**: `github.com/microsoft/copilot-metrics-dashboard`

**Limitación**: usa la API legacy (Copilot Metrics), así que necesitarás verificar si ya soporta la nueva API o planificar la migración antes de abril 2026.

### Camino 3: Solución con Carga de CSV/NDJSON

Si prefieres máxima flexibilidad y no depender de la API en tiempo real:

```
┌────────────────────┐     ┌───────────────────┐     ┌─────────────────┐
│  Descarga manual   │     │                   │     │                 │
│  NDJSON / CSV      │────▶│  App React/Next   │────▶│  Dashboard con  │
│  desde GitHub      │     │  con file upload   │     │  Recharts/D3    │
└────────────────────┘     └───────────────────┘     └─────────────────┘
```

Esto es lo más portátil: sin secrets, sin backend, el usuario sube el fichero y la app lo procesa en el cliente.

---

## Paso 4: Modelo de Datos

### Schema del NDJSON user-level

Cada línea del fichero NDJSON de user-level contiene campos como:

```json
{
  "date": "2026-03-15",
  "user_login": "jdoe",
  "is_active_user": true,
  "is_engaged_user": true,
  "copilot_ide_code_completions": {
    "is_engaged_user": true,
    "editors": [
      {
        "name": "vscode",
        "is_engaged_user": true,
        "models": [
          {
            "name": "default",
            "is_custom_model": false,
            "languages": [
              {
                "name": "typescript",
                "total_code_suggestions": 142,
                "total_code_acceptances": 87,
                "total_code_lines_suggested": 310,
                "total_code_lines_accepted": 195
              }
            ]
          }
        ]
      }
    ]
  },
  "copilot_ide_chat": {
    "is_engaged_user": true,
    "editors": [...]
  },
  "copilot_ide_agent": {
    "is_engaged_user": false
  },
  "totals_by_cli": {
    "is_engaged_user": false
  }
}
```

### Tablas sugeridas para persistencia

**users**
| Campo | Tipo |
|-------|------|
| user_login | string (PK) |
| display_name | string |
| team | string |
| has_seat | boolean |
| seat_assigned_date | date |

**daily_usage**
| Campo | Tipo |
|-------|------|
| user_login | string (FK) |
| date | date |
| is_active | boolean |
| is_engaged | boolean |
| completions_suggestions | int |
| completions_acceptances | int |
| completions_lines_suggested | int |
| completions_lines_accepted | int |
| chat_engaged | boolean |
| agent_engaged | boolean |
| cli_engaged | boolean |
| primary_editor | string |
| primary_language | string |

**daily_aggregate**
| Campo | Tipo |
|-------|------|
| date | date (PK) |
| total_active_users | int |
| total_engaged_users | int |
| total_suggestions | int |
| total_acceptances | int |
| acceptance_rate | float |

---

## Paso 5: Implementación del Data Pipeline

### 5.1 Script de ingesta (Python)

```python
import requests
import json
import os
from datetime import datetime, timedelta

GITHUB_TOKEN = os.environ["GITHUB_PAT"]
ENTERPRISE = "tu-enterprise-slug"
API_VERSION = "2026-03-10"

headers = {
    "Accept": "application/vnd.github+json",
    "Authorization": f"Bearer {GITHUB_TOKEN}",
    "X-GitHub-Api-Version": API_VERSION,
}

def fetch_user_metrics_28day():
    """Descarga el reporte user-level de 28 días."""
    url = f"https://api.github.com/enterprises/{ENTERPRISE}/copilot/metrics/reports/users-28-day/latest"
    resp = requests.get(url, headers=headers)
    resp.raise_for_status()
    data = resp.json()
    
    all_records = []
    for link in data["download_links"]:
        ndjson_resp = requests.get(link)
        ndjson_resp.raise_for_status()
        for line in ndjson_resp.text.strip().split("\n"):
            if line:
                all_records.append(json.loads(line))
    
    return all_records, data.get("report_start_day"), data.get("report_end_day")

def fetch_seats():
    """Obtiene la lista de seats asignados con last_activity_at."""
    url = f"https://api.github.com/orgs/{ORG}/copilot/billing/seats"
    seats = []
    page = 1
    while True:
        resp = requests.get(f"{url}?page={page}&per_page=100", headers=headers)
        resp.raise_for_status()
        data = resp.json()
        seats.extend(data.get("seats", []))
        if len(seats) >= data.get("total_seats", 0):
            break
        page += 1
    return seats

def flatten_user_record(record):
    """Aplana un registro NDJSON user-level a campos tabulares."""
    flat = {
        "date": record.get("date"),
        "user_login": record.get("user_login"),
        "is_active": record.get("is_active_user", False),
        "is_engaged": record.get("is_engaged_user", False),
    }
    
    # Completions
    completions = record.get("copilot_ide_code_completions", {})
    flat["completions_engaged"] = completions.get("is_engaged_user", False)
    
    total_suggestions = 0
    total_acceptances = 0
    total_lines_suggested = 0
    total_lines_accepted = 0
    editors_used = set()
    languages_used = set()
    
    for editor in completions.get("editors", []):
        if editor.get("is_engaged_user"):
            editors_used.add(editor["name"])
        for model in editor.get("models", []):
            for lang in model.get("languages", []):
                languages_used.add(lang["name"])
                total_suggestions += lang.get("total_code_suggestions", 0)
                total_acceptances += lang.get("total_code_acceptances", 0)
                total_lines_suggested += lang.get("total_code_lines_suggested", 0)
                total_lines_accepted += lang.get("total_code_lines_accepted", 0)
    
    flat["total_suggestions"] = total_suggestions
    flat["total_acceptances"] = total_acceptances
    flat["total_lines_suggested"] = total_lines_suggested
    flat["total_lines_accepted"] = total_lines_accepted
    flat["acceptance_rate"] = (
        total_acceptances / total_suggestions if total_suggestions > 0 else 0
    )
    flat["editors"] = ", ".join(sorted(editors_used))
    flat["languages"] = ", ".join(sorted(languages_used))
    
    # Chat
    chat = record.get("copilot_ide_chat", {})
    flat["chat_engaged"] = chat.get("is_engaged_user", False)
    
    # Agent
    agent = record.get("copilot_ide_agent", {})
    flat["agent_engaged"] = agent.get("is_engaged_user", False)
    
    # CLI
    cli = record.get("totals_by_cli", {})
    flat["cli_engaged"] = cli.get("is_engaged_user", False)
    
    return flat
```

### 5.2 Automatización con Azure Functions

Crea una **Timer-triggered Azure Function** que ejecute la ingesta diariamente:

```python
# function_app.py
import azure.functions as func
import logging

app = func.FunctionApp()

@app.timer_trigger(schedule="0 0 6 * * *", arg_name="timer")  # 6 AM UTC diario
def copilot_metrics_ingester(timer: func.TimerRequest):
    records, start, end = fetch_user_metrics_28day()
    flattened = [flatten_user_record(r) for r in records]
    # Guardar en Azure SQL / CosmosDB / Blob Storage
    save_to_storage(flattened)
    logging.info(f"Ingested {len(flattened)} records for {start} to {end}")
```

---

## Paso 6: Dashboard — Vistas y KPIs Clave

### 6.1 Vista de Adopción Global

KPIs principales:
- **Adoption Rate**: usuarios activos / usuarios con licencia × 100
- **DAU / WAU trend**: gráfico de línea temporal
- **Seats desperdiciados**: usuarios con licencia que no han usado Copilot en 7/14/30 días

### 6.2 Vista Individual (la tabla más importante para auditoría)

| Usuario | Última actividad | Días activo (28d) | Suggestions | Acceptances | Accept Rate | Chat | Agent | CLI |
|---------|-----------------|-------------------|-------------|-------------|-------------|------|-------|-----|
| dev01   | Hoy             | 22                | 1,240       | 876         | 70.6%       | ✅   | ✅    | ❌  |
| dev02   | Hace 15 días    | 3                 | 45          | 12          | 26.7%       | ❌   | ❌    | ❌  |
| dev03   | Nunca           | 0                 | 0           | 0           | —           | ❌   | ❌    | ❌  |

Categorías de usuario:
- 🟢 **Power User**: activo >80% de los días laborables, acceptance rate >50%
- 🟡 **Occasional**: activo entre 20-80% de los días
- 🔴 **Inactive**: activo <20% o sin actividad
- ⚫ **Never Used**: tiene licencia pero actividad = 0

### 6.3 Vista por Feature

- % de usuarios que usan completions vs. chat vs. agent mode vs. CLI
- Esto te muestra si la gente solo usa autocompletado o también explora chat/agent

### 6.4 Vista por Lenguaje y Editor

- Top lenguajes por suggestions/acceptances
- Distribución de editores (VS Code vs JetBrains)
- Útil para detectar si algún equipo no tiene el plugin instalado

### 6.5 Vista Temporal

- Tendencia de adoption rate semana a semana
- Correlación con eventos (formaciones, workshops de los IA Champions)

### 6.6 Vista de ROI

- **Líneas de código aceptadas por día** como proxy de productividad
- **Coste por usuario activo** = precio licencia mensual / usuarios que realmente usan
- **Ahorro estimado**: ratio acceptance × tiempo estimado ahorrado por sugerencia

---

## Paso 7: Soluciones Existentes que Puedes Reutilizar

### Microsoft Copilot Metrics Dashboard

- **Repo**: `github.com/microsoft/copilot-metrics-dashboard`
- **Stack**: Next.js, desplegable en Azure con `azd up`
- **Lo que hace**: acceptance rate, active users, adoption rate, seats, filtros por lenguaje/editor/team
- **Lo que falta**: la nueva API user-level, algunas visualizaciones custom
- **Veredicto**: buen punto de partida, fork y extiende

### GitHub Copilot Resources — Metrics Viewer

- **Repo**: `github.com/github-copilot-resources/copilot-metrics-viewer`
- **Stack**: Nuxt.js
- **Lo que hace**: visualización completa con filtros de fecha, team, export CSV, métricas de GitHub.com (Chat, PR Summaries)
- **Configuración**: variables de entorno `NUXT_PUBLIC_SCOPE`, `NUXT_PUBLIC_GITHUB_ORG`, etc.
- **Veredicto**: más completo que el de Microsoft para visualización, pero también basado en la API legacy

### Herramientas de terceros

- **DX (getdx.com)**: conector nativo para Copilot metrics, importa a su plataforma
- **Power BI**: mucha gente conecta directamente la API a Power BI (Power Query + NDJSON parsing con Python/Pandas)
- **Grafana**: si ya tienes Grafana en tu stack, puedes crear datasources custom

---

## Paso 8: Plan de Implementación Sugerido

### Semana 1: Fundamentos
1. Verificar tipo de licencia (Business/Enterprise)
2. Habilitar Copilot usage metrics en Enterprise settings
3. Crear PAT con scopes adecuados
4. Comunicar al equipo que habiliten telemetría en sus IDEs
5. Hacer las primeras llamadas a la API manualmente (curl) para validar acceso

### Semana 2: Data Pipeline
1. Escribir el script de ingesta Python
2. Decidir storage (SQLite para MVP, Azure SQL para producción)
3. Crear la Azure Function con timer trigger
4. Validar que los datos se descargan y parsean correctamente
5. Cruzar datos de la API de seats con los de usage metrics

### Semana 3: Dashboard MVP
1. Fork del Microsoft copilot-metrics-dashboard o crear React app desde cero
2. Implementar las vistas: adopción global, tabla individual, temporal
3. Añadir la carga de fichero NDJSON/CSV como alternativa a la API
4. Desplegar en Azure Static Web Apps

### Semana 4: Refinamiento y Rollout
1. Añadir categorización de usuarios (Power User / Occasional / Inactive / Never Used)
2. Implementar alertas (ej: usuario inactivo >14 días)
3. Crear report exportable (PDF/Excel) para compartir con management
4. Presentar dashboard al equipo de liderazgo
5. Establecer cadencia de revisión (semanal/quincenal)

---

## Paso 9: Consideraciones Importantes

### Latencia de datos
Los datos tardan hasta **2 días completos UTC** en estar disponibles. No esperes datos en tiempo real. Los drops en los últimos 2-3 días son normales y se estabilizan.

### Mínimo de 5 usuarios
La API legacy de métricas por org/team solo devuelve datos si hay al menos 5 miembros con licencia activa. La nueva API user-level no tiene esta restricción.

### Valor "Unknown"
En los detalles por editor/lenguaje/modelo, puedes ver valores `Unknown` cuando la telemetría del IDE no tiene suficiente detalle. Es normal — aparece en API/NDJSON pero se excluye del dashboard nativo.

### Privacidad y comunicación
Antes de desplegar un dashboard que muestre actividad individual, comunica al equipo:
- Qué datos se recogen (uso de herramienta, no contenido del código)
- Para qué se usan (adopción y enablement, no micromanagement)
- Que la telemetría debe estar habilitada
- El objetivo es ayudar, no vigilar

### Sunset de la API legacy
La API `/copilot/metrics` se cierra el **2 de abril de 2026**. Si usas herramientas basadas en ella (incluido el dashboard de Microsoft), planifica la migración ya.

---

## Paso 10: Integración con tu Programa de IA Champions

Este dashboard es una herramienta perfecta para los 5 IA Champions de APS:

- **Cada Champion puede ver las métricas de su squad** → detectar quién necesita ayuda
- **Correlacionar formaciones con picos de adopción** → medir impacto de las sesiones
- **Establecer targets de adopción** por squad (ej: 80% de usuarios activos en 3 meses)
- **Gamificación opcional**: ranking de squads por adoption rate o acceptance rate (con cuidado de que sea constructivo, no punitivo)

---

## Referencias

- [GitHub Copilot usage metrics (conceptos)](https://docs.github.com/en/copilot/concepts/copilot-usage-metrics/copilot-metrics)
- [Data available in Copilot usage metrics (campos)](https://docs.github.com/en/copilot/reference/copilot-usage-metrics/copilot-usage-metrics)
- [REST API endpoints for Copilot usage metrics](https://docs.github.com/rest/copilot/copilot-usage-metrics)
- [REST API endpoints for Copilot metrics (legacy)](https://docs.github.com/en/rest/copilot/copilot-metrics)
- [Reconciling metrics across sources](https://docs.github.com/en/copilot/reference/copilot-usage-metrics/reconciling-usage-metrics)
- [Microsoft Copilot Metrics Dashboard (repo)](https://github.com/microsoft/copilot-metrics-dashboard)
- [GitHub Copilot Metrics Viewer (repo)](https://github.com/github-copilot-resources/copilot-metrics-viewer)
- [Copilot metrics GA announcement (Feb 2026)](https://github.blog/changelog/2026-02-27-copilot-metrics-is-now-generally-available/)
