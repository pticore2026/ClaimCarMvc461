# SDK mở rộng nghiệp vụ

Project `ClaimCar.Sdk` tách contract khỏi web app. DLL mở rộng chỉ cần reference `ClaimCar.Sdk.dll`, kế thừa `ClaimExtensionBase` hoặc implement `IClaimExtension`, sau đó copy DLL vào `ClaimCar.Web/App_Data/ClaimExtensions`.

Extension nhận `ExtensionContext` gồm `Module`, `Operation` (Create/Update/Delete), `Entity`, `UserName`. Có các hook `Validate`, `BeforeSave`, `AfterSave`, `BeforeDelete`, `AfterDelete`.

Ví dụ:

```csharp
public class RequireSurveyorExtension : ClaimExtensionBase
{
    public override string Name { get { return "RequireSurveyor"; } }
    public override ExtensionValidationResult Validate(ExtensionContext context)
    {
        if (context.Module == "Thông tin chung")
        {
            var c = context.Entity as dynamic;
            if (c != null && string.IsNullOrWhiteSpace((string)c.SurveyorCode))
                return ExtensionValidationResult.Fail("Bắt buộc gán giám định viên.");
        }
        return ExtensionValidationResult.Ok();
    }
}
```

Cơ chế này phù hợp cho thay đổi rule, validation, tiền xử lý/hậu xử lý mà không sửa controller. Với thay đổi giao diện hoặc schema lớn, tạo module/service/repository mới theo cùng cấu trúc, thay vì nhét tất cả vào một controller khổng lồ.
