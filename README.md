# InvoicingLibrary
Libreria en C# para la generación de factura electrónica mexicana

## Logging

`TRSF.Invoicing` depends only on `Microsoft.Extensions.Logging.Abstractions` — no
concrete logging provider is bundled or required. Pass an `ILogger<T>` into the
classes that accept one (`CFDIv33`, `EcodexProvider`, `EcodexQRProvider`); if you don't,
they log nowhere (`NullLogger`) and behave exactly as before.

To wire up NLog, add `NLog.Extensions.Logging` to your application (not to this
library) and build a logger factory:

```csharp
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddNLog("nlog.config"));

var cfdi = new TRSF.Invoicing.CFDI.CFDIv33(
    certificatesRepository,
    satProvider,
    loggerFactory.CreateLogger<TRSF.Invoicing.CFDI.CFDIv33>());
```

Any other `Microsoft.Extensions.Logging`-compatible provider (Serilog, the built-in
Console provider, Application Insights, etc.) works the same way — swap the
`builder.Add...` call for that provider's own extension method.
