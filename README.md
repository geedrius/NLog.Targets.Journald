# Journald target for NLog

![NLog](/N.png) ![systemd](/systemd.png)

**NLog.Targets.Journald** is a custom target for [**NLog**](https://nlog-project.org/). 

It outputs `systemd-journald` [native protocol](https://systemd.io/JOURNAL_NATIVE_PROTOCOL/) to unix domain socket at `/run/systemd/journal/socket`.

It can be used with version 4.7.13 and later.

## Documentation

### How to use

 1. Add reference to `NLog.Targets.Journald` [NuGet package](https://www.nuget.org/packages/NLog.Targets.Journald/) in your project.
 2. Modify your application configuration to load and use `Journald` target.

### `NLog.config` configuration snippet

See [NLog docs](https://github.com/NLog/NLog/#getting-started) for more information.

```xml
<extensions>
    <add assembly="NLog.Targets.Journald"/>
</extensions>

<targets>
    <target xsi:type="Journald" name="journald" layout="${logger} ${message}" />
</targets>

<rules>
    <logger name="*" minlevel="Trace" writeTo="journald" />
</rules>
```

[NLog.config](/src/Demo/NLog.config) is simple but complete example.

### `appsettings.json` configuration snippet

For ASP.NET Core, .NET Core, .NET5 ant later projects. See [NLog.Extensions.Logging]( https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json) for more information.

```json
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
```

### Journal fields

NLog log events are emmited to Journald using the following fields.

| Journal field | Field type    | Description |
| ------------- | ------------- | ------------|
| `PRIORITY` | User | NLog `LogLevel` mapped to journal priority value between 2 (`crit`) and 7 (`debug`). |
| `MESSAGE` | User | Same as `${message}` in NLog layout config. |
| `LEVEL` | Custom | Same as `${level}` in NLog layout config. |
| `LOGGER` | Custom | Same as `${logger}` in NLog layout config. |
| `TIMESTAMP` | Custom | NLog event timestamp as [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) string. Example: `2022-02-11T15:30:47+00:00`. |
| `STACKTRACE` | Custom | NLog event `StackTrace` property as string. |
| `EXCEPTION_TYPE` | Custom | NLog event exception type name. Example: `System.ArgumentOutOfRangeException`. |
| `EXCEPTION_MESSAGE` | Custom | NLog event exception `Message` property. |
| `EXCEPTION_STACKTRACE` | Custom | NLog event exception `StackTrace` property. |


More info about [Journal fields](https://www.freedesktop.org/software/systemd/man/systemd.journal-fields.html).

### NLog `LogLevel` to Journal Priority mapping

| NLog LogLevel | Journal priority decimal | Journal priority string |
| ------------- | ------------- | ------------|
| `Fatal` | 2 | `crit` |
| `Error` | 3 | `err` |
| `Warn`  | 4 | `warning` |
| `Info`  | 6 | `info` |
| `Debug` | 7 | `debug` |
| `Trace` | 7 | `debug` |

## License

**NLog.Targets.Journald** is licensed under the terms of MIT license.

Please see the [LICENSE file](LICENSE.txt) for further information.

## References

Unix domain socket support is implemented using [UnixEndPoint](
https://github.com/mono/mono/blob/main/mcs/class/Mono.Posix/Mono.Unix/UnixEndPoint.cs) class from [Mono](https://github.com/mono/mono) platform library.