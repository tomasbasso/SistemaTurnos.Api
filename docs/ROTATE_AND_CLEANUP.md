# Rotación de secretos y limpieza del historial (guía)

Importante: Antes de limpiar el historial, **rotar** y **revocar** las credenciales comprometidas (JWT key, SMTP credentials, API keys). Si no rotas antes, los secretos seguirán siendo válidos aunque los borres del historial.

## Paso 1 — Rotar credenciales
1. Genera nuevas credenciales (ej. nueva `Jwt__Key`, cuentas SMTP nuevas, tokens API).
2. Añade los nuevos valores a GitHub Secrets (Settings → Secrets → Actions) con los nombres usados por la app (`Jwt__Key`, `SMTP_USER`, `SMTP_PASS`, etc.).
3. Verifica despliegue en staging y prod con los nuevos secretos.
4. Revoke / invalidar las credenciales antiguas en los servicios correspondientes.

## Paso 2 — Detectar ocurrencias locales
- Instala gitleaks (https://github.com/zricethezav/gitleaks) y ejecuta:

  ```powershell
  gitleaks detect --source . --report-path gitleaks-local-report.json --redact
  ```

- Revisa `gitleaks-local-report.json` y confirma que no quedan secretos activos.

## Paso 3 — Eliminar secretos del historial (dos opciones)

Opción A — git-filter-repo (recomendado):
1. Instala git-filter-repo (https://github.com/newren/git-filter-repo). En Windows usar WSL o pip install git-filter-repo.
2. Ejecuta (ejemplo para borrar la clave `Jwt__Key` si apareció):

```powershell
# Clona un espejo del repo
git clone --mirror git@github.com:TU_ORG/TU_REPO.git
cd TU_REPO.git

# Ejecuta el filtro (ejemplo para quitar coincidencias de un patrón)
# Ajusta --replace-text para los patrones exactos si es necesario
git filter-repo --invert-paths --path example-file-to-remove.txt
# O para reemplazar texto/secretos (ver doc oficial para sintaxis replace-text)

# Pushea el repo limpio (FORZAR)
git push --force
```

Opción B — BFG (más amigable):
1. Descargar BFG jar (https://rtyley.github.io/bfg-repo-cleaner/).
2. Crear un archivo `passwords.txt` con las cadenas a eliminar (cada secreto en una línea).
3. Ejecutar:

```powershell
git clone --mirror git@github.com:TU_ORG/TU_REPO.git
cd TU_REPO.git
java -jar ..\bfg.jar --replace-text ../passwords.txt
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

**Advertencia:** Esto reescribe el historial; todos los colaboradores deberán forzar un nuevo clone o resetear sus copias locales.

## Paso 4 — Validar y endurecer CI
1. Tras limpiar el historial, corre `gitleaks detect` localmente para confirmar que no hay hallazgos.
2. Habilita en GitHub: Settings → Branches → Protección de rama `main` y marca como checks requeridos los workflows `Gitleaks scan` y `CodeQL`.
3. Añade monitoreo/alertas (Dependabot alerts y scans recurrentes).

## Paso 5 — Comunicación y documentación
- Avisar al equipo y documentar (tiempo de rotación, secretos rotados, usuarios impactados).
- Actualizar `SECURITY.md` con el resumen de la incidencia y pasos realizados.

---

He añadido scripts y pasos prácticos; abajo está una **checklist accionable** y ejemplos de comandos para rotar claves y limpiar historial.

## Checklist rápida (orden recomendado) ✅
1. **Rotar credenciales en los proveedores** (JWT secret, cuentas SMTP, API keys). Generá y guardá nuevas claves antes de borrar historial.
   - Recomendación para JWT (HS256): generar al menos 32 bytes. Ejemplo con OpenSSL:
     ```powershell
     # Genera 32 bytes base64 (recomendado)
     openssl rand -base64 32
     # O genera 32 bytes hex (64 hex chars)
     openssl rand -hex 32
     ```
   - Para GitHub (usando GH CLI):
     ```bash
     gh secret set JWT__Key --body "$(openssl rand -base64 32)" --repo OWNER/REPO
     gh secret set EmailSettings__Username --body "smtp-user" --repo OWNER/REPO
     gh secret set EmailSettings__Password --body "smtp-pass" --repo OWNER/REPO
     ```
2. **Verificar despliegue en staging** con las nuevas secrets antes de revocar las antiguas.
3. **Revocar credenciales antiguas** en los proveedores (inmediato una vez staging OK).
4. **Detectar ocurrencias locales**: correr `scripts/verify-no-secrets.ps1` o `gitleaks detect --source . --report-path gitleaks-local-report.json --redact`.
5. **Limpiar historial** (después de rotar y revocar): usar `scripts/cleanup-history.ps1 -RepoUrl 'git@github.com:OWNER/REPO.git' -SecretsFile .\\secrets-to-remove.txt`.
   - Ejecutá primero con `-DryRun` para validar.
   - Confirmá que `gitleaks detect` reporta 0 hallazgos en el espejo limpio antes de `git push --force`.
6. **Habilitar protecciones en GitHub**: marcar `Gitleaks scan` y `CodeQL` como checks requeridos en Branch protection para `main`.
7. **Comunicación**: avisar por email/Slack/issue a todo el equipo indicando que deben re-clonar o resetear sus clones locales y documentar tiempos.

## Scripts añadidos 🔧
- `scripts/cleanup-history.ps1`: helper interactivo para clonar espejo y ejecutar `git-filter-repo` o BFG con confirmaciones y `DryRun`.
- `scripts/verify-no-secrets.ps1`: ejecuta `gitleaks detect` y falla si hay hallazgos.
- `gitleaks.toml`: archivo base de configuración para el scanner (ajustalo según falsos positivos locales).

---

Si querés, preparo el PR con estos cambios (archivos añadidos + docs actualizados) listo para revisión; ¿querés que cree la rama y prepare el commit/PR ahora? (puedo hacer la rama y dejarla lista para que la revises antes de push).