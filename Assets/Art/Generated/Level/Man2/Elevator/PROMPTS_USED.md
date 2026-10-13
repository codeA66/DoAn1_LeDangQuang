# Elevator — prompts used

## 2026-10-13: full set after user approval

User approved generated cabin and explicitly requested "ok vẽ và them vào project luôn"; this superseded the earlier preview-only gate. Delivered8nativePNG assets andPreview_Elevator.png; no Unityscene/prefab/script/meta or existingart modifications. White/blue is the user-approved elevator-only palette exception.

Tool: built-in image_gen, transparent_background=true. Structure reference guide was only used as generator input; final cabin, gates, lever parts andlightfacets are generated pixelart.

## Cabin

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-731b6138-2144-45be-bd0f-9b9d0484151a.png`

```text
Use case: stylized-concept. ONE production pixelart elevator CABIN OPEN STATE for Unity sideviewgame. Reference1 is EXACT STRUCTURAL guide, logical720x512, NOT art to copy literally: floor top y448, floor occupiesbottom64px to512; roof ALLwithinupper128px; two slim posts x24–36 and684–696 connectroofundersidey128 tofloor448. Interior center and both side entrances OPEN transparent, no walls or door panels, no central grilles, no backwall. Reference2 character is pixel density/style only, DO NOT draw character or use its turquoise colors. Design ORIGINAL antique elegant wroughtmetal elevator inspired by ornate HollowKnight cabin, NOT copying it: slender white/sapphireblue metal frame, decorative shallow arched/pointed ROOF with MANY curling filigree flourishes, S-shaped scrollwork, engravedborderstrips, delicate relief patterns, ornamentalcornerbrackets, short low side handrails not crossing passage. Roofdecor MUST stayinside y0–127 including small suspended lantern housings; no lanterns/chandeliers intrude belowy128. Leave320px or more clear standspace fromfloor448 toroof128. Flat horizontal solid walkable floor at exactlyy448, ornate carved fascia below it in64pxslab, not floating separate pedestal. No tall fence/backlattice blocking player. Two small lanterns nearroofcorners: housings and dark pale-blue lenses only, NO active light yet (light is separatelayer later). White and BLUE only: #F2F4F8 #BFD8F2 #4A7FC1 shadows#1E3557 contour#0C0A14. Strict flat palette, no teal/mint/turquoise #45DCC4, brown, orange/yellow/red. Fine crisp1–2nativepixelstair-steps density128px/tile, high ornamentdetail while silhouettes readable; no photorealism, gradients, softglow, antialias/blur/shadows. Truealpha transparentbackground, completeuncroppedcabin centered. Nativecanvas720x512; maintain EXACT guide proportions, no extra floor beyondcanvas, no ropes/cables outsideframe, no text/grid/labels. ONLY staticopen cabin layer.
```

## gates

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-e324b772-50d2-4371-8035-c75bdc683e4f.png`

```text
Use case: stylized-concept. Unity pixelart elevator CabinGates SEPARATE OVERLAY ONLY. Ref1 native720x512cabin determines exact placement/style. Draw ONLY TWO NARROW vertical closing wrought-metal GRILLE GATES, one atLEFT sidepost andone atRIGHT sidepost. No cabin/roof/floor/lantern/backwall. Same720x512transparentcanvas. LEFT gate logicalx78–148, RIGHTgate x572–642, TOPy128 BOTTOMy448, so320pxhigh fromfloor448 toroofunderside128. Each thin side gate consists of several narrow white-blue upright iron bars, delicate spiral curly tracery/brackets and engraved top/bottomcrosspieces, darkoutline. Leave the big central regionx150–570 TOTALLY emptytransparent: these are two SIDEdoors, not a fullwidthfrontalfence. Match ornate originalcabinwhiteblue carvedscrollstyle, veryslender bars/openholes, no solid panels. Each gateexactly320nativepx tall androughly70wide, attachedtosideposts/betweenroofandfloor, fullyvisible. Strict ONLY #F2F4F8 #BFD8F2 #4A7FC1 shadows#1E3557 contour#0C0A14. Native1–2pixelstair-stepdensity128px/tile, crispfinedetail. Actualalpha0/255transparentbackground, no softglow/gradientblur/antialias/shadows, teal/turquoise/orange/yellow/red/brown, labels/text/grid, character or scene. Two separated thin gates at the two side edges, no otherart.
```

## base

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-4e186a7e-9851-4f98-9798-41c7f54032df.png`

```text
Use case: stylized-concept. ONE Unity pixelart elevator control LeverBase sprite, logical128x192nativeframe, nohandle. Match referencecabin whiteandblue ornate wroughtmetal style, MANY delicate curl/engravedborderreliefdetails but readable small pedestal. Slender antique WHITE/BLUE metal pedestal with flat stable ornate foot at bottom, column body, ROUND paleBLUE/WHITE indicator lamp near top. Include smallround mechanical pivot hub on FRONT lowerhalf for attaching a separate rotating lever handle (hub center logicallyx64,y120). This round lowerhub is a fitting, separate from upperround indicatorlight. Objectcentered, flat bottomat192, wholebodywithinframe with8px horizontalmargin, native fine1–2pixel clusters128px/tile. Height184px native, lighttop aroundy24. Palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14; no turquoise/teal/mint #45DCC4, brown/orange/yellow/red. Brightlamp hardwhite/palebluefacetpixels, no glow/gradientblur. Actualalpha0/255transparentbackground, no antialias, shadows, text/grid, handle, cabin/scene/ground. Just ONE pedestal base andround lamp withlowerpivotmount, not entire combined lever.
```

## handle

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-1a9a90bf-4aaf-4837-9467-d0efc1bc74b8.png`

