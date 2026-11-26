# City Courier 2D – Driving City

A small top-down 2D driving city made in Unity for the **Week 5 Assignment – Build Your Own Driving City**.
Drive a car around the streets and explore the city.

**Author:** _Satayesh Esmaily_
**Unity version:** _e.g. 6000.6.6.f1_

---

## Screenshot

1- Game view

<img width="1589" height="945" alt="image" src="https://github.com/user-attachments/assets/0e09f861-2de4-47c3-8145-5cf979e9606c" />

2- Scene

<img width="1681" height="904" alt="image" src="https://github.com/user-attachments/assets/7bd94e02-74e6-48a5-9f88-16f346564d26" />
<img width="1690" height="930" alt="image" src="https://github.com/user-attachments/assets/51a10ef5-7fff-4e81-a6c3-7cb39b3ee53d" />



---

## Controls

| Key | Action |
|---|---|
| `W` / `↑` | Drive forward |
| `S` / `↓` | Reverse |
| `A` / `←` | Turn left |
| `D` / `→` | Turn right |

The car accelerates smoothly, slows down when no key is pressed, and only turns while it is moving (turning flips in reverse).
It cannot leave the playable city area.

---

## How to open the project

1. Clone the repository.
2. Open it with Unity Hub (**Add project from disk**).
3. Open the scene `Assets/Scenes/Stage1_City.unity`.
4. Press **Play**.

If the car does not move: `Edit > Project Settings > Player > Active Input Handling` → **Both**.


---

## What is in the city

- 8 buildings: modern villa, classic house, restaurant, hospital, apartment tower, office tower, shopping center, gas station
- Horizontal and vertical roads, T-junctions and a crossroad, crosswalks, sidewalks
- Grass, plaza with fountain, benches, bus stop
- Decorations: trees, bushes, flower beds, rocks, street lamps
- City gate with a highway entrance
- Traffic cars (taxi, bus, police, ambulance, van...) and pedestrians
- Visible start location for the player car

### Bonus
- Gas station
- Parking lots with marked spaces (hospital, shopping center)
- Park with trees and flowers
- Delivery markers: `PickupMarker` (yellow) and `DestinationMarker` (red) – visual only

---

## Project organization

### Hierarchy

```
DrivingCity
├── Ground
├── Roads
├── Buildings
├── Decorations
│   ├── Nature
│   ├── Props
│   └── Entrance
├── Vehicles        (traffic)
├── People
├── Markers         (StartPoint, PickupMarker, DestinationMarker)
└── PlayerCar
```

### Sorting Layers (draw order, bottom → top)

`Ground` → `Roads` → `Markers` → `Buildings` → `Props` → `Vehicles` → `Characters` → `Overhead` (trees, gate)

### Camera

Orthographic projection, top-down view, centered on the city.

### Folders

```
Assets
├── Sprites      Buildings, Vehicles, Characters, Nature, Roads (Streets / Highways), Ground, Props, Entrance, FX
├── Prefabs      one prefab per sprite (created by the builder)
├── Scripts      DrivingCarController.cs
├── Scenes       Stage1_City.unity
```

---

## Car script

`DrivingCarController.cs` – adjustable in the Inspector:

- Max Speed, Max Reverse Speed, Acceleration, Friction
- Turn Speed
- Min / Max Bounds (playable area)

Uses `Time.deltaTime` for frame-rate independent movement.

---
