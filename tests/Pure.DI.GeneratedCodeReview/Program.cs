using Pure.DI;
using Shouldly;
using static Pure.DI.Tag;
using static Pure.DI.Lifetime;
// ReSharper disable InvertIf
// ReSharper disable LocalizableElement
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ClassNeverInstantiated.Global

DI.Setup(nameof(Composition))
    .Bind<IClock>().As(Singleton).To<SystemClock>()
    .Bind<ILogger>().As(Singleton).To<ConsoleAppLogger>()
    .Bind<ICache>().As(Singleton).To<MemoryCache>()
    .Bind<IConfiguration>().As(Singleton).To<AppConfiguration>()
    .Bind<IEnvironmentInfo>().As(Singleton).To<EnvironmentInfo>()
    .Bind<IRepository<TT>>().As(PerBlock).To<InMemoryRepository<TT>>()
    .Bind<IExternalApiClient>(Public, null).As(Singleton).To<PublicApiClient>()
    .Bind<IExternalApiClient>(Internal).As(Singleton).To<InternalApiClient>()
    .Bind<INotificationSender>(Email, null).To<EmailNotificationSender>()
    .Bind<INotificationSender>(Sms).To<SmsNotificationSender>()
    .Bind<INotificationSender>(Push).To<PushNotificationSender>()
    .Bind<IPaymentGateway>(Card, null).As(Singleton).To<StripePaymentGateway>()
    .Bind<IPaymentGateway>(Wallet).As(Singleton).To<WalletPaymentGateway>()
 #pragma warning disable DIW003
    .Bind<IPaymentGateway>(Offline).To<OfflinePaymentGateway>()
 #pragma warning restore DIW003
    .Bind<IInventoryService>().As(PerBlock).To<InventoryService>()
    .Bind<IAuditLog>().As(Singleton).To<AuditLog>()
    .Bind<IFraudCheck>().To<FraudCheck>()
    .Bind<IPricingService>().To<PricingService>()
    .Bind<IDiscountPolicy>().To<LoyaltyDiscountPolicy>()
    .Bind<IShippingCalculator>().To<ShippingCalculator>()
    .Bind<IOrderValidator>().To<OrderValidator>()
    .Bind<IOrderService>().To<OrderService>()
    .Bind<IOrderWorkflow>().To<OrderWorkflow>()
    .Bind<IBackgroundJob>().To<OrderCleanupJob>()
    .Bind<IBackgroundJob>(StockSync).To<StockSyncJob>()
    .Bind<IDatabaseConnection>().To<DatabaseConnection>()
    .Bind<IBatchReport>().To<BatchReport>()
    .Bind("today").To(() => DateOnly.FromDateTime(DateTime.UtcNow))
    .Bind<ApiCredentials>().To((
        IConfiguration configuration,
        [Tag(ApiToken)] string apiToken) =>
        new ApiCredentials(configuration.BaseUrl, apiToken))
    .Bind<IReceiptFormatter>().To((ReceiptFormatter formatter, [Tag("today")] DateOnly date) =>
    {
        formatter.Init(date);
        return formatter;
    })
    .Bind().To<OrderNumberGenerator>()
    .Bind().To<OrderRequestFactory>()
    .Bind().To<PaymentSession>()
    .Bind().To<PaymentAttempt>()
    .Bind().To<NotificationEnvelope>()
 #pragma warning disable DIW003
    .Bind().To<WarehouseReservation>()
 #pragma warning restore DIW003
    .Bind().To<ManualOrderCommand>()
    .Arg<string>("baseUrl", BaseUrl)
    .Arg<string>("apiToken", ApiToken)
    .RootArg<string>("operatorId", Operator)
    .RootArg<string>("manualOrderId", ManualOrderId)
    .RootArg<decimal>("manualAmount", ManualAmount)
    .Root<IOrderWorkflow>("OrderWorkflow")
    .Root<IBackgroundJob>("CleanupJob")
    .Root<IBackgroundJob>("StockSyncJob", StockSync)
    .Root<IBatchReport>("BatchReport")
    .Root<Owned<IOrderService>>("OwnedOrderService")
 #pragma warning disable DIW005
    .Root<ManualOrderCommand>("CreateManualOrder");
 #pragma warning restore DIW005

