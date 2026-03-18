# Guia de Configuracion — Copilot Dashboard

## Indice

1. [Requisitos previos](#1-requisitos-previos)
2. [Configuracion en GitHub](#2-configuracion-en-github)
3. [Crear el Personal Access Token](#3-crear-el-personal-access-token)
4. [Configurar la aplicacion en local](#4-configurar-la-aplicacion-en-local)
5. [Arrancar en local con datos reales](#5-arrancar-en-local-con-datos-reales)
6. [Registrar apps en Entra ID (para produccion)](#6-registrar-apps-en-entra-id-para-produccion)
7. [Despliegue en Azure](#7-despliegue-en-azure)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Requisitos previos

- **Licencia**: GitHub Copilot **Business** o **Enterprise** (la API de metricas no esta disponible en planes individuales)
- **Rol**: Enterprise Owner o un custom role con el permiso `View Enterprise Copilot Metrics`
- **Herramientas locales**:
  - .NET 8 SDK (`dotnet --version`)
  - Node.js 20+ (`node --version`)
  - Git

---

## 2. Configuracion en GitHub

### 2.1 Habilitar metricas de uso de Copilot

Alguien con rol **Enterprise Owner** debe:

1. Ir a `github.com` > tu cuenta Enterprise
2. Pestana **Settings**
3. Sidebar izquierdo: **Copilot** > **Policies** o **AI Controls**
4. Buscar la seccion **Metrics**
5. Cambiar **Copilot usage metrics** a **Enabled**

> Sin este paso, la API no devuelve datos.

### 2.2 Asegurar telemetria habilitada en los IDEs

Las metricas solo se capturan si los usuarios tienen telemetria habilitada:

- **VS Code**: `telemetry.telemetryLevel` debe estar en `all` (no `off`)
- **JetBrains**: verificar que data sharing este habilitado
- Los usuarios deben tener la **ultima version** del plugin de Copilot

> Comunica al equipo que la telemetria debe estar activa. Si un dev la desactiva, no aparece en los informes.

### 2.3 (Opcional) Crear un custom role para acceso a metricas

Si no quieres dar acceso Enterprise Owner a quien administre el dashboard:

1. En Enterprise Settings > **Roles** > **New role**
2. Anade el permiso: `View Enterprise Copilot Metrics`
3. Asigna este role a la persona que gestionara el dashboard

A nivel de org existe el equivalente: `View organization Copilot metrics`.

---

## 3. Crear el Personal Access Token

### Opcion A: Classic PAT (mas sencillo)

1. Ve a **github.com** > Settings > Developer settings > **Personal access tokens** > **Tokens (classic)**
2. Click **Generate new token (classic)**
3. Nombre: `copilot-dashboard`
4. Expiration: elige segun tu politica (90 dias recomendado)
5. Scopes necesarios:
   - `manage_billing:copilot` — para metricas de uso
   - `read:org` — para leer seats y teams
   - `read:enterprise` — si consultas a nivel enterprise
6. Click **Generate token** y **copia el token** (no lo veras de nuevo)

### Opcion B: Fine-grained PAT

1. Ve a **Personal access tokens** > **Fine-grained tokens**
2. Resource owner: selecciona tu **enterprise** u **organization**
3. Permisos necesarios:
   - Organization permissions > **Copilot Business** > Read
   - Organization permissions > **Members** > Read

### Que puede ver el token

El token da acceso a metricas de **uso de herramienta** (sugerencias, aceptaciones, editores, lenguajes). **NO** da acceso al contenido del codigo de los desarrolladores.

### Donde se guarda

- **Local**: en `appsettings.Development.json` (este fichero esta en .gitignore)
- **Produccion**: en Azure Key Vault, referenciado como app setting

---

## 4. Configurar la aplicacion en local

### 4.1 Clonar el repositorio

```bash
git clone https://github.com/AirPlaneSolutions-IA/aps-dashboad.git
cd aps-dashboad
```

### 4.2 Configurar el backend

Edita el fichero `src/backend/CopilotDashboard.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "CopilotDashboard": "Debug"
    }
  },
  "GitHub": {
    "Enterprise": "tu-enterprise-slug",
    "Organization": "tu-org-slug",
    "Token": "ghp_xxxxxxxxxxxxxxxxxxxx",
    "ApiVersion": "2026-03-10",
    "LicenseCostPerMonth": 19
  }
}
```

**Donde encontrar los valores:**

| Campo | Donde encontrarlo |
|-------|-------------------|
| `Enterprise` | URL de tu enterprise: `github.com/enterprises/<ESTE-SLUG>` |
| `Organization` | URL de tu org: `github.com/<ESTE-SLUG>` |
| `Token` | El PAT creado en el paso 3 |
| `LicenseCostPerMonth` | 19 USD para Business, 39 USD para Enterprise |

> **IMPORTANTE**: `appsettings.Development.json` con datos reales NO debe subirse a git. El `.gitignore` ya excluye `appsettings.*.local.json` pero no este fichero por defecto. Si prefieres mas seguridad, usa `appsettings.Development.local.json` o variables de entorno.

Alternativa con variables de entorno (sin tocar ficheros):

```bash
export GitHub__Enterprise="tu-enterprise-slug"
export GitHub__Organization="tu-org-slug"
export GitHub__Token="ghp_xxxxxxxxxxxxxxxxxxxx"
```

### 4.3 Configurar el frontend

El fichero `src/frontend/.env.local` ya viene configurado para desarrollo:

```
NEXT_PUBLIC_API_URL=http://localhost:5182
NEXT_PUBLIC_DEV_MODE=true
```

No necesitas cambiar nada para dev local.

---

## 5. Arrancar en local con datos reales

### 5.1 Arrancar el backend

```bash
cd src/backend/CopilotDashboard.Api
dotnet run
```

Si el token esta configurado, veras en la consola:

```
info: CopilotDashboard.Api[0]
      GitHub token detected — syncing real data from GitHub API...
info: CopilotDashboard.Api.Services.GitHubCopilotService[0]
      Fetching user metrics 28-day from enterprises/tu-enterprise/copilot/metrics/reports/users-28-day/latest
info: CopilotDashboard.Api.Services.GitHubCopilotService[0]
      Received 2 download links for period 2026-02-18 to 2026-03-17
info: CopilotDashboard.Api[0]
      Real data sync completed
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5182
```

Si el token NO esta configurado, usara datos de ejemplo:

```
info: CopilotDashboard.Api[0]
      No GitHub token configured — using sample data.
```

### 5.2 Arrancar el frontend

En otra terminal:

```bash
cd src/frontend
npm run dev
```

### 5.3 Abrir el dashboard

Abre **http://localhost:3000** en tu navegador.

### 5.4 Re-sincronizar datos manualmente

Si quieres forzar un re-sync sin reiniciar el backend:

```bash
curl -X POST http://localhost:5182/api/sync/trigger
```

---

## 6. Registrar apps en Entra ID (para produccion)

> Solo necesario cuando vayas a desplegar en Azure. Para probar en local no hace falta.

### 6.1 Registrar la API (backend)

1. Ve a **Azure Portal** > **Microsoft Entra ID** > **App registrations** > **New registration**
2. Nombre: `Copilot Dashboard API`
3. Supported account types: **Single tenant** (tu organizacion)
4. Click **Register**
5. En la pagina de la app:
   - Copia el **Application (client) ID** y el **Directory (tenant) ID**
   - Ve a **Expose an API** > **Set Application ID URI**: `api://<client-id>`
   - Click **Add a scope**:
     - Scope name: `access_as_user`
     - Who can consent: **Admins and users**
     - Admin consent display name: `Access Copilot Dashboard`
     - Admin consent description: `Allows the SPA to call the Copilot Dashboard API`
     - State: **Enabled**

### 6.2 Registrar la SPA (frontend)

1. **New registration**
2. Nombre: `Copilot Dashboard SPA`
3. Supported account types: **Single tenant**
4. Redirect URIs: **Single-page application (SPA)**
   - `http://localhost:3000` (dev)
   - `https://<tu-static-web-app>.azurestaticapps.net` (prod)
5. Click **Register**
6. En la pagina de la app:
   - Copia el **Application (client) ID**
   - Ve a **API permissions** > **Add a permission** > **My APIs**
   - Selecciona `Copilot Dashboard API` > marca `access_as_user` > **Add permissions**
   - Click **Grant admin consent** (si tienes permisos de admin)

### 6.3 Configurar los IDs en la aplicacion

**Backend** (`appsettings.json` o app settings en Azure):

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<Directory (tenant) ID>",
    "ClientId": "<API Application (client) ID>",
    "Audience": "api://<API Application (client) ID>"
  }
}
```

**Frontend** (variables de entorno o `.env.production`):

```
NEXT_PUBLIC_DEV_MODE=false
NEXT_PUBLIC_API_URL=https://app-copilot-dashboard-prod.azurewebsites.net
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=<SPA Application (client) ID>
NEXT_PUBLIC_AZURE_AD_TENANT_ID=<Directory (tenant) ID>
NEXT_PUBLIC_AZURE_AD_API_CLIENT_ID=<API Application (client) ID>
```

---

## 7. Despliegue en Azure

> Detallado en una fase posterior. Resumen rapido:

1. Configurar secretos en GitHub:
   - `AZURE_CREDENTIALS` (service principal JSON)
   - `SQL_ADMIN_PASSWORD`
   - `SWA_DEPLOYMENT_TOKEN`
2. Configurar variables en GitHub Environments (dev/prod):
   - `ENVIRONMENT`, `API_URL`, `AZURE_AD_CLIENT_ID`, `AZURE_AD_TENANT_ID`, `AZURE_AD_API_CLIENT_ID`
   - `TF_RESOURCE_GROUP`, `TF_STORAGE_ACCOUNT`
3. Guardar el GitHub PAT en Azure Key Vault
4. Push a `main` triggers el CD pipeline

---

## 8. Troubleshooting

### La API devuelve 404 o datos vacios

- Verifica que las metricas estan habilitadas en Enterprise Settings (paso 2.1)
- Los datos tardan **hasta 2 dias** en estar disponibles. Si acabas de habilitar, espera 48h
- Verifica el enterprise slug: `curl -H "Authorization: Bearer ghp_xxx" -H "Accept: application/vnd.github+json" -H "X-GitHub-Api-Version: 2026-03-10" https://api.github.com/enterprises/TU-SLUG/copilot/metrics/reports/users-28-day/latest`

### Error 401 Unauthorized en la API de GitHub

- El token ha expirado o no tiene los scopes correctos
- Regenera el PAT con los scopes: `manage_billing:copilot`, `read:org`, `read:enterprise`

### Usuarios no aparecen en las metricas

- Tienen que tener la telemetria del IDE habilitada
- Tienen que haber usado Copilot en los ultimos 28 dias
- Si tienen `last_activity_at` en la API de seats pero no en metrics, el problema es la telemetria

### Valores "Unknown" en editores/lenguajes

- Normal cuando la telemetria del IDE no envia detalle suficiente
- Aparece en la API pero se filtra en el dashboard nativo de GitHub
- No es un error, es una limitacion de la telemetria

### Campos con valor 0 los primeros dias

- Comportamiento esperado: los ultimos 2-3 dias pueden mostrar datos incompletos
- El dashboard muestra `Data as of` con la fecha del ultimo dia completo

### La API legacy vs la nueva

Esta aplicacion usa la **nueva Copilot Usage Metrics API** (GA Feb 2026):
- Endpoints: `/enterprises/{slug}/copilot/metrics/reports/...`
- Formato: NDJSON (una linea JSON por registro)
- Header: `X-GitHub-Api-Version: 2026-03-10`

**NO** usa la API legacy (`/orgs/{org}/copilot/metrics`) que se cierra el 2 de abril de 2026.
