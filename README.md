# Entitas Visual Debugging - System Flow

## Design

Everyone,

How would you automatically generate a system diagram from a game that is using Entitas?

I'm looking for ideas.

One idea is to augment the nice visual debugger, so you can see the diagram as you play.

I haven't found an entity-retained event, so I would make a middleman reactive system class that the game reactive systems extend from. When compiled for debug visualization, in the execute method, I would place the debug entity game object in front of the debug system game object.

I would make a sprite and text mesh for each entity. The text would be the string value of the entity.

To see how an entity goes from one system to another, I would going to visualize a trail from one system to the next.

I would layout systems in a circle to see connections their relationship. I would order systems in their execute order.

What ideas do you have to quickly and automatically visualize the relationships between systems?

<https://github.com/sschmid/Entitas-CSharp/issues/660>

## Example flow

- [ ] TODO
- [x] Done
- For context

---

### Installation

1. Engineer installs Entitas-Csharp with visual debugging.
1. Engineer does not disable Entitas visual debugging.
1. Engineer programs a game using two or more reactive systems of Entitas.  Example:  see Deadly Diver <https://github.com/ethankennerly/ludumdare40>
1. [ ] Engineer installs this repo into their `Assets` path.  For example:  see Deadly Diver.
1. [ ] In each system file, engineer replaces text `ReactiveSystem` with `ObservableReactiveSystem`.
1. [ ] In the Unity editor, engineer drags prefab `Debug System Flow Observer` into the scene or creates one.
    1. [ ] If camera size is 5 (by default) then the height is 10.
    1. [ ] Engineer sets the x-position of the debug system flow to be 20 to be certainly offscreen to the right.
    1. [ ] If the camera will move, engineer places the flow observer somewhere out of view.
1. [ ] In the Unity editor, engineer creates debug system flow game object.
    1. [ ] Engineer attaches component: `Debug System Flow Observer`
        1. [ ] Engineer assigns the system prefab, which is how the system will appear.
            1. [ ] The system prefab has Unity text mesh linking its name.
        1. [ ] Engineer assigns the entity prefab, which is how the system will appear.
            1. [ ] The entity prefab has Unity text mesh linking its name.
        1. [ ] Text mesh and world space are used instead of UI.
    1. Engineer saves scene.

### Usage

1. [ ] In the Unity editor, engineer plays.  Entitas visual debugging is disabled outside the editor.
    1. [ ] Debug system flow observer does not destroy on load.
    1. [ ] Debug system flow observer moves debug systems to not destroy on load.
    1. [ ] Debug system flow observer constructs a game object for each terminal system.
        1. [ ] Each game object has the name of the system and is in a circle, clockwise by execution order.
        1. [ ] Each system game object is a child of (and centered around) the root flow observer.
        1. [ ] A game object named "No System" is created at the top.
        1. [ ] Each system is translucent by playing an animation, which represents the system has not been executed yet.
    1. [ ] In hierarchy window, engineer clicks `Debug System Flow Observer`.
        1. [ ] Entitas entity game objects are located at "No System".
        1. [ ] Engineer plays game and triggers a system to execute an entity.  For example in Deadly Diver, click a location on screen.
            1. [ ] Debug system flow observer moves entity game object to the system game objects.
            1. [ ] Debug system makes system sprite opaque by playing an animation.
            1. [ ] Over time, animation gradually fades.
            1. [ ] Debug system flow observer draws debug line with two halves`.
            1. [ ] Debug system writes on line near destination the name of the entity at the time of transition.