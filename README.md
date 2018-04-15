# Entitas Visual Debugging - System Flow

This prototype shows a flowchart of entities at systems while debugging in the Unity Editor.

If you like it, feel free to run with it and improve it.

## Demo

Install Deadly Diver and walkthrough the example flow below.

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

1. Engineer installs Entitas-Csharp with visual debugging.  <https://github.com/sschmid/Entitas-CSharp>
1. Engineer does not disable Entitas visual debugging.
1. Engineer programs a game using two or more reactive systems of Entitas.  Example:  see Deadly Diver <https://github.com/ethankennerly/ludumdare40>
1. [x] Engineer installs this repo into their `Assets` path.  For example:  see Deadly Diver.
1. [x] Engineer replaces text of `ReactiveSystem` with `ObservableReactiveSystem`.
    1. [x] Example vim sed command.  Sed can also be run from the command line.

            args **/*System.cs
            argdo %s/\<ReactiveSystem\>/ObservableReactiveSystem/e | update

    1. [x] To undo this, the substitute the reverse.  Example:

            argdo %s/\<ObservableReactiveSystem\>/ReactiveSystem/e | update

    1. [x] Engineer inserts call to base class execute:  `base.Execute(entities);`  Example vim sed command, if the indentation level is constant:

            args **/*System.cs
            argdo %s/\n/___NEW_LINE___/g | %s/\<Execute(List<[^>]*> \([A-Za-z]*\)[^{]*{/\0___NEW_LINE___            base.Execute(\1);/e | %s/___NEW_LINE___/\r/g | update

        - Perl can do this more succinctly. <https://unix.stackexchange.com/questions/26284/how-can-i-use-sed-to-replace-a-multi-line-string?newreg=dc0a577fedd84de3a9f33424eccfcbb3>
    1. [x] To undo if `base.Execute` is not used anywhere else is simpler:

            argdo %s/  *base.Execute([^)]*);\n//e | update

        - An alternative implementation that would avoid the middleman class would be if Entitas implemented an `entity.OnEntityRetained` event.
        - When a reactive system executes an entity, the system retains the entity.
1. [x] In the Unity editor, engineer drags prefab `DebugSystemFlowObserver` into the scene or creates one.
    1. [x] If camera size is 5 (by default) then the height is 10.
    1. [x] Engineer sets the x-position of the debug system flow to be 20 to be certainly offscreen to the right.
    1. [x] If the camera will move, engineer places the flow observer somewhere out of view.
1. [x] If creating a system flow prefab in the Unity editor:
    1. [x] Engineer attaches component: `Debug System Flow Observer`
        1. [x] Engineer assigns the system prefab, which is how the system will appear.
            1. [x] The system prefab has Unity text mesh linking its name.
        1. [x] Engineer assigns the entity prefab, which is how the system will appear.
            1. [x] The entity prefab has Unity text mesh linking its name.
            1. [x] Trail renderer on debug entity prefab displays in which systems processed the entity.
        1. [x] Engineer may use text mesh and world space instead of canvas for faster performance.
        1. [x] Engineer optionally links system positions as many as are needed, manually placing them.
        1. [x] Select if the entity will be reparented to the system object.
    1. Engineer saves scene.

### Usage

1. [x] In the Unity editor, engineer plays.  Entitas visual debugging is disabled outside the editor.
    1. [x] Debug system flow observer does not destroy on load.
    1. [x] Debug system flow observer moves debug systems behaviour to not destroy on load.
    1. [x] Debug system flow observer constructs a game object for each terminal system.
        1. [x] Each game object has the name of the system and is in a circle, clockwise by execution order.
        1. [x] Each system game object is a child of (and centered around) the root flow observer.
        1. [x] A game object named "No System" is created at the bottom.
        1. [x] Each system is translucent by playing an animation, which represents the system has not been executed yet.
        1. [x] Flow observer searches through debug systems for systems.
            1. [x] For each system, observer adds event to the collector's groups.
    1. [x] In hierarchy window, engineer clicks `Debug System Flow Observer`.
        1. [x] Entitas entity game objects are located at "No System".
        1. [x] Engineer plays game and triggers a system to execute an entity.  For example in Deadly Diver, click a location on screen.
            1. [x] Debug system flow observer moves entity game object to the system game objects.
            1. [x] Debug system makes system sprite opaque by playing an animation.
            1. [x] Over time, system animation gradually fades.
            1. [x] Likewise, entity animates when executed.
            1. [x] Optionally, system observer acquires entity behaviour as parent.
            1. [x] Trail renderer on debug entity prefab displays in which systems processed the entity.
            1. [x] Optionally, does not draw system connectors in case systems are replaced.
            1. [x] Distinguishes between entities of differing contexts.
            1. [x] Slightly offset each entity's position to prevent a total eclipse.
            1. [x] Logs in a list of strings on debug entity observer the systems that had processed.
            1. [x] Optionally, as entity moves, bring it forward towards the camera, so newest entity is on top.


Coding style follows Entitas-CSharp.
