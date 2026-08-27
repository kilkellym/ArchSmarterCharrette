# Charrette Prompt Library Format

This documents the JSON format the Charrette add-in uses for prompt libraries, so you can author a new library by hand or edit an existing one without reverse-engineering the structure.

## What a library is

A library is the vocabulary the UI offers. Each of six categories presents a dropdown, the user picks one item per category, and the non-empty phrases are concatenated into a single image-generation prompt. A library can also define themes, which are presets that select one item from each category at once.

The four reference libraries (`Charrette_PromptLibrary.json`, `Charrette_SciFi.json`, `Charrette_Fantasy.json`, `Charrette_Historical.json`) all follow this format and are the best examples to copy from.

## The two valid shapes

The loader accepts two top-level shapes.

**1. Flat array (legacy, no themes).** The root is an array of item objects. This is the original format. It still loads, but the theme dropdown will be empty, so prefer the object shape below.

```json
[
  { "Category": "Style", "DisplayName": "Default style", "Phrase": "" }
]
```

**2. Object with Items and Themes (current).** The root is an object with an `Items` array and a `Themes` array. Use this shape for any new library.

```json
{
  "Items": [ ],
  "Themes": [ ]
}
```

## Items

Every entry in `Items` has exactly three string properties, all PascalCase:

- `Category`: which dropdown the item belongs to. Must be one of the six category names below, spelled exactly.
- `DisplayName`: the label shown in the dropdown. Must be unique within its category, because themes reference items by this name.
- `Phrase`: the text injected into the prompt when this item is selected. An empty string means the item contributes nothing.

### The six categories, in order

1. Style
2. Lighting
3. Material
4. Background
5. Entourage
6. Weather

Keep items grouped by category and keep the categories in this order in the file. The add-in may rely on file order when it concatenates the prompt, and Style appearing first gives the stylistic treatment the most weight. If you reorder categories you can change how strongly each reads in the final image.

### Conventions that matter

**Each category leads with a default item whose Phrase is empty.** "Default style", "Default lighting", "As modeled", "As is", "No entourage", "As is". This lets a dropdown sit in an off state that injects nothing. Always include one per category.

**Style holds rendering mediums, not genre flavor.** Watercolor, oil painting, photoreal, concept art, line drawing. The medium is how the image is drawn. Do not put a genre like "Cyberpunk" or "Victorian" in Style, because a genre is a bundle that wants to control lighting, materials, and setting all at once. That bundle belongs in a theme. Keeping Style as a pure medium is what lets a user render any subject as, say, a clean massing study.

**Phrase wording should be additive and start with a consistent verb.** The reference libraries open Style phrases with "Render as" or "Render in", Lighting with "Use", Material with "Apply", Background with "Set the background to" or "Place the building", and so on. Matching this keeps the concatenated prompt grammatical. Phrases should describe an addition to the scene rather than an absolute override, which softens collisions when two categories pull in different directions.

## Themes

A theme is a named preset. It has two properties:

- `Name`: the label shown in the theme dropdown.
- `Selections`: an object mapping each category name to the `DisplayName` of the item that theme selects in that category.

```json
{
  "Name": "Cyberpunk",
  "Selections": {
    "Style": "Concept art",
    "Lighting": "Neon glow",
    "Material": "Brushed metal and glass",
    "Background": "Megacity skyline",
    "Entourage": "Drones and hovercraft",
    "Weather": "Acid rain"
  }
}
```

### Rules for themes

**Reference items by DisplayName. Never copy phrase text into a theme.** The phrase is always looked up from the item at render time, so there is one source of truth. Edit a phrase once in `Items` and every theme using it updates. Embedding phrase text in themes guarantees drift.

**Every DisplayName in a Selections block must exist in Items, in that category.** If a theme points at an item that was renamed or deleted, the loader skips that one selection and leaves the category at its default rather than failing. That is a safety net, not a feature to rely on. Keep them in sync.

**Include a "Manual" theme that selects all six defaults.** It gives the user a "preset nothing" option and doubles as a readable record of what every default item is named.

**A theme should set all six categories.** If a theme deliberately wants a category left clean, point it at that category's empty-phrase default (for example a utopian theme selecting "As is" for Weather). That is a real choice, not a gap.

## How selecting a theme behaves

Choosing a theme sets each category dropdown to the item named in its Selections. The user can then change any individual dropdown. Once a category no longer matches the active theme, the theme selector indicates the user has drifted ("Custom" or "{Theme} (modified)"). Selecting the theme again re-applies the full preset.

## Minimal complete example

```json
{
  "Items": [
    { "Category": "Style", "DisplayName": "Default style", "Phrase": "" },
    { "Category": "Style", "DisplayName": "Photorealistic", "Phrase": "Render as a photorealistic image with accurate materials, lighting, and reflections." },
    { "Category": "Lighting", "DisplayName": "Default lighting", "Phrase": "" },
    { "Category": "Lighting", "DisplayName": "Golden hour", "Phrase": "Use warm golden-hour sunlight with long soft shadows." },
    { "Category": "Material", "DisplayName": "As modeled", "Phrase": "" },
    { "Category": "Background", "DisplayName": "As is", "Phrase": "" },
    { "Category": "Entourage", "DisplayName": "No entourage", "Phrase": "" },
    { "Category": "Weather", "DisplayName": "As is", "Phrase": "" }
  ],
  "Themes": [
    {
      "Name": "Manual",
      "Selections": {
        "Style": "Default style",
        "Lighting": "Default lighting",
        "Material": "As modeled",
        "Background": "As is",
        "Entourage": "No entourage",
        "Weather": "As is"
      }
    },
    {
      "Name": "Hero shot",
      "Selections": {
        "Style": "Photorealistic",
        "Lighting": "Golden hour",
        "Material": "As modeled",
        "Background": "As is",
        "Entourage": "No entourage",
        "Weather": "As is"
      }
    }
  ]
}
```

## Authoring checklist

- Root is an object with `Items` and `Themes`.
- All six categories are present, in order, each leading with an empty-phrase default.
- Style items are mediums, not genres.
- Every `DisplayName` is unique within its category.
- Every theme sets all six categories and references only existing DisplayNames.
- A "Manual" theme selects all defaults.
- The file is valid JSON. A quick check: `python -m json.tool yourfile.json` will fail loudly if it is not.

## One thing to verify by testing

Prompt concatenation order is a weak priority signal, and the add-in's exact behavior when categories or themes push in competing directions (for example a strong Style against a strong Material) is worth confirming with a test render. If an applied theme reads as one coherent image, order is fine. If the subject takes one treatment and the entourage takes another, the concatenation order is worth a look in the code.
