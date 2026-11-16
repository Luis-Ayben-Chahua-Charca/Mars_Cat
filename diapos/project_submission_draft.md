# Mars - Miau House: project submission draft

All proposed form responses are in English. Copy only the response text or the individual link fields. Editing notes, placeholders, and review items are not form responses.

This draft describes a project in development. Update its status only after confirming what the final submitted build actually supports.

## 1. Project Name

### Copy-ready response

Mars - Miau House

## 2. Summary

### Copy-ready response

Mars - Miau House is an educational 2.5D colony-management game in development for children ages 7–12. Players guide a colony of intelligent cats as they restore a damaged Martian base, balancing energy, water, food, and oxygen.

Designed for the “Build a Junior Astronaut Mission Trainer” challenge, the project turns mission-planning concepts into practical decisions. Planned activities include cleaning solar panels using local CO₂, choosing where to drill for ice, and maintaining oxygen-production filters. Each task exposes a trade-off: energy spent on one system is unavailable to another.

NASA resources, including MOXIE, SWIM, VEG-05, and Mars Trek, inform the scientific foundation and setting. DONKI is a reference for future space-weather scenarios. Our aim is to help young learners understand resource dependencies, maintenance, and cause and effect through an approachable story. The current submission presents the game design, concept visuals, and technical architecture; gameplay implementation and data integration are ongoing.

### Editing note

The form suggests 50–200 words and displays a 1,500-character limit. This response fits both limits. Recheck the character count after editing.

## 3. Project Demonstration

### Field value to complete

```text
[ADD_FULL_PUBLIC_URL_TO_THE_SEVEN_SLIDE_PRESENTATION]
```

### Editing notes

- Upload the presentation to a public hosting platform and paste its complete HTTPS URL into this field.
- The local presentation is [mars_miau_house_v1.pptx](mars_miau_house_v1.pptx). A local file path is not a public demonstration URL.
- The slide presentation contains seven slides, including the cover. Keep that total when revising it.
- Test the shared URL while signed out. It must open without a password, access request, or registration.
- Complete the SI image-labeling items in section 9 before publishing the presentation.

## 4. Project

### Candidate field value

```text
https://github.com/Luis-Ayben-Chahua-Charca/Mars_Cat
```

### Editing note

This address comes from the repository's configured origin. Anonymous public access was not verified. Use it only after confirming that judges can open the repository and access the final project without signing in. Otherwise, replace it with the complete URL of a publicly accessible project page or release.

## 5. Project Details

### Copy-ready response

### A mission-planning game for young learners

Mars - Miau House is an educational 2.5D management game being developed for children ages 7–12. A colony of intelligent cats wakes inside a damaged Martian base after the human crew evacuates. The player's mission is to restore the systems that keep the colony alive.

The project addresses “Build a Junior Astronaut Mission Trainer” by turning the responsibilities of a mission planner into understandable choices. The central rule is simple: on Mars, no choice is free. Spending electricity on water extraction affects what remains for crops and oxygen production. Maintenance also takes workers and time away from other tasks.

### How the game is designed to work

The planned learning loop is to identify a need, repair a building, complete its preparation or maintenance task, and observe how the resource balance changes. The initial scope is a guided tutorial that introduces the base's interconnected systems. It is designed without a defeat condition so that learners can encounter these relationships before the broader survival challenge.

Three concept minigames illustrate the approach:

- **Solar Rescue:** clean dusty panels using compressed Martian CO₂. Each blast costs electricity, connecting the use of local resources with an energy trade-off.
- **Martian Alchemist:** choose a drilling location and depth, then manage pressure and heat while processing ice into water. The task introduces resource exploration and controlled processing conditions.
- **MOXIE: Oxygen Rescue:** maintain intake filters to restore oxygen-production efficiency. The task connects life support with energy use and the time needed for maintenance.

The wider resource model also links crop production with water and light, and organic waste with compost. These connections are intended to help players reason about the base as a system rather than treat each resource as an isolated score.

