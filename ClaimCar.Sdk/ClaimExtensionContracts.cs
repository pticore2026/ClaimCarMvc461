using System;
using System.Collections.Generic;

namespace ClaimCar.Sdk
{
    public enum ExtensionOperation { Create, Update, Delete }

    public sealed class ExtensionContext
    {
        public string Module { get; set; }
        public ExtensionOperation Operation { get; set; }
        public object Entity { get; set; }
        public string UserName { get; set; }
        public IDictionary<string, object> Items { get; private set; }
        public ExtensionContext() { Items = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase); }
    }

    public sealed class ExtensionValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public static ExtensionValidationResult Ok() { return new ExtensionValidationResult { IsValid = true }; }
        public static ExtensionValidationResult Fail(string message) { return new ExtensionValidationResult { IsValid = false, Message = message }; }
    }

    public interface IClaimExtension
    {
        string Name { get; }
        int Order { get; }
        ExtensionValidationResult Validate(ExtensionContext context);
        void BeforeSave(ExtensionContext context);
        void AfterSave(ExtensionContext context);
        void BeforeDelete(ExtensionContext context);
        void AfterDelete(ExtensionContext context);
    }

    public abstract class ClaimExtensionBase : IClaimExtension
    {
        public abstract string Name { get; }
        public virtual int Order { get { return 100; } }
        public virtual ExtensionValidationResult Validate(ExtensionContext context) { return ExtensionValidationResult.Ok(); }
        public virtual void BeforeSave(ExtensionContext context) { }
        public virtual void AfterSave(ExtensionContext context) { }
        public virtual void BeforeDelete(ExtensionContext context) { }
        public virtual void AfterDelete(ExtensionContext context) { }
    }
}
