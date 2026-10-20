import os
import math
from PIL import Image, ImageDraw, ImageFilter

OUTPUT_DIR = r"D:\Hexa Sort\Assets\Asset\Art\ShopGenerated"
os.makedirs(OUTPUT_DIR, exist_ok=True)

def create_awning():
    # 1080 x 240 canvas with 6 scallops
    w, h = 1080, 240
    # Render at 2x for supersampled anti-aliasing
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    num_scallops = 6
    scallop_w = sw / num_scallops
    hang_y = int(170 * scale)
    radius = int(scallop_w / 2)

    # Colors
    # Purple stripe gradient: #8C3CEB to #6618C4
    # White stripe gradient: #EBF4FC to #C5DEF5
    purple_colors = [(140, 60, 235), (102, 24, 196)]
    white_colors = [(235, 244, 252), (197, 222, 245)]

    # Draw drop shadow first
    shadow_img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    s_draw = ImageDraw.Draw(shadow_img)
    shadow_offset = int(10 * scale)

    for i in range(num_scallops):
        x0 = int(i * scallop_w)
        x1 = int((i + 1) * scallop_w)
        cx = int((x0 + x1) / 2)
        # Rect top
        s_draw.rectangle([x0, 0, x1, hang_y + shadow_offset], fill=(15, 10, 30, 160))
        # Circle bottom
        s_draw.pieslice([cx - radius, hang_y + shadow_offset - radius,
                         cx + radius, hang_y + shadow_offset + radius],
                        0, 180, fill=(15, 10, 30, 160))

    shadow_img = shadow_img.filter(ImageFilter.GaussianBlur(8 * scale))
    img.paste(shadow_img, (0, 0), shadow_img)

    # Draw each stripe with rounded scallop bottom
    for i in range(num_scallops):
        x0 = int(i * scallop_w)
        x1 = int((i + 1) * scallop_w)
        cx = int((x0 + x1) / 2)
        is_purple = (i % 2 == 0)
        c_top, c_bot = (purple_colors if is_purple else white_colors)

        stripe_mask = Image.new("L", (sw, sh), 0)
        m_draw = ImageDraw.Draw(stripe_mask)
        m_draw.rectangle([x0, 0, x1, hang_y], fill=255)
        m_draw.pieslice([cx - radius, hang_y - radius,
                         cx + radius, hang_y + radius],
                        0, 180, fill=255)

        # Gradient
        stripe_img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
        for y in range(hang_y + radius):
            ratio = min(1.0, max(0.0, y / (hang_y + radius)))
            r = int(c_top[0] * (1 - ratio) + c_bot[0] * ratio)
            g = int(c_top[1] * (1 - ratio) + c_bot[1] * ratio)
            b = int(c_top[2] * (1 - ratio) + c_bot[2] * ratio)
            stripe_img.paste((r, g, b, 255), [x0, y, x1, y + 1])

        # Bottom fold shading
        fold_draw = ImageDraw.Draw(stripe_img)
        fold_c = (50, 10, 100, 70) if is_purple else (120, 150, 190, 60)
        fold_draw.pieslice([cx - radius, hang_y - radius,
                            cx + radius, hang_y + radius],
                           0, 180, outline=fold_c, width=int(4 * scale))

        img.paste(stripe_img, (0, 0), stripe_mask)

    # Downscale for crisp AA
    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Awning_6Stripes.png"))
    print("Saved Shop_Awning_6Stripes.png")

