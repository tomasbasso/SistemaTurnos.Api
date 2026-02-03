# security: rotation checklist + history-cleanup scripts

## Resumen
Este PR añade una guía práctica para la rotación de secretos, scripts seguros para verificar y limpiar el historial del repo, y una configuración base para `gitleaks`.

**Archivos añadidos:**
- `docs/ROTATE_AND_CLEANUP.md` (guía paso a paso)
- `scripts/cleanup-history.ps1` (helper interactivo para git-filter-repo / BFG, con DryRun)
- `scripts/verify-no-secrets.ps1` (ejecuta `gitleaks detect` y falla si hay hallazgos)
- `gitleaks.toml` (configuración base para gitleaks)

## Objetivo
Proveer una ruta reproducible y segura para:
1. Rotar y revocar credenciales comprometidas.
2. Detectar ocurrencias locales con `gitleaks`.
3. Reescribir historial (DryRun → verificación → push forzado coordinado).
4. Habilitar protecciones y monitoreo en GitHub.

## Pasos recomendados para validar (operador)
1. ROTAR credenciales en los proveedores (JWT, SMTP, API keys). Añadir las nuevas a GitHub Secrets (staging primero).
2. Ejecutar `scripts/verify-no-secrets.ps1` o `gitleaks detect --source . --report-path gitleaks-local-report.json --redact --config gitleaks.toml` y resolver cualquier hallazgo.
3. Preparar `secrets-to-remove.txt` con los secretos/patrones a eliminar.
4. Ejecutar `scripts/cleanup-history.ps1 -RepoUrl 'git@github.com:OWNER/REPO.git' -SecretsFile .\secrets-to-remove.txt -DryRun` y revisar las acciones propuestas.
5. Si DryRun OK, coordinar con el equipo y ejecutar la limpieza real (sin `-DryRun`) y confirmar `gitleaks` sobre el espejo limpio antes de `git push --force`.
6. Habilitar en GitHub Branch protection para `main` y marcar `Gitleaks scan` y `CodeQL` como checks requeridos.

## Checklist para reviewers
- [ ] Confirmar que la guía cubre la rotación previa a limpieza.
- [ ] Revisar scripts y validar que DryRun no hace push.
- [ ] Aprobar PR y coordinar ventana de mantenimiento con los owners antes del push forzado.

---

Si querés, puedo abrir el PR por vos (requiere `gh` y acceso) o te dejo los pasos listos para que lo hagas. Si preferís, (A) te guío para configurar SSH y pushear, o (B) cambiamos el remote a HTTPS y usás un PAT para pushear. 

---

### Mensaje sugerido para Slack/Issue (copiar/pegar)
> Se rotaron credenciales y se limpió el historial del repo. Por favor: re-clonar el repositorio:
> `git clone git@github.com:OWNER/REPO.git`
> Fecha/hora de la operación: YYYY-MM-DD HH:MM UTC
> Si tenés ramas locales, respaldalas antes de re-clonar. Contacto: @tu_usuario
