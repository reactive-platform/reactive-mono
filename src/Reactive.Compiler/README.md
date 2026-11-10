# Reactive.Compiler

Source generators and analyzers for [Reactive](https://github.com/reactive-platform/reactive-mono), a reactive UI library for Unity.

The package only runs at compile time: nothing is added to your build output.

## Installation

```xml
<PackageReference Include="Reactive.Compiler" Version="..." PrivateAssets="all" />
```

## Features

### State bindings

Settable properties of components get generated bindings with the `s` prefix, which keep the property in sync with a state:

```csharp
var count = Remember(0);

new Label {
    sText = count.Map(x => $"Clicked {x} times")
};
```

A binding is generated when it's assigned inside a member marked with `[StateGen]`, which includes every `Construct` override. The prefix can be changed with the attribute's patterns. Properties marked with `[RawState]` take an `IState<T>` themselves and are skipped.

### Required properties

`[Required]` makes a property mandatory when constructing a component, as a replacement for the `required` keyword. `[SetsRequired]` marks properties that initialize required ones on their behalf.

### Variadic overloads

`[VariadicGen]` generates versions of a method or a type taking from 2 up to a given number of repeated parameters, using the marked one as a template.

## Diagnostics

Ids are grouped by what they check. Each feature owns a range of 100 ids. A feature that covers several areas splits
its range into blocks of 20, one per area, while a feature that doesn't numbers its diagnostics in order:

| Range | Feature             | Blocks                                                                       |
|-------|---------------------|------------------------------------------------------------------------------|
| RV0xx | State bindings      | RV001 patterns, RV021 raw states, RV041 generated bindings, RV061 captures   |
| RV1xx | Required properties |                                                                              |
| RV2xx | Component design    |                                                                              |

New diagnostics take the next free id of their block or feature, starting from 1. A new area takes the next free block.
Ids of removed diagnostics are never reused.

### State bindings

| Id    | Severity | Description                                                         |
|-------|----------|---------------------------------------------------------------------|
| RV001 | Error    | A state pattern is incomplete                                       |
| RV002 | Error    | A state pattern is invalid                                          |
| RV021 | Info     | A raw state isn't managed by the state generator                    |
| RV022 | Error    | A raw state isn't an `IState<T>`                                    |
| RV041 | Error    | A generated binding is used outside of a `[StateGen]` member        |
| RV061 | Warning  | A state is captured by a closure instead of passed as a dependency  |

### Required properties

| Id    | Severity | Description                                                         |
|-------|----------|---------------------------------------------------------------------|
| RV102 | Error    | A required property isn't initialized                               |
| RV103 | Error    | A shadowed required property doesn't exist in the target type       |
| RV104 | Error    | A property is required and sets required members at the same time   |
| RV105 | Error    | A static property is marked as required                             |
| RV106 | Error    | A property of a static class is marked as required                  |
| RV107 | Error    | A required property isn't initialized in the parameterless ctor     |

### Component design

| Id    | Severity | Description                                                         |
|-------|----------|---------------------------------------------------------------------|
| RV201 | Warning  | A reactive component exposes a public method                        |
| RV202 | Warning  | A reactive component exposes a public event                         |
