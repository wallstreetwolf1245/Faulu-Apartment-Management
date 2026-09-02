def hex_to_rgb(h):
    h=h.lstrip('#')
    return [int(h[i:i+2],16)/255.0 for i in (0,2,4)]

def srgb_to_lin(c):
    return c/12.92 if c<=0.03928 else ((c+0.055)/1.055)**2.4

def rel_luminance(hexc):
    r,g,b = hex_to_rgb(hexc)
    return 0.2126*srgb_to_lin(r)+0.7152*srgb_to_lin(g)+0.0722*srgb_to_lin(b)

def contrast_ratio(a,b):
    la = rel_luminance(a)
    lb = rel_luminance(b)
    L1 = max(la,lb)
    L2 = min(la,lb)
    return (L1+0.05)/(L2+0.05)

pairs = [('#4a8ba2','#ffffff'),('#4a8ba2','#f9fafb'),('#4a8ba2','#e6f1f7')]
for a,b in pairs:
    print(a,b,round(contrast_ratio(a,b),3))