### Scientific foundation

NASA resources guide the design. MOXIE provides the experimental foundation for producing oxygen from Martian atmospheric CO₂. SWIM maps support the concept of subsurface ice as a resource. VEG-05 informs the role of light and fertilization in space crop production. Mars Trek supports exploration of the Martian setting. DONKI has been selected as a reference for future space-weather scenarios; live event integration is not presented as a completed feature.

These resources inform the game concept and its scientific explanations. They do not establish that the fictional colony's full resource economy has been validated. Gameplay timings and adaptations must remain distinguishable from measured scientific results.

### Tools and technical approach

The project uses Unity 6 and C#. Its architecture separates the pure C# simulation in MarsCat.Logic from the interface and scene integration in MarsCat.Unity. External CSV configuration keeps resource-balance values separate from the code. NUnit is the planned framework for checking consumption, priorities, and state transitions. The team's documented art pipeline includes Blender and OpenToonz.

The presentation uses editable text, a conceptual interface wireframe, and a native architecture diagram. Concept artwork illustrates the intended experience while implementation continues.

### Creativity and intended impact

The cats provide an approachable way into an engineering problem: sustaining life with limited resources. The Martian environment creates the challenge, while repairs and maintenance make scientific dependencies visible through actions and consequences.

The intended learning outcomes are to prioritize limited supplies, recognize causes and consequences, and use scientific reasoning to solve problems. Classroom and family use are potential applications. Educational effectiveness has not yet been measured with learners.

### Current status and SI assistance

This submission presents the game design, concept visuals, and intended architecture. Gameplay implementation, scientific data integration, and validation are ongoing. Later concepts, including broader exploration, nuclear power, and solar-event scenarios, are outside the initial tutorial scope.

SI tools supported research, English presentation and submission copy, presentation-authoring scripts, and selected presentation artwork. The team retains responsibility for scientific verification, attribution, and the final project decisions.

### Editing notes — do not copy these into Project Details

- Keep the implementation-status wording accurate for the build that is actually submitted.
- The tutorial's protected learning experience and the wider game's survival consequences are different stages of the design.
- Blender and OpenToonz appear in the documented team workflow. Confirm their actual use in the submitted materials before keeping that sentence.
- The learning outcomes are intended benefits, not results from a completed evaluation.

## 6. Use of Super Intelligence (SI)

### Suggested checkbox selections

- [x] Research or Information Gathering
- [x] Visual Design or Image Creation
- [x] Coding, Debugging, or Troubleshooting

These selections cover the assistance used in this preparation workflow, including presentation-authoring scripts. They do not assert that every part of the game code was SI-generated. Do not select “I did not use SI”. Add any other applicable categories or tools used by the team.

### Optional explanatory text

The pasted form shows a selection field rather than an additional written-response field. The following wording can be used in the project's disclosures or adapted if another SI-description field is available:

We used OpenAI Codex to assist with research, English drafting and translation, and presentation-authoring scripts. OpenAI image generation was used to produce a menu-free variant of the supplied cover artwork and a new closing illustration based on the game's visual references. NASA source screenshots were kept unchanged. The team retains responsibility for checking scientific claims, reviewing assisted work, and making the final design and implementation decisions. Additional SI assistance used elsewhere in the project should be disclosed with the relevant code, data, or media.

### Team-specific details to complete

- Other SI tools used by team members: `[ADD_TOOLS_AND_SPECIFIC_USES_OR_REMOVE_IF_NONE]`.
- Game-code assistance beyond the presentation scripts: `[DESCRIBE_ACTUAL_USE_IF_APPLICABLE]`.
- SI provenance of the supplied game-art references: `[CONFIRM_WITH_THE_DESIGN_TEAM]`.
- SI-generated audio or video, if included in the final submission: `[DESCRIBE_ACTUAL_USE_OR_REMOVE_IF_NONE]`.

