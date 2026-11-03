# Camera Controller Design

## Geometry and Dead Zone

The camera follows a focus point offset from the player so the player sits at 24% of the view width and 25% of the view height. At level edges, the camera center is clamped to the map rectangle inset by the camera's half-width and half-height. The current scene has a finite 54-unit floor and side boundaries; `Geometry` is kept as the scene-authoring container for later level edits.

While idle, the camera does not move when the focus point remains within a 0.42 by 0.34 world-unit dead zone. Crossing that region activates tracking. Tracking stays active while the player moves, then becomes idle only after the focus point and both velocities settle.

## Tracking Law

For one axis, let `e = target - camera`, `v_t` be target velocity, and `v_c` be camera velocity. While tracking, the requested closing speed is

```text
u = sign(e) * min(v_catch, k * abs(e), sqrt(2 * a_camera_axis * abs(e)))
v_c_requested = v_t + u
```

The camera moves each velocity component toward its request with a bounded acceleration. The total camera acceleration is at most 12 world units/s^2, so each camera axis is limited to

```text
a_camera_axis = 12 / sqrt(2)
```

For a fixed target or an interval of constant player velocity, the square-root term is the stopping-speed envelope: a closing speed `u` needs `u^2 / (2 * a_camera_axis)` distance to stop. Limiting `u` to `sqrt(2 * a_camera_axis * abs(e))` keeps that stopping distance within the remaining error. The proportional term reduces speed near the target, and `v_t` feed-forward makes camera velocity converge to player velocity during steady movement. At rest, both velocities converge to zero and the camera settles on the focus point. The current grounded player stays below the camera's minimum vertical center, so the vertical map clamp holds the camera steady while gravity settles the player onto the floor.

The player now switches horizontal velocity immediately between `-maxSpeed`, zero, and `+maxSpeed`. During an interval with constant input, the camera's available per-axis braking acceleration is `a_camera_axis`. Key presses, releases, and collision stops are instantaneous target-velocity changes, so they have no finite acceleration bound; the envelope cannot guarantee no overshoot at those transitions.

## Why Not a Spring Alone

A critically damped spring uses `a_c = a_t + 2 * omega * (v_t - v_c) + omega^2 * e`. In a continuous, unsaturated model its error obeys `(d/dt + omega)^2 e = 0`, giving `e(t) = (A + B*t) * exp(-omega*t)` for a fixed target. That response does not overshoot from rest. In a real frame loop, acceleration limits, collisions, and changes of target velocity can invalidate the ideal response. The stopping-speed envelope gives the controller an explicit braking-distance constraint during constant-velocity intervals instead of relying only on damping.

## Guarantee Boundary

For a target with bounded acceleration, the no-crossing argument also requires the camera to begin inside the controller's reachable braking envelope. The current player instead changes speed instantaneously on A/D input. If camera position and velocity match the moving target at steady state, then a key release can leave the camera moving with no distance remaining to brake; no finite-acceleration controller can both stop before the target and prevent overshoot in that case. The camera remains acceleration-limited and the map clamp still prevents views beyond the level, but strict no-overshoot at abrupt input or collision transitions is not guaranteed.
