using System;
using System.Collections.Generic;

namespace ClaimCar.Web.Models
{
    public class AttachmentViewModel
    {
        public int? ClaimId { get; set; }
        public string ClaimNumber { get; set; }
        public string Type { get; set; }
        public string Sort { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public IList<Claim> Claims { get; set; }
        public IList<AttachmentCategory> Categories { get; set; }
        public IList<AttachmentItem> Files { get; set; }
        public IList<AttachmentItem> Images { get; set; }
    }
    public class AttachmentCategory { public string Code { get; set; } public string Name { get; set; } }
    public class AttachmentItem
    {
        public string FileName { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsImage { get; set; }
    }
    public class AttachmentPreviewViewModel
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; }
        public AttachmentItem File { get; set; }
    }
}