```text
Use case: stylized-concept. ONE Unity pixelart elevator LeverHandle SEPARATE HANDLE ONLY, logical64x160frame. Referencecabin style: elegant ornate WHITE/SAPPHIREblue antique metal with curvedengraved small motifs. A mostly straight slender vertical handle shaft, ROUND knob at top, knobwhite/paleblue withblue shadedfacet, darkoutline, shaft thinwithsmall ornamentalengravedcollar. Pivot is EXACTLY SHAFT FOOT atmiddlebottomedge coordinate(32,160), renderfoot touchesbottomcenter, symmetric knobcenterx32, knobradius14nativepx, fullheight160. No pedestal/base/indicator lamp/extra floor. Wholehandleinside64pxwidth withmargin, single roundtopknob, notpointedspike. Palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. Fine1–2nativepixel density128px/tile, hardsharpstair-steps, transparentalpha0/255background, no blur/antialias/gradients/softglow/shadow, no teal/turquoise/orange/yellow/red/brown, text/labels/grid. Code willrotate aroundfoot, so do NOT draw a support base aroundpivot.
```

## light

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-89e8979b-ad8f-44d1-9cd1-f3bb6eb60812.png`

```text
Use case: stylized-concept. Elevator CabinLight production horizontal ANIMATION SHEET, EXACTLY FOUR DISTINCT FRAMES in ONE ROW logical2880x512, each720x512cell. Reference1 is complete cabin for exactlampalignment ONLY. Draw ONLY SMALL luminous glass fills of TWO roof-corner lanterns and a tiny lit decorative roofmotif, EVERYTHINGELSEtransparent. DO NOT draw cabin chassis/floor/posts/lanternhousing. Native targetlight positions: leftglass approxx8–29,y87–104 andrightglassx691–712,y87–104 ineachcell; small central roofdiamond/glint aroundx353–367,y72–86. Only thesethreesmallisolatedbrightshapes. Lenslight is several narrow angular/cutglass hardpixel white/paleblue facets withdarkblue facetedge, not circular halos. Fixedsizes/positions all4frames. Subtleloopflicker: phase0 moderatewhiteplanearea, phase1 slightlymorewhite, phase2 mediumwhite withdifferentglint, phase3 slightlylesswhite approachingphase0. Allfourdistinctpixelpatterns, not changingoverallgeometry/not pulsesgroworshrink. STRICT #F2F4F8 whitecores #BFD8F2 paleBLUE and #4A7FC1 blueedge ONLY, absolutely no turquoise/green/teal #45DCC4, warmcolors/red. Light is ALPHAbinaryhardpixels, no softglow/gradient/halo/blur/antialias. Fine1nativepixelstair-steps matchingcabin, transparentbackground. Hugeblankgaps are correct because onlylittlelights atroofpositions. No labels/grid/secondrow/objects/character. Just4successivelightlayerframes derivedfromref.
```

## Mechanical preparation

Cabin generated roof/middle/floor art segments resampled nearest-neighbour to0–128,128–448,448–512respectively; longfloorupperedge detected from sourcealpha. Strict five-color quantization to#F2F4F8,#BFD8F2,#4A7FC1,#1E3557,#0C0A14, hardalpha0/255 and transparentRGB0. No smoothglow/gradients remain. Source lampfacets were separated fromcabin into fixed light footprints;4generatedfacet patterns mapped onto samefootprint, lightlayer only white/paleblue/blue. Cabinlampglassunderneath set to darkblue. Two generated gateislands normalized70x320 and placed atleftx78/rightx572,y128. LeverBase normalized inside128x192; handle foot touches(32,160)boundary in64x160. These are nativePNG coordinates, no .meta pivot/import settings configured.

Preview contains openCabin+Light00, another withGates, combinedlever, separateCast_04x1.6 (256->410roundedpixels), Earthtilex4(32->128), and characterstandinginsidecabin. Actualplayer visibleheight237px fromreference rather thanREADMEapprox218; it fits320pxclearance with83pxheadroom. Preview lever mount at(64,132)inbase is visual assembly only, no prefabposition applied.

## Measured INSTALLED asset checks

| Asset range | Count | Measured frame | Result |
|---|---:|---|---|
| Cabin/Cabin_00.png | 1 | 720x512 | Pass |
| Cabin/CabinLight_00..03.png | 4 | 720x512 | Pass |
| Cabin/CabinGates_00.png | 1 | 720x512 | Pass |
| Lever/LeverBase_00.png | 1 | 128x192 | Pass |
| Lever/LeverHandle_00.png | 1 | 64x160 | Pass |

Every installedfile reopened: exactfilename/count/size, PNG RGBA, alpha only0/255, transparentRGB0, visiblecolors limited toapprovedfivecolors. Fourlightframes have differentpixelhashes, occupyonlycabinopaquefootprint. No forbidden teal/red/orange/yellow in newassets.
- Actualfloor starts atrow448, 64pxfromframebottom. Lowest centralroofextent ends atrow128, clearverticalspan320px. MeasurementROI x270..449. Sidepostcornerbrackets andlowhandrailornaments are not the centralroof/standspace.
- Gateoverlay occupiesonlytwosides, boundsy128..447, bigcenterx148..571transparent. Handle hasvisiblefootpixel atx32,y159; mathematicalpivotbottomcenter(32,160).
- Lightwhitepixelcounts:[57, 85, 54, 57]; adjacent/wrap changedpixels:[59, 61, 30, 27]. Visualoverlaychecked inpreview; notplayed inUnity.

PPU128 and handlepivot(0.5,0) are specifiedtargets, NOT appliedvia.meta. OriginalreferencePNG/meta hashesunchanged. No Unityimport/runtime or collision checks performed.