## 7. NASA Data

Enter each resource below as a separate Link Text / Link URL pair. The usage notes explain its role and are not additional URL-field content. This list covers the setting reference documented in the repository and the four scientific resources shown in the presentation, plus the documented Utopia Planitia research reference.

### 7.1. Mars Trek

**Link Text:** NASA Mars Trek — Martian terrain and setting reference

**Link URL:** <https://trek.nasa.gov/mars/>

**Usage note:** Referenced in the project documentation for the proposed Martian setting around Vivero crater in Utopia Planitia. Do not claim a verified ice deposit directly beneath the chosen crater; that location-specific relationship remains to be checked.

### 7.2. MOXIE

**Link Text:** NASA/JPL — MOXIE Oxygen-Generating Experiment

**Link URL:** <https://www.nasa.gov/solar-system/nasas-oxygen-generating-experiment-moxie-completes-mars-mission/>

**Usage note:** Scientific reference for extracting oxygen from Martian atmospheric CO₂ and source of the MOXIE screenshot in the presentation. The filter minigame is a gameplay adaptation, not a reproduction of a validated NASA operating procedure. Retain the source credit, NASA/JPL-Caltech.

### 7.3. SWIM

**Link Text:** NASA — SWIM Map of Subsurface Water Ice on Mars

**Link URL:** <https://science.nasa.gov/resource/swim-map-shows-subsurface-water-ice-on-mars/>

**Usage note:** Scientific and visual reference for subsurface water ice. The map appears unchanged in the presentation. Credit NASA/JPL-Caltech/Planetary Science Institute.

### 7.4. VEG-05

**Link Text:** NASA — VEG-05 Space Crop Investigation

**Link URL:** <https://science.nasa.gov/biological-physical/investigations/veg-05-rjtim/>

**Usage note:** Reference for the effects of light and fertilization on space crop production, with an unchanged source screenshot in the presentation. VEG-05 concerns dwarf tomatoes on the ISS; it does not establish potato yields on Mars.

### 7.5. DONKI

**Link Text:** NASA CCMC — DONKI Space Weather Database

**Link URL:** <https://ccmc.gsfc.nasa.gov/tools/DONKI/>

**Usage note:** Research and presentation reference for future space-weather scenarios. The screenshot is used as source evidence. A functioning live API connection is not claimed.

### 7.6. Utopia Planitia ice research

**Link Text:** NASA/JPL — Subsurface Water Ice in Utopia Planitia

**Link URL:** <https://www.jpl.nasa.gov/news/mars-ice-deposit-holds-as-much-water-as-lake-superior/>

**Usage note:** Listed in the project documentation as background research for the setting and ice-extraction concept. Its regional findings do not, by themselves, confirm the exact fictional base's subsurface conditions. Include this entry if that setting research remains part of the submitted project.

### Additional NASA references

If the final project uses other references from the research documents—such as BVAD, MEDA/PDS, RedWater, Kilopower, or OSDR—add their exact original sources and describe their actual use. Do not present proposed data integrations as completed ones or copy unverified balance numbers into the form.

## 8. Space Agency Partner & Other Data

### 8.1. Resources used in the current development or presentation workflow

Add these as separate link pairs where applicable to the final submitted project.

| Link Text | Link URL | Use / editing note |
| --- | --- | --- |
| Unity — Game Engine | <https://unity.com/> | Unity 6 game development and scene/interface integration. |
| Microsoft — C# Language Documentation | <https://learn.microsoft.com/en-us/dotnet/csharp/> | Language used for the simulation and Unity scripts. |
| GitHub — Project Collaboration and Source Hosting | <https://github.com/> | Repository hosting and development coordination. Public project access must be confirmed separately. |
| OpenAI Codex — Research, Writing, and Coding Assistance | <https://openai.com/codex/> | Research support, English drafting and translation, and presentation-authoring scripts. Add the team's other actual uses if applicable. |
| OpenAI — Image Generation | <https://openai.com/> | Menu-free cover variant and closing concept illustration. This identifies the provider without guessing the model version used. |
| Fira Sans — Presentation Typeface | <https://github.com/mozilla/Fira> | Typeface used in the editable PowerPoint. |

