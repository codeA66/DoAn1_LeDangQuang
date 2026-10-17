# PROMPTS USED — Props preview

Date: 2026-10-17. Built-in ImageGen. Initial preview: 16 static samples. Full set subsequently approved and installed; see FULL SET section below.

## References
- Elevator/Cabin/Cabin_00.png: white/blue palette and pixel finish.
- Environment/BuriedDepths/Platforms/Platforms_Stone.png: weathered material texture.
- Preview uses actual Earth_r0_c1 and Earth_r1_c1 tiles at x4 and Cast_04 at x1.6 (410x410 canvas; actual opaque height 237 px).

## Preparation and measurements
Cropped generated alpha bounds at threshold 128, fitted proportionally with nearest-neighbor to requested canvas, anchored top / left / bottom per README, quantized to the five approved colors. Final alpha is binary 0/255. No soft lighting added. Native samples are in scratch pending approval; project deliverable is Preview_Props.png. No Unity import/settings or Game View validation performed.

| Sample | Measured size | Alpha | Palette | Result |
|---|---|---|---|---|
| GlowMushroom_00 | 128x128 | 0/255 | 5 allowed colors | PASS |
| HangingLamp_00 | 128x320 | 0/255 | 5 allowed colors | PASS |
| CrystalCluster_00 | 192x192 | 0/255 | 5 allowed colors | PASS |
| RootHang_S | 128x256 | 0/255 | 5 allowed colors | PASS |
| RootHang_M | 128x448 | 0/255 | 5 allowed colors | PASS |
| RootHang_L | 192x640 | 0/255 | 5 allowed colors | PASS |
| RootWall_00 | 256x256 | 0/255 | 5 allowed colors | PASS |
| Chain_Hang | 64x512 | 0/255 | 5 allowed colors | PASS |
| Pipe_Broken_00 | 384x192 | 0/255 | 5 allowed colors | PASS |
| Support_Beam | 192x512 | 0/255 | 5 allowed colors | PASS |
| Minecart_Wrecked | 320x192 | 0/255 | 5 allowed colors | PASS |
| Stalactite_00 | 64x128 | 0/255 | 5 allowed colors | PASS |
| Stalactite_01 | 96x192 | 0/255 | 5 allowed colors | PASS |
| Stalactite_02 | 128x256 | 0/255 | 5 allowed colors | PASS |
| Rubble_00 | 192x96 | 0/255 | 5 allowed colors | PASS |
| MossTuft_00 | 128x64 | 0/255 | 5 allowed colors | PASS |

## Exact prompt set

### GlowMushroom_00
Use case: stylized-concept. Asset type: ONE native pixelart decorative GlowMushroom_00 SAMPLE for Unity2D buriedearthgame. Logical transparentframe 128x128px, density128px/tile; generate ONLY one static sample, NOT animation/variants/collection. ONE low cluster of3–5bioluminescent mushrooms growingfromsmallflatstonebase. Rounded caps, whitecores andpaleblue stepped luminous rims, curvingblue stems; lowfriendlydecoration, notcollectibleicon. No smoothhalo. Reference1 Elevator cabin supplies approved WHITE and BLUE palette/finish and fine1–2nativepixel edge density; reference2 buriedplatform supplies ONLY material grain/weathering, NOT brown/orange colors. Main visible masses MUST be WHITE and BLUE and stand outstrongly against brownsoil/mossgreenambientbackground. Strict flat palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 withcontour#0C0A14. NO turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Flat bottomrestingbaseline, nothingextendsbelowfootprint; wholeobjectvisible. Fine crisp native1–2pixelstair-steps and texturedpixelclusters, NOT coarse oversizedblockpixels. Hard pixel opaque body withactualalpha0background, no softglow/blur/antialias/gradients/shadows, no scene/ground/floorbackground/text/labels/grid. Keep complete isolatedobject centered andinsideframe, no croppingother than specified attachmentedge. A decoration, notinteractablepickup/hazard. Lightingifpresent uses hardwhitecore andpalebluesteppedrim, never softglow.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-0daf5d40-0f0e-4584-bc54-645d12256b61.png`

