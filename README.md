# MG5 Mountain Meadow

Click the ground to move the pudu. Press Space to return to the position before your most recent valid click. Walk close to the sparrows and they will run away.

## Devlog

### NavMesh and physics movement

I would use a NavMesh when a character needs to find a route across the ground and around obstacles. For example, in a game like League of Legends, I can click somewhere and expect the character to find a way there. A NavMesh makes sense because I am choosing a destination instead of controlling every step. In a platform game like Super Mario, I would use movement code with physics and collision checks. Jumping, falling, and controlling the character in the air are a big part of that game, so following a path on the ground would not be enough.

For the teleport, I used NavMeshAgent.Warp. I save the pudu's position when a new valid click is made. Space moves it back to that saved position, and ResetPath stops it from immediately walking toward the old destination again.

### The bird loop

The loop starts with i = 0 and continues while i is less than _birds.Length. Each time through, _birds[i] gets one bird from the array, and i++ moves to the next entry. Starting at zero includes the first bird, and using the array's length means the loop checks every bird without trying to use an index beyond the end.

Vector3.Distance measures the distance between that bird and the player in world units. If the distance is less than or equal to _interactDistance, the manager calls RunAway on that bird and passes in the player's position. In this scene the distance is 5, so all birds within 5 units, including one exactly 5 units away, run away. Birds farther away stay where they are until the player gets closer. I put all five sparrows in the Inspector array so each one gets checked.

## Unity documentation

- [NavMeshAgent.SetDestination](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.NavMeshAgent.SetDestination.html)
- [NavMeshAgent.Warp](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.NavMeshAgent.Warp.html)
- [NavMeshAgent.ResetPath](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.NavMeshAgent.ResetPath.html)

## Open-Source Assets

Original course project from UCI-GDIM31/MG5.

- [Quirky animal 3D models](https://assetstore.unity.com/packages/3d/characters/animals/quirky-series-free-animals-pack-178235)
- [Mountain fantasy environment models](https://www.fab.com/listings/60487a72-3f20-4bb2-aa78-247ed1582656)
