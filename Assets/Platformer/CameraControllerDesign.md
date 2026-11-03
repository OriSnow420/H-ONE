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

The proportional term reduces speed near the target, and `v_t` feed-forward lets camera velocity match player velocity during steady movement when enough position margin remains. The safety filter can leave a fixed position gap instead of converging to the focus point. Since the player can change from rest to `maxSpeed` immediately, even a stationary camera needs a gap before it can guarantee that a reversing player will not cross it. The current grounded player stays below the camera's minimum vertical center, so the vertical map clamp holds the camera steady while gravity settles the player onto the floor.

The player switches horizontal velocity immediately between `-maxSpeed`, zero, and `+maxSpeed`, so the camera uses the player's speed limit instead of assuming bounded target acceleration. The controller updates camera position with the new velocity each `LateUpdate`, so the safety filter accounts for that discrete step. For one axis, let `d = abs(e)`, `s = sign(e)`, `v_c,next` be the candidate camera velocity after this frame, `V` be the maximum target speed, `a = 12 / sqrt(2)`, and `dt` be the frame duration. The worst-case closing speed after the step is

```text
q = max(0, s * v_c,next + V)
d >= q * dt + q^2 / (2 * a)
```

The second term is the camera's braking distance after the target has moved toward it by at most `V * dt` during this frame. Solving the inequality gives the allowed closing-speed bound `sqrt((a * dt)^2 + 2 * a * d) - a * dt`; the controller projects its requested next velocity onto the reachable velocities below that bound. If the current state is already outside the safe set, it uses the strongest reachable braking command. At 60 frames per second and `V = 4.4`, the minimum distance is about 1.21 units for a stationary camera and 4.71 units while the camera matches a player moving at full speed. These are safety minima, not desired offsets; position convergence is secondary to preserving the no-crossing margin.

## Why Not a Spring Alone

A critically damped spring uses `a_c = a_t + 2 * omega * (v_t - v_c) + omega^2 * e`. In a continuous, unsaturated model its error obeys `(d/dt + omega)^2 e = 0`, giving `e(t) = (A + B*t) * exp(-omega*t)` for a fixed target. That response does not overshoot from rest. In a real frame loop, acceleration limits, collisions, and changes of target velocity can invalidate the ideal response. The stopping-speed envelope gives the controller an explicit braking-distance constraint during constant-velocity intervals instead of relying only on damping.

## Guarantee Boundary

The discrete safety condition guarantees no camera crossing for a continuous target whose speed stays within `V`, provided the camera begins inside the safe set and map-boundary clamping does not teleport either object. When the camera begins too close to the target, or the target exceeds its configured speed bound, finite camera acceleration cannot guarantee no crossing; the filter brakes as strongly as it can until a safe margin is available. At map edges the clamp has priority and prevents views beyond the level. The controller intentionally gives up exact position matching when that is required to preserve the safety margin.
