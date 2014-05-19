# ![Bau](https://raw.githubusercontent.com/bau-build/bau/dev/assets/bau.128.png).MSBuild

> Run MSBuild

[![Gitter chat](https://badges.gitter.im/bau-build/bau.png)](https://gitter.im/bau-build/bau)

## Install

```batch
> scriptcs -install Bau.MSBuild -pre
```

## Usage

### Fluent API

```C#
bau.MSBuild("foo")
    .Do(msbuild => // TBD
```

#### Methods

* TBD

### Declarative API

```C#
bau.Exec("foo")
    .Do(exec =>
    {
        // TBD
    });
```

#### Properties

* TBD