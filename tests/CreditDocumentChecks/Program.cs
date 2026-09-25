using System.Reflection;
using CrmSaas.Application.DTOs;
using CrmSaas.Application.Abstractions;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CrmSaas.Api.Controllers;
using CrmSaas.Api.Services;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using FluentValidation;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static void Invoke(string name, params object?[] args) => typeof(CreditApplicationsController)
    .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, args);

var application = new SolicitudCredito
{
    ClienteId = Guid.NewGuid(), RuntConsultado = true, SimitConsultado = true,
    IdentidadValidada = true, DataCreditoClienteConsultado = true
};
var profile = new PerfilRequisito();
profile.Documentos.Add(new DocumentoPerfilRequisito { Nombre = "Referencias personales", Obligatorio = true });
Invoke("AddChecklistDocuments", application, profile);
Check(application.Documentos.Count == 15, "Debe usar el catálogo de 15 documentos, no el perfil.");
Check(application.Documentos.Select(d => d.Nombre).Distinct().Count() == 15, "Sin duplicados.");
Check(application.Documentos.All(d => d.ClienteId == application.ClienteId && d.FechaVencimiento == null), "Sin vencimientos y asociados al cliente.");
Check(!application.Documentos.Any(d => d.Tipo == TipoDocumentoCredito.Referencias), "Referencias no se solicita como documento.");
try
{
    Invoke("ValidateDecision", application, EstadoSolicitudCredito.EnEstudio);
    throw new Exception("Debe exigir la confirmación explícita.");
}
catch (TargetInvocationException e) when (e.InnerException is ValidationException) { }
application.DocumentacionCompleta = true;
Invoke("ValidateDecision", application, EstadoSolicitudCredito.EnEstudio);
Check(application.Documentos.All(d => d.Estado == EstadoDocumentoCredito.Pendiente), "Permite avanzar sin exigir archivos opcionales ni falsear sus estados.");
application.Estado = EstadoSolicitudCredito.DocumentosRecibidos;
application.FechaDocumentacionCompleta = DateTime.UtcNow;
application.UsuarioDocumentacionCompleta = "tester";
Invoke("ClearDocumentationConfirmation", application);
Check(!application.DocumentacionCompleta && application.FechaDocumentacionCompleta == null && application.UsuarioDocumentacionCompleta == null, "Cambiar documentos revoca la confirmación.");
Check(application.Estado == EstadoSolicitudCredito.DocumentosPendientes, "Reabre el estado documental.");
application.Estado = EstadoSolicitudCredito.Aprobada;
Invoke("ClearDocumentationConfirmation", application);
Check(application.Estado == EstadoSolicitudCredito.Aprobada, "No altera decisiones de crédito existentes.");
Console.WriteLine("OK: catálogo, perfil ignorado, fechas, confirmación, documentos opcionales y reapertura.");

static CreditCoDebtorDto Person(string id) => new(null, "Persona " + id, id, "300" + id, "Familiar", 2000000,
    "Referencia 1", "310" + id, "Amigo", "Referencia 2", "320" + id, "Familiar");
