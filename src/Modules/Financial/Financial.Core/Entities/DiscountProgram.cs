using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class DiscountProgram : SoftDeletableEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public DiscountType Type { get; set; } = DiscountType.Custom;
    public decimal DiscountPercentage { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Description { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
