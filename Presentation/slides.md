---
# try also 'default' to start simple
theme: default
# random image from a curated Unsplash collection by Anthony
# like them? see https://unsplash.com/collections/94734566/slidev
background: https://source.unsplash.com/collection/94734566/1920x1080
# apply any windi css classes to the current slide
class: 'text-center'
# https://sli.dev/custom/highlighters.html
highlighter: shiki
# show line numbers in code blocks
lineNumbers: false
# persist drawings in exports and build
drawings:
  persist: false
# use UnoCSS (experimental)
css: unocss
---

# NOVACARE NDC Workshop/Hackathon

---

# Agenda

- 12:30 - 13:00: Intro og demo av kube
- 13:00 - 17:30: Åpent hackathon
- 17:30 - 18:??: Oppsummering

---

# Hva er gjort før hackathon?

- 🔩 **Deler er kjøpt og kuben er montert sammen** - Matriser, elektronikk og håndkontrollere
- 🐍 **Fungerende snake spill** - Kjernelogikken for snake

---

# Demo

---

# Hva gjenstår?

- Videreutvikle spillet. Legge til ny funksjonalitet og gjøre selve spillopplevelsen bedre.
  - Sette opp deamon for å aktivere spillet ved oppstart av pien.
  - Countdown før man starter spillet.
  - Lydeffekter.
  - Kontrollere.
- Flyten av det hele. Fra når man har lyst til å prøve spillet, til at man har spilt ferdig.
- Highscore tavle. Hvordan registrere seg?

---

# Ressurser

- 📚 **Github repo** - https://github.com/novacare-as/cube-snake
- 🥧 **SSH til Pi** - ssh pi@insert.ip.here
- ☁️ **NCG sin Azure subscription** -  Bruk 'cube-snake' sin ressursgruppe

<!--
You can have `style` tag in markdown to override the style for the current page.
Learn more: https://sli.dev/guide/syntax#embedded-styles
-->

<!-- <style>
h1 {
  background-color: #2B90B6;
  background-image: linear-gradient(45deg, #4EC5D4 10%, #146b8c 20%);
  background-size: 100%;
  -webkit-background-clip: text;
  -moz-background-clip: text;
  -webkit-text-fill-color: transparent;
  -moz-text-fill-color: transparent;
}
</style> -->
