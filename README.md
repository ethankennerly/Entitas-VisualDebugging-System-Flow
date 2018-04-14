# Entitas Visual Debugging - System Flow

Everyone,

How would you automatically generate a system diagram from a game that is using Entitas?

I'm looking for ideas.

One idea is to augment the nice visual debugger, so you can see the diagram as you play.

I haven't found an entity-retained event, so I would make a middleman reactive system class that the game reactive systems extend from. When compiled for debug visualization, in the execute method, I would place the debug entity game object in front of the debug system game object.

I would make a sprite and text mesh for each entity. The text would be the string value of the entity.

To see how an entity goes from one system to another, I would going to visualize a trail from one system to the next.

I would layout a collection of systems (called a feature) as a row of sprites, one sprite for each terminal system.

I would layout a series of systems in their execute order.

What ideas do you have to quickly and automatically visualize the relationships between systems?

<https://github.com/sschmid/Entitas-CSharp/issues/660>