DI.Setup("SecondaryComposition")
    .Bind<IClock>().To<SystemClock>()
    .Bind().To<OrderNumberGenerator>()
    .Root<OrderNumberGenerator>("OrderNumberGenerator");

var composition = new Composition(
    baseUrl: "https://api.example.test",
    apiToken: "token-123");

var secondary = new SecondaryComposition();
secondary.OrderNumberGenerator.Next().ShouldStartWith("ORD-");

var workflow = composition.OrderWorkflow;
var result = workflow.Place("ORD-1001", 125.50m);
result.ShouldContain("Accepted ORD-1001 via Stripe");

composition.CleanupJob.Run().ShouldContain("cleanup");
composition.StockSyncJob.Run().ShouldContain("stock");
composition.BatchReport.Render().ShouldContain("orders");

var manual = composition.CreateManualOrder(
    operatorId: "ops-7",
    manualOrderId: "MAN-42",
    manualAmount: 18.25m);
manual.Execute().ShouldContain("ops-7:Accepted MAN-42 via Stripe");

using var owned = composition.OwnedOrderService;
owned.Value.Place(new OrderRequest("OWN-1", 5m)).ShouldContain("Accepted");

Console.WriteLine("Pure.DI generated-code review workload completed.");

interface IOrderService
{
    string Place(OrderRequest request);
}

interface IOrderWorkflow
{
    string Place(string orderId, decimal amount);
}

interface IPaymentGateway
{
    PaymentReceipt Charge(PaymentSession session);
}

interface INotificationSender
{
    string Send(NotificationEnvelope notification);
}

interface IInventoryService
{
    WarehouseReservation Reserve(OrderRequest request);
}

interface IAuditLog
{
    void Record(string message);
}

interface IClock
{
    DateTimeOffset Now { get; }
}

interface ILogger
{
    void Info(string message);
}

interface ICache
{
    string GetOrAdd(string key, Func<string> valueFactory);
}

interface IRepository<in T>
{
    void Save(T entity);
    int Count { get; }
}

interface IConfiguration
{
    string BaseUrl { get; }
}

interface IEnvironmentInfo
{
    string Name { get; }
}

interface IExternalApiClient
{
    string Get(string path);
}

interface IFraudCheck
{
    bool IsAllowed(OrderRequest request);
}

interface IPricingService
{
    decimal Total(OrderRequest request);
}

interface IDiscountPolicy
{
    decimal Apply(decimal amount);
}

interface IShippingCalculator
{
    decimal ShippingFor(OrderRequest request);
}

interface IOrderValidator
{
    void Validate(OrderRequest request);
}

interface IReceiptFormatter
{
    string Format(PaymentReceipt receipt);
}

interface IBackgroundJob
{
    string Run();
}

interface IDatabaseConnection : IDisposable
{
    string ConnectionId { get; }
}

interface IBatchReport
{
    string Render();
}

record OrderRequest(string OrderId, decimal Amount);
record OrderAccepted(string OrderId, decimal Amount);
record PaymentReceipt(string Gateway, decimal Amount);
record ApiCredentials(string BaseUrl, string Token);

sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

sealed class ConsoleAppLogger(IClock clock) : ILogger
{
    public void Info(string message) => _ = $"{clock.Now:o} {message}";
}

sealed class MemoryCache : ICache
{
    private readonly Dictionary<string, string> _items = [];

    public string GetOrAdd(string key, Func<string> valueFactory)
    {
        if (!_items.TryGetValue(key, out var value))
        {
            value = valueFactory();
            _items[key] = value;
        }

        return value;
    }
}

sealed class AppConfiguration([Tag(BaseUrl)] string baseUrl) : IConfiguration
{
    public string BaseUrl { get; } = baseUrl;
}

