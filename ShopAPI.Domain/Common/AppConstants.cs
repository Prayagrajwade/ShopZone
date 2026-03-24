namespace ShopAPI.Common
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string User = "user";
            public const string Admin = "admin";
        }

        public static class OrderStatus
        {
            public const string Pending = "pending";
            public const string Paid = "paid";
            public const string OnTheWay = "on_the_way";
            public const string PaymentFailed = "payment_failed";
        }

        public static class PaymentEvents
        {
            public const string Created = "payment_intent.created";
            public const string Checked = "payment_intent.checked";
            public const string FailedManualCheck = "payment_intent.failed_manual_check";
            public const string Succeeded = "payment_intent.succeeded";
            public const string Failed = "payment_intent.payment_failed";
        }

        public static class PaymentStatus
        {
            public const string Succeeded = "succeeded";
            public const string Failed = "failed";
            public const string Pending = "requires_payment_method";
        }

        public static class Defaults
        {
            public const string Currency = "usd";
        }

        public static class Notes
        {
            public const string PaymentSucceeded = "Payment succeeded via Stripe";
            public const string PaymentFailed = "Payment failed via Stripe";
            public const string OrderCreated = "Order created after successful payment";
        }

        public static class Errors
        {
            public const string CartEmpty = "Cart is empty.";
            public const string PaymentNotCompleted = "Payment not completed.";
        }

        public static class TemeplateType
        {
            public const string Invoice = "Invoice";
        }

    }
}