var multi = new SolicitudCredito { EmpresaId = Guid.NewGuid(), FechaPrimerVencimiento = new DateTime(2026, 10, 20) };
Check(CreditCoDebtorService.Apply(multi, [Person("100"), Person("200")]), "Agregar dos codeudores debe registrar un cambio.");
Check(multi.Codeudores.Count == 2 && multi.Codeudores.All(x => x.Activo && x.EmpresaId == multi.EmpresaId && x.SolicitudCreditoId == multi.Id), "Codeudores vinculados a la solicitud y empresa.");
var people = multi.Codeudores.OrderBy(x => x.Orden).Select(CreditCoDebtorService.ToDto).ToArray();
Check(!CreditCoDebtorService.Apply(multi, people), "Guardar sin cambios no revoca confirmaciones.");
Check(multi.CodeudorNombre == people[0].Name, "Conservar compatibilidad con el primer codeudor.");
var file = new DocumentoSolicitudCredito { CodeudorId = people[0].Id, RutaArchivo = "existing-file.pdf", SolicitudCreditoId = multi.Id };
multi.Documentos.Add(file);
Check(CreditCoDebtorService.Apply(multi, [people[1] with { Name = "Nombre editado" }]), "Editar y retirar detecta cambios.");
Check(multi.Codeudores.Count == 2 && multi.Codeudores.Count(x => x.Activo) == 1, "Retirar no elimina el registro histórico.");
Check(multi.Codeudores.Single(x => x.Activo).Id == people[1].Id, "Editar conserva el identificador.");
Check(file.CodeudorId == people[0].Id && file.RutaArchivo == "existing-file.pdf", "No reasigna ni borra archivos al retirar.");
Check(!CreditCoDebtorService.Apply(multi, null) && multi.Codeudores.Count(x => x.Activo) == 1, "Omitir colección conserva codeudores.");
try
{
    CreditCoDebtorService.Apply(multi, [Person("300") with { Id = Guid.NewGuid() }]);
    throw new Exception("Debe rechazar identificadores de otra solicitud.");
}
catch (ValidationException) { }
Check(multi.Codeudores.Count(x => x.Activo) == 1, "Validación inválida no modifica codeudores.");
try
{
    CreditCoDebtorService.Apply(multi, [Person("300"), Person("300")]);
    throw new Exception("Debe rechazar codeudores repetidos.");
}
catch (ValidationException) { }
Check(CreditCoDebtorService.Apply(multi, []) && multi.Codeudores.All(x => !x.Activo), "Lista vacía retira a todos.");
Check(multi.CodeudorNombre == null && multi.FechaPrimerVencimiento == new DateTime(2026, 10, 20), "Retirar limpia el espejo, no el vencimiento.");
var legacy = new SolicitudCredito { CodeudorNombre = "Histórico", CodeudorIdentificacion = "123", CodeudorCelular = "300" };
CreditCoDebtorService.Apply(legacy, null);
Check(legacy.Codeudores.Single().Nombre == "Histórico", "Cliente antiguo migra su codeudor sin perder datos.");
Console.WriteLine("OK: varios codeudores, edición, retiro, identidad estable, archivos, validación y compatibilidad.");

var tenant = new TestTenant();
using var db = new CrmDbContext(new DbContextOptionsBuilder<CrmDbContext>()
    .UseSqlServer("Server=(local);Database=ModelChecks;Integrated Security=True;TrustServerCertificate=True").Options, tenant);
