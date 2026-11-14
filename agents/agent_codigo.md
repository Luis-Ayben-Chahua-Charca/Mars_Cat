# Perfil: Código

Para agentes que ayudan a los integrantes de **Lógica C#** y **Unity**. Solo escriben archivos `.cs` (y, para Lógica, datos `.json`/`.csv` si el issue lo pide).

## Reglas comunes

- Solo creas o editas `.cs` del **dueño con el que trabajas** y dentro de su carpeta (ver `STRUCTURE.md`).
- No creas `.meta`. Los genera el integrador al abrir Unity.
- No tocas escenas, prefabs, materiales ni `ProjectSettings/`. Si tu código necesita que algo se configure en el Editor (asignar una referencia, agregar un componente a un objeto), lo **describes** en el comentario para la tarjeta.
- Antes de escribir un sistema nuevo, revisa que exista el **contrato** en el issue (clase, namespace, métodos y eventos públicos). Si no existe, propón uno y espera confirmación.
- No cambias la firma pública (métodos, eventos, campos públicos) de una clase que ya otros usan sin avisar en el issue.
- Un archivo por clase; el nombre del archivo es igual al de la clase.
- El código debe compilar. Si no puedes comprobarlo, dilo explícitamente al humano.
- Nada de paquetes externos, `asmdef` nuevos ni cambios en `Packages/manifest.json` sin pedido explícito.

## Sección Lógica (C# puro)

- Carpeta: `MarsCat/Assets/Scripts/Logic/` — namespace `MarsCat.Logic.<Sistema>`.
- **Sin dependencias de Unity:** no `using UnityEngine`, no `MonoBehaviour`, no `ScriptableObject`. Clases C# normales.
- Exponer estado con propiedades y cambios con eventos C# (`event Action<...>`), para que Unity los escuche.
- Tiempo como parámetro (`Tick(float deltaTime)`), nunca leer `Time.deltaTime` directamente.
- Valores de balance (costos, tasas de consumo, capacidades) **no van hardcodeados**: se reciben en una clase de configuración o se leen de `MarsCat/Assets/Data/`.
- Datos NASA: lectores/parsers en `MarsCat.Logic.Data`. Documentar en un comentario la fuente exacta del dato (dataset, URL o archivo).
- Si el issue lo permite, incluir pruebas simples en `MarsCat/Assets/Scripts/Logic/Tests/` (NUnit, sin Unity).

## Sección Unity (MonoBehaviours)

- Carpeta: `MarsCat/Assets/Scripts/Unity/` (UI en `MarsCat/Assets/Scripts/Unity/UI/`) — namespace `MarsCat.Unity.<Área>`.
- Los MonoBehaviours son **adaptadores**: crean o reciben las clases de Lógica, llaman sus métodos y escuchan sus eventos. No duplican reglas del juego.
- Referencias del Editor con `[SerializeField] private`, no campos públicos.
- Configuración editable en el Editor mediante `ScriptableObject` en `MarsCat.Unity.Config`; el `.asset` lo crea el integrador.
- Evitar `Find`, `FindObjectOfType` y `SendMessage`; usar referencias serializadas o eventos.
- No crear objetos en escenas por código para "montar" la escena; eso lo hace el integrador.

## Al terminar

No escribas documentación aparte. Da al humano:

- El commit sugerido: `[logica|unity] descripción #n`
- Solo si aplica, un comentario corto para la tarjeta: pasos en el Editor para el integrador, cambios de contrato o pendientes.
