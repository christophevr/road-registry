namespace RoadRegistry.BackOffice.Extracts
{
    using FluentValidation;

    public class RequestRoadNetworkExtractValidator : AbstractValidator<Messages.RequestRoadNetworkExtract>
    {
        private const int MaximumDescriptionStringLength = 250;

        public RequestRoadNetworkExtractValidator()
        {
            RuleFor(c => c.ExternalRequestId).NotEmpty();
            RuleFor(c => c.DownloadId).NotEmpty();
            RuleFor(c => c.Description).MaximumLength(MaximumDescriptionStringLength);
            RuleFor(c => c.Contour).NotNull().SetValidator(new RoadNetworkExtractGeometryValidator());
        }
    }
}
