namespace TadHub.SharedKernel.Api;

/// <summary>
/// Marker interface for minimal reference DTOs.
/// 
/// Every module defines {Entity}RefDto (e.g., ClientRefDto { Id, Name }) 
/// implementing IRefDto for use in nested objects without include,
/// and {Entity}Dto for the full shape when included.
/// 
/// Services expose ToRefDto() and ToDto() mapping methods.
/// The controller (or include logic) decides which to call.
/// 
/// Example:
/// - ClientRefDto { Id, Name } - shown in contract.client when include=client is NOT used
/// - ClientDto { Id, Name, EmiratesId, Category, Phone... } - shown when include=client IS used
/// </summary>
public interface IRefDto
{
    /// <summary>
    /// The entity's unique identifier.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Base record for reference DTOs with Id and a display name.
/// Most RefDtos need just these two fields.
/// </summary>
public record RefDtoBase : IRefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