sealed class EnvironmentInfo : IEnvironmentInfo
{
    public string Name => "review";
}

sealed class InMemoryRepository<T>(ILogger logger) : IRepository<T>
{
    private readonly List<T> _items = [];

    public int Count => _items.Count;

    public void Save(T entity)
    {
        _items.Add(entity);
        logger.Info($"saved {typeof(T).Name}");
    }
}

sealed class PublicApiClient(ApiCredentials credentials, ICache cache) : IExternalApiClient
{
    public string Get(string path) => cache.GetOrAdd(path, () => $"{credentials.BaseUrl}/public/{path}");
}

sealed class InternalApiClient(ApiCredentials credentials) : IExternalApiClient
{
    public string Get(string path) => $"{credentials.BaseUrl}/internal/{path}";
}

sealed class EmailNotificationSender(IExternalApiClient apiClient, ILogger logger) : INotificationSender
{
    public string Send(NotificationEnvelope notification)
    {
        logger.Info(apiClient.Get("email"));
        return $"email:{notification.OrderId}";
    }
}

sealed class SmsNotificationSender([Tag(Internal)] IExternalApiClient apiClient) : INotificationSender
{
    public string Send(NotificationEnvelope notification) => $"sms:{apiClient.Get(notification.OrderId)}";
}

sealed class PushNotificationSender(IEnvironmentInfo environment) : INotificationSender
{
    public string Send(NotificationEnvelope notification) => $"push:{environment.Name}:{notification.OrderId}";
}

sealed class StripePaymentGateway(IReceiptFormatter formatter) : IPaymentGateway
{
    public PaymentReceipt Charge(PaymentSession session)
    {
        var receipt = new PaymentReceipt("Stripe", session.Amount);
        _ = formatter.Format(receipt);
        return receipt;
    }
}

sealed class WalletPaymentGateway : IPaymentGateway
{
    public PaymentReceipt Charge(PaymentSession session) => new("Wallet", session.Amount);
}

sealed class OfflinePaymentGateway : IPaymentGateway
{
    public PaymentReceipt Charge(PaymentSession session) => new("Offline", session.Amount);
}

sealed class InventoryService(IRepository<WarehouseReservation> reservations, [Tag(Internal)] IExternalApiClient apiClient) : IInventoryService
{
    public WarehouseReservation Reserve(OrderRequest request)
    {
        _ = apiClient.Get($"stock/{request.OrderId}");
        var reservation = new WarehouseReservation(request.OrderId);
        reservations.Save(reservation);
        return reservation;
    }
}

sealed class AuditLog(IClock clock, ILogger logger) : IAuditLog
{
    public void Record(string message) => logger.Info($"{clock.Now:o}:{message}");
}

sealed class FraudCheck([Tag(Internal)] IExternalApiClient apiClient) : IFraudCheck
{
    public bool IsAllowed(OrderRequest request) => apiClient.Get($"fraud/{request.OrderId}").Length > 0;
}

sealed class PricingService(IDiscountPolicy discountPolicy, IShippingCalculator shippingCalculator) : IPricingService
{
    public decimal Total(OrderRequest request) => discountPolicy.Apply(request.Amount) + shippingCalculator.ShippingFor(request);
}

sealed class LoyaltyDiscountPolicy : IDiscountPolicy
{
    public decimal Apply(decimal amount) => amount >= 100m ? amount * 0.95m : amount;
}

sealed class ShippingCalculator(IEnvironmentInfo environment) : IShippingCalculator
{
    public decimal ShippingFor(OrderRequest request) => environment.Name == "review" && request.Amount > 0 ? 4.99m : 0m;
}

sealed class OrderValidator(IFraudCheck fraudCheck) : IOrderValidator
{
    public void Validate(OrderRequest request)
    {
        if (request.Amount <= 0 || !fraudCheck.IsAllowed(request))
        {
            throw new InvalidOperationException(request.OrderId);
        }
    }
}

