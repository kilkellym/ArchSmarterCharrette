"""Drop-in stand-in for cairosvg.svg2png, backed by resvg.

cairosvg needs a native libcairo-2.dll that Windows does not ship. resvg is a
self-contained Rust rasterizer with a Windows wheel, so it renders the same
vector geometry with no system dependency. Only the one call rasterize.py makes
is implemented; everything else about the skill's contract is unchanged
(direct per-size vector render, transparent background, DPI tag downstream).
"""

import resvg_py

__version__ = "resvg-shim"


def svg2png(bytestring=None, output_width=None, output_height=None, **_ignored):
    svg_text = bytestring.decode("utf-8") if isinstance(bytestring, bytes) else bytestring
    data = resvg_py.svg_to_bytes(
        svg_string=svg_text,
        width=output_width,
        height=output_height,
        shape_rendering="geometric_precision",
    )
    return bytes(data) if not isinstance(data, bytes) else data