def create_coin_hex():
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = size // 2, size // 2
    r_outer = 114
    r_inner = 98

    def get_hex_points(r):
        pts = []
        for i in range(6):
            # Hexagon rotated with flat top/bottom or pointy top
            angle = math.radians(60 * i - 30)
            pts.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
        return pts

    # Outer dark gold rim
    draw.polygon(get_hex_points(r_outer), fill=(185, 115, 0, 255), outline=(130, 75, 0, 255))
    # Inner bright gold gradient mask
    inner_mask = Image.new("L", (size, size), 0)
    m_draw = ImageDraw.Draw(inner_mask)
    m_draw.polygon(get_hex_points(r_inner), fill=255)

    gold_grad = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    for y in range(size):
        t = y / size
        # #FFF054 to #FF9F00
        r = int(255 * (1 - t) + 255 * t)
        g = int(240 * (1 - t) + 159 * t)
        b = int(84 * (1 - t) + 0 * t)
        gold_grad.paste((r, g, b, 255), [0, y, size, y + 1])

    img.paste(gold_grad, (0, 0), inner_mask)

    # Draw inner dollar sign in dark gold
    d_draw = ImageDraw.Draw(img)
    # 3D highlight bevel
    d_draw.polygon(get_hex_points(r_inner), outline=(255, 255, 180, 200))
    
    # Dollar symbol
    dollar_w = 26
    # Vertical bars
    d_draw.rectangle([cx - 4, cy - 58, cx + 4, cy + 58], fill=(150, 78, 0, 255))
    # S curve approximations with thick strokes
    d_draw.arc([cx - 28, cy - 46, cx + 28, cy - 2], 90, 270, fill=(150, 78, 0, 255), width=16)
    d_draw.arc([cx - 28, cy - 8, cx + 28, cy + 36], 270, 90, fill=(150, 78, 0, 255), width=16)
    d_draw.line([cx - 20, cy - 4, cx + 20, cy - 4], fill=(150, 78, 0, 255), width=16)

    # Resize to 128x128
    img = img.resize((128, 128), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Coin_Hex.png"))
    print("Saved Shop_Coin_Hex.png")

def create_card_purple():
    # 9-sliceable purple card: 512 x 256, corner radius 48
    w, h = 512, 256
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 48 * scale
    border = 6 * scale

    # Outer gold/yellow rim mask
    outer_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(outer_mask)
    m_draw.rounded_rectangle([0, 0, sw - 1, sh - 1], radius=rad, fill=255)

    # Gold rim gradient (#FFE045 to #E69D00)
    gold_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(255 * (1 - t) + 230 * t)
        g = int(224 * (1 - t) + 157 * t)
        b = int(69 * (1 - t) + 0 * t)
        gold_grad.paste((r, g, b, 255), [0, y, sw, y + 1])
    img.paste(gold_grad, (0, 0), outer_mask)

    # Inner purple body
    inner_mask = Image.new("L", (sw, sh), 0)
    in_draw = ImageDraw.Draw(inner_mask)
    in_draw.rounded_rectangle([border, border, sw - 1 - border, sh - 1 - border],
                             radius=rad - border, fill=255)

    # Purple gradient (#8F3EEB at top to #631AB6 at bottom)
    purple_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(143 * (1 - t) + 99 * t)
        g = int(62 * (1 - t) + 26 * t)
        b = int(235 * (1 - t) + 182 * t)
        purple_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Add subtle lighter top highlight inside purple
    p_draw = ImageDraw.Draw(purple_grad)
    p_draw.line([border + rad, border + 4 * scale, sw - border - rad, border + 4 * scale],
                fill=(200, 150, 255, 120), width=3 * scale)

    img.paste(purple_grad, (0, 0), inner_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "ShopCard_Purple_Frame.png"))
    print("Saved ShopCard_Purple_Frame.png")

def create_card_blue_base():
    # 9-sliceable deep royal blue card base: 512 x 256, corner radius 48
    w, h = 512, 256
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 48 * scale
    border = 6 * scale

    # Outer border mask (#103882)
    outer_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(outer_mask)
    m_draw.rounded_rectangle([0, 0, sw - 1, sh - 1], radius=rad, fill=255)

    # Blue border gradient (#3C8CFF to #0C3078)
    b_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(60 * (1 - t) + 12 * t)
        g = int(140 * (1 - t) + 48 * t)
        b = int(255 * (1 - t) + 120 * t)
        b_grad.paste((r, g, b, 255), [0, y, sw, y + 1])
    img.paste(b_grad, (0, 0), outer_mask)

    # Inner deep blue body
    inner_mask = Image.new("L", (sw, sh), 0)
    in_draw = ImageDraw.Draw(inner_mask)
    in_draw.rounded_rectangle([border, border, sw - 1 - border, sh - 1 - border],
                             radius=rad - border, fill=255)

    # Royal blue gradient (#1A5DD6 to #103E98)
    body_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(26 * (1 - t) + 16 * t)
        g = int(93 * (1 - t) + 62 * t)
        b = int(214 * (1 - t) + 152 * t)
        body_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    img.paste(body_grad, (0, 0), inner_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "ShopCard_Blue_Base.png"))
    print("Saved ShopCard_Blue_Base.png")