### HangingLamp_00
Use case: stylized-concept. Asset type: ONE native pixelart decorative HangingLamp_00 SAMPLE for Unity2D buriedearthgame. Logical transparentframe 128x320px, density128px/tile; generate ONLY one static sample, NOT animation/variants/collection. ONE old mininglantern hangingfromnarrowlinkedchain atTOPEDGE, completechainpluslamp. White/blue chippedmetal cagewithrivets, smallroundedbulb whitecore paleblue hardsteppedring. Antiqueindustrialwear andtinyengravingmatchesreferenceelevator, not a modernstreetlight. Atrestvertical. Reference1 Elevator cabin supplies approved WHITE and BLUE palette/finish and fine1–2nativepixel edge density; reference2 buriedplatform supplies ONLY material grain/weathering, NOT brown/orange colors. Main visible masses MUST be WHITE and BLUE and stand outstrongly against brownsoil/mossgreenambientbackground. Strict flat palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 withcontour#0C0A14. NO turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Attachment touches TOPedge of logicalframe; objecthangsdown and wholebottomvisible. Fine crisp native1–2pixelstair-steps and texturedpixelclusters, NOT coarse oversizedblockpixels. Hard pixel opaque body withactualalpha0background, no softglow/blur/antialias/gradients/shadows, no scene/ground/floorbackground/text/labels/grid. Keep complete isolatedobject centered andinsideframe, no croppingother than specified attachmentedge. A decoration, notinteractablepickup/hazard. Lightingifpresent uses hardwhitecore andpalebluesteppedrim, never softglow.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-9e8a8ca3-b0fd-4f39-a207-0e3998f55be4.png`

### CrystalCluster_00
Use case: stylized-concept. ONE native pixelart decorative SAMPLE CrystalCluster_00 for Unity2D buriedearthlevel. Logicaltransparentframe 192x192, density128px/tile; ONLY one static sample, no animation/extra variants. ONE cluster offacetedWHITE/BLUE mineralcrystals growingfromsmallflatdarkstonebase. Unequal crystallinecolumnheights, clearfacetedplanes andwidebluntedpoints, whitecores palebluemineralfaces, bluebands. Notgreen/notturquoise, no glowingmist. Reference1 elevator supplies WHITE/BLUEcolorfinish and native1–2pixel edgedensity. Reference2 buriedplatform supplies ONLY oldbark/stone/metal texture, NOT its browncolors. Objects MUST stand outstrongly againstbrownsoil/mossgreenambient. Strictflat paletteONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. No turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprint, no protrusionsbelowsupportingfloor. Allotherpartsinsideframe, fullyvisible, isolated on actualalpha0background, bodyopaquehardpixelstair-steps andfine native1–2pixelclusters. No giantchunky8bitblocks, no smoothglow/gradientblur/antialias/softshadow, text/labels/grid/scene/ground. Whitepaleblue lightifpresent hasonlycrispsteppedbands. Decorationonly, no danger/collectible/readabletext.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-8d89a56f-12e7-4199-a584-c61ed17b6e43.png`

### RootHang_S
Use case: stylized-concept. ONE native pixelart decorative SAMPLE RootHang_S for Unity2D buriedearthlevel. Logicaltransparentframe 128x256, density128px/tile; ONLY one static sample, no animation/extra variants. ONE SHORT hangingrootbundle, canopyrootsattach atTOPEDGE; twistedorganicbarkfissures andsmallrootthreads cascade DOWN, sparsebifurcations, bluntcurledtips, weatheredwhite/blue mineralizedbark andsmallwhite/blue industrialclamps. Short2tilelength, nonhazard. Reference1 elevator supplies WHITE/BLUEcolorfinish and native1–2pixel edgedensity. Reference2 buriedplatform supplies ONLY oldbark/stone/metal texture, NOT its browncolors. Objects MUST stand outstrongly againstbrownsoil/mossgreenambient. Strictflat paletteONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. No turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Attachment touchesTOPframeedge, wholeobjecthangsdown. Allotherpartsinsideframe, fullyvisible, isolated on actualalpha0background, bodyopaquehardpixelstair-steps andfine native1–2pixelclusters. No giantchunky8bitblocks, no smoothglow/gradientblur/antialias/softshadow, text/labels/grid/scene/ground. Whitepaleblue lightifpresent hasonlycrispsteppedbands. Decorationonly, no danger/collectible/readabletext.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-ba36c0e1-fa96-4327-9de9-b0f249d53ea7.png`

