using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RentalClient;

public record AuthRequest(string Login, string Password);
public record AuthResponse(string Token, Guid UserId, string UserName, string Email, string Phone);

public record UpdateProfileRequest(Guid UserId, string UserName, string Email, string Phone);
public record RegisterRequest(string Email, string UserName, string Phone, string Password);

public record ShortRentalResponse(
    Guid Id, 
    string Title, 
    decimal Price, 
    string City, 
    [property: JsonPropertyName("mainThumbnailUrl")] string ImageUrl);

public record RentalRequest(
    string Title,
    string Description,
    decimal Price,
    string City,
    string Address,
    string Type,
    float LivingSpace,
    int RoomCount);

public record RentalResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string City,
    string Address,
    string Type,
    float LivingSpace,
    int RoomCount,
    Guid UserId,
    DateTime CreateDateTime,
    [property: JsonPropertyName("fullImageUrls")] List<string> ImageUrl);

public record RentalPostResponse(Guid Id);