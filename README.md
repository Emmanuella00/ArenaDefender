# Arena Defender

A 2D arena survival game built with C# and MonoGame. You pilot a ship in a fixed
arena, fighting off endless waves of enemies that spawn from every edge and close
in on you. Defeat enemies to score points, collect power-ups they drop, and survive
long enough to reach the target score and win.

## Gameplay

- Enemies spawn continuously and grow more numerous, faster, and tougher each wave.
- Three enemy types: **Fast** (weak, quick), **Standard** (balanced), and **Tank** (slow, high health).
- Defeated enemies may drop power-ups: **Health** (instant heal), **Fire Rate** (shoot faster), and **Speed** (move faster). Buffs are timed.
- **Win** by reaching the target score. **Lose** if your health reaches zero.

## How to Play

Move your ship with the `W`, `A`, `S`, `D` keys and aim with the mouse. Hold the
left mouse button to fire toward the cursor. Aiming near an enemy gives a small
aim-assist nudge, and a marker on the screen edge points toward the nearest threat.
 
On the Game Over or Victory screen, press `R` to play again. Press `Esc` at any
time to quit.

## How to Run

**Requirements:** [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later.

```bash
# From the solution root:
dotnet run --project ArenaDefender
```

## Running the Tests

```bash
dotnet test
```

The solution includes a separate NUnit test project (`ArenaDefender.Tests`) with
31 unit tests covering the game's logic and mathematics.

## Project Structure

```
ArenaDefender/            # Game project
  Entities/               # Player, Enemy (+ subclasses), Projectile, PowerUp
  Systems/                # WaveManager
  Math/                   # GameMath (dot, cross, distance, lerp, collision)
  Rendering/              # ShapeRenderer
  Game1.cs                # Main loop: input, state, rendering
ArenaDefender.Tests/      # NUnit unit tests
```

Game logic is kept separate from rendering so it can be unit-tested without a
graphics device.



## Credits

- Sprites and font: [Kenney.nl](https://kenney.nl) (CC0) and
  [Press Start 2P](https://fonts.google.com/specimen/Press+Start+2P) (SIL Open Font License).
- Built with [MonoGame](https://www.monogame.net/).
