# Camera Controller Design

## Geometry and Dead Zone

The camera follows a focus point offset from the player so the player sits at 24% of the view width and 25% of the view height. At level edges, the camera center is clamped to the map rectangle inset by the camera's half-width and half-height.

While idle, the camera does not move when the focus point remains within a 0.42 by 0.34 world-unit dead zone. Crossing that region activates tracking. Tracking stays active while the player moves, then becomes idle only after the focus point and both velocities settle.

## Tracking Law

For one axis, let `e = target - camera`, `v_t` be target velocity, and `v_c` be camera velocity. While tracking, the requested closing speed is

```text
u = sign(e) * min(v_catch, k * abs(e), sqrt(2 * a_rel * abs(e)))
v_c_requested = v_t + u
```

The camera moves each velocity component toward its request with a bounded acceleration. The total camera acceleration is at most 12 world units/s^2. The player acceleration is at most 6 world units/s^2, so the conservative per-axis relative braking budget is

```text
a_rel = 12 / sqrt(2) - 6 > 0
```

For a fixed or smoothly moving target, the square-root term is the stopping-speed envelope: a closing speed `u` needs `u^2 / (2 * a_rel)` distance to stop. Limiting `u` to `sqrt(2 * a_rel * abs(e))` keeps that stopping distance within the remaining error. The proportional term reduces speed near the target, and `v_t` feed-forward makes camera velocity converge to player velocity during steady movement. At rest, both velocities converge to zero and the camera settles on the focus point.

## Why Not a Spring Alone

A critically damped spring uses `a_c = a_t + 2 * omega * (v_t - v_c) + omega^2 * e`. In a continuous, unsaturated model its error obeys `(d/dt + omega)^2 e = 0`, giving `e(t) = (A + B*t) * exp(-omega*t)` for a fixed target. That response does not overshoot from rest. In a real frame loop, acceleration limits, collisions, and changes in target velocity can invalidate the ideal response. The stopping-speed envelope gives the controller an explicit braking-distance constraint instead of relying only on damping.

## Guarantee Boundary

The no-crossing argument assumes the target follows the game's bounded-acceleration movement law and the camera begins inside the controller's reachable braking envelope. A target teleport or an instantaneous collision stop can change target velocity faster than any finite-acceleration camera can match. No controller can guarantee zero overshoot for arbitrary discontinuous target motion while also limiting camera acceleration. The player controller bounds ordinary WASD acceleration; map clamping separately prevents the camera from showing space beyond the level.
