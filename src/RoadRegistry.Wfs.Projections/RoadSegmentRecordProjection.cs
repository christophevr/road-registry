namespace RoadRegistry.Wfs.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Schema;
    using Syndication.Schema;

    public class RoadSegmentRecordProjection : ConnectedProjection<WfsContext>
    {
        public RoadSegmentRecordProjection(IStreetNameCache streetNameCache)
        {
            When<Envelope<SynchronizeWithStreetNameCache>>(async (context, envelope, token) =>
            {
                var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);

                var outdatedRoadSegments = await context.RoadSegments
                    .Where(record => record.StreetNameCachePosition < streetNameCachePosition)
                    .OrderBy(record => record.StreetNameCachePosition)
                    .Take(envelope.Message.BatchSize)
                    .ToListAsync();

                var outdatedStreetNameIds = outdatedRoadSegments
                    .Select(record => record.LeftSideStreetNameId)
                    .Union(outdatedRoadSegments.Select(record => record.RightSideStreetNameId))
                    .Where(i => i.HasValue)
                    .Select(i => i.Value);

                var streetNamesById = await streetNameCache.GetStreetNamesByIdAsync(outdatedStreetNameIds, token);

                foreach (var roadSegment in outdatedRoadSegments)
                {
                    if (roadSegment.LeftSideStreetNameId.HasValue &&
                        streetNamesById.ContainsKey(roadSegment.LeftSideStreetNameId.Value))
                        roadSegment.LeftSideStreetName = streetNamesById[roadSegment.LeftSideStreetNameId.Value];

                    if(roadSegment.RightSideStreetNameId.HasValue &&
                       streetNamesById.ContainsKey(roadSegment.RightSideStreetNameId.Value))
                        roadSegment.RightSideStreetName = streetNamesById[roadSegment.RightSideStreetNameId.Value];

                    roadSegment.StreetNameCachePosition = streetNameCachePosition;
                }
            });

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var status = RoadSegmentStatus.Parse(envelope.Message.Status);
                var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var category = RoadSegmentCategory.Parse(envelope.Message.Category);
                var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
                var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.LeftSide.StreetNameId, token);
                var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.RightSide.StreetNameId, token);

                await context.RoadSegments.AddAsync(new RoadSegmentRecord
                {
                    Id = envelope.Message.Id,
                    BeginTime = envelope.Message.Origin.Since,
                    MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                    MaintainerName = envelope.Message.MaintenanceAuthority.Name,
                    MethodDutchName = method.Translation.Name,
                    CategoryDutchName = category.Translation.Name,
                    Geometry2D = WfsGeometryTranslator.Translate2D(envelope.Message.Geometry),
                    MorphologyDutchName = morphology.Translation.Name,
                    StatusDutchName = status.Translation.Name,
                    AccessRestrictionId = accessRestriction.Translation.Identifier,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    LeftSideStreetName = leftSideStreetNameRecord?.DutchNameWithHomonymAddition ??
                                         envelope.Message.LeftSide.StreetName,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    RightSideStreetName = rightSideStreetNameRecord?.DutchNameWithHomonymAddition ??
                                          envelope.Message.RightSide.StreetName,
                    BeginRoadNodeId = envelope.Message.StartNodeId,
                    EndRoadNodeId = envelope.Message.EndNodeId,
                    StreetNameCachePosition = streetNameCachePosition,
                }, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {

                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            await AddRoadSegment(streetNameCache, context, envelope, roadSegmentAdded, token);
                            break;

                        case RoadSegmentModified roadSegmentModified:
                            await ModifyRoadSegment(streetNameCache, context, envelope, roadSegmentModified, token);
                            break;

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await RemoveRoadSegment(roadSegmentRemoved, context);
                            break;
                    }
                }
            });
        }

        private static async Task AddRoadSegment(IStreetNameCache streetNameCache,
            WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAdded roadSegmentAdded,
            CancellationToken token)
        {

            var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);

            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);

            var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);

            var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);

            var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.RightSide.StreetNameId, token);

            await context.RoadSegments.AddAsync(new RoadSegmentRecord
            {
                Id = roadSegmentAdded.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = roadSegmentAdded.MaintenanceAuthority.Name,
                MethodDutchName = method.Translation.Name,
                CategoryDutchName = category.Translation.Name,
                Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry),
                MorphologyDutchName = morphology.Translation.Name,
                StatusDutchName = status.Translation.Name,
                AccessRestrictionId = accessRestriction.Translation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                LeftSideStreetName = leftSideStreetNameRecord?.DutchName,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                RightSideStreetName = rightSideStreetNameRecord?.DutchName,
                BeginRoadNodeId = roadSegmentAdded.StartNodeId,
                EndRoadNodeId = roadSegmentAdded.EndNodeId,
                StreetNameCachePosition = streetNameCachePosition,
            }, token);
        }

        private static async Task ModifyRoadSegment(IStreetNameCache streetNameCache,
            WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentModified roadSegmentModified,
            CancellationToken token)
        {
            var method =
                RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);

            var accessRestriction =
                RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);

            var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);

            var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);

            var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.RightSide.StreetNameId, token);

            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id).ConfigureAwait(false);

            if (roadSegmentRecord == null)
                throw new Exception($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found!");

            roadSegmentRecord.Id = roadSegmentModified.Id;
            roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadSegmentRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
            roadSegmentRecord.MaintainerName = roadSegmentModified.MaintenanceAuthority.Name;
            roadSegmentRecord.MethodDutchName = method.Translation.Name;
            roadSegmentRecord.CategoryDutchName = category.Translation.Name;
            roadSegmentRecord.Geometry2D = WfsGeometryTranslator.Translate2D(roadSegmentModified.Geometry);
            roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;
            roadSegmentRecord.StatusDutchName = status.Translation.Name;
            roadSegmentRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            roadSegmentRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
            roadSegmentRecord.LeftSideStreetName = leftSideStreetNameRecord?.DutchName;
            roadSegmentRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
            roadSegmentRecord.RightSideStreetName = rightSideStreetNameRecord?.DutchName;
            roadSegmentRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
            roadSegmentRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
            roadSegmentRecord.StreetNameCachePosition = streetNameCachePosition;
        }

        private static async Task RemoveRoadSegment(RoadSegmentRemoved roadSegmentRemoved, WfsContext context)
        {
            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                return;
            }
            context.RoadSegments.Remove(roadSegmentRecord);
        }

        private static async Task<StreetNameRecord> TryGetFromCache(
            IStreetNameCache streetNameCache,
            int? streetNameId,
            CancellationToken token)
        {
            return streetNameId.HasValue ?
                await streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) :
                null;
        }
    }
}
