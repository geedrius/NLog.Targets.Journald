# Journald target for NLog

![NLog](/N.png) ![systemd](/systemd.png)

**NLog.Targets.Journald** is a custom target for [**NLog**](https://nlog-project.org/). 

It outputs `systemd-journald` [native protocol](https://systemd.io/JOURNAL_NATIVE_PROTOCOL/) to unix domain socket at `/run/systemd/journal/socket`.

It can be used with version 4.7.13 and later.

## Documentation

### How to use

 1. Add reference to `NLog.Targets.Journald.csproj` in your application project.
 2. Modify your application configuration to load and use `Journald` target.

### `NLog.config` configuration snippet

See [NLog docs](https://github.com/NLog/NLog/#getting-started) for more information.

```xml
...
<extensions>
    <add assembly="NLog.Targets.Journald"/>
</extensions>

<targets>
    <target xsi:type="Journald" name="journald" layout="${logger} ${message}" />
</targets>

<rules>
    <logger name="*" minlevel="Trace" writeTo="journald" />
</rules>
...
```

[NLog.config](/src/Demo/NLog.config) is simple but complete example.

### `appsettings.json` configuration snippet

For ASP.NET Core, .NET Core, .NET5 ant later projects. See [NLog.Extensions.Logging]( https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json) for more information.

```json
...
"NLog": {
    "extensions": [
      { "assembly": "NLog.Targets.Journald" }
    ],
    "targets": {
      "journald": {
        "type": "Journald",
        "layout": "${logger} ${message}"
      }
    },
    "rules": {
      "10": {
        "logger": "*",
        "minLevel": "Trace",
        "writeTo": "journald"
      }
    }
  }
...
```

### To Do

- Upload Nuget package

## License

**NLog.Targets.Journald** is licensed under the terms of MIT license.

Please see the [LICENSE file](LICENSE.txt) for further information.

## References

Unix domain socket support is implemented using [UnixEndPoint](
https://github.com/mono/mono/blob/main/mcs/class/Mono.Posix/Mono.Unix/UnixEndPoint.cs) class from [Mono](https://github.com/mono/mono) platform library.