# Mars - Miau House: slide draft

- **Audience:** NASA Space Apps Challenge judges.
- **Length:** 7 slides in total, including the cover.
- **Status:** editable content to accompany the design team's concept visuals.
- **Label for concept images:** “Concept visual · Project in development”.
- **Language:** English throughout, including captions, diagram labels, and any visible interface text. Keep the project name “Mars - Miau House” unchanged.
- **Format:** widescreen 16:9.

The “On-screen text” sections contain the proposed slide copy. Notes and sources are working material and do not need to appear in full on the slides.

## Presentation generation instructions

Create exactly seven slides in English, including the cover, in 16:9 format. Use the supplied game artwork to guide the colors and visual style of the presentation. Keep text editable and preserve a consistent typographic hierarchy with strong contrast and readable captions.

Use only the “On-screen text” sections as slide copy. Treat design notes, asset paths, source descriptions, editing guidelines, and pending items as production instructions. Source names, credits, and links may appear as captions where explicitly requested. Do not copy production instructions or pending-item labels onto the slides.

Keep the slide 3 wireframe and the slide 6 architecture diagram editable. Preserve scientific screenshots unchanged and proportional. Clearly label concept visuals and planned features. Do not invent results, implementation status, scientific values, or links.

All asset paths below are relative to the Mars_Cat repository root. If using a presentation tool without access to this repository, attach the image files along with this draft. A written local path alone does not give an external tool access to an image.

## 1. Cover: the mission

### On-screen text

**MARS - MIAU HOUSE**

A colony of cats and a damaged base. Your mission: keep the colony alive.

Space mission training through a 2.5D management game.

*Junior Astronaut Mission Trainer*

### Design notes

- Use `diapos/Portada_videojuego.jpeg` as the main artwork. The audience should immediately understand who the protagonists are and what the player must do.
- The artwork already includes the game title. Use that title once; do not place a duplicate title over it.
- The current artwork includes Spanish menu buttons. The final cover needs an English or menu-free version from the design team. A layout crop may exclude the menu if it preserves the title and protagonists. Keep the original file unchanged; do not display Spanish menu labels in the final slide.
- Place the premise and mission-training description in a clear text area without covering the main characters or logo. Links belong on the final slide.

## 2. The problem and the golden rule

### On-screen text

**On Mars, no choice is free**

Do you use your energy to extract water or light your crops?

Each system supports another. When one fails, the entire colony feels the consequences.

**Learning science through decisions.**

### Design and content notes

The cat illustration can anchor a short failure sequence: dust, reduced solar power, and life support at risk once reserves run out. I would avoid the “two sols” deadline because it does not match the current technical specifications.

This approach also communicates the educational challenge more effectively than claiming, without evidence, that children find existing materials boring.

- Use `diapos/gato_daño.jpeg` and `diapos/gato_muerte.jpeg` as references for the consequences of system failure. Select a few clear stages rather than displaying every animation frame. These illustrate risks in the broader game concept; do not imply that the guided tutorial includes a defeat condition.

## 3. How it works: connected resources

### On-screen text

**The base works as an ecosystem**

**Energy**

Powers water extraction, MOXIE, and crop production.

**Water**

Sustains the cats and enables crops to grow.

**Food and oxygen**

Sustain the colony that repairs and operates the base.

**Waste**

Becomes compost for new crops.

**Your job is to keep the whole system in balance.**

### Design and content notes

- Show a simplified, editable wireframe of the general game interface. The GUI is still in development; present this as an interface concept rather than a finished gameplay screenshot.
- If a general GUI reference image is supplied with the assets, use it to guide the layout. No specific GUI image path has been assigned in this draft. Without a supplied reference, build a schematic wireframe around the resource relationships described here, without inventing finalized controls, numerical values, or features.
- Place the resource labels around the wireframe and use clear connectors to explain dependencies: energy supports water extraction, MOXIE, and crops; water supports cats and crops; food and oxygen sustain the working colony; waste becomes compost for crops.
- Use the visible caption “Interface wireframe · In development”. Keep labels and connectors editable; do not add explanatory paragraphs inside the wireframe.
- Use “connected resources” or “ecosystem” rather than “closed loop”: water recycling comes after the tutorial, and the base still needs energy and supplies.

## 4. Minigames: hands-on science

### On-screen text

**Every repair teaches a skill**

**Solar Rescue**

Clean with Martian CO₂. Every blast uses energy.

**Martian Alchemist**

Choose where to drill and control pressure and heat to obtain water.

**MOXIE: Oxygen Rescue**

Clear the filters to restore oxygen production. Maintenance takes time your colony needs elsewhere.

### Design and content notes

- Show exactly three minigames, each with its concept image above its title and description. Use balanced image areas and readable captions.
- Preserve image proportions. The MOXIE image is nearly square while the other two are widescreen; fit them within the layout without stretching or cutting off key controls.
- Label the images as concept visuals, not captured gameplay from a completed build.

| Minigame | Concept image |
| --- | --- |
| Solar Rescue | `diapos/Minijuego_limpieza_paneles_solares.jpeg` |
| Martian Alchemist | `diapos/Minijuego_taladro_recolector_agua.jpeg` |
| MOXIE: Oxygen Rescue | `diapos/Minijuego_limpieza_moxie.jpeg` |

The current Martian Alchemist image includes presentation controls in the bottom-left corner. Use a clean export from the design team for the final presentation. Keep the original image file unchanged.

## 5. Scientific foundation: from source to mechanic

### On-screen text

**NASA science turned into decisions**

**MOXIE**

Oxygen production from Mars' CO₂.

**SWIM**

Subsurface ice as a resource for exploration.

**VEG-05**

