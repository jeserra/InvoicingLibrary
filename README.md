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

## Building a Receptor from a Constancia de Situación Fiscal

CFDI 4.0 requires the receptor's `RegimenFiscalReceptor` and `DomicilioFiscalReceptor` —
exactly the data printed on the SAT's own "Constancia de Situación Fiscal" (CSF) PDF.
`TRSF.Invoicing.ConstanciaFiscal.PdfConstanciaFiscalReader` reads that PDF (using
iText7, already a dependency) and builds a `BindingModels.Receptor` from it directly:

```csharp
using TRSF.Invoicing.ConstanciaFiscal;

var constancia = PdfConstanciaFiscalReader.Leer("constancia.pdf");
var receptor = constancia.ToReceptor(usoCFDI: "G03");
```

A taxpayer can have more than one active regime at once (e.g. salaried employment plus
a professional-services activity, as in SAT's own sample layout); `ToReceptor` picks the
most recently started one with no end date. Inspect `constancia.Regimenes` and pass a
specific code as `ToReceptor(usoCFDI, regimenFiscalCodigo: "605")` if the invoice should
use a different one.

This is best-effort text extraction against SAT's current CSF layout, not a stable,
versioned data format — fields that can't be recognized come back `null` rather than
throwing, so check the result before using it to seal an invoice. `ConstanciaFiscalTextParser.Parse`
takes raw text directly if you already have it from another source (OCR, a different
PDF library, etc.).