### RootHang_M
Use case: stylized-concept. ONE native pixelart decorative SAMPLE RootHang_M for Unity2D buriedearthlevel. Logicaltransparentframe 128x448, density128px/tile; ONLY one static sample, no animation/extra variants. ONE MEDIUM length hangingrootbundle, attach atTOPEDGE; severalintertwinedslenderroots fallDOWN withknots andsmallfibers, unevenbranchcurves, blunt curledends. White/blue mineralizedbark, prominentwhiteblueweatheredbands/clamps toseparatefromearth.3.5tilelength. Reference1 elevator supplies WHITE/BLUEcolorfinish and native1–2pixel edgedensity. Reference2 buriedplatform supplies ONLY oldbark/stone/metal texture, NOT its browncolors. Objects MUST stand outstrongly againstbrownsoil/mossgreenambient. Strictflat paletteONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. No turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Attachment touchesTOPframeedge, wholeobjecthangsdown. Allotherpartsinsideframe, fullyvisible, isolated on actualalpha0background, bodyopaquehardpixelstair-steps andfine native1–2pixelclusters. No giantchunky8bitblocks, no smoothglow/gradientblur/antialias/softshadow, text/labels/grid/scene/ground. Whitepaleblue lightifpresent hasonlycrispsteppedbands. Decorationonly, no danger/collectible/readabletext.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-71133a83-a7f4-4c50-a8cd-f37f5930c7b4.png`

### RootHang_L
Use case: stylized-concept. ONE native pixelart decorative SAMPLE RootHang_L for Unity2D buriedearthlevel. Logicaltransparentframe 192x640, density128px/tile; ONLY one static sample, no animation/extra variants. ONE LONG hangingrootbundle, attach atTOPEDGE; meanderingthickerbarkrootsandmanysmallhairroots, fewbrokenoldwhiteblueconduitbandscaughtamongroots. Organicserpentinecontour anddanglingroundedtips, detailedbarktexturebutreadablesilhouette, white/blue mineralizedtones.5tilelength. Reference1 elevator supplies WHITE/BLUEcolorfinish and native1–2pixel edgedensity. Reference2 buriedplatform supplies ONLY oldbark/stone/metal texture, NOT its browncolors. Objects MUST stand outstrongly againstbrownsoil/mossgreenambient. Strictflat paletteONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. No turquoise/teal/mint #45DCC4, green/mossgreen, red, orange/yellow/brown, warmLIGHT. Attachment touchesTOPframeedge, wholeobjecthangsdown. Allotherpartsinsideframe, fullyvisible, isolated on actualalpha0background, bodyopaquehardpixelstair-steps andfine native1–2pixelclusters. No giantchunky8bitblocks, no smoothglow/gradientblur/antialias/softshadow, text/labels/grid/scene/ground. Whitepaleblue lightifpresent hasonlycrispsteppedbands. Decorationonly, no danger/collectible/readabletext.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-8541bb1b-082e-40e9-9c58-5b909d509ced.png`

