# CONTRIBUTING

## Antes de hacer push

1. Solo subes **tus archivos**, de **tu carpeta** (ver `STRUCTURE.md`). Nunca `git add .`.
2. Lo que subes está **completo y compila**.
3. **No subes `.meta`** (salvo Integración) ni escenas ajenas.
4. Revisas lo que vas a subir: `git diff --staged`.

## Orden para subir

```
git add <tus archivos>
git commit -m "[area] descripción corta #issue"
git pull
git push
```

- Nunca `git pull` con cambios sin commitear.
- Si al hacer pull se abre un editor con "Merge branch 'main'...", guardar y cerrar.
- Prohibido: `--force`, `rebase`, `reset --hard`, ramas.
- Si tu push rompe la compilación, arreglarla es tu prioridad.

## Mensaje de commit

```
[area] descripción corta #issue
```

- **area:** `arte`, `logica`, `unity`, `integracion`, `datos`, `docs`, `repo`, `fix`
- **descripción:** en minúsculas, qué hiciste, menos de 60 caracteres.
- **#issue:** el número de la tarjeta.

Ejemplo: `[logica] consumo de oxígeno por colono #8`

Con el commit `#n` basta: no hace falta escribir nada más. Los checks de la tarjeta los marca Gestión.

> ⚠️ Nunca escribir `closes`, `fixes` ni `resolves` antes de `#n`: cierran el issue solo.
