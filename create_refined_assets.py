import os
import math
from PIL import Image, ImageDraw, ImageFilter

OUTPUT_DIR = r"D:\Hexa Sort\Assets\Asset\Art\ShopGenerated"
os.makedirs(OUTPUT_DIR, exist_ok=True)

def create_booster_swap():
    # Two curved arrows in coral/orange forming a circle
    size = 256
    scale = 2
    ss = size * scale
    img = Image.new("RGBA", (ss, ss), (0, 0, 0, 0))

    cx, cy = ss // 2, ss // 2
    r_outer = 95 * scale
    r_inner = 55 * scale
    thick = (r_outer - r_inner)

    # We draw two arcs: Top-right arrow and Bottom-left arrow
    # Arc 1: from -20 deg to 140 deg
    # Arc 2: from 160 deg to 320 deg
    coral_light = (255, 128, 100, 255) # #FF8064
    coral_dark = (225, 60, 45, 255)   # #E13C2D
    border_col = (150, 25, 15, 255)   # Dark rim

    mask = Image.new("L", (ss, ss), 0)
    draw = ImageDraw.Draw(mask)

    # Arc 1 (top-right going counterclockwise)
    draw.arc([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer],
             -30, 135, fill=255, width=thick)
    # Arrowhead 1 at ~135 deg:
    # Actually in standard orientation, arrow head at top-left:
    head1 = [
        (cx - 30 * scale, cy - r_outer + 10 * scale),
        (cx - 90 * scale, cy - r_outer + 30 * scale),
        (cx - 50 * scale, cy - r_inner - 25 * scale),
    ]

    # Let's draw via polygon paths for clean cartoon arrows:
    mask2 = Image.new("L", (ss, ss), 0)
    d2 = ImageDraw.Draw(mask2)

    # Arrow 1: top half
    d2.arc([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer],
           200, 360, fill=255, width=thick)
    # Arrowhead 1 at angle 200 (pointing down-left)
    ax1, ay1 = cx - r_outer + thick // 2, cy
    d2.polygon([(ax1 - 35 * scale, ay1 + 10 * scale),
                (ax1 + 15 * scale, ay1 - 35 * scale),
                (ax1 + 15 * scale, ay1 + 35 * scale)], fill=255)

    # Arrow 2: bottom half
    d2.arc([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer],
           20, 180, fill=255, width=thick)
    # Arrowhead 2 at angle 20 (pointing up-right)
    ax2, ay2 = cx + r_outer - thick // 2, cy
    d2.polygon([(ax2 + 35 * scale, ay2 - 10 * scale),
                (ax2 - 15 * scale, ay2 + 35 * scale),
                (ax2 - 15 * scale, ay2 - 35 * scale)], fill=255)

    # Outer border (stroke)
    border_mask = mask2.filter(ImageFilter.MaxFilter(9))
    border_img = Image.new("RGBA", (ss, ss), border_col)
    img.paste(border_img, (0, 0), border_mask)

    # Gradient fill
    grad = Image.new("RGBA", (ss, ss), (0, 0, 0, 0))
    for y in range(ss):
        t = y / ss
        r = int(coral_light[0] * (1 - t) + coral_dark[0] * t)
        g = int(coral_light[1] * (1 - t) + coral_dark[1] * t)
        b = int(coral_light[2] * (1 - t) + coral_dark[2] * t)
        grad.paste((r, g, b, 255), [0, y, ss, y + 1])

    # Highlight line on top
    h_draw = ImageDraw.Draw(grad)
    h_draw.arc([cx - r_outer + 4 * scale, cy - r_outer + 4 * scale,
                cx + r_outer - 4 * scale, cy + r_outer - 4 * scale],
               220, 340, fill=(255, 200, 180, 200), width=5 * scale)

    img.paste(grad, (0, 0), mask2)

    img = img.resize((size, size), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Booster_Swap.png"))
    print("Saved Shop_Booster_Swap.png")

def create_green_button_3d():
    # 9-sliceable casual mobile button: 256 x 96, corner radius 36
    w, h = 256, 96
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 36 * scale
    bevel_h = 8 * scale

    # 1. Dark forest green 3D bottom bevel
    s_mask = Image.new("L", (sw, sh), 0)
    s_draw = ImageDraw.Draw(s_mask)
    s_draw.rounded_rectangle([0, bevel_h, sw - 1, sh - 1], radius=rad, fill=255)
    img.paste((10, 105, 35, 255), (0, 0), s_mask)

    # 2. Outer dark border
    o_mask = Image.new("L", (sw, sh), 0)
    o_draw = ImageDraw.Draw(o_mask)
    o_draw.rounded_rectangle([0, 0, sw - 1, sh - 1 - bevel_h], radius=rad, fill=255)
    img.paste((14, 135, 45, 255), (0, 0), o_mask)

    # 3. Main green gradient (#38F06C at top to #14B844 at bottom)
    border_th = 3 * scale
    gh = sh - bevel_h
    m_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(m_mask)
    m_draw.rounded_rectangle([border_th, border_th, sw - 1 - border_th, gh - border_th],
                            radius=rad - border_th, fill=255)

    g_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(gh):
        t = y / gh
        r = int(56 * (1 - t) + 20 * t)
        g = int(240 * (1 - t) + 184 * t)
        b = int(108 * (1 - t) + 68 * t)
        g_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Crisp white top shine
    c_draw = ImageDraw.Draw(g_grad)
    c_draw.rounded_rectangle([border_th + 3 * scale, border_th + 2 * scale,
                              sw - border_th - 3 * scale, border_th + 14 * scale],
                             radius=8 * scale, fill=(255, 255, 255, 140))

    img.paste(g_grad, (0, 0), m_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Green_Button.png"))
    print("Saved Shop_Green_Button.png")

def create_tag_pink_cloud():
    # Bubbly cloud badge: 240 x 120
    w, h = 240, 120
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    cx, cy = sw // 2, sh // 2

    # Drop shadow
    shadow_mask = Image.new("L", (sw, sh), 0)
    s_draw = ImageDraw.Draw(shadow_mask)
    s_offset = 6 * scale

    # Cloud blobs
    blobs = [
        (cx - 65 * scale, cy, 38 * scale),
        (cx + 65 * scale, cy, 38 * scale),
        (cx - 30 * scale, cy - 14 * scale, 35 * scale),
        (cx + 30 * scale, cy - 14 * scale, 35 * scale),
        (cx - 30 * scale, cy + 14 * scale, 35 * scale),
        (cx + 30 * scale, cy + 14 * scale, 35 * scale),
        (cx, cy, 42 * scale),
    ]

    for bx, by, br in blobs:
        s_draw.ellipse([bx - br, by + s_offset - br, bx + br, by + s_offset + br], fill=255)
    shadow_mask = shadow_mask.filter(ImageFilter.GaussianBlur(5 * scale))
    img.paste((20, 5, 25, 150), (0, 0), shadow_mask)

    # Main cloud mask
    c_mask = Image.new("L", (sw, sh), 0)
    c_draw = ImageDraw.Draw(c_mask)
    for bx, by, br in blobs:
        c_draw.ellipse([bx - br, by - br, bx + br, by + br], fill=255)

    # Pink gradient (#FF2E84 to #D61266)
    p_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(255 * (1 - t) + 214 * t)
        g = int(46 * (1 - t) + 18 * t)
        b = int(132 * (1 - t) + 102 * t)
        p_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Inner highlight line
    h_draw = ImageDraw.Draw(p_grad)
    for bx, by, br in blobs[:4]:
        h_draw.arc([bx - br + 4 * scale, by - br + 4 * scale,
                    bx + br - 4 * scale, by + br - 4 * scale],
                   180, 360, fill=(255, 210, 230, 180), width=4 * scale)

    img.paste(p_grad, (0, 0), c_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Tag_Pink.png"))
    print("Saved Shop_Tag_Pink.png")

if __name__ == "__main__":
    create_booster_swap()
    create_green_button_3d()
    create_tag_pink_cloud()
    print("Refined assets generated successfully!")