### RootWall_00
Use case: stylized-concept. ONE native pixelart decorative RootWall_00 SAMPLE, Unity2D buriedearthlevel, logicalframe256x256,128px/tile. ONE wallclingingorganicroot fan. Mainthickroot attachesalongLEFTEDGE, smallerknottedrootsspreadandcurlRIGHT acrosswall, branchingbarkridgesandlittlefissures. Sparseopennegativeareas, palegrayblueweatheredroots withwhite/blueclampaccents, decorative/nohazard. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. AttachatLEFTframeedge, grow/spreadRIGHT. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-6f32e597-74fc-46b6-be02-7c59c1223952.png`

### Chain_Hang
Use case: stylized-concept. ONE native pixelart decorative Chain_Hang SAMPLE, Unity2D buriedearthlevel, logicalframe64x512,128px/tile. ONE singleLONG slim miningCHAIN hangingstraightDOWN fromTOPEDGE, interlinkedelongatedovallinks withalternatingfront/sideorientations, chippedwhitepaint/blueoxidizedmetal, darkbluecrevices, a fewwornrivetedlinkrepairs. Full4tilelength, no lantern/hookweapon atbottom. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. AttachatTOPframeedge, hangDOWN, wholebottomvisible. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-889bb01c-4f46-452a-8b92-81a867604a56.png`

### Pipe_Broken_00
Use case: stylized-concept. ONE native pixelart decorative Pipe_Broken_00 SAMPLE, Unity2D buriedearthlevel, logicalframe384x192,128px/tile. ONE BROKEN buriedindustrialPIPE segment horizontalwithoneelbow, raggeddarkopenbrokenmouth, boltedflanges andrivetedrepairbands. Mainframechippedwhite/bluepaintedmetal, blueshadowedoldwear, littleencrustedcharcoalstoneatflatbottom suggests halfburied. Placedagainstwallfloor, no glowingwater. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprintwithnothingbelowbaseline. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-7aacba93-f338-44fd-927b-8c1489698b36.png`

### Support_Beam
Use case: stylized-concept. ONE native pixelart decorative Support_Beam SAMPLE, Unity2D buriedearthlevel, logicalframe192x512,128px/tile. ONE uprightOLD MINE SUPPORTBEAM, weatheredtimbergrain representedcoldbluegray withchippedwhite/sapphireblue IRONBANDS, boltsandsmallbrace. Tall4tilepost withflatfootandshorttopcrosspiece, not a wholearchway. Slimdecorativestaticwallprop, finewoodgrain/hatching. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprintwithnothingbelowbaseline. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-26a87703-74c1-4016-b275-ebddccaf23ac.png`

### Minecart_Wrecked
Use case: stylized-concept. ONE native pixelart decorative Minecart_Wrecked SAMPLE, Unity2D buriedearthlevel, logicalframe320x192,128px/tile. ONE wreckedoldMINECART tippedslightlytooneside, bentopenmetalbucket, missing/brokenwheel, shortaxle, chippedwhite/bluepaintedframes/rivets, darkbluescratchedmetalinsides, somecharcoalbluegrayrubble partiallyburyingloweredge. Lowoblongsilhouette andflatrestingfootprint; no rails/scene. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprintwithnothingbelowbaseline. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-7e4c3bd8-b41c-4093-840f-b2d979a4877f.png`

### Stalactite_00
Use case: stylized-concept. ONE native pixelart decorative Stalactite_00 SAMPLE, Unity2D buriedearthlevel, logicalframe64x128,128px/tile. ONE SMALL DECORATIVE caveSTALACTITE, broadflattopattachesceiling, roundedsoftbluntdroopingtip likea wornlimestonefinger, NOT pointedspike/nothazard. Coldwhitebluegraycalcitewithsubtlelayeredmineralbands. Singleclearroundedpendant withridges. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. AttachatTOPframeedge, hangDOWN, wholebottomvisible. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-59c55b11-74c6-438f-ab91-7bfa73e133fc.png`

