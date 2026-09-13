using ErrorOr;
using Offices.Application.Common.Interfaces;
using Offices.Application.Queries.GetReferencedPhotos;

namespace Offices.UnitTests.Application.Queries;

public sealed class GetReferencedPhotosQueryHandlerTests
{
    private static readonly Guid FirstPhotoId = new("1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c17");
    private static readonly Guid SecondPhotoId = new("2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d18");
    private static readonly Guid UnrequestedPhotoId = new("3c4d5e6f-7a8b-4c9d-8e1f-2a3b4c5d6e19");

    private readonly IPhotoReferenceRepository _repository = Substitute.For<IPhotoReferenceRepository>();
    private readonly GetReferencedPhotosQueryHandler _handler;

    public GetReferencedPhotosQueryHandlerTests()
    {
        _handler = new GetReferencedPhotosQueryHandler(_repository);
    }

    private void GivenPhotoUrls(params string[] photoUrls)
        => _repository.GetPhotoUrlsAsync(Arg.Any<CancellationToken>()).Returns(photoUrls);

    private Task<ErrorOr<IReadOnlyList<Guid>>> Handle(params Guid[] documentIds)
        => _handler.Handle(new GetReferencedPhotosQuery(documentIds), TestContext.Current.CancellationToken);

    [Fact]
    public async Task Handle_NoDocumentIds_ReturnsEmptyWithoutReadingPhotos()
    {
        var result = await Handle();

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _repository.DidNotReceive().GetPhotoUrlsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoOfficeHasPhoto_ReturnsEmpty()
    {
        GivenPhotoUrls();

        var result = await Handle(FirstPhotoId);

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PhotoUrlContainsRequestedId_ReturnsIt()
    {
        GivenPhotoUrls($"/api/documents/{FirstPhotoId}");

        var result = await Handle(FirstPhotoId);

        result.Value.Should().Equal(FirstPhotoId);
    }

    [Fact]
    public async Task Handle_IdWrittenInUpperCase_ReturnsIt()
    {
        GivenPhotoUrls($"/api/documents/{FirstPhotoId.ToString().ToUpperInvariant()}");

        var result = await Handle(FirstPhotoId);

        result.Value.Should().Equal(FirstPhotoId);
    }

    [Fact]
    public async Task Handle_UrlWithSeveralIds_ReturnsEveryRequestedOne()
    {
        GivenPhotoUrls($"/api/documents/{FirstPhotoId}/versions/{SecondPhotoId}");

        var result = await Handle(FirstPhotoId, SecondPhotoId);

        result.Value.Should().BeEquivalentTo([FirstPhotoId, SecondPhotoId]);
    }

    [Fact]
    public async Task Handle_RequestedIdNotReferenced_IsNotReturned()
    {
        GivenPhotoUrls(
            $"/api/documents/{FirstPhotoId}",
            $"/api/documents/{UnrequestedPhotoId}",
            "/api/documents/not-a-guid",
            string.Empty);

        var result = await Handle(FirstPhotoId, SecondPhotoId);

        result.Value.Should().Equal(FirstPhotoId);
    }
}
