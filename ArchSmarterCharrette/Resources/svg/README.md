# Ribbon icon sources

Tokenized SVG masters for the Charrette ribbon buttons. These are the source of
truth; the PNGs one folder up are generated from them.

Colors are authored as `{{NEUTRAL}}` tokens rather than literal hex, so
recoloring the whole set is a re-render, not a redraw.

| SVG            | Generates                                                         |
|----------------|-------------------------------------------------------------------|
| `Render.svg`   | `Render_16/32.png`, `RenderDark_16/32.png`                        |
| `Video.svg`    | `Video_16/32.png`, `VideoDark_16/32.png`                          |
| `Settings.svg` | `Settings_16/32.png`, `SettingsDark_16/32.png`                    |

The light and dark PNGs come from the same SVG with a different `--neutral`.

## Regenerating

Rendering uses `rasterize.py` from the Prompticon skill. Each PNG is rendered
directly from vector at its target size — never downscaled from 32 to 16, which
goes muddy.

```bash
pip install resvg-py pillow
```

`rasterize.py` imports `cairosvg`, which needs a native `libcairo-2.dll` that
Windows does not ship and no pip wheel provides. `cairosvg_shim.py` here stands
in for it, backed by `resvg` (self-contained Rust rasterizer, Windows wheel).
Put this folder on `PYTHONPATH` as `cairosvg` to use it:

```bash
cp cairosvg_shim.py /tmp/shim/cairosvg.py
PYTHONPATH=/tmp/shim python rasterize.py --svg . --out-dir .. --neutral "#3C3C3C"
```

Dark-theme set — same SVGs, light neutral, then rename the outputs to the
`*Dark_*` names the resx expects:

```bash
PYTHONPATH=/tmp/shim python rasterize.py --svg . --out-dir ./dark --neutral "#E6E6E6"
```

## Design constraints

These exist so the glyphs survive 16px. Worth honoring if you edit them.

- `viewBox="0 0 32 32"`, ~2px interior margin
- Solid fills, no strokes; single `fill-rule="evenodd"` path per icon
- Negative-space gaps >= 3 units on the 32 grid, or they fill in at 16px
- No gradients, filters, text, or background rect — alpha must stay transparent
- `Render.svg` and `Video.svg` share an identical frame (`3,6` -> `29,26`,
  `rx=3`) so the pair reads as a family. Keep them in sync if you adjust one.