sealed class ReceiptFormatter : IReceiptFormatter
{
    private DateOnly _date;

    public void Init(DateOnly date) => _date = date;

    public string Format(PaymentReceipt receipt) => $"{_date:yyyy-MM-dd}:{receipt.Gateway}:{receipt.Amount:0.00}";
}

sealed class OrderService(
    IOrderValidator validator,
    IInventoryService inventory,
    IPricingService pricing,
    IPaymentGateway paymentGateway,
    [Tag(Email)] INotificationSender email,
    [Tag(Sms)] INotificationSender sms,
    [Tag(Push)] INotificationSender push,
    IRepository<OrderAccepted> acceptedOrders,
    Func<decimal, PaymentSession> paymentSessionFactory,
    Func<NotificationEnvelope> notificationFactory,
    IAuditLog auditLog)
    : IOrderService
{
    public string Place(OrderRequest request)
    {
        validator.Validate(request);
        _ = inventory.Reserve(request);
        var total = pricing.Total(request);
        var receipt = paymentGateway.Charge(paymentSessionFactory(total));
        var notification = notificationFactory() with { OrderId = request.OrderId };
        _ = email.Send(notification);
        _ = sms.Send(notification);
        _ = push.Send(notification);
        acceptedOrders.Save(new OrderAccepted(request.OrderId, total));
        auditLog.Record(request.OrderId);
        return $"Accepted {request.OrderId} via {receipt.Gateway}:{receipt.Amount:0.00}";
    }
}

sealed class OrderWorkflow(IOrderService orderService, OrderRequestFactory requestFactory) : IOrderWorkflow
{
    public string Place(string orderId, decimal amount) => orderService.Place(requestFactory.Create(orderId, amount));
}

sealed class OrderNumberGenerator(IClock clock)
{
    public string Next() => $"ORD-{clock.Now.ToUnixTimeMilliseconds()}";
}

sealed class OrderRequestFactory(OrderNumberGenerator generator)
{
    public OrderRequest Create(string? orderId, decimal amount) => new(orderId ?? generator.Next(), amount);
}

sealed class PaymentSession(decimal amount)
{
    public decimal Amount { get; } = amount;
}

sealed class PaymentAttempt(PaymentSession session, [Tag(Wallet)] IPaymentGateway gateway)
{
    public PaymentReceipt TryCharge() => gateway.Charge(session);
}

sealed record NotificationEnvelope(string OrderId = "", string Body = "accepted");

sealed record WarehouseReservation(string OrderId);

sealed class OrderCleanupJob(IDatabaseConnection connection, IRepository<OrderAccepted> orders) : IBackgroundJob
{
    public string Run() => $"cleanup:{connection.ConnectionId}:orders={orders.Count}";
}

sealed class StockSyncJob([Tag(Internal)] IExternalApiClient apiClient, ILogger logger) : IBackgroundJob
{
    public string Run()
    {
        logger.Info(apiClient.Get("stock-sync"));
        return "stock synchronized";
    }
}

sealed class DatabaseConnection : IDatabaseConnection
{
    public string ConnectionId { get; } = Guid.NewGuid().ToString("N");

    public void Dispose()
    {
    }
}

sealed class BatchReport(
    IRepository<OrderAccepted> acceptedOrders,
    IRepository<WarehouseReservation> reservations,
    Func<decimal, PaymentAttempt> paymentAttemptFactory)
    : IBatchReport
{
    public string Render()
    {
        _ = paymentAttemptFactory(12.34m).TryCharge();
        return $"orders={acceptedOrders.Count};reservations={reservations.Count}";
    }
}

sealed class ManualOrderCommand(
    [Tag(Operator)] string operatorId,
    [Tag(ManualOrderId)] string manualOrderId,
    [Tag(ManualAmount)] decimal manualAmount,
    IOrderWorkflow workflow)
{
    public string Execute() => $"{operatorId}:{workflow.Place(manualOrderId, manualAmount)}";
}