### Stalactite_01
Use case: stylized-concept. ONE native pixelart decorative Stalactite_01 SAMPLE, Unity2D buriedearthlevel, logicalframe96x192,128px/tile. ONE MEDIUM DECORATIVE caveSTALACTITE withtwofusedroundedbulbouslobes, broadflattopattachesceiling, erodedROUND BLUNTBOTTOM tips, NOT sharpcrystal/spike/hazard. White/bluegraysedimentarymineralridges andfinecoldblueedge. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. AttachatTOPframeedge, hangDOWN, wholebottomvisible. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-0215b564-fa26-44cb-a963-30f50dbbcb59.png`

### Stalactite_02
Use case: stylized-concept. ONE native pixelart decorative Stalactite_02 SAMPLE, Unity2D buriedearthlevel, logicalframe128x256,128px/tile. ONE LARGE DECORATIVE caveSTALACTITE, asymmetricalroundedcalcitecolumn/lobes connectedunderbroadflattop, deepwornmineralgrooves, visiblyROUNDED BLUNTEND likea drippedstoneformation, NOT needle/sharpgai. White/palebluecalcitewithdarkbluegraycrevices. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. AttachatTOPframeedge, hangDOWN, wholebottomvisible. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-714c4ae5-2153-4144-a32a-def4c60792ea.png`

### Rubble_00
Use case: stylized-concept. ONE native pixelart decorative Rubble_00 SAMPLE, Unity2D buriedearthlevel, logicalframe192x96,128px/tile. ONE LOW pileofsmallbrokenround/angularSTONES andsmallpaleBLUEmoss onflatbottom. Unevenstoneswithhairlinecracks, coldbluegraystoneplanes andwhiteedges, somewhite/blueoldmetalbandfragmentcaughtunderrocks. Lowtrimdoesnotobscurefloorpath, no sharpspikes. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprintwithnothingbelowbaseline. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-724db72b-a0c4-4d44-84a7-bba330f19aa6.png`

### MossTuft_00
Use case: stylized-concept. ONE native pixelart decorative MossTuft_00 SAMPLE, Unity2D buriedearthlevel, logicalframe128x64,128px/tile. ONE veryLOW softMOSS/shortFERNtuft withroundedtinyleaves, paleWHITE andBLUE ONLY, darkbluefibers atflatbase, gentlycurledstems, notgreenandnotsharpgrassspikes. Tinyfloortrimlesshalfatilehigh, leavesgroupedinreadablepixelclusters. Reference1 elevator WHITE/BLUEpalette andfinepixelartdensity. Reference2 buriedplatform MATERIALTEXTUREONLY, DO NOT copybrowncolors. AllmajorvisiblemasseswhiteandBLUE tostandoutagainstbrownsoil/mossgreenambient. Strictflatpalette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour#0C0A14. NO green/mossgreen, turquoise/teal/mint #45DCC4, red, orange/yellow/brown, warmLIGHT. Flatbottomfootprintwithnothingbelowbaseline. Fine1–2nativepixelstair-steps andweatheredtinytextureclusters, notgiant8bitblocks. Wholeisolatedobjectfullyinsideframe, actualtransparentbackground alpha0 andopaquehardpixels. No blur/antialias/glow/gradient/softshadow/text/grid/labels/scene/ground. ONLY onestaticitem, no animation/extra variants, decorative/nonhazard.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-9cd96dc7-0fe7-4418-abe7-15944af0755b.png`


# FULL SET — approved 2026-10-17

Installed all 29 requested PNG into Light/ and Deco/. Built-in ImageGen produced 7 distinct extra variants and two horizontal 4-frame animation sheets using approved sprites as references. Original static samples retained. Animation sheets sliced into equal cells, alpha-cropped, resized with one common scale per sequence, anchored at ground or fixed ceiling mount; no soft light/code gradient added. Palette normalized to five approved colors and alpha thresholded to 0/255. The two animated frame-00 files use the new sheet renders of the approved design. Registration cleanup limits lamp sway to 2 native pixels, ceiling mount fixed; dim mushroom frame preserves body and only uses the generated cap highlight pattern to replace white with pale blue, avoiding excessive darkening.

