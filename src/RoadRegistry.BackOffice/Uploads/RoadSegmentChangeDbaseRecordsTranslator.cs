namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class
        RoadSegmentChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry,
            IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            while (records.MoveNext())
            {
                var record = records.Current;
                if (record != null)
                {
                    switch (record.RECORDTYPE.Value)
                    {
                        case RecordType.AddedIdentifier:
                            changes = changes.AppendChange(
                                new AddRoadSegment(
                                    records.CurrentRecordNumber,
                                    record.EVENTIDN.HasValue && record.EVENTIDN.Value != 0
                                        ? new RoadSegmentId(record.EVENTIDN.Value)
                                        : new RoadSegmentId(record.WS_OIDN.Value),
                                    new RoadNodeId(record.B_WK_OIDN.Value),
                                    new RoadNodeId(record.E_WK_OIDN.Value),
                                    new OrganizationId(record.BEHEERDER.Value),
                                    RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                    RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                    RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                    RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                    RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                    record.LSTRNMID.Value.HasValue
                                        ? new CrabStreetnameId(record.LSTRNMID.Value.Value)
                                        : new CrabStreetnameId?(),
                                    record.RSTRNMID.Value.HasValue
                                        ? new CrabStreetnameId(record.RSTRNMID.Value.Value)
                                        : new CrabStreetnameId?()
                                )
                            );
                            break;
                        case RecordType.IdenticalIdentifier:
                            changes = changes.AppendProvisionalChange(
                                new ModifyRoadSegment(
                                    records.CurrentRecordNumber,
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    new RoadNodeId(record.B_WK_OIDN.Value),
                                    new RoadNodeId(record.E_WK_OIDN.Value),
                                    new OrganizationId(record.BEHEERDER.Value),
                                    RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                    RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                    RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                    RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                    RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                    record.LSTRNMID.Value.HasValue ? new CrabStreetnameId(record.LSTRNMID.Value.Value) : new CrabStreetnameId?(),
                                    record.RSTRNMID.Value.HasValue ? new CrabStreetnameId(record.RSTRNMID.Value.Value) : new CrabStreetnameId?()
                                )
                            );
                            break;
                        case RecordType.ModifiedIdentifier:
                            changes = changes.AppendChange(
                                new ModifyRoadSegment(
                                    records.CurrentRecordNumber,
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    new RoadNodeId(record.B_WK_OIDN.Value),
                                    new RoadNodeId(record.E_WK_OIDN.Value),
                                    new OrganizationId(record.BEHEERDER.Value),
                                    RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                    RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                    RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                    RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                    RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                    record.LSTRNMID.Value.HasValue
                                        ? new CrabStreetnameId(record.LSTRNMID.Value.Value)
                                        : new CrabStreetnameId?(),
                                    record.RSTRNMID.Value.HasValue
                                        ? new CrabStreetnameId(record.RSTRNMID.Value.Value)
                                        : new CrabStreetnameId?()
                                )
                            );
                            break;
                        case RecordType.RemovedIdentifier:
                            changes = changes.AppendChange(
                                new RemoveRoadSegment(
                                    records.CurrentRecordNumber,
                                    new RoadSegmentId(record.WS_OIDN.Value)
                                )
                            );
                            break;
                    }
                }
            }

            return changes;
        }
    }
}