var tracked = new SolicitudCredito { EmpresaId = tenant.EmpresaId!.Value, ClienteId = Guid.NewGuid() };
Invoke("AddChecklistDocuments", tracked, null);
db.Attach(tracked);
var controller = new CreditApplicationsController(db, null!, tenant);
var sync = typeof(CreditApplicationsController).GetMethod("SyncCoDebtors", BindingFlags.Instance | BindingFlags.NonPublic)!;
sync.Invoke(controller, [tracked, new[] { Person("400"), Person("500") }]);
db.ChangeTracker.DetectChanges();
Check(tracked.Documentos.Count == 45, "Catálogos separados: 15 cliente y 15 por cada codeudor.");
Check(tracked.Documentos.Where(d => d.CodeudorId.HasValue).All(d => d.ClienteId == null), "Los documentos del codeudor no se atribuyen al cliente.");
Check(tracked.Codeudores.All(c => tracked.Documentos.Count(d => d.CodeudorId == c.Id) == 15), "Cada codeudor recibe su propio catálogo.");
Check(db.ChangeTracker.Entries<CodeudorSolicitudCredito>().All(e => e.State == EntityState.Added), "Nuevos GUID se insertan; no se intentan actualizar filas inexistentes.");
Check(db.ChangeTracker.Entries<DocumentoSolicitudCredito>().Count(e => e.State == EntityState.Added) == 30, "Se insertan sólo documentos nuevos.");
var active = tracked.Codeudores.Select(CreditCoDebtorService.ToDto).ToArray();
sync.Invoke(controller, [tracked, active]);
Check(tracked.Documentos.Count == 45, "Editar no duplica documentos.");
var query = db.CodeudoresSolicitudCredito.ToQueryString();
Check(query.Contains("EmpresaId"), "La consulta filtra por empresa.");
Check(db.Model.FindEntityType(typeof(CodeudorSolicitudCredito))!.GetQueryFilter() != null, "Filtro tenant explícito.");
tracked.Cliente = new Cliente { Nombre = "Cliente prueba" };
tracked.Producto = new Producto { Nombre = "Producto prueba" };
tracked.FechaPrimerVencimiento = new DateTime(2026, 10, 20);
var dto = (CreditApplicationDto)typeof(CreditApplicationsController).GetMethod("ToDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [tracked])!;
Check(dto.CoDebtors?.Count == 2 && dto.FirstDueDate == tracked.FechaPrimerVencimiento, "DTO conserva codeudores y vencimiento.");
var lines = (List<string>)typeof(SimplePdfGenerator).GetMethod("CreditRequest", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [dto, "Empresa prueba"])!;
Check(lines.Contains("CODEUDOR 1") && lines.Contains("CODEUDOR 2"), "PDF incluye todos los codeudores.");
Check(lines.Any(line => line.StartsWith("Primer vencimiento acordado:") && line.Contains("2026")), "PDF incluye primer vencimiento.");
Check(lines.Any(line => line.StartsWith("Codeudor Persona 400 -")) && lines.Any(line => line.StartsWith("Cliente Cliente prueba -")), "PDF identifica el dueño de los documentos.");
Console.WriteLine("OK: catálogos separados, seguimiento EF, no duplicación, tenant, DTO y contenido PDF.");

var quoteDate = new DateTime(2026, 9, 17);
var quoteItem = new CreateQuoteItemDto(Guid.NewGuid(), 5000000, 800000, 500000,
    [new QuoteInitialPaymentDto(quoteDate.AddDays(30), 100000), new QuoteInitialPaymentDto(quoteDate.AddDays(60), 200000)],
    0, 0, 24, 2);
var normalizePlan = typeof(QuotesController).GetMethod("NormalizeInitialPaymentPlan", BindingFlags.NonPublic | BindingFlags.Static)!;
var plan = normalizePlan.Invoke(null, [quoteItem, quoteDate])!;
Check((decimal)plan.GetType().GetProperty("PaidToday")!.GetValue(plan)! == 500000, "API guarda la cuota inicial base.");
Check(((IReadOnlyCollection<QuoteInitialPaymentDto>)plan.GetType().GetProperty("Schedule")!.GetValue(plan)!).Sum(x => x.Amount) == 300000, "API programa sólo la cuota extra.");
var calculate = typeof(QuotesController).GetMethod("CalculateSimulation", BindingFlags.NonPublic | BindingFlags.Static)!;
var calculation = calculate.Invoke(null, [5000000m, 800000m, 0m, 0m, 24, 2m, null, null, null])!;
Check((decimal)calculation.GetType().GetProperty("FinancedAmount")!.GetValue(calculation)! == 4200000, "Financiación descuenta inicial más extra.");
foreach (var wrongSchedule in new[] { 100000m, 800000m })
{
    try
    {
        normalizePlan.Invoke(null, [quoteItem with { InitialPaymentSchedule = [new QuoteInitialPaymentDto(quoteDate, wrongSchedule)] }, quoteDate]);
        throw new Exception("Debe rechazar planes que no suman la cuota extra.");
    }
    catch (TargetInvocationException e) when (e.InnerException is ValidationException) { }
}
normalizePlan.Invoke(null, [quoteItem with { DownPayment = 500000, InitialPaymentSchedule = [] }, quoteDate]);
Console.WriteLine("OK: contrato de cotización, inicial completa, cuota extra y financiación.");

var cashItem = (CreateQuoteItemDto)typeof(QuotesController).GetMethod("NormalizeCashItem", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [quoteItem])!;
Check(cashItem.ProductId == quoteItem.ProductId && cashItem.ProductPrice == quoteItem.ProductPrice, "Contado conserva producto y precio.");
Check(cashItem.DownPayment == 0 && cashItem.InitialPaymentPaidToday == 0 && cashItem.InitialPaymentSchedule!.Count == 0 && cashItem.TermMonths == 0 && cashItem.MonthlyInterestRate == 0 && cashItem.Insurance == 0 && cashItem.AdministrativeFees == 0, "Contado descarta valores de crédito ocultos.");
var cashCalculation = typeof(QuotesController).GetMethod("CashSimulation", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [4500000m])!;
Check((decimal)cashCalculation.GetType().GetProperty("TotalPayment")!.GetValue(cashCalculation)! == 4500000, "Contado paga sólo precio después del descuento.");
Check((decimal)cashCalculation.GetType().GetProperty("FinancedAmount")!.GetValue(cashCalculation)! == 0, "Contado no financia.");
var cashQuote = new Cotizacion { TipoCredito = "Contado", PrecioProducto = 5000000, DescuentoPromocion = 500000, TotalPagarEstimado = 4500000, PlazoMeses = 0 };
cashQuote.Items.Add(new CotizacionItem { TipoCredito = "Contado", PrecioProducto = 5000000, DescuentoPromocion = 500000, TotalPagarEstimado = 4500000, PlazoMeses = 0 });
var cashDto = (QuoteDto)typeof(QuotesController).GetMethod("ToDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [cashQuote])!;
Check(cashDto.TermMonths == 0 && cashDto.FinancedAmount == 0 && cashDto.EstimatedTotalPayment == 4500000, "Leer contado no reconstruye crédito de 24 cuotas.");
Check(cashDto.Items.Single().TermMonths == 0 && cashDto.Items.Single().FinancedAmount == 0, "Leer artículos respeta contado.");
var customerCashDto = (QuoteDto)typeof(CustomersController).GetMethod("ToQuoteDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [cashQuote])!;
Check(customerCashDto.TermMonths == 0 && customerCashDto.FinancedAmount == 0, "Vista 360 conserva modalidad contado.");
Console.WriteLine("OK: contado normalizado, precio con descuento, DTOs y PDF sin financiación.");

// A global initial can exceed the first article, but must be deducted only once from the whole package.
var globalItem = quoteItem with { ProductPrice = 400000, DownPayment = 1500000, InitialPaymentPaidToday = 1000000,
    InitialPaymentSchedule = [new QuoteInitialPaymentDto(quoteDate.AddDays(30), 500000)] };
var normalizedBundleItem = (CreateQuoteItemDto)typeof(QuotesController).GetMethod("NormalizeBundleItem", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [quoteItem, globalItem])!;
Check(normalizedBundleItem.DownPayment == 0 && normalizedBundleItem.InitialPaymentPaidToday == 0 && normalizedBundleItem.InitialPaymentSchedule!.Count == 0, "El articulo no duplica inicial ni plan global.");
Check(normalizedBundleItem.ProductPrice == quoteItem.ProductPrice && normalizedBundleItem.TermMonths == globalItem.TermMonths, "Paquete conserva precio y aplica plazo global.");
var bundleCalculation = calculate.Invoke(null, [2000000m, globalItem.DownPayment, 0m, 0m, globalItem.TermMonths, 2m, null, null, null])!;
Check((decimal)bundleCalculation.GetType().GetProperty("FinancedAmount")!.GetValue(bundleCalculation)! == 500000, "Paquete resta 1.5 millones completos, aunque el primer articulo valga 400 mil.");
var globalPlan = normalizePlan.Invoke(null, [globalItem, quoteDate])!;
Check(((IReadOnlyCollection<QuoteInitialPaymentDto>)globalPlan.GetType().GetProperty("Schedule")!.GetValue(globalPlan)!).Sum(x => x.Amount) == 500000, "Un solo plan para la extra del paquete.");
var bundleDto = cashDto with { IsBundle = true, CreditType = "Manual", ProductName = "Paquete de electrodomesticos", ProductPrice = 2000000,
    DiscountedProductPrice = 2000000, PromotionDiscount = 0, DownPayment = 1500000, InitialPaymentPaidToday = 1000000,
    FinancedAmount = 500000, TermMonths = 24, EstimatedMonthlyPayment = 31000, InitialPaymentSchedule = globalItem.InitialPaymentSchedule!,
    CreditStartDate = quoteDate.AddDays(30), QuoteDate = quoteDate, ValidUntil = quoteDate.AddDays(7),
    Items = Enumerable.Range(1, 4).Select(i => cashDto.Items.Single() with { ProductName = "Electrodomestico de prueba " + i, DiscountedProductPrice = 500000, Order = i }).ToArray() };
cashQuote.EsPaquete = true;
var persistedBundle = (QuoteDto)typeof(QuotesController).GetMethod("ToDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [cashQuote])!;
Check(persistedBundle.IsBundle, "La modalidad paquete se conserva al leer la cotizacion.");
if (args.Length > 0)
{
    Directory.CreateDirectory(args[0]);
    File.WriteAllBytes(Path.Combine(args[0], "bundle-quote.pdf"), SimplePdfGenerator.Quote(bundleDto, "Empresa de prueba"));
}
Console.WriteLine("OK: inicial global, plan unico, articulo sin duplicados, DTO y PDF de paquete.");

var validateTerm = typeof(QuotesController).GetMethod("ValidateQuoteTerm", BindingFlags.NonPublic | BindingFlags.Static)!;
foreach (var valid in new[] { (1, "Motos"), (40, "Motos"), (1, "Electrodomésticos"), (24, "ELECTRODOMESTICOS") })
    validateTerm.Invoke(null, [valid.Item1, valid.Item2]);
foreach (var invalid in new[] { (0, "Motos"), (-1, "Motos"), (41, "Motos"), (25, "Electrodomésticos"), (40, "ELECTRODOMESTICOS") })
{
    try { validateTerm.Invoke(null, [invalid.Item1, invalid.Item2]); throw new Exception("Debe rechazar plazo fuera de rango."); }
    catch (TargetInvocationException e) when (e.InnerException is ValidationException) { }
}
var rateWithOldLimit = new TasaPuntoVenta { PlazoMaximoMeses = 30, TasaFactorMensual = 2 };
foreach (var financial in new ConfiguracionFinancieraEmpresa?[] { null, new() { PlazoMaximoMeses = 30 } })
{
    var forty = calculate.Invoke(null, [5000000m, 0m, 0m, 0m, 40, 2m, financial, null, rateWithOldLimit])!;
    Check((int)forty.GetType().GetProperty("TermMonths")!.GetValue(forty)! == 40, "No recorta silenciosamente las 40 cuotas elegidas a 30.");
}
var normalizeName = typeof(QuotesController).GetMethod("NormalizeCustomerName", BindingFlags.NonPublic | BindingFlags.Static)!;
Check((string)normalizeName.Invoke(null, ["  María José Muñoz  "])! == "MARÍA JOSÉ MUÑOZ", "Nombre en mayusculas conserva tildes y elimina espacios externos.");
Check(normalizeName.Invoke(null, [" "]) is null, "Nombre opcional vacio permanece vacio.");
Console.WriteLine("OK: limites backend 1-40/1-24, plazo exacto con tasas antiguas y nombres en mayusculas.");

var resolveTerms = typeof(QuotesController).GetMethod("ResolveTerms", BindingFlags.NonPublic | BindingFlags.Static)!;
var selectedTerms = (int[])resolveTerms.Invoke(null, [new[] { 24, 12, 18, 12 }, 24, "Electrodomesticos"])!;
Check(selectedTerms.SequenceEqual(new[] { 12, 18, 24 }), "Alternativas ordenadas sin duplicados.");
Check(((int[])resolveTerms.Invoke(null, [null, 18, "Motos"])!).Single() == 18, "Contrato anterior conserva un plazo.");
foreach (var invalidTerms in new[] { Array.Empty<int>(), new[] { 12, 25 }, new[] { 0, 12 }, new[] { 41 } })
{
    try { resolveTerms.Invoke(null, [invalidTerms, 24, "Electrodomesticos"]); throw new Exception("Debe validar cada alternativa."); }
    catch (TargetInvocationException e) when (e.InnerException is ValidationException) { }
}
var calculateOptions = typeof(QuotesController).GetMethod("CalculateOptions", BindingFlags.NonPublic | BindingFlags.Static)!;
var financingOptions = (IReadOnlyCollection<QuoteFinancingOptionDto>)calculateOptions.Invoke(null, [selectedTerms, 1500000m, 300000m, 0m, 0m, 0m, null, null, null])!;
Check(financingOptions.Select(x => x.MonthlyPayment).SequenceEqual(new[] { 100000m, 66667m, 50000m }), "Cada plazo calcula su propia cuota sobre el mismo saldo.");
var savedOptions = System.Text.Json.JsonSerializer.Serialize(financingOptions);
Check(QuoteFinancingOptions.Read(savedOptions, "Manual", 12, 100000, 1500000).SequenceEqual(financingOptions), "Recupera valores guardados sin recalcular tasas actuales.");
Check(QuoteFinancingOptions.Read(savedOptions, "Contado", 12, 100000, 1500000).Count == 0, "Contado no muestra alternativas ocultas.");
cashQuote.TipoCredito = "Manual";
cashQuote.AlternativasPlazoJson = savedOptions;
cashQuote.Items.Single().TipoCredito = "Manual";
cashQuote.Items.Single().AlternativasPlazoJson = savedOptions;
var readOptions = (QuoteDto)typeof(QuotesController).GetMethod("ToDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [cashQuote])!;
Check(readOptions.FinancingOptions!.Count == 3 && readOptions.Items.Single().FinancingOptions!.Count == 3, "DTO devuelve todas las alternativas del registro y articulo.");
var readCustomerOptions = (QuoteDto)typeof(CustomersController).GetMethod("ToQuoteDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [cashQuote])!;
Check(readCustomerOptions.FinancingOptions!.Count == 3 && readCustomerOptions.Items.Single().FinancingOptions!.Count == 3, "Vista de cliente conserva alternativas.");
var optionsQuote = bundleDto with { Number = "COT-PRUEBA-PLAZOS", FinancingOptions = financingOptions, DownPayment = 300000, FinancedAmount = 1200000,
    InitialPaymentPaidToday = 300000, ProductPrice = 1500000, DiscountedProductPrice = 1500000, SalesPointRateName = "Tasa de prueba" };
var manyOptions = Enumerable.Range(1, 40).Select(term => new QuoteFinancingOptionDto(term, 1200000m / term, 1500000)).ToArray();
if (args.Length > 0)
{
    File.WriteAllBytes(Path.Combine(args[0], "term-options.pdf"), SimplePdfGenerator.Quote(optionsQuote, "Empresa de prueba"));
    File.WriteAllBytes(Path.Combine(args[0], "term-options-40.pdf"), SimplePdfGenerator.Quote(optionsQuote with { FinancingOptions = manyOptions }, "Empresa de prueba"));
}
Console.WriteLine("OK: multiplazos, deduplicacion, limites, persistencia de cuotas y PDF paginado.");

QuotePdfLayoutChecks.Run(optionsQuote, args);
CreditPhoneChecks.Run();
CreditFormDetailsChecks.Run();
CreditSignaturePdfChecks.Run(dto, args);

sealed class TestTenant : ITenantContext
{
    public Guid? EmpresaId { get; private set; } = Guid.NewGuid();
    public string? Subdominio => "test";
    public string UsuarioActual => "test";
    public void SetTenant(Guid empresaId, string? subdominio = null) => EmpresaId = empresaId;
}
