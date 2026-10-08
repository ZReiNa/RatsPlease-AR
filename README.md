# Rats Please AR

**Rats Please AR** is a mobile augmented reality game made in **Unity** with **AR Foundation** for Android. The player must serve the correct item to each guest in a fast-paced AR gameplay loop before the 3-minute timer runs out.

## About the game

At the start of the game, a short tutorial explains the rules. Then the match begins:

- A **random rule image** appears on screen.
- A **guest** spawns on top of a tracked image marker.
- The player chooses between **cheese** or **mousetrap** using on-screen toggles.
- The selected 3D object can be placed on a detected AR plane and dragged toward the guest.
- If the correct object touches the guest, the player earns points.
- If the wrong object touches the guest, no points are awarded.
- When the timer ends, the final score is shown and the player returns to the main menu.

## Controls

- **Start button**: begins the game
- **Next button**: advances the tutorial dialogue
- **Cheese / Trap toggles**: select which object to spawn
- **Tap on a detected plane**: places the selected object
- **Drag the object**: moves it toward the guest
- **Tap and drag a guest**: rotates the guest
- **Menu / Exit button**: returns to the main menu

## Technical features

This project includes:

- **Plane tracking**
- **Image tracking**
- **AR occlusion support** implemented, but not fully working yet
- **3-minute countdown timer**
- **Score system**
- **Random guest spawning** from 9 different guest prefabs
- **Collision-based gameplay** using trigger colliders
- **Tutorial system** that only appears the first time the game is launched in a session
- **Results screen** showing the final score

## Development note

This was developed as a **student project**. Some **images, 3D models, and other assets were not created by me** and were used only for educational purposes within the project.

## Author

Developed by **Saüc Pellejero Galvez**.