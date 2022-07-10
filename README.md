# NOVACARE NDC Workshop/Hackathon

## Hva er gjort før hackathon
I Mai bygget jeg kuben. En mindre versjon av den som bygges for NDC. Originalen er 32x32 piksler fordelt på 5 sider. NDC versjonen er 64x64 og 19.2 cm.

Mikrokomputer er Raspberry Pi 3B+.

Snakekoden var i utgangspunktet laget som en F# demo og er portet over på C#. Korte trekk: 

- Lager et brett av en 2-dimensjonal array bestående av et spillobjekt(snake, eple eller tomt/bakke) og en retning (opp, ned, høyre og venstre). 
- Tar vare på hodets posisjon for en hver loop og traverserer igjennom spillarrayen for å bevege slangen videre. Gjort dette mest for å finne halen til slangen.
- Man har et begrenset antall steg før man sulter ihjel og man spiser eple for å få flere steg.
- Man blir lengre for hvert eple man spiser.
- Dersom man treffer kanten nede eller tyr til selvkannibalisme så dør man.
- 100 poeng for hvert eple man spiser.

Når man skal lage snake på en kube så krever det en spesiell håndtering av kantene. Matrisene er seriekoblet og fremstår om en lang rektangel 320x64. Når man legger disse sammen som en kube så må man bestemme hvor pikslene skal ende når slangen går over kantene. Spesielt håndtering fra toppen til sidene og motsatt. Det er også en håndtering  når slangen går rundt enden av matrisen (320) og tilbake til start igjen (64). 

--- Rammen er 3d printet av Tomas Renaa. 3d modell og print. Kudos! Forrige prototype var skranglete og unøyaktig satt sammen så 3d printingen hjalp med byggekvaliteten. ---

Man skal kunne bevege seg med trådløse BT kontrollere. Lydeffekter må også programmeres inn. 

## Hva forventes av hackathonet?
Hackathonet foregår på en ettermiddag og jeg forventer at vi får en kickstart på utviklingen av komponentene rundt kuben og at flere får kompetanse på selve kuben. 

Dette er ferdig:

- Bygget kube
- Kjernefunksjonalitet av spillet

Aktuelt for workshop/hackathon:

- Videreutvikle spillet. Legge til ny funksjonalitet og gjøre selve spillopplevelsen bedre.
    - Sette opp deamon for å aktivere spillet ved oppstart av pien.
    - Countdown før man starter spillet.
    - Lydeffekter.
    - Kontrollere.
- Flyten av det hele. Fra når man har lyst til å prøve spillet, til at man har spilt ferdig. 
- Highscore tavle. Hvordan registrere seg?

## Inspirasjon/egne notater
NDC gruppa ønsker en retro estetikk og jeg synes at nes.css kunne vært bra til dette.

Registrer deg på mobilen for å stå i kø. Tungvindt? Stå manuelt i kø? Tema for diskusjon.

- Jeg mener at det skal være enkelt å komme i gang og at man skal kunne starte spillet med startknappen. Etter endt spill så kan man velge om å registrere seg ved å gå inn på en link med en generert kode knyttet til scoren din. Da registrerer man seg i etterkant hvis man har lyst.

https://khalidabuhakmeh.com/play-audio-files-with-net#conclusion
https://github.com/dotnet/iot/tree/main/src/devices/RGBLedMatrix
https://github.com/nahueltaibo/gamepad
https://freesound.org/people/LittleRobotSoundFactory/packs/16681/?page=1#sound