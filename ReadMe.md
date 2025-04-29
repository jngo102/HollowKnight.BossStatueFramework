# Boss Statue Framework

A Hollow Knight modding library for quickly setting up statues for custom boss fights in the Hall of Gods.

## Setting up a boss mod
1. Add this library as an MSBuild reference to your project.
2. Inherit your base mod class from the [IBossStatueMod](./IBossStatueMod.cs) interface.
3. Implement the members of `IBossStatueMod`.

**This library provides only a scalable way to manage custom boss statues, it does not provide any utilities for developing the custom bosses and their arenas themselves.**