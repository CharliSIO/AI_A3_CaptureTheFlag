CONTROLS	
I didn't get time to implement UI to explain the controls so here they are.
WASD to move
1234 to switch between playing each agent in the player's team.
The toggle on the main menu toggles between 2-team and 4-team modes.

- Replaying the game from menu does not seem to work
- After building flags seem to be appearing below the characters instead of on top of their sprites
- Frame rate seems very slow in build (fine in editor??)



AI TECHNIQUES
Agents use weightings and a rule-based system to add to these weightings
Certain rules directly define states (in prison, returning from prison, returning with flag)
The other rules add to the weightings
The weightings are added together and randomly decided between
State is chosen based on this random choice
- Seek flags
- Seek prison
- Defend flags
- Defend prison

An AI manager holds data about the status of each team's members
Player's state is estimated and included in this
Agents add to weightings based on what other team members are doing
Agents evaluate this every ~1 second, so not every frame

EXTRA FEATURES
- 4 team mode (map isn't very fair whoops)
- More than one agent can leave team zone at a time