## Measured file report

| File | Measured size | Alpha | Palette | Result |
|---|---|---|---|---|
| Deco\Chain_Hang.png | 64x512 | 0/255 | 5 allowed colors | PASS |
| Deco\Minecart_Wrecked.png | 320x192 | 0/255 | 5 allowed colors | PASS |
| Deco\MossTuft_00.png | 128x64 | 0/255 | 5 allowed colors | PASS |
| Deco\MossTuft_01.png | 128x64 | 0/255 | 5 allowed colors | PASS |
| Deco\MossTuft_02.png | 128x64 | 0/255 | 5 allowed colors | PASS |
| Deco\MossTuft_03.png | 128x64 | 0/255 | 5 allowed colors | PASS |
| Deco\Pipe_Broken_00.png | 384x192 | 0/255 | 5 allowed colors | PASS |
| Deco\Pipe_Broken_01.png | 384x192 | 0/255 | 5 allowed colors | PASS |
| Deco\RootHang_L.png | 192x640 | 0/255 | 5 allowed colors | PASS |
| Deco\RootHang_M.png | 128x448 | 0/255 | 5 allowed colors | PASS |
| Deco\RootHang_S.png | 128x256 | 0/255 | 5 allowed colors | PASS |
| Deco\RootWall_00.png | 256x256 | 0/255 | 5 allowed colors | PASS |
| Deco\RootWall_01.png | 256x256 | 0/255 | 5 allowed colors | PASS |
| Deco\Rubble_00.png | 192x96 | 0/255 | 5 allowed colors | PASS |
| Deco\Rubble_01.png | 192x96 | 0/255 | 5 allowed colors | PASS |
| Deco\Rubble_02.png | 192x96 | 0/255 | 5 allowed colors | PASS |
| Deco\Stalactite_00.png | 64x128 | 0/255 | 5 allowed colors | PASS |
| Deco\Stalactite_01.png | 96x192 | 0/255 | 5 allowed colors | PASS |
| Deco\Stalactite_02.png | 128x256 | 0/255 | 5 allowed colors | PASS |
| Deco\Support_Beam.png | 192x512 | 0/255 | 5 allowed colors | PASS |
| Light\CrystalCluster_00.png | 192x192 | 0/255 | 5 allowed colors | PASS |
| Light\GlowMushroom_00.png | 128x128 | 0/255 | 5 allowed colors | PASS |
| Light\GlowMushroom_01.png | 128x128 | 0/255 | 5 allowed colors | PASS |
| Light\GlowMushroom_02.png | 128x128 | 0/255 | 5 allowed colors | PASS |
| Light\GlowMushroom_03.png | 128x128 | 0/255 | 5 allowed colors | PASS |
| Light\HangingLamp_00.png | 128x320 | 0/255 | 5 allowed colors | PASS |
| Light\HangingLamp_01.png | 128x320 | 0/255 | 5 allowed colors | PASS |
| Light\HangingLamp_02.png | 128x320 | 0/255 | 5 allowed colors | PASS |
| Light\HangingLamp_03.png | 128x320 | 0/255 | 5 allowed colors | PASS |

## Animation measurements

{
  "GlowMushroom": {
    "unique_frames": 4,
    "changed_pixels_including_wrap": [
      1547,
      1550,
      2727,
      1563
    ],
    "bounds": [
      [
        6,
        0,
        122,
        128
      ],
      [
        6,
        0,
        122,
        128
      ],
      [
        6,
        0,
        122,
        128
      ],
      [
        6,
        0,
        122,
        128
      ]
    ]
  },
  "HangingLamp": {
    "unique_frames": 4,
    "changed_pixels_including_wrap": [
      6260,
      6524,
      5709,
      5981
    ],
    "bounds": [
      [
        24,
        0,
        104,
        320
      ],
      [
        25,
        0,
        105,
        320
      ],
      [
        24,
        0,
        104,
        320
      ],
      [
        23,
        0,
        103,
        320
      ]
    ]
  }
}