def create_card_blue_inset():
    # 9-sliceable light sky-blue inset panel: 512 x 256, corner radius 36
    w, h = 512, 256
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 36 * scale
    border = 4 * scale

    # Outer border mask
    outer_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(outer_mask)
    m_draw.rounded_rectangle([0, 0, sw - 1, sh - 1], radius=rad, fill=255)

    # Sky-blue gradient (#74B5FA at top to #5097EB at bottom)
    sky_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(116 * (1 - t) + 80 * t)
        g = int(181 * (1 - t) + 151 * t)
        b = int(250 * (1 - t) + 235 * t)
        sky_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Inner white border highlight
    s_draw = ImageDraw.Draw(sky_grad)
    s_draw.rounded_rectangle([border, border, sw - 1 - border, sh - 1 - border],
                            radius=rad - border, outline=(255, 255, 255, 140), width=border)

    img.paste(sky_grad, (0, 0), outer_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "ShopCard_Blue_Inset.png"))
    print("Saved ShopCard_Blue_Inset.png")

def create_green_button():
    # 9-sliceable casual green button: 256 x 96, corner radius 38
    w, h = 256, 96
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 38 * scale
    bevel_h = 7 * scale

    # 1. Dark green 3D shadow at bottom
    shadow_mask = Image.new("L", (sw, sh), 0)
    s_draw = ImageDraw.Draw(shadow_mask)
    s_draw.rounded_rectangle([0, bevel_h, sw - 1, sh - 1], radius=rad, fill=255)
    shadow_color = Image.new("RGBA", (sw, sh), (10, 110, 36, 255))
    img.paste(shadow_color, (0, 0), shadow_mask)

    # 2. Outer dark border
    outer_mask = Image.new("L", (sw, sh), 0)
    o_draw = ImageDraw.Draw(outer_mask)
    o_draw.rounded_rectangle([0, 0, sw - 1, sh - 1 - bevel_h], radius=rad, fill=255)
    border_color = Image.new("RGBA", (sw, sh), (14, 138, 44, 255))
    img.paste(border_color, (0, 0), outer_mask)

    # 3. Main lime green gradient (#24E262 to #13AF41)
    b_thickness = 3 * scale
    main_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(main_mask)
    m_draw.rounded_rectangle([b_thickness, b_thickness, sw - 1 - b_thickness, sh - 1 - bevel_h - b_thickness],
                            radius=rad - b_thickness, fill=255)

    green_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    gh = sh - bevel_h
    for y in range(gh):
        t = y / gh
        r = int(36 * (1 - t) + 19 * t)
        g = int(226 * (1 - t) + 175 * t)
        b = int(98 * (1 - t) + 65 * t)
        green_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Top white shine inside
    g_draw = ImageDraw.Draw(green_grad)
    g_draw.line([rad, b_thickness + 3 * scale, sw - rad, b_thickness + 3 * scale],
                fill=(255, 255, 255, 160), width=3 * scale)

    img.paste(green_grad, (0, 0), main_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Green_Button.png"))
    print("Saved Shop_Green_Button.png")

def create_tag_pink():
    # POPULAR / BEST VALUE pink scalloped ribbon: 240 x 120
    w, h = 240, 120
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    # Bubbly cloud/scallop shape
    mask = Image.new("L", (sw, sh), 0)
    draw = ImageDraw.Draw(mask)

    # Central pill + side bubbles
    cx, cy = sw // 2, sh // 2
    draw.rounded_rectangle([30 * scale, 15 * scale, sw - 30 * scale, sh - 15 * scale],
                           radius=25 * scale, fill=255)

    # Scallops around periphery
    bubbles = [
        (40 * scale, cy, 26 * scale),
        (sw - 40 * scale, cy, 26 * scale),
        (cx - 50 * scale, 20 * scale, 24 * scale),
        (cx + 50 * scale, 20 * scale, 24 * scale),
        (cx - 50 * scale, sh - 20 * scale, 24 * scale),
        (cx + 50 * scale, sh - 20 * scale, 24 * scale),
        (cx, 16 * scale, 25 * scale),
        (cx, sh - 16 * scale, 25 * scale),
    ]
    for bx, by, br in bubbles:
        draw.ellipse([bx - br, by - br, bx + br, by + br], fill=255)

    # Pink gradient (#FF3388 to #D81165)
    pink_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(255 * (1 - t) + 216 * t)
        g = int(51 * (1 - t) + 17 * t)
        b = int(136 * (1 - t) + 101 * t)
        pink_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    # Inner stroke
    p_draw = ImageDraw.Draw(pink_grad)
    p_draw.rounded_rectangle([32 * scale, 17 * scale, sw - 32 * scale, sh - 17 * scale],
                            radius=23 * scale, outline=(255, 200, 230, 180), width=3 * scale)

    img.paste(pink_grad, (0, 0), mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Tag_Pink.png"))
    print("Saved Shop_Tag_Pink.png")

def create_tag_extra800():
    # 800% EXTRA flower burst badge: 180 x 180
    size = 180
    scale = 2
    ss = size * scale
    img = Image.new("RGBA", (ss, ss), (0, 0, 0, 0))

    cx, cy = ss // 2, ss // 2
    r_center = 62 * scale
    num_petals = 12
    petal_r = 24 * scale
    dist = 52 * scale

    mask = Image.new("L", (ss, ss), 0)
    draw = ImageDraw.Draw(mask)
    draw.ellipse([cx - r_center, cy - r_center, cx + r_center, cy + r_center], fill=255)

    for i in range(num_petals):
        angle = math.radians(360 / num_petals * i)
        px = cx + int(dist * math.cos(angle))
        py = cy + int(dist * math.sin(angle))
        draw.ellipse([px - petal_r, py - petal_r, px + petal_r, py + petal_r], fill=255)

    pink_grad = Image.new("RGBA", (ss, ss), (0, 0, 0, 0))
    for y in range(ss):
        t = y / ss
        r = int(255 * (1 - t) + 216 * t)
        g = int(51 * (1 - t) + 17 * t)
        b = int(136 * (1 - t) + 101 * t)
        pink_grad.paste((r, g, b, 255), [0, y, ss, y + 1])

    # White inner circle highlight
    p_draw = ImageDraw.Draw(pink_grad)
    p_draw.ellipse([cx - 50 * scale, cy - 50 * scale, cx + 50 * scale, cy + 50 * scale],
                   outline=(255, 255, 255, 160), width=3 * scale)

    img.paste(pink_grad, (0, 0), mask)

    img = img.resize((size, size), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Tag_Extra800.png"))
    print("Saved Shop_Tag_Extra800.png")

def create_coin_pill():
    # Dark coin capsule: 256 x 80
    w, h = 256, 80
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    rad = (h // 2) * scale
    # Dark semi-transparent pill (#18132B, alpha 210)
    draw.rounded_rectangle([0, 0, sw - 1, sh - 1], radius=rad,
                           fill=(24, 19, 43, 215), outline=(55, 45, 90, 200), width=2 * scale)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_CoinPill_Bg.png"))
    print("Saved Shop_CoinPill_Bg.png")

def create_popup_window():
    # 9-sliceable popup window: 512 x 512, corner radius 52
    w, h = 512, 512
    scale = 2
    sw, sh = w * scale, h * scale
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))

    rad = 52 * scale
    border = 7 * scale

    # Cyan/white glowing rim
    outer_mask = Image.new("L", (sw, sh), 0)
    m_draw = ImageDraw.Draw(outer_mask)
    m_draw.rounded_rectangle([0, 0, sw - 1, sh - 1], radius=rad, fill=255)

    rim_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(100 * (1 - t) + 160 * t)
        g = int(210 * (1 - t) + 80 * t)
        b = int(255 * (1 - t) + 240 * t)
        rim_grad.paste((r, g, b, 255), [0, y, sw, y + 1])
    img.paste(rim_grad, (0, 0), outer_mask)

    # Inner deep royal purple body (#4C238A to #2B1058)
    inner_mask = Image.new("L", (sw, sh), 0)
    in_draw = ImageDraw.Draw(inner_mask)
    in_draw.rounded_rectangle([border, border, sw - 1 - border, sh - 1 - border],
                             radius=rad - border, fill=255)

    purple_grad = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    for y in range(sh):
        t = y / sh
        r = int(76 * (1 - t) + 43 * t)
        g = int(35 * (1 - t) + 16 * t)
        b = int(138 * (1 - t) + 88 * t)
        purple_grad.paste((r, g, b, 255), [0, y, sw, y + 1])

    img.paste(purple_grad, (0, 0), inner_mask)

    img = img.resize((w, h), Image.Resampling.LANCZOS)
    img.save(os.path.join(OUTPUT_DIR, "Shop_Popup_Window.png"))
    print("Saved Shop_Popup_Window.png")

if __name__ == "__main__":
    create_awning()
    create_coin_hex()
    create_card_purple()
    create_card_blue_base()
    create_card_blue_inset()
    create_green_button()
    create_tag_pink()
    create_tag_extra800()
    create_coin_pill()
    create_popup_window()
    print("All custom shop assets generated successfully!")