Light and fertilization as variables in space crop production.

**DONKI**

Solar events for future risk scenarios.

*Sources selected. Data integration in development.*

### Design and content notes

The composition should connect each source to a scientific fact and a player decision. This demonstrates traceability more effectively than listing institution names.

- Use the four supplied scientific images in `diapos/externos/` as source evidence. Keep each image intact and proportional: no cropping, stretching, recoloring, retouching, or rewriting its contents. Uniform resizing is allowed.
- Place the source name, linked reference, and corresponding gameplay statement outside each image. Keep credits already present in the images visible and add external credits where needed.
- The slide's main message must remain readable without reading the embedded website paragraphs or chart details. Use the four short source statements as the explanation; the screenshots provide supporting evidence.
- Give all four sources a clear place in the composition. Keep DONKI explicitly associated with future risk scenarios rather than an implemented live-data integration.

### Official sources and possible visual material

| Source | Supplied image | What it provides and how it could support the slide |
| --- | --- | --- |
| [MOXIE experiment](https://www.nasa.gov/solar-system/nasas-oxygen-generating-experiment-moxie-completes-mars-mission/) | `diapos/externos/Captura_moxie.png` | An explanation of oxygen extraction and a photograph of the instrument. Supports the scientific principle behind the building. |
| [SWIM ice map](https://science.nasa.gov/resource/swim-map-shows-subsurface-water-ice-on-mars/) | `diapos/externos/Mapa de hielo SWIN.png` | A downloadable map of regions with evidence of subsurface ice. Could illustrate the concept of searching for resources underground. |
| [VEG-05 investigation](https://science.nasa.gov/biological-physical/investigations/veg-05-rjtim/) | `diapos/externos/Captura_VEG-05.png` | Studies how light and fertilization affect tomatoes grown on the ISS. Supports variables in the minigame, but does not by itself establish potato yields on Mars. |
| [NASA CCMC: DONKI](https://ccmc.gsfc.nasa.gov/tools/DONKI/) | `diapos/externos/Captura_DONKI.png` | A catalog and access to space weather information. Could supply events for later stages, with an explicit adaptation for the game. |

These sources include datasets, maps, experiments, and research. The title “Scientific foundation” represents them accurately. The project's [NASA data register](../docs/diseno/05_datos_nasa.md) also identifies references awaiting verification and integration.

To highlight biological recycling, [MELiSSA](https://www.esa.int/articles/closed-loop-concept) provides a relevant reference on recovering resources from waste. It must be credited to **ESA**.

## 6. Architecture and AI use

### On-screen text

**A simulation designed to be tested**

**Pure C#**

Rules for resources, needs, and production.

**External CSV configuration**

Game balance values kept separate from code.

**Unity 2.5D**

Environment, interface, and minigames.

**Planned validation with NUnit**

Tests for consumption, priorities, and state changes.

**AI supports development, with team review.**

### Design and content notes

- Use a simple, editable architecture diagram with three main elements: external CSV configuration, the pure C# simulation, and the Unity 2.5D presentation layer.
- Show CSV values feeding the C# simulation. Show Unity reading simulation state and sending player actions back to the simulation.
- Place NUnit beside the C# simulation as the planned validation mechanism for consumption, priorities, and state changes. The diagram describes the intended architecture and does not claim that tests have already passed.
- Place the AI disclosure on a separate line below the diagram. Confirm its wording against the team's actual tools and uses before final submission.
- Use the project's architecture as evidence for this slide. Keep this technical diagram distinct from the scientific source images on slide 5.

## 7. Closing and educational impact

### On-screen text

**The next mission starts with a decision**

Mars - Miau House aims to help children ages 7–12 practice how to:

- **Prioritize limited resources.**
- **Recognize causes and consequences.**
- **Use science to solve problems.**

Start by repairing a base. Learn to plan a mission.

**Explore the project**

Demo · Source code

### Design and content notes

- Reserve approximately half the slide for the closing artwork and half for the educational message and project links.
- Suggested artwork brief: show the cats together beside a repaired, illuminated Martian base, with a hopeful mood and the same visual style as the supplied game art. Leave a quiet area for the text. Use the closing artwork once supplied; until then, reserve its area in the draft layout.
- Links will be added once public destinations are available. Do not invent URLs, QR codes, or working demo access. While links are pending, keep the access block as a production placeholder and omit it from any presentation shared with judges.

### Pending links

- Public demo: pending.
- Public repository: pending.

## Pending visual assets

- **Cover:** an English or menu-free version if the Spanish menu cannot be excluded through layout cropping.
- **Slide 3:** optional general GUI reference image; the slide itself must show an editable wireframe.
- **Slide 4:** a clean Martian Alchemist export without the presentation controls.
- **Slide 7:** closing artwork showing the cats and repaired base.

## Editing guidelines

- Each slide communicates one main idea.
- Slides 3 through 6 rely, respectively, on an interface wireframe with resource relationships, three gameplay actions, four connections to scientific sources, and a technical diagram.
- Consumption figures, catalog identifiers, and implementation details can accompany the submission in “Project Details” without competing with the visuals.
- Avoid presenting planned mechanics as implemented features.
- The draft avoids specifying the number of cats: the initial brief mentions 20, while the technical specifications call for 15 at the start.
- The tutorial is designed without a defeat condition. KRUSTY, automation, and the beacon crisis belong to later stages.

## Project documentation consulted

- [Tutorial scope and progression](../docs/diseno/02_core_loop.md).
- [Systems and minigames](../docs/diseno/03_sistemas.md).
- [Technical specifications and CSV configuration](../docs/06_guia_logica.md).
- [Scientific source register](../docs/diseno/05_datos_nasa.md).