All four frames in each loop differ. Changed pixel counts include 03→00; this checks uniqueness and wrap distance, not Unity playback. No Unity import settings, scene/prefab edits or Game View test performed. PPU128 remains an import target in README; existing .meta files were not edited.

## Additional exact prompts

### GlowMushroom_Loop
Use case: stylized-concept. Production native pixel art Unity2D decorative cave prop GlowMushroom_Loop, logical frame 128x128. Four animation frames in ONE horizontal row of four equal cells, 512x128 logical sheet. Preserve EXACT mushroom cluster anatomy, silhouette, baseline and position from reference in every cell. ONLY cap white/pale-blue pixel highlights pulse very gently: medium, bright, medium, dim, seamlessly repeat. No waving, no added mushrooms. Use attached APPROVED sprite as strict style/material/palette/scale reference. Transparent background actual alpha, hard opaque pixel edges. Strict ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green, turquoise, red, orange, yellow, brown; no soft glow, blur, antialias or smooth gradients. Fine 1-2 native pixel stair steps and small weathered clusters, not oversized coarse pixels. Whole object visible. No labels, grid, backdrop, ground plane or extra items. Decoration only, no dangerous spikes. EXACTLY FOUR equal cells aligned horizontally; SAME sprite scale and anchor in all cells.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-e5dba904-7959-47f5-95eb-b7384b0767a4.png`

### HangingLamp_Loop
Use case: stylized-concept. Production native pixel art Unity2D decorative cave prop HangingLamp_Loop, logical frame 128x320. Four animation frames in ONE horizontal row of four equal cells, 512x320 logical sheet. Preserve EXACT lantern cage, chain link count and ceiling mount of reference. Very slight pendulum sway in pixel art: center, 2 pixels right, center, 2 pixels left; top mount fixed at same top center in every cell. Bulb varies very gently white/pale blue. Keep complete lamp inside each cell, same height and size, seamless loop. Use attached APPROVED sprite as strict style/material/palette/scale reference. Transparent background actual alpha, hard opaque pixel edges. Strict ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green, turquoise, red, orange, yellow, brown; no soft glow, blur, antialias or smooth gradients. Fine 1-2 native pixel stair steps and small weathered clusters, not oversized coarse pixels. Whole object visible. No labels, grid, backdrop, ground plane or extra items. Decoration only, no dangerous spikes. EXACTLY FOUR equal cells aligned horizontally; SAME sprite scale and anchor in all cells.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-34f7803b-3e4c-42f2-8d2b-5926aebb3fea.png`

### RootWall_01
Use case: stylized-concept. Production native pixel art Unity2D decorative cave prop RootWall_01, logical frame 256x256. One distinct second variant wall root fan, not mirror/copy: thick knotted main root along LEFT edge, two broad arcing branches curl RIGHT, sparse smaller trailing root fibers. Match reference material, fine pixel density, white-blue mineralized bark. Attach at LEFT frame edge. Use attached APPROVED sprite as strict style/material/palette/scale reference. Transparent background actual alpha, hard opaque pixel edges. Strict ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green, turquoise, red, orange, yellow, brown; no soft glow, blur, antialias or smooth gradients. Fine 1-2 native pixel stair steps and small weathered clusters, not oversized coarse pixels. Whole object visible. No labels, grid, backdrop, ground plane or extra items. Decoration only, no dangerous spikes. ONLY one isolated sprite.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-e9462bc3-ceeb-4b5a-bce9-afb736a6f9c4.png`

### Pipe_Broken_01
Create one native pixel art sprite Pipe_Broken_01, logical frame 384x192. One distinct second broken pipe variant: a short horizontal industrial pipe with fractured OPEN RIGHT mouth, left flange and small upward broken elbow at left, riveted white-blue repair plates and chipped paint, flat stone resting base. Different silhouette and damage from reference, same cold material. Use attached approved sprite as strict material, palette and fine pixel-density reference. Entire object isolated on actual transparent background. ONLY colors #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green/teal/red/orange/yellow. Hard pixel edges, no antialiasing/blur/softglow/gradient. Flat resting bottom; no text or backdrop. One sprite, not a sheet.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-51b556c2-3d39-45b4-809c-ea3e3998461b.png`

