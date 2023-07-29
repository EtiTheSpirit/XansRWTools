# XansTools

A collection of assorted goodies for Rain World modding, primarily oriented at my own mods but with some stuff that other modders may find useful.

# Disgusting hacks ahead; woe be upon ye (like unironically though, brace yourself weary programmer)

Rain World has a lot of jank, and I chose to fight jank with jank. 

...And by "jank" I mean "I retrofitted macros from C into C#". This primarily has to do with the fact that the version of BIE that ships with Rain World is so freakishly old that Harmony's `__args` parameter wasn't even implemented yet, so doing fast injections with the (wonderful) `[ShadowedOverride]` attribute required generating a shit ton of methods yandere dev style for every number of args from 0 to 10. Rest assured, the applicable sacrifices for my sins have already been made.

This does mean you will need to install C++ development kits (in visual studio installer), and then edit the .csproj file by hand to declare the CL variable. It'll be at the bottom as `$(DevEnvDir)..\..\VC\Tools\MSVC\14.36.32532\bin\Hostx64\x64\cl.exe`. Make sure that matches your own install. Version number is prone to changing, of course.

# Feature list
Here's a list of the mod's features. The bigger ones are sorted first.
* `[ShadowedOverride]` attribute, to encourage class inheritance by allowing any method or property to be (effectively) overridden.
  * This is the main feature powering my flagship mod, "Dreams of Infinite Glass", and is part of why I was able to so easily create a custom iterator. I extend the base iterator class, and then any methods that needed to be overridden (but which were *not* virtual) could simply be declared as shadows and use this method.
  * Unfortunately this technique completely sacrifices the ability to use base behavior. Calling base.Method() will cause a stack overflow. When I can figure out how to cache the original method in memory, this limitation will be lifted.
* A number of extensions to Mono.Cecil, mostly methods to match field access, property access, and method calls by the name of the member (`instruction.MatchCall("MethodName")`)
* Extensions for various maths.
  * Convert tile coordinates to world coordinates without a room reference.
  * Alpha to intensity (color.rgb \* color.a)
  * Game ticks to realtime
* .NET extensions
  * `IsDefault<T>(T value)` returns whether or not the provided value is the same as `default(T)`, but it supports user defined operators.
  * `GetStatic` and `SetStatic` that can be called on a type to get/set fields/properties by name.
  * `Default(Type type)` is a nongeneric variant of `default(T)` that can be used on an arbitrary type
  * `ReflectiveCast` allows casting an object into another type, but unlike `Convert.ChangeType`, this supports user-defined implicit and explicit operators. It also supports casting enums into any numeric type instead of just their underlying type.
* Unique mod error reporter component that allows terminating the startup sequence of the game with a popup.
* A system to enable the depth and/or stencil buffer.
* A system to get a reference to the current room camera from any context.
* Other assorted things that are not special enough to warrant listing here.