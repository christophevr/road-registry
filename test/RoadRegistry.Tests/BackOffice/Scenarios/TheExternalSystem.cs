namespace RoadRegistry.BackOffice.Scenarios
{
    using Framework;
    using Messages;

    public static class TheExternalSystem
    {
        public static Command PutsInARoadNetworkExtractRequest(
            ExternalExtractRequestId requestId,
            DownloadId downloadId,
            RoadNetworkExtractGeometry contour,
            ExtractDescription description)
        {
            return new Command(new RequestRoadNetworkExtract
            {
                ExternalRequestId = requestId,
                DownloadId = downloadId,
                Contour = contour,
                Description = description
            });
        }

        public static Command UploadsRoadNetworkExtractChangesArchive(
            ExtractRequestId requestId,
            DownloadId downloadId,
            UploadId uploadId,
            ArchiveId archiveId)
        {
            return new Command(new UploadRoadNetworkExtractChangesArchive
            {
                RequestId = requestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            });
        }
    }
}