### Rubble_01
Use case: stylized-concept. Production native pixel art Unity2D decorative cave prop Rubble_01, logical frame 192x96. One low rubble pile variant, central wide flat cracked boulder with smaller stones and tiny pale-blue moss along sides, flat bottom. Different silhouette and stone arrangement from reference, same fine pixel finish. Use attached APPROVED sprite as strict style/material/palette/scale reference. Transparent background actual alpha, hard opaque pixel edges. Strict ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green, turquoise, red, orange, yellow, brown; no soft glow, blur, antialias or smooth gradients. Fine 1-2 native pixel stair steps and small weathered clusters, not oversized coarse pixels. Whole object visible. No labels, grid, backdrop, ground plane or extra items. Decoration only, no dangerous spikes. ONLY one isolated sprite.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-66547eb2-b6dd-4273-9e99-4d366ff00355.png`

### Rubble_02
Create one native pixel art sprite Rubble_02, logical frame 192x96. One low rubble pile variant, two rounded stone groups with shallow valley between, small bent white-blue riveted metal scrap partly buried, pale-blue moss. Flat bottom, distinct from reference. Use attached approved sprite as strict material, palette and fine pixel-density reference. Entire object isolated on actual transparent background. ONLY colors #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green/teal/red/orange/yellow. Hard pixel edges, no antialiasing/blur/softglow/gradient. Flat resting bottom; no text or backdrop. One sprite, not a sheet.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-8f3f9472-b1f3-4c3e-acaa-376ec3b7647f.png`

### MossTuft_01
Create one native pixel art sprite MossTuft_01, logical frame 128x64. One very low blue-white moss tuft variant, rounded cushion-like little leaf clusters with a few curled fern stems leaning left; flat base, no sharp grass, distinct arrangement from reference. Use attached approved sprite as strict material, palette and fine pixel-density reference. Entire object isolated on actual transparent background. ONLY colors #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green/teal/red/orange/yellow. Hard pixel edges, no antialiasing/blur/softglow/gradient. Flat resting bottom; no text or backdrop. One sprite, not a sheet.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-111b5829-bcfe-47c2-ba8d-65ab4e8d86d7.png`

### MossTuft_02
Create one native pixel art sprite MossTuft_02, logical frame 128x64. One very low blue-white moss tuft variant, two small separated rosettes of rounded leaves and tiny curled fiddleheads between, flat base, distinct arrangement from reference. Use attached approved sprite as strict material, palette and fine pixel-density reference. Entire object isolated on actual transparent background. ONLY colors #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green/teal/red/orange/yellow. Hard pixel edges, no antialiasing/blur/softglow/gradient. Flat resting bottom; no text or backdrop. One sprite, not a sheet.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-74c4f05a-417e-46bf-9848-335883e7dd82.png`

### MossTuft_03
Create one native pixel art sprite MossTuft_03, logical frame 128x64. One very low blue-white moss tuft variant, asymmetric tiny broad round leaves with a few soft fern fronds leaning right and creeping stems, flat base, distinct arrangement from reference. Use attached approved sprite as strict material, palette and fine pixel-density reference. Entire object isolated on actual transparent background. ONLY colors #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14. No green/teal/red/orange/yellow. Hard pixel edges, no antialiasing/blur/softglow/gradient. Flat resting bottom; no text or backdrop. One sprite, not a sheet.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-a3380ef7-ac2c-4e60-8f96-f16f997af98a.png`