### 8.2. Additional project references and tools — confirm actual use

Only add an entry to the form if it was used in the submitted project, its design research, or its assets. Do not list a tool solely because it is planned.

| Link Text | Link URL | When to include |
| --- | --- | --- |
| ESA MELiSSA — Closed Loop Concept | <https://www.esa.int/Enabling_Support/Space_Engineering_Technology/Melissa/Closed_Loop_Concept> | If the organic-recycling design uses this research reference. It is cited in the project's design notes and must be attributed to ESA, not NASA. |
| Blender — 3D Art and Modeling | <https://www.blender.org/> | If used to create the submitted 3D art or other included assets. Listed in the team's documented art pipeline. |
| OpenToonz — 2D Animation | <https://opentoonz.github.io/e/> | If used for the submitted animation or sprite assets. Listed in the team's documented art pipeline. |
| NUnit — C# Testing Framework | <https://nunit.org/> | If actual tests or testing materials are included. Planned tests alone do not establish completed validation. |

### Other external material to add if used

- Additional SI tools, with a public provider or product URL and an accurate description of their use.
- Third-party code libraries, asset packs, fonts, textures, sound effects, music, and images included in the final project.
- Original scientific papers used for mechanics that the NASA resources above do not support, such as specific biological processing assumptions.
- Public source links and required attribution for any external materials in the supplied concept art. Confirm provenance with the design team rather than assuming all supplied visuals were created without external resources.

## 9. Items to finish before submission — editing notes only

### Public links

- Replace the demonstration placeholder with the public presentation URL.
- Confirm the candidate project repository is publicly accessible, or supply a different public project link.

### SI image labels

The supplied form requires a visible SI-generated indicator on generated images or videos. The current PowerPoint's “Concept visual · Project in development” captions do not explicitly identify SI generation.

Before publication, add a clearly readable **“SI-generated image”** label to the generated cover variant and closing illustration where they are displayed. Review the other supplied artwork with the design team and label any additional SI-generated images accordingly. Keep unmodified NASA scientific screenshots separate from these generated visuals.

Known generated presentation assets:

- `ppt_assets_v1/cover_menu_free.png` — supplied cover adapted to remove the Spanish menu.
- `ppt_assets_v1/closing_colony.png` — new closing concept illustration based on the supplied game-art reference.

The pasted form also prohibits SI-generated content from using, containing, or modifying NASA branding or design elements. Review generated visuals against that condition; the original NASA screenshots in the scientific-source slide were not generated or altered with SI.

### SI code, data, audio, and video disclosure

The form requests descriptive and metadata disclosure for SI-assisted code/data and audio. Record the actual scope in the appropriate project descriptions and metadata before submitting. Do not state that this disclosure is already complete unless the team has checked it. Add audio/video-specific disclosures if those media are included.

### Final team verification

The terms-and-conditions agreement and originality/permissions confirmation are declarations for the submitting team to review and make. This draft does not certify that all work is original, that all third-party permissions are in place, or that submission has occurred.

Reconcile the resource list with the final project and media before making those declarations. Saving progress and submitting for judging are separate actions in the supplied form; no form has been submitted through this preparation work.

## Local working references — not form content

- [Slide content and design brief](borrador_diapos.md).
- [Editable presentation](mars_miau_house_v1.pptx).
- [NASA resource register](../docs/diseno/05_datos_nasa.md).
- [Tutorial scope](../docs/diseno/02_core_loop.md).
- [Game systems](../docs/diseno/03_sistemas.md).
- [Technical approach](../docs/06_guia_logica.md).
- [Documented toolchain](../docs/00_stack.md).
