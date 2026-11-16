# Mars Miau House — NASA Space Apps Challenge 2026

A colony-management game on Mars, built on real NASA data. `Mars Cat` / `MarsCat` is the technical name of the repository and the Unity project.

## Summary

The humans had to evacuate their base on Mars after a meteor shower. Before leaving, they froze the genetically enhanced cats that worked with them inside the main cabin, in **Vivero crater** (48.97° N, 118.83° E, Utopia Planitia; NASA Mars Trek). The cats wake up with little food, little water and almost no power, guided by the base AI. To survive they have to repair the base one building at a time.

There is no villain: **the Martian environment is the antagonist**. Every building teaches a real problem of living on Mars, and every rule of the game comes from a NASA mission, an ISS experiment or a published study:

| Step | Building | What the player does | What it teaches |
|---|---|---|---|
| 1 | Solar panels | *Operation Solar Rescue*: blast dust off the panels with compressed CO₂ | Mars gets ~43 % of Earth's sunlight; dust cut Pathfinder's power by 0.28 % per day; water can't be used to clean because it boils at ~6 mbar |
| 2 | MOXIE | Keep its 5 filters clean | MOXIE made oxygen from Martian CO₂ on Perseverance; ISS crews vacuum their air filters every week |
| 3 | Water extractor | Melt buried ice: pressurize first, then heat | Subsurface ice (NASA SWIM, SHARAD); liquid water is not stable at Martian pressure |
| 4 | Plant farm | *Soil Factory*: wash the perchlorates out of the regolith and mix in compost | Phoenix found 0.4–0.6 % perchlorate in Martian soil; potato data from NASA BVAD |
| 5 | Rat farm | *The Raid*: catch the loose rats to start the farm | Cats are obligate carnivores; a finite resource runs out, so you have to move from hunting to farming |
| 6 | Organic recycler | *Bacteria Soup*: balance carbon and nitrogen (~30:1) and keep the compost at 55–65 °C | Waste is a resource: the loop closes back into compost for the plants |

Meanwhile the player watches the base air (O₂ %, with the OSHA and NIOSH thresholds of 19.5 %, 16 % and 10 %), the day–night cycle of a sol, power at night, and each cat's hunger and thirst. Cats are assigned to jobs automatically.

This hackathon build is the **minimum viable product**: the guided tutorial (8 steps) in the safe and middle zones of the base. It has no way to lose.

## How to play

1. Download the Windows build from the repository's **Releases** page.
2. Unzip it and run `Mars Miau House.exe`.
3. Follow the AI's missions in the top-right corner.

| Action | Control |
|---|---|
| Move the camera | `W` `A` `S` `D` / arrow keys, or drag with the right mouse button |
| Zoom / rotate | Mouse wheel / `Q` `E` |
| Select a building | Left click: opens its detail panel (repair, prepare) |
| Pause | `Space` or the pause button |
| Close a minigame | `Esc` |

Minigame controls are shown on screen. Examples: `Q` `W` `E` fire the three nozzles in *Solar Rescue*, and `Q` `E` (heat) plus holding `Space` (sprinkler) run the *Bacteria Soup*.

## Real data

- Every balance value lives in [`MarsCat/Assets/Data/balance_pmv.csv`](MarsCat/Assets/Data/balance_pmv.csv). Its `fuente` (source) column marks each one as a real reference, a design value derived from one, or a placeholder.
- The full list of NASA datasets, papers and links is in [`docs/diseno/05_datos_nasa.md`](docs/diseno/05_datos_nasa.md).
- Where no published value was found, the team says so. For example, the C:N ratio of cat feces is estimated at ~10:1.

## Open the project

1. Install **Git LFS** (`git lfs install`) and clone the repository.
2. Open the `MarsCat/` folder with **Unity 6000.6.7f1** from Unity Hub (use this exact version).
3. Open the scene `Assets/Scenes/Main.unity` and press Play.
4. The game logic is plain C# with NUnit tests: **Window → General → Test Runner → EditMode**.

| Folder | Content |
|---|---|
| `MarsCat/Assets/Scripts/Logic/` | Game rules in plain C#, with no Unity dependency (`MarsCat.Logic.*`), plus their tests |
| `MarsCat/Assets/Scripts/Unity/` | MonoBehaviours that connect the logic to the scene, the HUD and the minigames (`MarsCat.Unity.*`) |
| `MarsCat/Assets/Data/` | Balance values (`balance_pmv.csv`) and game texts (`texts_es.csv`) |
| `docs/diseno/` | Game design: vision, core loop, systems, economy, NASA data, lore, interface |
| `Art-source/` | Source art (Blender, OpenToonz, UI) |

## Use of AI

Most of the code was written with AI assistance: **Claude Code** (Anthropic), **Codex** (OpenAI) and **Gemini** (Google). The team designed the game, reviewed and integrated the code, and built the scenes and the art. Every script says which tool wrote it on its first line (`// Asistencia de IA: ...`).

## Built with

Unity 6 (URP, Input System, TextMeshPro) · C# · Blender · OpenToonz.

---

*Note: the rest of the documentation (`docs/`, code comments and the in-game texts) is in **Spanish**, because Mars Miau House was made by a Spanish-speaking development team.*
